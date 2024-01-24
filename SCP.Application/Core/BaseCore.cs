using Microsoft.Extensions.Logging;
using SCP.Application.Common;

namespace SCP.Application.Core
{
    public class BaseCore
    {
        private readonly ILogger logger;

        public BaseCore(ILogger logger)
        {
            this.logger = logger;
        }

        public CoreResponse<T> Good<T>(T value)
        {
            return new CoreResponse<T>()
            {
                Data = value,
                IsSuccess = true,
            };
        }

        public CoreResponse<T> Bad<T>(string errorMsg)
        {
            logger.LogError(errorMsg);
            return new CoreResponse<T>(errorMsg);
        }

        public CoreResponse<T> Bad<T>(string[] errorMsgs)
        {
            string res = "";
            foreach (var m in errorMsgs)
            {
                res += m + "\n";
            }

            logger.LogError(res);
            return new CoreResponse<T>(errorMsgs);
        }


        public CoreResponse<bool> Bad(string errorMsg)
        {
            logger.LogError(errorMsg);
            return new CoreResponse<bool>(errorMsg);
        }
    }
}