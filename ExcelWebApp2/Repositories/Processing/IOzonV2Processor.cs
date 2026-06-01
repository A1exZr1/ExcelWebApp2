using ExcelWebApp2.Models;

namespace ExcelWebApp2.Repositories.Processing
{
    public interface IOzonV2Processor
    {
        List<ProcessedOzonResultV2Model> Process(
            IReadOnlyCollection<AccrualRecordV2Model> accruals,
            IReadOnlyCollection<PrimeCostModel> primeCosts);
    }
}
