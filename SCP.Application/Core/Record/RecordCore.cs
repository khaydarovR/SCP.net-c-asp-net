using FluentValidation.Results;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SCP.Application.Common;
using SCP.Application.Common.Helpers;
using SCP.Application.Common.Response;
using SCP.Application.Common.Validators;
using SCP.Application.Core.ApiKey;
using SCP.Application.Core.Safe;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Enum;

namespace SCP.Application.Core.Record
{
    public partial class RecordCore : BaseCore
    {
        private readonly AppDbContext dbContext;
        private readonly AsymmetricCryptoService asymmetricCryptoService;
        private readonly SymmetricCryptoService symmetricCrypto;
        private readonly SafeCore safeCore;
        private readonly SafeGuardCore safeGuard;
        private readonly RLogService rLog;

        public RecordCore(AppDbContext dbContext,
                          AsymmetricCryptoService asymmetricCryptoService,
                          SymmetricCryptoService symmetricCrypto,
                          SafeCore safeCore,
                          SafeGuardCore safeGuard,
                          RLogService rLog)
        {
            this.dbContext = dbContext;
            this.asymmetricCryptoService = asymmetricCryptoService;
            this.symmetricCrypto = symmetricCrypto;
            this.safeCore = safeCore;
            this.safeGuard = safeGuard;
            this.rLog = rLog;
        }

        public async Task<CoreResponse<bool>> CreateRecord(CreateRecordCommand command)
        {
            var prk = safeCore.GetClearPrivateKeyFromSafe(command.SafeId);

            //var signatureIsValid = RSAUtils.VerifySignature(command.ClientPrivK, rawData, command.Signature);
            //if (signatureIsValid == false )
            //{
            //    return Bad<bool>("Цифровая подпись не прошла проверку");
            //}

            var neadPer = SystemSafePermisons.AddRecordToSafe;
            var isAccess = safeGuard.AuthorHasAccessToSafe(command.SafeId, Guid.Parse(command.UserId), neadPer.Slug);
            if (isAccess == false)
            {
                return Bad<bool>("Отстутсвует разрешение на: " + neadPer.Name);
            }

            CreateRecordV validator = new();
            ValidationResult results = validator.Validate(command);
            if (results.IsValid == false)
            {
                return Bad<bool>(results.Errors.Select(e => e.ErrorMessage).ToArray());
            }


            var recordId = Guid.NewGuid();
            dbContext.Records.Add(new Domain.Entity.Record
            {
                ELogin = command.Login,
                EPw = command.Pw,
                ESecret = command.Secret,
                ForResource = command.ForResource,
                Title = command.Title,
                IsDeleted = false,
                SafeId = Guid.Parse(command.SafeId),
                Id = recordId,
            });

            dbContext.RecordRights.Add(new Domain.Entity.RecordRight
            {
                AppUserId = Guid.Parse(command.UserId),
                RecordId = recordId,
                EnumPermission = RecRightEnum.Delete
            });

            //дать права всем пользователям сейфа для новой записи
            var safeUsers = await safeCore.GetAllUsersFromSafe(Guid.Parse(command.SafeId));
            foreach (var user in safeUsers.Data!)
            {
                dbContext.RecordRights.Add(new Domain.Entity.RecordRight
                {
                    AppUserId = user.Id,
                    RecordId = recordId,
                    EnumPermission = RecRightEnum.Delete
                });
            }

            await rLog.Push("Создание на основаннии разрешения: " + neadPer.Name, recordId);

            return Good(true);
        }


        public async Task<CoreResponse<List<GetRecordResponse>>> GetAllRecord(string safeId, Guid authorId)
        {
            var isAccess = safeGuard.AuthorHasAccessToSafe(Guid.Parse(safeId), authorId, SystemSafePermisons.GetRecordList.Slug);
            if (isAccess == false)
            {
                return Bad<List<GetRecordResponse>>("Отстутсвует разрешение на: " + SystemSafePermisons.GetRecordList.Name);
            }
            var records = await dbContext.Records
                .Where(r => r.SafeId == Guid.Parse(safeId))
                .Where(r => r.IsDeleted == false)
                .ProjectToType<GetRecordResponse>()
                .ToListAsync();


            foreach (var record in records)
            {
                var rightInCurrentRec = await dbContext.RecordRights
                    .FirstAsync(rr => rr.AppUserId == authorId && rr.RecordId == record.Id);

                record.RightToCurentUser = rightInCurrentRec.MapRightEnumToString();
            }


            return Good(records);
        }


