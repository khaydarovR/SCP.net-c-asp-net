using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCP.Application.Common;
using SCP.Application.Common.Helpers;
using SCP.Application.Common.Response;
using SCP.Application.Common.Validators;
using SCP.Application.Core.Safe;
using SCP.Application.Core.SafeGuard;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Enum;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SCP.Application.Core.Record
{
    public partial class RecordCore : BaseCore
    {
        private readonly AppDbContext dbContext;
        private readonly AsymmetricCryptoService asymmetricCryptoService;
        private readonly SymmetricCryptoService symmetricCrypto;
        private readonly SafeCore safeCore;
        private readonly SafeGuardCore safeGuard;

        public RecordCore(AppDbContext dbContext,
                          AsymmetricCryptoService asymmetricCryptoService,
                          SymmetricCryptoService symmetricCrypto,
                          SafeCore safeCore,
                          SafeGuardCore safeGuard)
        {
            this.dbContext = dbContext;
            this.asymmetricCryptoService = asymmetricCryptoService;
            this.symmetricCrypto = symmetricCrypto;
            this.safeCore = safeCore;
            this.safeGuard = safeGuard;
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
            foreach ( var user in safeUsers.Data! )
            {
                dbContext.RecordRights.Add(new Domain.Entity.RecordRight
                {
                    AppUserId = user.Id,
                    RecordId = recordId,
                    EnumPermission = RecRightEnum.Delete
                });
            }


            dbContext.SaveChanges();

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

            return Good(true);
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

            return new CoreResponse<ReadRecordResponse>(data);

        }
    }
}
