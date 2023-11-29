using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SCP.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Services
{
    public class TwoFactorAuthService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly EmailService emailService;

        public TwoFactorAuthService(UserManager<AppUser> userManager, EmailService emailService)
        {
            this.userManager = userManager;
            this.emailService = emailService;
        }

        public async Task<string> GenerateTwoFactorCodeAsync(AppUser user)
        {
            var token = await userManager.GenerateTwoFactorTokenAsync(user, "email");
            // You can customize the token format or expiration if needed
            return token;
        }

        public async Task SendTwoFactorCodeByEmailAsync(AppUser user)
        {
            var code = await GenerateTwoFactorCodeAsync(user);

            var subject = "Two-Factor Authentication Code";

            // Create an HTML message with a nice layout
            var htmlMessage = $@"
        <html>
            <body>
                <h1>{subject}</h1>
                <p>Your two-factor authentication code is: <strong>{code}</strong></p>
                <p>This code will expire in a short time. Please use it promptly.</p>
            </body>
        </html>";


            // Send the email with the code
            await emailService.SendEmailAsync(user.Email, subject, htmlMessage);
        }

        public async Task<bool> VerifyTwoFactorCodeAsync(AppUser user, string twoFactorCode)
        {
            var isTwoFactorTokenValid = await userManager.VerifyTwoFactorTokenAsync(user, "email", twoFactorCode);
            return isTwoFactorTokenValid;
        }
    }

}
