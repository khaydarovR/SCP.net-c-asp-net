using SCP.Application.Common;

namespace SCP.Application.Core
{
    public class BaseCore
    {
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
            return new CoreResponse<T>(errorMsg);
        }
    }
}