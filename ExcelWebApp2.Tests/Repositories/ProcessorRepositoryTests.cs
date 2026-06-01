using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using ExcelWebApp2.Repositories.Processing;

namespace ExcelWebApp2.Tests.Repositories;

public class ProcessorRepositoryTests
{
    [Fact]
    public void GetMissingInputs_ForOzonV1_ReturnsAllRequiredFiles()
    {
        var repository = new ProcessorRepository(
            new OzonV1Processor(),
            new OzonV2Processor(),
            new WildberriesProcessor());

        var missing = repository.GetMissingInputs(ProcessingType.OzonV1);

        Assert.Contains("Файл себестоимости отсутствует", missing);
        Assert.Contains("Файл отчёта по товарам отсутствует", missing);
        Assert.Contains("Файл рекламы отсутствует", missing);
    }
}
