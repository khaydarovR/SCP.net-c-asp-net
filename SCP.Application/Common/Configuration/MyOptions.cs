namespace SCP.Application.Common.Configuration
{
    public class MyOptions
    {
        public string JWT_KEY { get; set; } = string.Empty;
        public string JWT_ISSUER { get; set; } = string.Empty;
        public string CRT_KEY { get; set; } = string.Empty;
    }
}