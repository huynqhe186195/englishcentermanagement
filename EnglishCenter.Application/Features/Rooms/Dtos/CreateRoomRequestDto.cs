namespace EnglishCenter.Application.Features.Rooms.Dtos;

public class CreateRoomRequestDto
{
    public long CampusId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomType { get; set; }
    public int Status { get; set; } = 1;
}
