namespace EnglishCenter.Application.Features.Discounts.Dtos;

public class DiscountDetailDto
{
    public long Id { get; set; }
    public string DiscountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DiscountType { get; set; }
    public decimal Value { get; set; }
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
