using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories.Processing;

namespace ExcelWebApp2.Tests.Processing;

public class OzonV2ProcessorTests
{
    [Fact]
    public void Process_ReturnsCalculatedProductResult()
    {
        var processor = new OzonV2Processor();

        var accruals = new List<AccrualRecordV2Model>
        {
            new()
            {
                ArticleName = "ART-1",
                Sku = "SKU-1",
                AccrualType = "Доставка покупателю",
                Warehouse = "WH-1",
                Quantity = "2",
                PreCommissionAmount = "1000",
                OzonFee = "-50",
                HandlingFee = "-10",
                LastMileFee = "-20",
                LogisticsFee = "-30",
                TotalAmount = "890"
            },
            new()
            {
                ArticleName = "ART-1",
                Sku = "SKU-1",
                AccrualType = "Услуга размещения",
                TotalAmount = "-40"
            },
            new()
            {
                AccrualType = "Не связанный расход",
                TotalAmount = "-100"
            }
        };

        var primeCosts = new List<PrimeCostModel>
        {
            new()
            {
                ArticleName = "ART-1",
                WorkCost = "50",
                MaterialCost = "100"
            }
        };

        var result = processor.Process(accruals, primeCosts);

        var product = Assert.Single(result);
        Assert.Equal("ART-1", product.ArticleName);
        Assert.Equal("SKU-1", product.Sku);
        Assert.Equal("WH-1", product.Warehouse);
        Assert.Equal(2m, product.Quantity);
        Assert.Equal(1000m, product.PreCommissionAmount);
        Assert.Equal(100m, product.WorkCost);
        Assert.Equal(200m, product.MaterialCost);
        Assert.Equal(-100m, product.UnlinkedExpenses);
        Assert.Equal(-40m, product.AdditionalFees["Услуга размещения"]);
        Assert.Equal(450m, product.NetProfit);
        Assert.Equal(45m, product.ProfitPercent);
    }
}
