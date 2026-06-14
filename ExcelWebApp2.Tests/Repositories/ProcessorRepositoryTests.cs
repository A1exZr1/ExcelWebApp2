using ExcelWebApp2.Infrastructure;
using ExcelWebApp2.Models;
using ExcelWebApp2.Repositories;
using ExcelWebApp2.Repositories.Processing;

namespace ExcelWebApp2.Tests.Repositories;

public class ProcessorRepositoryTests
{
    [Fact]
    public void GetMissingInputs_ForOzonV1_ReturnsAllRequiredFiles()
    {
        var repository = CreateRepository();

        var missing = repository.GetMissingInputs(ProcessingType.OzonV1);

        Assert.Contains("Файл себестоимости отсутствует", missing);
        Assert.Contains("Файл отчета по товарам отсутствует", missing);
        Assert.Contains("Файл рекламы отсутствует", missing);
    }

    [Fact]
    public void HasAllInputs_ForWildberries_DoesNotRequireCancellationFile()
    {
        var repository = CreateRepository();

        repository.SetAccrualsWb(
        [
            new AccrualRecordWbModel
            {
                SupplierArticleName = "ART-1",
                Sku = "123"
            }
        ]);
        repository.SetPrimeCostsWb(
        [
            new PrimeCostWbModel
            {
                ArticleName = "ART-1",
                Sku = "123"
            }
        ]);

        Assert.True(repository.HasAllInputs(ProcessingType.Wildberries));
        Assert.DoesNotContain("Файл отмен отсутствует", repository.GetMissingInputs(ProcessingType.Wildberries));
    }

    [Fact]
    public void HasAllInputs_UsesSeparateStateForDifferentUsers()
    {
        var sessionStore = new ProcessingSessionStore();
        var firstUserRepository = CreateRepository("first-user", sessionStore);
        var secondUserRepository = CreateRepository("second-user", sessionStore);

        firstUserRepository.SetAccrualsV2([new AccrualRecordV2Model()]);
        firstUserRepository.SetPrimeCosts([new PrimeCostModel()]);

        Assert.True(firstUserRepository.HasAllInputs(ProcessingType.OzonV2));
        Assert.False(secondUserRepository.HasAllInputs(ProcessingType.OzonV2));
    }

    private static ProcessorRepository CreateRepository(
        string userId = "test-user",
        IProcessingSessionStore? sessionStore = null)
    {
        return new ProcessorRepository(
            new OzonV1Processor(),
            new OzonV2Processor(),
            new WildberriesProcessor(),
            sessionStore ?? new ProcessingSessionStore(),
            new TestCurrentUserContext(userId));
    }

    private class TestCurrentUserContext(string userId) : ICurrentUserContext
    {
        public string UserId { get; } = userId;
    }
}
