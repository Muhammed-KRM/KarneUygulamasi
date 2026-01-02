using System.Text.Json.Serialization;

namespace KeremProject1backend.Models.Responses
{
    public class BaseResponse
    {
        public object? Response { get; set; } = null;
        public long ReturnValue { get; set; } = 0;
        public int ErrorCode { get; set; } = 0;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool Errored { get; set; } = false;
        [JsonIgnore]
        public int _userid = 0;
        public int SetUserID(int input) => _userid = input;

        public BaseResponse GenerateError(int errorcode, string errorMessage)
        {
            ReturnValue = errorcode;
            ErrorMessage = errorMessage;
            Errored = true;
            return this;
        }

        public BaseResponse GenerateSuccess(long returnvalue, string Message)
        {
            ReturnValue = returnvalue;
            ErrorMessage = Message;
            return this;
        }
        public BaseResponse GenerateSuccess(string Message)
        {
            ErrorMessage = Message;
            return this;
        }
        public BaseResponse GenerateSuccess()
        {
            return this;
        }
    }

    public class SessionInfos
    {
        public long LastAccessTime { get; set; }
        public int Userid { get; set; }
        public DateTime LastAcsessRealTime { get; set; }
    }
}
