namespace ZaloOAWebhook.Class.Response
{

    public class Response<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public ErrorDetails? Error { get; set; }
        public int StatusCode { get; set; }

        public Response()
        {
            Success = true; // Mặc định là thành công
        }

        public Response(string message)
        {
            Success = false;
            Message = message;
        }

        public Response(int statusCode, bool success, string message, int errorCode, string errorName,
                        string errorReason = "", string refDoc = "", string errorDescription = "")
        {
            Success = success;
            Message = message;
            StatusCode = statusCode;

            var error = new ErrorDetails
            {
                ErrorCode = errorCode,
                ErrorName = errorName,
                ErrorDescription = errorDescription,
                ErrorReason = errorReason,
                RefDoc = refDoc
            };

            Error = error;
        }

        public void set_StatusCode_Success_Messeage(int statusCode, bool success, string message)
        {
            Success = success;
            Message = message;
            StatusCode = statusCode;

        }
        public void set_DetailError(int errorCode, string errorName, string errorReason = "", string refDoc = "", string errorDescription = "")
        {
            var error = new ErrorDetails
            {
                ErrorCode = errorCode,
                ErrorName = errorName,
                ErrorDescription = errorDescription,
                ErrorReason = errorReason,
                RefDoc = refDoc
            };

            Error = error;
        }


    }

    public class ErrorDetails
    {
        public string ErrorName { get; set; } = string.Empty;
        public string ErrorReason { get; set; } = string.Empty;
        public string RefDoc { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public int ErrorCode { get; set; }
    }
}
