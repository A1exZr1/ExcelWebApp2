using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories.Processing;

namespace ExcelWebApp2.Tests.Processing;

public class WildberriesProcessorTests
{
    [Fact]
    public void Process_ReturnsProductCancellationOnlyProductAndAdditionalRows()
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
            },
            new()
            {
                ArticleName = "ART-ONLY-CANCEL",
                Sku = "999",
                Brand = "OTHER",
                WorkCost = "40",
                MaterialCost = "80"
            }
        };

        var cancellations = new List<WbCancellationModel>
        {
            new()
            {
                SupplierArticleName = "ART-1",
                Sku = "123",
                Status = "Отказ покупателем"
            },
            new()
            {
                SupplierArticleName = "ART-1",
                Sku = "123",
                Status = "Отказ покупателем"
            },
            new()
            {
                SupplierArticleName = "ART-1",
                Sku = "123",
                Status = "Создано"
            },
            new()
            {
                SupplierArticleName = "ART-ONLY-CANCEL",
                Sku = "999",
                Status = "Отказ покупателем"
            },
            new()
            {
                SupplierArticleName = "ART-NO-PRIME",
                Sku = "777",
                Status = "Отказ покупателем"
            }
        };

        var result = processor.Process(accruals, primeCosts, cancellations, 15);

        Assert.Equal(5, result.Count);

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
        Assert.Equal(50m, product.ReturnWorkCost);
        Assert.Equal(15m, product.ReturnMaterialDamageCost);
        Assert.Equal(2, product.CancellationWorkQuantity);
        Assert.Equal(100m, product.CancellationWorkCost);
        Assert.Equal(30m, product.CancellationMaterialDamageCost);
        Assert.Equal(155m, product.NetProfit);
        Assert.Equal(22.14m, product.ProfitPercent);

        var cancellationOnly = result.Single(x => x.SupplierArticleName == "ART-ONLY-CANCEL");
        Assert.Equal("999", cancellationOnly.Sku);
        Assert.Equal("OTHER", cancellationOnly.Brand);
        Assert.Equal(0m, cancellationOnly.Quantity);
        Assert.Equal(1, cancellationOnly.CancellationWorkQuantity);
        Assert.Equal(40m, cancellationOnly.CancellationWorkCost);
        Assert.Equal(12m, cancellationOnly.CancellationMaterialDamageCost);
        Assert.Equal(0m, cancellationOnly.WorkCost);
        Assert.Equal(0m, cancellationOnly.MaterialCost);
        Assert.Equal(0m, cancellationOnly.ReturnWorkCost);
        Assert.Equal(-52m, cancellationOnly.NetProfit);
        Assert.Null(cancellationOnly.ProfitPercent);

        var missingPrimeCost = result.Single(x => x.SupplierArticleName == "ART-NO-PRIME");
        Assert.Equal("777", missingPrimeCost.Sku);
        Assert.Equal(0m, missingPrimeCost.Quantity);
        Assert.Equal(1, missingPrimeCost.CancellationWorkQuantity);
        Assert.Equal(0m, missingPrimeCost.CancellationWorkCost);
        Assert.Equal(0m, missingPrimeCost.CancellationMaterialDamageCost);
        Assert.Null(missingPrimeCost.WorkCost);
        Assert.Null(missingPrimeCost.MaterialCost);
        Assert.Equal(0m, missingPrimeCost.ReturnWorkCost);
        Assert.Equal(0m, missingPrimeCost.NetProfit);
        Assert.Null(missingPrimeCost.ProfitPercent);

        var storage = result.Single(x => x.SupplierArticleName == "～ ХРАНЕНИЕ");
        Assert.Equal(-30m, storage.NetProfit);

        var withholdings = result.Single(x => x.SupplierArticleName == "～ УДЕРЖАНИЯ");
        Assert.Equal(-20m, withholdings.NetProfit);
    }
}
