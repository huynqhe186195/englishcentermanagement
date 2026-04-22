namespace EnglishCenter.Application.Features.Campus.Dtos;

public class CreateCampusRequestDto
{
    public string CampusCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int Status { get; set; } = 1;
}
