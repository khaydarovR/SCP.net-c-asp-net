using Mapster;
using Microsoft.EntityFrameworkCore;
using SCP.Application.Common;
using SCP.Application.Common.Helpers;
using SCP.Application.Common.Response;
using SCP.Application.Core.Safe;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Enum;
using System.Security.Cryptography;
using System.Text;

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
            //защита до сейфа
            var records = await dbContext.Records
                .Where(r => r.SafeId == Guid.Parse(safeId))
                .Where(r => r.IsDeleted == false)
                .ProjectToType<GetRecordResponse>()
                .ToListAsync();

            //защита до записи
            foreach (var record in records)
            {
                var rightInCurrentRec = await dbContext.RecordRights
                    .FirstAsync(rr => rr.AppUserId == userId && rr.RecordId == record.Id);

                record.RightToCurentUser = rightInCurrentRec.MapRightEnumToString();
            }

            return Good(records);
        }


        /// <summary>
        /// Отправляет данные зашифрованные с помощью полученного ключа от клиента
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CoreResponse<ReadRecordResponse>> ReadRecord(ReadRecordCommand command)
        {
            var rec = await dbContext.Records
                .Include(r => r.Safe)
                .FirstAsync(r => r.Id == command.RecordId);

            var clearSafePrivateKey = safeCore.GetClearPrivateKeyFromSafe(rec.SafeId.ToString());

            var clearLogin = asymmetricCryptoService.DecryptFromClientData(rec.ESecret, clearSafePrivateKey);
            var clearPw = asymmetricCryptoService.DecryptFromClientData(rec.EPw, clearSafePrivateKey);
            var clearSecret = asymmetricCryptoService.DecryptFromClientData(rec.ESecret, clearSafePrivateKey);

            var eLoging = asymmetricCryptoService.EncryptDataForClient(clearLogin, command.PubKeyFromClient);
            var ePw = asymmetricCryptoService.EncryptDataForClient(clearPw, command.PubKeyFromClient);
            var eSecret = asymmetricCryptoService.EncryptDataForClient(clearSecret, command.PubKeyFromClient);


            var data = new ReadRecordResponse
            {
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
