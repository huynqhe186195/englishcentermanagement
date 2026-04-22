namespace EnglishCenter.Web.Models;

public class LoginResponse
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public long? CampusId { get; set; }
    public bool HasStudentProfile { get; set; }
    public bool HasCompletedStudentProfile { get; set; }
    public bool HasAnyEnrollment { get; set; }
    public List<string> Roles { get; set; } = new();
}
