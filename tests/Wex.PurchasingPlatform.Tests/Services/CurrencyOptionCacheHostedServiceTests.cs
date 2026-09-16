using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Wex.PurchasingPlatform.Api.Common.Exceptions;
using Wex.PurchasingPlatform.Api.Entities;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;
using Wex.PurchasingPlatform.Api.Services.Implementation;
using Wex.PurchasingPlatform.Api.Services.Interfaces;
using Wex.PurchasingPlatform.Tests.TestDoubles;

namespace Wex.PurchasingPlatform.Tests.Services;

public class CurrencyOptionCacheHostedServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCurrencyOptionsAlreadyExist_SkipsRefresh()
    {
        var currencyOptionRepository = new Mock<CurrencyOptionRepository>(TestDbContextFactory.CreateUnconfigured());
        currencyOptionRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CurrencyOption { Country = "Canada", CurrencyName = "Dollar" }]);

        var cacheService = new Mock<ICurrencyOptionCacheService>();

        var hostedService = CreateHostedService(currencyOptionRepository.Object, cacheService.Object);

        await hostedService.StartAsync(CancellationToken.None);
        await hostedService.ExecuteTask!;
        await hostedService.StopAsync(CancellationToken.None);

        cacheService.Verify(service => service.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCurrencyOptionsAreEmpty_RunsRefresh()
    {
        var currencyOptionRepository = new Mock<CurrencyOptionRepository>(TestDbContextFactory.CreateUnconfigured());
        currencyOptionRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var cacheService = new Mock<ICurrencyOptionCacheService>();

        var hostedService = CreateHostedService(currencyOptionRepository.Object, cacheService.Object);

        await hostedService.StartAsync(CancellationToken.None);
        await hostedService.ExecuteTask!;
        await hostedService.StopAsync(CancellationToken.None);

        cacheService.Verify(service => service.RefreshAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRefreshThrows_DoesNotPropagateException()
    {
        var currencyOptionRepository = new Mock<CurrencyOptionRepository>(TestDbContextFactory.CreateUnconfigured());
        currencyOptionRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var cacheService = new Mock<ICurrencyOptionCacheService>();
        cacheService
            .Setup(service => service.RefreshAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExchangeRateProviderUnavailableException("unreachable", new HttpRequestException()));

        var hostedService = CreateHostedService(currencyOptionRepository.Object, cacheService.Object);

        await hostedService.StartAsync(CancellationToken.None);
        var exception = await Record.ExceptionAsync(() => hostedService.ExecuteTask!);
        await hostedService.StopAsync(CancellationToken.None);

        Assert.Null(exception);
    }

    private static CurrencyOptionCacheHostedService CreateHostedService(CurrencyOptionRepository currencyOptionRepository, ICurrencyOptionCacheService cacheService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(currencyOptionRepository);
        services.AddSingleton(cacheService);
        var provider = services.BuildServiceProvider();

        return new CurrencyOptionCacheHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Mock.Of<ILogger<CurrencyOptionCacheHostedService>>());
    }
}
