using ExcelWebApp2.Infrastructure;
using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories.Processing;

namespace ExcelWebApp2.Repositories
{
    public class ProcessorRepository(
        IOzonV1Processor ozonV1Processor,
        IOzonV2Processor ozonV2Processor,
        IWildberriesProcessor wildberriesProcessor,
        IProcessingSessionStore sessionStore,
        ICurrentUserContext currentUserContext)
    {
        private ProcessingSessionState State => sessionStore.Get(currentUserContext.UserId);

        public void SetAccrualsV1(List<AccrualRecordV1Model> accruals) => State.AccrualsV1 = accruals;
        public void SetAccrualsV2(List<AccrualRecordV2Model> accruals) => State.AccrualsV2 = accruals;
        public void SetAccrualsWb(List<AccrualRecordWbModel> accruals) => State.AccrualsWb = accruals;
        public void SetWbCancellations(List<WbCancellationModel> cancellations) => State.WbCancellations = cancellations;
        public void SetAds(List<AdvertisingModel> ads) => State.Ads = ads;
        public void SetPrimeCosts(List<PrimeCostModel> costs) => State.PrimeCosts = costs;
        public void SetPrimeCostsWb(List<PrimeCostWbModel> costs) => State.PrimeCostWbModels = costs;

        public bool HasAllInputs(ProcessingType processingType)
        {
            return processingType switch
            {
                ProcessingType.OzonV1 => State.AccrualsV1.Count != 0 && State.Ads.Count != 0 && State.PrimeCosts.Count != 0,
                ProcessingType.OzonV2 => State.AccrualsV2.Count != 0 && State.PrimeCosts.Count != 0,
                ProcessingType.Wildberries => State.AccrualsWb.Count != 0 && State.PrimeCostWbModels.Count != 0,
                _ => throw new NotImplementedException(),
            };
        }

        public void Clear()
        {
            sessionStore.Clear(currentUserContext.UserId);
        }

        public string GetMissingInputs(ProcessingType processingType)
        {
            var missing = new List<string>();

            switch (processingType)
            {
                case ProcessingType.OzonV1:
                    if (State.PrimeCosts.Count == 0) missing.Add("Файл себестоимости отсутствует");
                    if (State.AccrualsV1.Count == 0) missing.Add("Файл отчета по товарам отсутствует");
                    if (State.Ads.Count == 0) missing.Add("Файл рекламы отсутствует");
                    break;
                case ProcessingType.OzonV2:
                    if (State.PrimeCosts.Count == 0) missing.Add("Файл себестоимости отсутствует");
                    if (State.AccrualsV2.Count == 0) missing.Add("Файл отчета по товарам отсутствует");
                    break;
                case ProcessingType.Wildberries:
                    if (State.AccrualsWb.Count == 0) missing.Add("Файлы отчетов по товарам отсутствуют");
                    if (State.PrimeCostWbModels.Count == 0) missing.Add("Файл себестоимости отсутствует");
                    break;
                default:
                    throw new NotImplementedException();
            }

            return string.Join('\n', missing);
        }

        public List<ProcessedOzonResultV1Model> ProcessOzonV1()
        {
            return ozonV1Processor.Process(State.AccrualsV1, State.Ads, State.PrimeCosts);
        }

        public List<ProcessedOzonResultV2Model> ProcessOzonV2()
        {
            return ozonV2Processor.Process(State.AccrualsV2, State.PrimeCosts);
        }

        public List<ProcessedWbResultModel> ProcessWb(decimal returnMaterialDamagePercent)
        {
            return wildberriesProcessor.Process(
                State.AccrualsWb,
                State.PrimeCostWbModels,
                State.WbCancellations,
                returnMaterialDamagePercent);
        }
    }
}
