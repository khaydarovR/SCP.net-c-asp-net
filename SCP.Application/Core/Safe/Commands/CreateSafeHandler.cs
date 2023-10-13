using MediatR;
using SCP.Application.Services;
using SCP.DAL;
using SCP.DAL.Migrations;
using SCP.Domain.Entity;
using System.Security.Cryptography.Xml;

namespace SCP.Application.Core.Safens.Commands
{
    /// <summary>
    /// Создание сейфа
    /// </summary>
    public class CreateSafeHandler: IRequestHandler<CreateSafeCommand, Unit>
    {
        private readonly AppDbContext appDbContext;
        private readonly CryptorService cryptorService;

        public CreateSafeHandler(AppDbContext appDbContext, CryptorService cryptorService)
        {
            this.appDbContext = appDbContext;
            this.cryptorService = cryptorService;
        }

        public async Task<Unit> Handle(CreateSafeCommand request, CancellationToken cancellationToken)
        {
            string? encryptedKeyForSafe = string.Empty;
            if (request.ClearKey != null)
            {
                encryptedKeyForSafe = cryptorService.EncryptWithSecretKey(request.ClearKey);
            }

            var model = new Safe
            {
                Title = request.Title,
                Description = request.Description ?? "",
                EKey = encryptedKeyForSafe,
                BotApiKey = string.Empty,
            };

            await appDbContext.Safes.AddAsync(model, cancellationToken);
            await appDbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
