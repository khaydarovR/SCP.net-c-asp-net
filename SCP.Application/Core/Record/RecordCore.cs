using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCP.Application.Common;
using SCP.Application.Common.Helpers;
using SCP.Application.Common.Response;
using SCP.Application.Common.Validators;
using SCP.Application.Core.Safe;
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

        public RecordCore(AppDbContext dbContext,
                          AsymmetricCryptoService asymmetricCryptoService,
                          SymmetricCryptoService symmetricCrypto,
                          SafeCore safeCore)
        {
            this.dbContext = dbContext;
            this.asymmetricCryptoService = asymmetricCryptoService;
            this.symmetricCrypto = symmetricCrypto;
            this.safeCore = safeCore;
        }

        public async Task<CoreResponse<bool>> CreateRecord(CreateRecordCommand command)
        {
            var prk = safeCore.GetClearPrivateKeyFromSafe(command.SafeId);

            //var signatureIsValid = RSAUtils.VerifySignature(command.ClientPrivK, rawData, command.Signature);
            //if (signatureIsValid == false )
            //{
            //    return Bad<bool>("Цифровая подпись не прошла проверку");
            //}

            //TODO: SafeGuard
            var neadPer = SystemSafePermisons.AddRecordToSafe;

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

            dbContext.SaveChanges();

            return Good(true);
        }


        public async Task<CoreResponse<List<GetRecordResponse>>> GetAllRecord(string safeId, Guid userId)
        {
            //TODO: SafeGuard 
            //защита до сейфа
            var needPer = SystemSafePermisons.GetRecordList;


            var records = await dbContext.Records
                .Where(r => r.SafeId == Guid.Parse(safeId))
                .Where(r => r.IsDeleted == false)
                .ProjectToType<GetRecordResponse>()
                .ToListAsync();


            foreach (var record in records)
            {
                var rightInCurrentRec = await dbContext.RecordRights
                    .FirstAsync(rr => rr.AppUserId == userId && rr.RecordId == record.Id);

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


            //TODO SafeGuard
            if (IsHaveRight(Guid.Parse(command.UserId), dbRec.Id, RecRightEnum.Edit))
            {
                return Bad<bool>("Не достаточно прав для редактирования");

            }
            if (command.IsDeleted)
            {
                if (IsHaveRight(Guid.Parse(command.UserId), dbRec.Id, RecRightEnum.Delete))
                {
                    return Bad<bool>("Не достаточно прав для удаления");
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

        private bool IsHaveRight(Guid usesId, Guid recId, RecRightEnum recRight)
        {
            var res = dbContext.RecordRights
                .Where(rr => rr.AppUserId == usesId)
                .Where(rr => rr.RecordId == recId)
                .Any(rr => rr.EnumPermission == recRight);

            return res;
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

            //TODO: SafeGuard
            //var needRight = rec.UserRight.EnumPermission > RecRightEnum.Read;

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
