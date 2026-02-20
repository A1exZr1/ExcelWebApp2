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

        public static string GetFieldByIndex(this IXLRangeRow row, int? index, string? fieldName = null)
        {
            if (index is null)
                throw new FileReaderException("Can't get field for null index");
            var cell = row.Cell((int)index);
            return GetCellStringValue(cell, fieldName);
        }

        public static string? FindFieldByIndex(this IXLRangeRow row, int? index, string? fieldName = null)
        {
            if (index is null)
                return null;

            var cell = row.Cell((int)index);
            return GetCellStringValue(cell, fieldName);
        }

        private static string GetCellStringValue(IXLCell cell, string? fieldName)
        {
            try
            {
                return cell.GetValue<string>();
            }
            catch (Exception ex)
            {
                var address = cell.Address.ToString();
                var rowNumber = cell.Address.RowNumber;
                var columnNumber = cell.Address.ColumnNumber;
                var worksheetName = cell.Worksheet.Name;
                var fieldPart = string.IsNullOrWhiteSpace(fieldName) ? string.Empty : $" поля '{fieldName}'";

                throw new FileReaderException(
                    $"Ошибка чтения{fieldPart} в ячейке '{worksheetName}'!{address} (row: {rowNumber}, column: {columnNumber}).",
                    ex);
            }
        }
    }
}
