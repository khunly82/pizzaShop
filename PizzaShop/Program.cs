using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PizzaShop;
using PizzaShop.Components;
using PizzaShop.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureMonitoring();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<PizzaShopContext>(b => b.UseSqlServer(
    builder.Configuration.GetConnectionString("Main")
));

builder.Services.AddMudServices(o =>
{
    o.SnackbarConfiguration.MaxDisplayedSnackbars = 10;
    o.SnackbarConfiguration.PreventDuplicates = false;
});

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
