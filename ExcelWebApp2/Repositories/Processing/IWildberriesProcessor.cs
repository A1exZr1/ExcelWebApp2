using ExcelWebApp2.Models;

namespace ExcelWebApp2.Repositories.Processing
{
    public interface IWildberriesProcessor
    {
        List<ProcessedWbResultModel> Process(
            IReadOnlyCollection<AccrualRecordWbModel> accruals,
            IReadOnlyCollection<PrimeCostWbModel> primeCosts);
    }
}
