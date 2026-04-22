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

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;

    if (path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/images", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    var rawRoles = context.Session.GetString("Roles");
    var roles = string.IsNullOrWhiteSpace(rawRoles)
        ? new List<string>()
        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(rawRoles) ?? new List<string>();

    bool hasRole(string role) => roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    if (path.StartsWith("/SuperAdmins", StringComparison.OrdinalIgnoreCase))
    {
        if (!roles.Any())
        {
            context.Response.Redirect("/Account/Login");
            return;
        }

        if (!hasRole("SUPER_ADMIN"))
        {
            context.Response.Redirect("/Admin/Index");
            return;
        }
    }

    if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
    {
        if (!roles.Any())
        {
            context.Response.Redirect("/Account/Login");
            return;
        }

        if (hasRole("SUPER_ADMIN"))
        {
            context.Response.Redirect("/SuperAdmins/Dashboard");
            return;
        }

        if (!hasRole("CENTER_ADMIN") && !hasRole("MANAGER") && !hasRole("ADMIN") && !hasRole("STAFF"))
        {
            context.Response.Redirect("/Account/Login");
            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapRazorPages();

app.Run("https://localhost:5105");
