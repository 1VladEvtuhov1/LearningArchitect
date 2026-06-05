namespace LearningArchitect.Modules.InterviewArena.Net
{
    public readonly struct ApiResult<T>
    {
        public bool Succeeded { get; }
        public T Value { get; }
        public string Error { get; }
        public string ErrorCode { get; }
        public long StatusCode { get; }

        private ApiResult(bool succeeded, T value, string error, string errorCode, long statusCode)
        {
            Succeeded = succeeded;
            Value = value;
            Error = error;
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public static ApiResult<T> Success(T value, long statusCode = 200) =>
            new ApiResult<T>(true, value, null, null, statusCode);

        public static ApiResult<T> Failure(string error, string errorCode = null, long statusCode = 0) =>
            new ApiResult<T>(false, default, error, errorCode, statusCode);
    }
}