        /// <summary>
        /// Обновить секрет
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CoreResponse<bool>> PatchRecord(PatchRecordCommand command)
        {
            PatchRecordV validator = new();
            ValidationResult results = validator.Validate(command);
            if (results.IsValid == false)
            {
                return Bad<bool>(results.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var dbRec = dbContext.Records
                .Include(r => r.RightUsers)
                .FirstOrDefault(r => r.Id == Guid.Parse(command.Id));

            if (dbRec == null)
            {
                return Bad<bool>("Не найден секрет");
            }


            if (safeGuard.AuthorHasAccessToSafe(dbRec.SafeId, Guid.Parse(command.UserId), SystemSafePermisons.ReadAndEditSecrets.Slug) == false)
            {
                return Bad<bool>("Отстутсвует разрешение на: " + SystemSafePermisons.ReadAndEditSecrets.Name);
            }

            if (safeGuard.IsHaveRight(Guid.Parse(command.UserId), dbRec.Id, RecRightEnum.Edit) == false)
            {
                return Bad<bool>("Отстутсвует разрешение на: " + EnumUtil.MapRightEnumToString(RecRightEnum.Edit));
            }

            if (command.IsDeleted)
            {

                if (safeGuard.AuthorHasAccessToSafe(dbRec.SafeId, Guid.Parse(command.UserId), SystemSafePermisons.SoftDelete.Slug) == false)
                {
                    return Bad<bool>("Отстутсвует разрешение на: " + SystemSafePermisons.SoftDelete.Name);
                }
                if (safeGuard.IsHaveRight(Guid.Parse(command.UserId), dbRec.Id, RecRightEnum.Delete) == false)
                {
                    return Bad<bool>("Отстутсвует разрешение на: " + EnumUtil.MapRightEnumToString(RecRightEnum.Delete));
                }
            }

            dbRec.Title = command.Title;
            dbRec.ELogin = command.Login;
            dbRec.EPw = command.Pw;
            dbRec.ESecret = command.Secret;
            dbRec.IsDeleted = command.IsDeleted;
            dbRec.ForResource = command.ForResource;

            dbContext.Records.Update(dbRec);
            dbContext.SaveChanges();
            await rLog.Push("Редактирование на основаннии разрешения: " + SystemSafePermisons.ReadAndEditSecrets.Name, command.Id);
            return Good(true);
        }


        public async Task<CoreResponse<ReadMatchRecordResponse>> ReadBestMatch(string forRes, Guid contextUserId)
        {
            var linkedSafesRes = await safeCore.GetLinked(new GetLinkedSafesQuery { UserId = contextUserId });

            if (linkedSafesRes.IsSuccess == false)
            {
                return Bad<ReadMatchRecordResponse>(linkedSafesRes.ErrorList.ToArray());
            }

            var linkedSafes = linkedSafesRes.Data.ToList();

            var filtredSafesIds = new List<Guid>();
            foreach (var s in linkedSafes)
            {
                var canReed = safeGuard.AuthorHasAccessToSafe(s.Id, contextUserId, SystemSafePermisons.ReadSecrets.Slug);
                if (canReed)
                {
                    filtredSafesIds.Add(Guid.Parse(s.Id));
                }
            }

            var authorName = dbContext.AppUsers.Where(u => u.Id == contextUserId).Select(u => u.UserName).FirstOrDefault();
            var resourseName = GetDomainName(forRes);
            var bestMatchRec = dbContext.Records
                .Where(r => filtredSafesIds.Any(id => r.SafeId == id))
                .FirstOrDefault(r => r.ForResource.Contains(resourseName) || r.ForResource.Contains($"@{authorName}"));

            if (bestMatchRec == null)
            {
                return Bad<ReadMatchRecordResponse>("Нет подходящей записей для ресурса " + forRes);
            }


            var res = new ReadMatchRecordResponse();
            var clearSafePrivateKey = safeCore.GetClearPrivateKeyFromSafe(bestMatchRec.SafeId.ToString());

            res.Login = asymmetricCryptoService.DecryptFromClientData(bestMatchRec.ELogin, clearSafePrivateKey);
            res.Pw = asymmetricCryptoService.DecryptFromClientData(bestMatchRec.EPw, clearSafePrivateKey);
            res.Secret = asymmetricCryptoService.DecryptFromClientData(bestMatchRec.ESecret, clearSafePrivateKey);
            res.Title = bestMatchRec.Title;
            res.Id = bestMatchRec.Id.ToString();

            await rLog.Push("Чтение для расширения на основаннии разрешения пользователя по личному токену: " + SystemSafePermisons.ReadSecrets.Name, res.Id);

            return Good(res);

        }

        private string GetDomainName(string url)
        {
            // Удаление протокола и слэшей из URL
            string cleanedUrl = url.Replace("https://", "").Replace("http://", "").Trim('/');

            // Парсинг значения между "://" и первым встреченным слэшем (/)
            int startIndex = cleanedUrl.IndexOf("://") + 3;
            int endIndex = cleanedUrl.IndexOf('/');

            // Получение подстроки с именем домена
            string domainName = cleanedUrl.Substring(startIndex, endIndex - startIndex);

            return domainName;
        }


        /// <summary>
        /// Отправляет данные зашифрованные с помощью полученного ключа от клиента
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CoreResponse<ReadRecordResponse>> ReadRecord(ReadRecordCommand command)
        {

            //зашфирвонная запись
            var rec = await dbContext.Records
                .Include(r => r.Safe)
                .FirstAsync(r => r.Id == command.RecordId);

            var isAccessSafe = safeGuard.AuthorHasAccessToSafe(rec.SafeId, command.AuthorId, SystemSafePermisons.ReadSecrets.Slug);
            if (isAccessSafe == false)
            {
                return Bad<ReadRecordResponse>("Отстутсвует разрешение на: " + SystemSafePermisons.ReadSecrets.Name);
            }

            var isAccessRec = safeGuard.IsHaveRight(command.AuthorId, command.RecordId, RecRightEnum.Read);
            if (isAccessRec == false)
            {
                return Bad<ReadRecordResponse>("Отстутсвует разрешение на: " + EnumUtil.MapRightEnumToString(RecRightEnum.Read));
            }


            //получение ключа
            var clearSafePrivateKey = safeCore.GetClearPrivateKeyFromSafe(rec.SafeId.ToString());

            //расшифровка
            var clearLogin = asymmetricCryptoService.DecryptFromClientData(rec.ELogin, clearSafePrivateKey);
            var clearPw = asymmetricCryptoService.DecryptFromClientData(rec.EPw, clearSafePrivateKey);
            var clearSecret = asymmetricCryptoService.DecryptFromClientData(rec.ESecret, clearSafePrivateKey);

            //шифрование с помощью публичного ключа клиента
            var eLoging = asymmetricCryptoService.EncryptDataForClient(clearLogin, command.PubKeyFromClient);
            var ePw = asymmetricCryptoService.EncryptDataForClient(clearPw, command.PubKeyFromClient);
            var eSecret = asymmetricCryptoService.EncryptDataForClient(clearSecret, command.PubKeyFromClient);


            var data = new ReadRecordResponse
            {
                Id = rec.Id.ToString(),
                ELogin = eLoging,
                EPw = ePw,
                ESecret = eSecret,
                Title = rec.Title,
                ForResource = rec.ForResource,
                IsDeleted = rec.IsDeleted,
            };
            var author = dbContext.AppUsers.FirstOrDefault(x => x.Id == command.AuthorId);
            await rLog.Push(author?.Email + " | Чтение в зашифрованном виде на основаннии разрешения: " + SystemSafePermisons.ReadSecrets.Name, data.Id);

            return new CoreResponse<ReadRecordResponse>(data);

        }

        public async Task<CoreResponse<ReadRecordResponse>> ReadWithKey(string apiKey, ReadRecordWithKeyCommand cmd)
        {
            if (Guid.TryParse(cmd.RecordId.ToString(), out var res) == false)
            {
                return Bad<ReadRecordResponse>("Не правильный формат ID: " + cmd.RecordId);
            }
            var dbRec = dbContext.Records.FirstOrDefault(r => r.Id == Guid.Parse(cmd.RecordId) && r.IsDeleted == false);
            if (dbRec == null)
            {
                return Bad<ReadRecordResponse>("Секрет с идентификатором " + cmd.RecordId + " не найден");
            }

            var msg = "";
            var keyIsValid = safeGuard.ApiKeyIsValid(apiKey, dbRec.SafeId, out msg);
            if (keyIsValid == false)
            {
                return Bad<ReadRecordResponse>(msg);
            }

            //получение ключа
            var clearSafePrivateKey = safeCore.GetClearPrivateKeyFromSafe(dbRec.SafeId.ToString());

            //расшифровка
            var clearLogin = asymmetricCryptoService.DecryptFromClientData(dbRec.ELogin, clearSafePrivateKey);
            var clearPw = asymmetricCryptoService.DecryptFromClientData(dbRec.EPw, clearSafePrivateKey);
            var clearSecret = asymmetricCryptoService.DecryptFromClientData(dbRec.ESecret, clearSafePrivateKey);

            //шифровка
            var eLogin = asymmetricCryptoService.EncryptDataForClient(clearLogin, cmd.PubKeyFromClient);
            var ePw = asymmetricCryptoService.EncryptDataForClient(clearPw, cmd.PubKeyFromClient);
            var eSecret = asymmetricCryptoService.EncryptDataForClient(clearSecret, cmd.PubKeyFromClient);

            var data = new ReadRecordResponse
            {
                Id = dbRec.Id.ToString(),
                ELogin = eLogin,
                EPw = ePw,
                ESecret = eSecret,
                Title = dbRec.Title,
                ForResource = dbRec.ForResource,
                IsDeleted = dbRec.IsDeleted,
            };
            await rLog.Push("Чтение секрета на основаннии API KEY: " + SystemSafePermisons.ReadSecrets.Name, data.Id);

            return new CoreResponse<ReadRecordResponse>(data);

        }

        public async Task<CoreResponse<List<RLogsResponse>>> GetLogs(Guid rId, Guid contextUserId)
        {
            var logs = await dbContext.ActivityLogs
                .Where(l => l.RecordId == rId)
                .ToListAsync();

            var res = new List<RLogsResponse>();
            foreach (var l in logs)
            {
                res.Add(l.Adapt<RLogsResponse>());
            }
            return Good<List<RLogsResponse>>(res);
        }
    }
}
