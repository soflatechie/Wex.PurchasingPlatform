using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Data;

namespace Wex.PurchasingPlatform.Tests.TestDoubles;

public static class TestDbContextFactory
{
    // Concrete repository classes take an AppDbContext constructor argument that Moq's class
    // mocks still require even though CallBase defaults to false and none of these mocks ever
    // touch the database - this satisfies that constructor without configuring a real provider.
    public static AppDbContext CreateUnconfigured()
    {
        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().Options);
    }
}
