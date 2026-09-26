using HBS.API.DI;
using HBS.API.Db;
using HBS.API.Db.Interceptors;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

if (args.Length > 0 && args[0].StartsWith("--seed-", StringComparison.Ordinal))
{
    Environment.ExitCode = await HBS.API.Db.Seeds.SeedCommand.RunAsync(args, HBS.API.Db.Seeds.SeedImportDatabase.Create);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDependencies(builder.Configuration);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>();
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );

    options.AddInterceptors(auditInterceptor);
});

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
