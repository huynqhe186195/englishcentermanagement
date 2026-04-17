namespace EnglishCenter.Application.Features.Rooms.Dtos;

public class UpdateRoomRequestDto
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomType { get; set; }
    public int Status { get; set; }
}
