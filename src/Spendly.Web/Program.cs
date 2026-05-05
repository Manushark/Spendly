using Spendly.Web.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Data Protection: persist encryption keys so cookies survive app recycles
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("Spendly");

// HttpContext accessor for services
builder.Services.AddHttpContextAccessor();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7267/";

// Add AuthApiClient with HttpClient configuration
builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Dashboard API client
builder.Services.AddHttpClient<DashboardApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Recurring expenses API client
builder.Services.AddHttpClient<RecurringExpenseApiClient>(client => {  
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

// Add BudgetApiClient with HttpClient configuration (includes JWT token)
builder.Services.AddHttpClient<BudgetApiClient>(client => {
    client.BaseAddress = new Uri(apiBaseUrl);
});


// Add ExpenseApiClient with HttpClient configuration (includes JWT token)
builder.Services.AddHttpClient<ExpenseApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// User profile API client
builder.Services.AddHttpClient<UserApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Category API client
builder.Services.AddHttpClient<CategoryApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Income API client
builder.Services.AddHttpClient<IncomeApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Notification API client
builder.Services.AddHttpClient<NotificationApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Insights API client
builder.Services.AddHttpClient<InsightsApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Savings Goal API client
builder.Services.AddHttpClient<SavingsGoalApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Tag API client
builder.Services.AddHttpClient<TagApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Import API client
builder.Services.AddHttpClient<ImportApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Keep the API warm in production (prevents Azure F1 cold start)
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<ApiWarmupService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Localization middleware
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("es") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
// Everything below is for integration testing purposes only