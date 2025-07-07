namespace ExcelWebApp2.Repositories
{
    public class FileReaderException : Exception
    {

        public FileReaderException(Exception innerException)
            : base("", innerException)
        { }

        public FileReaderException() : base(null)
        { }
        public FileReaderException(string message) : base(message)
        { }
    }
}
