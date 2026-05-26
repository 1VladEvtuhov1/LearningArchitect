namespace LearningArchitect.Modules.InterviewArena
{
    public readonly struct Result
    {
        public bool Succeeded { get; }
        public string Error { get; }

        private Result(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Error = error;
        }

        public static Result Success() => new Result(true, null);

        public static Result Failure(string error) => new Result(false, error);
    }
}
