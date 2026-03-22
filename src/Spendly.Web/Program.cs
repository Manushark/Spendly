using Spendly.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpContext accessor for services
builder.Services.AddHttpContextAccessor();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7267/";
var isDevelopment = builder.Environment.IsDevelopment();

// In development, bypass SSL validation for server-to-server calls to the local API.
// The ASP.NET Core dev certificate is trusted by browsers (dotnet dev-certs --trust),
// but not necessarily by the server-side HttpClient runtime.
HttpClientHandler CreateApiHandler() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = isDevelopment
        ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        : null
};

// Add AuthApiClient with HttpClient configuration
builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).ConfigurePrimaryHttpMessageHandler(CreateApiHandler);

// Dashboard API client
builder.Services.AddHttpClient<DashboardApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).ConfigurePrimaryHttpMessageHandler(CreateApiHandler);

// Insights API client
builder.Services.AddHttpClient<InsightsApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).ConfigurePrimaryHttpMessageHandler(CreateApiHandler);

// Recurring expenses API client
builder.Services.AddHttpClient<RecurringExpenseApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).ConfigurePrimaryHttpMessageHandler(CreateApiHandler);

// Add BudgetApiClient with HttpClient configuration (includes JWT token)
builder.Services.AddHttpClient<BudgetApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).ConfigurePrimaryHttpMessageHandler(CreateApiHandler);

// Add ExpenseApiClient with HttpClient configuration (includes JWT token)
builder.Services.AddHttpClient<ExpenseApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).ConfigurePrimaryHttpMessageHandler(CreateApiHandler);

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

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
