var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5223";
builder.Services.AddHttpClient("EnglishCenterApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
