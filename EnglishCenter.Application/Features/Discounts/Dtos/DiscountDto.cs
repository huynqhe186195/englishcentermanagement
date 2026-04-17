namespace EnglishCenter.Application.Features.Discounts.Dtos;

public class DiscountDto
{
    public long Id { get; set; }
    public string DiscountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DiscountType { get; set; }
    public decimal Value { get; set; }
    public int Status { get; set; }
}
