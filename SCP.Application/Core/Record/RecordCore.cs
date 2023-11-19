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
        private readonly SymmetricCryptoService symmetricCrypto;

        public RecordCore(AppDbContext dbContext,
                          AsymmetricCryptoService asymmetricCryptoService,
                          SymmetricCryptoService symmetricCrypto)
        {
            this.dbContext = dbContext;
            this.asymmetricCryptoService = asymmetricCryptoService;
            this.symmetricCrypto = symmetricCrypto;
        }

        public async Task<CoreResponse<bool>> CreateRecord(CreateRecordCommand command)
        {
            var rawData = command.Secret;
            var prk = GetPrivateKeyFromSafe(command.SafeId);
            var clearData = asymmetricCryptoService.DecryptData(rawData, prk);

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

        private string GetPrivateKeyFromSafe(string safeId)
        {
            var ePrivateKey = dbContext.Safes
                .Where(s => s.Id == Guid.Parse(safeId))
                .Select(s => s.EPrivateKpem)
                .FirstOrDefault();

            var privateKey = this.symmetricCrypto.DecryptWithSecretKey(ePrivateKey);

            return privateKey;
            
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
                //string dSecret = asymmetricCryptoService.DecryptWithRSA(rec.Safe.EPrivateKpem, rec.ESecret);

                //// Encrypt this plain text using client's public key
                //string eSecret = asymmetricCryptoService.DecryptWithRSA(command.PubKeyFromClient, dSecret);

                var data = new ReadRecordResponse { ESecret = "emty" };
                return new CoreResponse<ReadRecordResponse>(data);
            }
        }
}
