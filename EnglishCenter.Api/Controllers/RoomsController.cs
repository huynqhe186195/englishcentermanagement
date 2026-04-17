using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using EnglishCenter.Application.Features.Rooms;
using EnglishCenter.Application.Features.Rooms.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomsController(RoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetRoomsPagingRequestDto request)
    {
        var result = await _roomService.GetPagedAsync(request);
        return Ok(ApiResponse<PagedResult<RoomDto>>.SuccessResponse(result, "Get rooms successfully"));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _roomService.GetByIdAsync(id);
        return Ok(ApiResponse<RoomDetailDto>.SuccessResponse(result, "Get room successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequestDto request)
    {
        var id = await _roomService.CreateAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Room created successfully"));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoomRequestDto request)
    {
        await _roomService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Room updated successfully"));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _roomService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Room deleted successfully"));
    }
}
