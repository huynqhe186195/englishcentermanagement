var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddHttpClient("Api", client =>
{
    var baseUrl = builder.Configuration.GetValue<string>("Api:BaseUrl")?.Trim();
    if (!string.IsNullOrEmpty(baseUrl))
    {
        if (!baseUrl.EndsWith("/"))
        {
            baseUrl += "/";
        }

        if (!baseUrl.Contains("/api/", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl += "api/";
        }

        client.BaseAddress = new Uri(baseUrl);
    }
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EnglishCenter.Web.Services.IApiClient, EnglishCenter.Web.Services.ApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run("https://localhost:5105");