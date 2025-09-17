using BlazorApp.Web.Components;
using BlazorApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Infrastructure services (Entity Framework, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Ensure database is created and seeded
await app.Services.EnsureDatabaseAsync();
await app.Services.SeedDataAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
