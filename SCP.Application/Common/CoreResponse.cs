namespace SCP.Application.Common
{
    public class CoreResponse<T>
    {
        public bool IsSuccess { get; init; } = false;
        public T? Data { get; set; }
        public List<string> ErrorList { get; set; }

        public CoreResponse(T data, bool isSuccess = true)
        {
            this.Data = data;
            IsSuccess = isSuccess;
        }

        public CoreResponse(string errorText)
        {
            IsSuccess = false;
            ErrorList = new List<string>() { errorText };
        }

        public CoreResponse(IEnumerable<string> errorTexts)
        {
            IsSuccess = false;
            ErrorList = errorTexts.ToList();
        }

        public CoreResponse()
        {
            IsSuccess = false;
        }
    }
}
