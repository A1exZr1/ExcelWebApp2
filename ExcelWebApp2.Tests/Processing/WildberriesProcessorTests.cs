using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories.Processing;

namespace ExcelWebApp2.Tests.Processing;

public class WildberriesProcessorTests
{
    [Fact]
    public void Process_ReturnsProductAndAdditionalRows()
    {
        var processor = new WildberriesProcessor();

        var accruals = new List<AccrualRecordWbModel>
        {
            new()
            {
                ArticleName = "Предмет",
                SupplierArticleName = "ART-1",
                Sku = "123",
                DocumentType = "Продажа",
                PaymentReason = "Продажа",
                RetailPrice = "1000",
                Quantity = "2",
                AmountPayableToSeller = "700"
            },
            new()
            {
                SupplierArticleName = "ART-1",
                Sku = "123",
                PaymentReason = "Логистика",
                Logistics = "50"
            },
            new()
            {
                ArticleName = "Предмет",
                SupplierArticleName = "ART-1",
                Sku = "123",
                DocumentType = "Возврат",
                PaymentReason = "Возврат",
                AmountPayableToSeller = "100"
            },
            new()
            {
                PaymentReason = "Хранение",
                StorageFee = "30"
            },
            new()
            {
                PaymentReason = "Удержание",
                Withholdings = "20"
            }
        };

        var primeCosts = new List<PrimeCostWbModel>
        {
            new()
            {
                ArticleName = "ART-1",
                Sku = "123",
                Brand = "OLSON",
                WorkCost = "50",
                MaterialCost = "100"
            }
        };

        var cancellations = new List<WbCancellationModel>
        {
            new()
            {
                SupplierArticleName = "ART-1",
                Sku = "123",
                Status = "Отсортировано"
            },
            new()
            {
                SupplierArticleName = "ART-1",
                Sku = "123",
                Status = "Отсортировано"
            },
            new()
            {
                SupplierArticleName = "ART-1",
                Sku = "123",
                Status = "Создано"
            }
        };

        var result = processor.Process(accruals, primeCosts, cancellations, 15);

        Assert.Equal(3, result.Count);

        var product = result.Single(x => x.SupplierArticleName == "ART-1");
        Assert.Equal("Предмет", product.ArticleName);
        Assert.Equal("123", product.Sku);
        Assert.Equal("OLSON", product.Brand);
        Assert.Equal(2m, product.Quantity);
        Assert.Equal(1000m, product.RetailPriceSumm);
        Assert.Equal(700m, product.AmountPayableToSellerSumm);
        Assert.Equal(50m, product.LogisticsFee);
        Assert.Equal(100m, product.ReturnedSumm);
        Assert.Equal(1, product.ReturnedQuantity);
        Assert.Equal(100m, product.WorkCost);
        Assert.Equal(200m, product.MaterialCost);
        Assert.Equal(100m, product.CancellationWorkCost);
        Assert.Equal(185m, product.NetProfit);
        Assert.Equal(26.43m, product.ProfitPercent);

        var storage = result.Single(x => x.SupplierArticleName == "～ ХРАНЕНИЕ");
        Assert.Equal(-30m, storage.NetProfit);

        var withholdings = result.Single(x => x.SupplierArticleName == "～ УДЕРЖАНИЯ");
        Assert.Equal(-20m, withholdings.NetProfit);
    }
}
