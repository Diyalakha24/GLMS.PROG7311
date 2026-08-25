using GLMS.Web.Services;

// Code Attribution
// Title: ASP.NET Core MVC with HttpClient
// Author: Microsoft
// Date: 2026
// Availability: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Session for JWT token storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// Register ApiService — this is the ONLY way the MVC app communicates with data
// No DbContext here — MVC is fully decoupled from the database
builder.Services.AddHttpClient<ApiService>(client =>
{
    var apiUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7043/";
    client.BaseAddress = new Uri(apiUrl);
});

// FIX: Register CurrencyService so it can be injected into ServiceRequestsController
builder.Services.AddHttpClient<CurrencyService>();

var app = builder.Build();

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
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();