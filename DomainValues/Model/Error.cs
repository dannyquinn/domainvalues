namespace DomainValues.Model
{
    internal class Error
    {
        public Error(string message) : this(message, false)
        {
        }
        public Error(string message,bool outputWindowOnly)
        {
            Message = message;
            OutputWindowOnly = outputWindowOnly;
        }
        public string Message { get;  }
        public bool OutputWindowOnly { get; }
    }
}
