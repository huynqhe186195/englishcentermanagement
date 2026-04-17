namespace EnglishCenter.Application.Features.Campus.Dtos;

public class CampusDetailDto
{
    public long Id { get; set; }
    public string CampusCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
