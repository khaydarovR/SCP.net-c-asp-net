using Microsoft.EntityFrameworkCore;
using SCP.Api.Responses;
using SCP.Application.Common;
using SCP.Application.Services;
using SCP.DAL;

namespace SCP.Application.Core.Record
{
    public class RecordCore
    {
        private readonly AppDbContext _dbContext;
        private readonly AsymmetricCryptoService _asymmetricCryptoService;
        public RecordCore(AppDbContext dbContext,
                          AsymmetricCryptoService asymmetricCryptoService)
        {
            _dbContext = dbContext;
            _asymmetricCryptoService = asymmetricCryptoService;
        }


        /// <summary>
        /// Отправляет данные зашифрованные с помощью полученного ключа от клиента
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CoreResponse<ReadRecordResonse>> ReadRecord(ReadRecordCommand command)
        {
            var rec = await _dbContext.Records
                .Include(r => r.Safe)
                .FirstAsync(r => r.Id == command.RecordId);

            // Decrypt the secret using the user's private key
            string dSecret = _asymmetricCryptoService.Decrypt(rec.ESecret, rec.Safe.PrivateK);

            // Encrypt this plain text using client's public key
            string eSecret = _asymmetricCryptoService.Encrypt(dSecret, command.PubKeyFromClient);

            var data = new ReadRecordResonse { ESecret = eSecret };
            return new CoreResponse<ReadRecordResonse>(data);
        }
    }
}
