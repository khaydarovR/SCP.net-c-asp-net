using Microsoft.EntityFrameworkCore;
using SCP.Application.Common;
using SCP.Application.Common.Helpers;
using SCP.Application.Common.Response;
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
        public RecordCore(AppDbContext dbContext,
                          AsymmetricCryptoService asymmetricCryptoService)
        {
            this.dbContext = dbContext;
            this.asymmetricCryptoService = asymmetricCryptoService;
        }

        public async Task<CoreResponse<bool>> CreateRecord(CreateRecordCommand command)
        {
            var rawData = command.Login + command.Pw + command.Secret;

            var signatureIsValid = RSAUtils.VerifySignature(command.ClientPubK, rawData, command.Signature);
            if (signatureIsValid == false )
            {
                return Bad<bool>("Цифровая подпись не прошла проверку");
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

                // Decrypt the secret using the user's private key
                string dSecret = asymmetricCryptoService.Decrypt(rec.ESecret, rec.Safe.EPrivateK);

                // Encrypt this plain text using client's public key
                string eSecret = asymmetricCryptoService.Encrypt(dSecret, command.PubKeyFromClient);

                var data = new ReadRecordResponse { ESecret = eSecret };
                return new CoreResponse<ReadRecordResponse>(data);
            }
        }
}
