using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Student;

public class ProfileModel : PageModel
{
    private readonly IApiClient _apiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProfileModel(IApiClient apiClient, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public long StudentId { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public StudentProfileDetailDto Student { get; set; } = new();

    [BindProperty]
    public UpdateStudentProfileRequestDto ProfileForm { get; set; } = new();

    [BindProperty]
    public ChangePasswordFormDto PasswordForm { get; set; } = new();

    public bool ProfileUpdated { get; set; }
    public bool PasswordUpdated { get; set; }
    public string? ProfileMessage { get; set; }
    public string? PasswordMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadProfileAsync();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        await LoadProfileAsync();
        if (StudentId <= 0)
        {
            ProfileMessage = "Không tìm thấy học viên liên kết với tài khoản hiện tại.";
            return Page();
        }

        var (ok, message) = await SendApiAsync(HttpMethod.Put, "students/me/profile", ProfileForm);
        ProfileUpdated = ok;
        ProfileMessage = ok ? "Cập nhật thông tin thành công." : $"Cập nhật thông tin thất bại: {message}";

        if (ok)
        {
            var profileCompleted = IsProfileCompleted(ProfileForm);
            HttpContext.Session.SetString("HasCompletedStudentProfile", profileCompleted ? "true" : "false");
            await LoadProfileAsync();

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        await LoadProfileAsync();

        if (string.IsNullOrWhiteSpace(PasswordForm.CurrentPassword)
            || string.IsNullOrWhiteSpace(PasswordForm.NewPassword)
            || string.IsNullOrWhiteSpace(PasswordForm.ConfirmNewPassword))
        {
            PasswordMessage = "Vui lòng nhập đầy đủ thông tin đổi mật khẩu.";
            return Page();
        }

        var (ok, message) = await SendApiAsync(HttpMethod.Post, "auth/change-password", PasswordForm);
        PasswordUpdated = ok;
        PasswordMessage = ok ? "Đổi mật khẩu thành công." : $"Đổi mật khẩu thất bại: {message}";
        if (ok)
        {
            PasswordForm = new ChangePasswordFormDto();
        }

        return Page();
    }

    private async Task LoadProfileAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me != null)
        {
            UserName = me.UserName;
            FullName = me.FullName;
            StudentId = me.StudentId ?? 0;
        }

        if (StudentId <= 0)
        {
            Student = new StudentProfileDetailDto();
            return;
        }

        Student = await _apiClient.GetAsync<StudentProfileDetailDto>($"students/{StudentId}") ?? new StudentProfileDetailDto();
        ProfileForm = new UpdateStudentProfileRequestDto
        {
            FullName = Student.FullName,
            DateOfBirth = Student.DateOfBirth,
            Gender = Student.Gender,
            Phone = Student.Phone,
            Email = Student.Email,
            SchoolName = Student.SchoolName,
            EnglishLevel = Student.EnglishLevel,
            Note = Student.Note,
            Status = Student.Status
        };
    }

    private async Task<(bool Ok, string Message)> SendApiAsync(HttpMethod method, string url, object payload)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(payload)
        };

        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            return (true, string.Empty);
        }

        return (false, ExtractErrorMessage(content, (int)response.StatusCode));
    }

    private static bool IsProfileCompleted(UpdateStudentProfileRequestDto profile)
    {
        return !string.IsNullOrWhiteSpace(profile.FullName)
            && profile.DateOfBirth.HasValue
            && profile.Gender.HasValue
            && !string.IsNullOrWhiteSpace(profile.Phone)
            && !string.IsNullOrWhiteSpace(profile.Email)
            && !string.IsNullOrWhiteSpace(profile.SchoolName)
            && !string.IsNullOrWhiteSpace(profile.EnglishLevel)
            && profile.Status == 1;
    }

    private static string ExtractErrorMessage(string content, int statusCode)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"HTTP {statusCode}";
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var messageEl) && messageEl.ValueKind == JsonValueKind.String)
                return messageEl.GetString() ?? $"HTTP {statusCode}";

            if (root.TryGetProperty("Message", out var messageEl2) && messageEl2.ValueKind == JsonValueKind.String)
                return messageEl2.GetString() ?? $"HTTP {statusCode}";

            if (root.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String)
                return titleEl.GetString() ?? $"HTTP {statusCode}";

            if (root.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == JsonValueKind.Object)
            {
                var first = errorsEl.EnumerateObject().FirstOrDefault();
                if (first.Value.ValueKind == JsonValueKind.Array && first.Value.GetArrayLength() > 0)
                {
                    return first.Value[0].GetString() ?? $"HTTP {statusCode}";
                }
            }
        }
        catch
        {
            // ignored
        }

        return content.Length > 240 ? content[..240] + "..." : content;
    }
}
