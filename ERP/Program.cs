using DinkToPdf;
using DinkToPdf.Contracts;
using ERP.Data;
using ERP.Models;
using ERP.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Stimulsoft.Report;
using StackExchange.Redis;
using ERP.Interface;
using ERP.Services;
using ERP.Hubs;

Stimulsoft.Base.StiLicense.Key = "6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHkO46nMQvol4ASeg91in+mGJLnn2KMIpg3eSXQSgaFOm15+0l" +
"hekKip+wRGMwXsKpHAkTvorOFqnpF9rchcYoxHXtjNDLiDHZGTIWq6D/2q4k/eiJm9fV6FdaJIUbWGS3whFWRLPHWC" +
"BsWnalqTdZlP9knjaWclfjmUKf2Ksc5btMD6pmR7ZHQfHXfdgYK7tLR1rqtxYxBzOPq3LIBvd3spkQhKb07LTZQoyQ" +
"3vmRSMALmJSS6ovIS59XPS+oSm8wgvuRFqE1im111GROa7Ww3tNJTA45lkbXX+SocdwXvEZyaaq61Uc1dBg+4uFRxv" +
"yRWvX5WDmJz1X0VLIbHpcIjdEDJUvVAN7Z+FW5xKsV5ySPs8aegsY9ndn4DmoZ1kWvzUaz+E1mxMbOd3tyaNnmVhPZ" +
"eIBILmKJGN0BwnnI5fu6JHMM/9QR2tMO1Z4pIwae4P92gKBrt0MqhvnU1Q6kIaPPuG2XBIvAWykVeH2a9EP6064e11" +
"PFCBX4gEpJ3XFD0peE5+ddZh+h495qUc1H2B";

StiOptions.Engine.AllowSetCurrentDirectory = false;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Identity configuration
builder.Services.AddIdentity<Users, Roles>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<ERPContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication();

// تنظیمات اضافی برای Authentication/Authorization
builder.Services.ConfigureApplicationCookie(options =>
{
    // تعیین path لاگین
    options.LoginPath = "/Account/Login"; // یا "/Account/Login" بر اساس پروژه شما
    
    // صفحه‌ای برای دسترسی غیرمجاز
    options.AccessDeniedPath = "/Account/AccessDenied"; // یا مسیر مناسب
    
    // Sliding Expiration - تمدید خودکار session در هر request
    options.SlidingExpiration = true;
    
    // مدت زمان انقضای Cookie
    options.ExpireTimeSpan = TimeSpan.FromDays(60);
    
    // Cookie نباید در client-side قابل دسترسی باشد
    options.Cookie.HttpOnly = true;
    
    // Cookie تنها در HTTPS ارسال شود
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    
    // Cookie essential است
    options.Cookie.IsEssential = true;
});

// Database Context - SQL Server
var connectionString = builder.Configuration.GetConnectionString("ERPDb");
builder.Services.AddDbContext<ERPContext>(options =>
    options.UseSqlServer(connectionString));

var connectionString2 = builder.Configuration.GetConnectionString("EMPDb");
builder.Services.AddDbContext<EMPContext>(options =>
    options.UseSqlServer(connectionString2));


builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<IServices,Services>();

// Kendo UI
builder.Services.AddKendo();

// PDF Converter (DinkToPdf)
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserIdClaimType = System.Security.Claims.ClaimTypes.NameIdentifier;
});

// ========================
// Redis Configuration
// ========================
//var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
//builder.Services.AddScoped<IBasketRepository, BasketRepository>();

//if (!string.IsNullOrEmpty(redisConnectionString))
//{
//    // ثبت ConnectionMultiplexer به صورت Singleton
//    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

//    // کش توزیع‌شده بر پایه Redis
//    builder.Services.AddStackExchangeRedisCache(options =>
//    {
//        options.Configuration = redisConnectionString;
//        // گزینه‌های دلخواه:
//        // options.InstanceName = "ERP_";
//    });

//    // Session بر پایه Redis
//    builder.Services.AddSession(options =>
//    {
//        options.IdleTimeout = TimeSpan.FromMinutes(30);
//        options.Cookie.HttpOnly = true;
//        options.Cookie.IsEssential = true;
//    });
//}
//else
//{
    // اگر Redis در دسترس نبود (مثلاً در توسعه محلی بدون Redis)، از Memory Cache استفاده کن
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession();
//}

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // مهم: قبل از Authentication/Authorization

app.UseAuthentication();
app.UseAuthorization();

// اضافه کردن Authentication Exception Middleware
app.UseMiddleware<AuthenticationExceptionMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.Run();