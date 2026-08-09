using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;

namespace SparkyProject.API.Controllers;

// Owner: Khalid Al Hashemi
// Required cases (min. 8) — see capstone brief p.11-12:
// 1. POST   Create
// 2. PUT/PATCH  Update
// 3. PUT/PATCH  Second distinct update (status change / update via related FK)
// 4. DELETE Delete (consider soft-delete)
// 5. GET (list)   Include() a related navigation property
// 6. GET (find)   By Id
// 7. GET (filter) LINQ Where() on a meaningful field
// 8. GET (sort/aggregate) OrderBy / Count / Sum / Average / GroupBy

[ApiController]
[Route("api/[controller]")]
public class RoomTypeController : ControllerBase
{
    private readonly AppDbContext context;

    public RoomTypeController(AppDbContext _context)
    {
        context = _context;
    }



    /////////////////// Case 1: POST Create Room Type ///////////////////
    [HttpPost("CreateRoomType")]
    public IActionResult CreateRoomType(RoomType roomType)
    {
        context.RoomTypes.Add(roomType);
        context.SaveChanges();

        return Ok("Room type created successfully");
    }



    /////////////////// Case 2: PUT/PATCH Update Room Type ///////////////////
    [HttpPut("UpdateRoomType")]
    public IActionResult UpdateRoomType(int id, RoomType updatedRoomType)
    {
        RoomType roomType = context.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {
            return NotFound("Room type not found");
        }
        else
        {
            roomType.RoomName = updatedRoomType.RoomName;
            roomType.BasePrice = updatedRoomType.BasePrice;
            roomType.Capacity = updatedRoomType.Capacity;
            context.SaveChanges();

            return Ok("Room type updated successfully");
        }
    }



    /////////////////// Case 3: PUT/PATCH Update Hotel ID for Room Type ///////////////////
    [HttpPatch("UpdateHotelIdForRoomType")]
    public IActionResult UpdateHotelIdForRoomType(int id, int newHotelID)
    {
        RoomType roomType = context.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {
            return NotFound("Room type not found");
        }

        Hotel hotel = context.Hotels.FirstOrDefault(h => h.HotelId == newHotelID);

        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        roomType.HotelId = newHotelID;
        context.SaveChanges();
        return Ok("Hotel ID updated successfully" + newHotelID);
    }



    /////////////////// Case 4: DELETE Remove Room Type ///////////////////
    [HttpDelete("RemoveRoomType")]
    public IActionResult RemoveRoomType(int id)
    {
        RoomType roomType = context.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {
            return NotFound("Room type not found");
        }
        else
        {
            context.RoomTypes.Remove(roomType);
            context.SaveChanges();
            return Ok("Room type removed successfully");
        }
    }



    /////////////////// Case 5: GET List Room Types with Hotel Information ///////////////////
    [HttpGet("GetAllRoomTypes")]
    public IActionResult GetAllRoomTypes()
    {
        List<RoomType> roomTypes = context.RoomTypes
                                          .Include(rt => rt._hotel)
                                          .ToList();
        return Ok(roomTypes);
    }



    /////////////////// Case 6: GET Find Room Type by ID ///////////////////
    [HttpGet("GetRoomTypeById")]
    public IActionResult GetRoomTypeById(int id)
    {
        RoomType roomType = context.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {
            return NotFound("Room type not found");
        }

        return Ok(roomType);
    }



    /////////////////// Case 7: GET Find Room Types by Capacity ///////////////////
    [HttpGet("GetRoomTypesByCapacity")]
    public IActionResult GetRoomTypesByCapacity(int minCapacity)
    {
        List<RoomType> roomTypes = context.RoomTypes.Where(rt => rt.Capacity >= minCapacity)
                                                    .ToList();

        return Ok(roomTypes);
    }



    /////////////////// Case 8: GET Find Room Types sorted by Price ///////////////////
    [HttpGet("GetRoomTypesSortedByPrice")]
    public IActionResult GetRoomTypesSortedByPrice()
    {
        List<RoomType> roomTypes = context.RoomTypes.OrderBy(rt => rt.BasePrice)
                                                    .ToList();

        return Ok(roomTypes);
    }
}
