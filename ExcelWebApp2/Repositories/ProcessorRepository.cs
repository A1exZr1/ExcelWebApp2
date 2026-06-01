using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories.Processing;

namespace ExcelWebApp2.Repositories
{
    public class ProcessorRepository(
        IOzonV1Processor ozonV1Processor,
        IOzonV2Processor ozonV2Processor,
        IWildberriesProcessor wildberriesProcessor)
    {
        private List<AccrualRecordV1Model> _accrualsV1 = [];
        private List<AccrualRecordV2Model> _accrualsV2 = [];
        private List<AccrualRecordWbModel> _accrualsWb = [];
        private List<AdvertisingModel> _ads = [];
        private List<PrimeCostModel> _primeCosts = [];
        private List<PrimeCostWbModel> _primeCostWbModels = [];
        private List<ProcessedOzonResultV1Model> _processedResultsV1 = [];
        private List<ProcessedOzonResultV2Model> _processedResultsV2 = [];
        private List<ProcessedWbResultModel> _processedResultsWb = [];

        public void SetAccrualsV1(List<AccrualRecordV1Model> accruals) => _accrualsV1 = accruals;
        public void SetAccrualsV2(List<AccrualRecordV2Model> accruals) => _accrualsV2 = accruals;
        public void SetAccrualsWb(List<AccrualRecordWbModel> accruals) => _accrualsWb = accruals;
        public void SetAds(List<AdvertisingModel> ads) => _ads = ads;
        public void SetPrimeCosts(List<PrimeCostModel> costs) => _primeCosts = costs;
        public void SetPrimeCostsWb(List<PrimeCostWbModel> costs) => _primeCostWbModels = costs;

        public bool HasAllInputs(ProcessingType processingType)
        {
            return processingType switch
            {
                ProcessingType.OzonV1 => _accrualsV1.Count != 0 && _ads.Count != 0 && _primeCosts.Count != 0,
                ProcessingType.OzonV2 => _accrualsV2.Count != 0 && _primeCosts.Count != 0,
                ProcessingType.Wildberries => _accrualsWb.Count != 0 && _primeCostWbModels.Count != 0,
                _ => throw new NotImplementedException(),
            };
        }

        public void Clear()
        {
            _accrualsV1.Clear();
            _accrualsV2.Clear();
            _ads.Clear();
            _primeCosts.Clear();
            _processedResultsV1.Clear();
            _processedResultsV2.Clear();
            _accrualsWb.Clear();
            _primeCostWbModels.Clear();
        }

        public string GetMissingInputs(ProcessingType processingType)
        {
            var missing = new List<string>();

            switch (processingType)
            {
                case ProcessingType.OzonV1:
                    if (_primeCosts.Count == 0) missing.Add("Файл себестоимости отсутствует");
                    if (_accrualsV1.Count == 0) missing.Add("Файл отчёта по товарам отсутствует");
                    if (_ads.Count == 0) missing.Add("Файл рекламы отсутствует");
                    break;
                case ProcessingType.OzonV2:
                    if (_primeCosts.Count == 0) missing.Add("Файл себестоимости отсутствует");
                    if (_accrualsV2.Count == 0) missing.Add("Файл отчёта по товарам отсутствует");
                    break;
                case ProcessingType.Wildberries:
                    if (_accrualsWb.Count == 0) missing.Add("Файлы отчётов по товарам отсутствует");
                    if (_primeCostWbModels.Count == 0) missing.Add("Файл себестоимости отсутствует");
                    break;
                default:
                    throw new NotImplementedException();
            }

            return string.Join('\n', missing);
        }

        public List<ProcessedOzonResultV1Model> ProcessOzonV1()
        {
            _processedResultsV1 = ozonV1Processor.Process(_accrualsV1, _ads, _primeCosts);
            return _processedResultsV1;
        }

        public List<ProcessedOzonResultV2Model> ProcessOzonV2()
        {
            _processedResultsV2 = ozonV2Processor.Process(_accrualsV2, _primeCosts);
            return _processedResultsV2;
        }

        public List<ProcessedWbResultModel> ProcessWb()
        {
            _processedResultsWb = wildberriesProcessor.Process(_accrualsWb, _primeCostWbModels);
            return _processedResultsWb;
        }
    }
}
