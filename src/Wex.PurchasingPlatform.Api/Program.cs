using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.Middleware;
using Wex.PurchasingPlatform.Api.Repositories.Implementation;
using Wex.PurchasingPlatform.Api.Repositories.Interfaces;
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

builder.Services.AddScoped<IPurchaseTransactionRepository, PurchaseTransactionRepository>();
builder.Services.AddScoped<ICurrencyOptionRepository, CurrencyOptionRepository>();

builder.Services.AddScoped<IValidator<CreatePurchaseTransactionRequest>, CreatePurchaseTransactionRequestValidator>();
builder.Services.AddScoped<IValidator<UpdatePurchaseTransactionRequest>, UpdatePurchaseTransactionRequestValidator>();

builder.Services.AddScoped<IPurchaseTransactionService, PurchaseTransactionService>();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
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

public partial class Program
{
}
