using ClosedXML.Excel;

namespace ExcelWebApp2.Repositories.Extensions
{
    public static class IXLRangeRowExtensions
    {
        public static int GetFieldIndex(this IXLRangeRow row, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new FileReaderException("Can't get field index for empty field");
            return row.Search(fieldName).FirstOrDefault()?.Address.ColumnNumber ??
                   throw new FileReaderException($@"Can't find the field {fieldName}");
        }

        public static int? FindFieldIndex(this IXLRangeRow row, string fieldName)
        {
            return !string.IsNullOrEmpty(fieldName) ? row.Search(fieldName).FirstOrDefault()?.Address.ColumnNumber : null;
        }

        public static string GetFieldByIndex(this IXLRangeRow row, int? index)
        {
            if (index is null)
                throw new FileReaderException("Can't get field for null index");
            return row.Cell((int)index).GetValue<string>();
        }

        public static string FindFieldByIndex(this IXLRangeRow row, int? index)
        {
            return index is null ? null : row.Cell((int)index).GetValue<string>();
        }
    }
}
