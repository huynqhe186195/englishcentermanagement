using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models;
using System.Net;

namespace EnglishCenter.Web.Services;

public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("Api");
        var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        var refreshToken = _httpContextAccessor.HttpContext?.Session.GetString("RefreshToken");
        if (string.IsNullOrEmpty(refreshToken)) return false;

        var campusIdRaw = _httpContextAccessor.HttpContext?.Session.GetString("CampusId");
        long? campusId = null;
        if (long.TryParse(campusIdRaw, out var parsedCampusId) && parsedCampusId > 0)
        {
            campusId = parsedCampusId;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var resp = await client.PostAsJsonAsync("auth/refresh-token", new { RefreshToken = refreshToken, CampusId = campusId });
            if (!resp.IsSuccessStatusCode) return false;

            var json = await resp.Content.ReadAsStringAsync();
            var login = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (login == null) return false;

            // update session tokens
            _httpContextAccessor.HttpContext?.Session.SetString("AccessToken", login.AccessToken ?? string.Empty);
            _httpContextAccessor.HttpContext?.Session.SetString("RefreshToken", login.RefreshToken ?? string.Empty);
            _httpContextAccessor.HttpContext?.Session.SetString("UserName", login.UserName ?? string.Empty);
            _httpContextAccessor.HttpContext?.Session.SetString("CampusId", login.CampusId?.ToString() ?? string.Empty);
            _httpContextAccessor.HttpContext?.Session.SetString("Roles", JsonSerializer.Serialize(login.Roles ?? new List<string>()));

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<byte[]?> GetFileAsync(string url)
    {
        var client = CreateClient();
        var resp = await client.GetAsync(url);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            var ok = await TryRefreshTokenAsync();
            if (ok)
            {
                client = CreateClient();
                resp = await client.GetAsync(url);
            }
        }

        if (!resp.IsSuccessStatusCode)
        {
            try
            {
                var txt = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("GetFileAsync failed. Url: {Url}, Status: {Status}, Response: {Response}", url, resp.StatusCode, txt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetFileAsync failed reading response body. Url: {Url}, Status: {Status}", url, resp.StatusCode);
            }

            return default;
        }

        return await resp.Content.ReadAsByteArrayAsync();
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var client = CreateClient();
        var resp = await client.GetAsync(url);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            var ok = await TryRefreshTokenAsync();
            if (ok)
            {
                client = CreateClient();
                resp = await client.GetAsync(url);
            }
        }

        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync();
        try
        {
            // try to parse ApiResponse<T>
            var apiResp = JsonSerializer.Deserialize<ApiResponse<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (apiResp != null)
            {
                return apiResp.Data;
            }
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return default;
        }
    }

    public async Task<T?> PostAsync<TRequest, T>(string url, TRequest body)
    {
        var client = CreateClient();
        var resp = await client.PostAsJsonAsync(url, body);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            var ok = await TryRefreshTokenAsync();
            if (ok)
            {
                client = CreateClient();
                resp = await client.PostAsJsonAsync(url, body);
            }
        }

        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync();
        try
        {
            var apiResp = JsonSerializer.Deserialize<ApiResponse<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (apiResp != null) return apiResp.Data;
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return default;
        }
    }

    public async Task<T?> PostMultipartAsync<T>(string url, IFormFile file)
    {
        var client = CreateClient();
        using var content = new MultipartFormDataContent();
        if (file != null)
        {
            var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            if (!string.IsNullOrEmpty(file.ContentType))
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);
        }

        var resp = await client.PostAsync(url, content);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            var ok = await TryRefreshTokenAsync();
            if (ok)
            {
                client = CreateClient();
                resp = await client.PostAsync(url, content);
            }
        }

        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync();
        try
        {
            var apiResp = JsonSerializer.Deserialize<ApiResponse<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (apiResp != null) return apiResp.Data;
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return default;
        }
    }

    public async Task<bool> PostAsync(string url, object body)
    {
        var client = CreateClient();
        var resp = await client.PostAsJsonAsync(url, body);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            var ok = await TryRefreshTokenAsync();
            if (ok)
            {
                client = CreateClient();
                resp = await client.PostAsJsonAsync(url, body);
            }
        }
        return resp.IsSuccessStatusCode;
    }

    public async Task<T?> PutAsync<TRequest, T>(string url, TRequest body)
    {
        var client = CreateClient();
        var resp = await client.PutAsJsonAsync(url, body);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            var ok = await TryRefreshTokenAsync();
            if (ok)
            {
                client = CreateClient();
                resp = await client.PutAsJsonAsync(url, body);
            }
        }

        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync();
        try
        {
            var apiResp = JsonSerializer.Deserialize<ApiResponse<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (apiResp != null) return apiResp.Data;
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return default;
        }
    }

    public async Task<bool> PutAsync(string url, object body)
    {
        var client = CreateClient();
        var resp = await client.PutAsJsonAsync(url, body);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            var ok = await TryRefreshTokenAsync();
            if (ok)
            {
                client = CreateClient();
                resp = await client.PutAsJsonAsync(url, body);
            }
        }
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string url)
    {
        var client = CreateClient();
        var resp = await client.DeleteAsync(url);
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            var ok = await TryRefreshTokenAsync();
            if (ok)
            {
                client = CreateClient();
                resp = await client.DeleteAsync(url);
            }
        }
        return resp.IsSuccessStatusCode;
    }
}
