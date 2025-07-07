namespace ExcelWebApp2.Repositories
{
    public abstract class FileReaderBase
    {
        protected static bool IsHeadersCorrect(IEnumerable<string> fileHeadlineNames, string settingsHeadlineNames, string delimiter = ";")
        {
            var fileHeaders = string.Join(delimiter, fileHeadlineNames);
            return settingsHeadlineNames == fileHeaders;
        }
    }
}
