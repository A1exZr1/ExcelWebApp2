using ExcelWebApp2.Models;

namespace ExcelWebApp2.Repositories.Processing
{
    public interface IOzonV1Processor
    {
        List<ProcessedOzonResultV1Model> Process(
            IReadOnlyCollection<AccrualRecordV1Model> accruals,
            IReadOnlyCollection<AdvertisingModel> ads,
            IReadOnlyCollection<PrimeCostModel> primeCosts);
    }
}
