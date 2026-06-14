using ExcelWebApp2.Models;

namespace ExcelWebApp2.Repositories
{
    public class ProcessingSessionState
    {
        public List<AccrualRecordV1Model> AccrualsV1 { get; set; } = [];
        public List<AccrualRecordV2Model> AccrualsV2 { get; set; } = [];
        public List<AccrualRecordWbModel> AccrualsWb { get; set; } = [];
        public List<WbCancellationModel> WbCancellations { get; set; } = [];
        public List<AdvertisingModel> Ads { get; set; } = [];
        public List<PrimeCostModel> PrimeCosts { get; set; } = [];
        public List<PrimeCostWbModel> PrimeCostWbModels { get; set; } = [];
    }
}
