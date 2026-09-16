using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wex.PurchasingPlatform.Api.Configuration;
using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.ExternalServices.Implementation;
using Wex.PurchasingPlatform.Api.ExternalServices.Interfaces;
using Wex.PurchasingPlatform.Api.Middleware;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;
using Wex.PurchasingPlatform.Api.Services.Implementation;
using Wex.PurchasingPlatform.Api.Services.Interfaces;
using Wex.PurchasingPlatform.Api.Validation;
using Wex.PurchasingPlatform.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PurchaseTransactionRepository>();
builder.Services.AddScoped<CurrencyOptionRepository>();

builder.Services.AddScoped<IValidator<CreatePurchaseTransactionRequest>, CreatePurchaseTransactionRequestValidator>();
builder.Services.AddScoped<IValidator<UpdatePurchaseTransactionRequest>, UpdatePurchaseTransactionRequestValidator>();

builder.Services.AddScoped<IPurchaseTransactionService, PurchaseTransactionService>();

builder.Services.Configure<TreasuryOptions>(builder.Configuration.GetSection(TreasuryOptions.SectionName));

builder.Services.AddHttpClient<IExchangeRateProvider, TreasuryExchangeRateClient>((serviceProvider, client) =>
{
    var treasuryOptions = serviceProvider.GetRequiredService<IOptions<TreasuryOptions>>().Value;
    client.BaseAddress = new Uri(treasuryOptions.BaseUrl);
});

builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
builder.Services.AddScoped<ICurrencyOptionCacheService, CurrencyOptionCacheService>();
builder.Services.AddHostedService<CurrencyOptionCacheHostedService>();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<ExchangeRateExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
