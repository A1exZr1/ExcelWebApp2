using ClosedXML.Excel;
using ExcelWebApp2.Models;

namespace ExcelWebApp2.Repositories
{
    public class FileReaderRepository
    {
        public async Task<ReadResult<AccrualRecordModel>> ReadAccruals(Stream stream)
        {
            var result = new ReadResult<AccrualRecordModel>();

            try
            {
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First();

                foreach (var row in worksheet.RowsUsed().Skip(1)) // skip header
                {
                    var record = new AccrualRecordModel
                    {
                        Article = row.Cell(1).GetString(),      // A
                        Revenue = row.Cell(2).GetDouble(),     // B
                        Quantity = row.Cell(3).GetValue<int>()  // C
                    };

                    result.Data.Add(record);
                }

                result.Success = true;
                result.Message = $"Read {result.Data.Count} rows.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
            }

            return result;
        }
    }
}
