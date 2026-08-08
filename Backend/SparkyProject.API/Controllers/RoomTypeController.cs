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
    public void CreateRoomType(RoomType roomType)
    {
        context.RoomTypes.Add(roomType);
        context.SaveChanges();
    }



    /////////////////// Case 2: PUT/PATCH Update Room Type ///////////////////
    public void UpdateRoomType(int id, RoomType updatedRoomType)
    {
        RoomType roomType = context.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {

        }
        else
        {
            roomType.RoomName = updatedRoomType.RoomName;
            roomType.BasePrice = updatedRoomType.BasePrice;
            roomType.Capacity = updatedRoomType.Capacity;
            context.SaveChanges();
        }
    }



    /////////////////// Case 3: PUT/PATCH Update Hotel ID for Room Type ///////////////////
    public void UpdateHotelIdForRoomType(int id, int hotelID, int newHotelID)
    {
        RoomType roomType = context.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {

        }

        Hotel hotel = context.Hotels.FirstOrDefault(h => h.HotelId == hotelID);

        if (hotel == null)
        {

        }

        roomType.HotelId = newHotelID;
        context.SaveChanges();
    }



    /////////////////// Case 4: DELETE Remove Room Type ///////////////////
    public void RemoveRoomType(int id)
    {
        RoomType roomType = context.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {

        }
        else
        {
            context.RoomTypes.Remove(roomType);
            context.SaveChanges();
        }
    }



    /////////////////// Case 5: GET List Room Types with Hotel Information ///////////////////
    public List<RoomType> GetRoomTypes()
    {
        List<RoomType> roomTypes = context.RoomTypes
                                          .Include(rt => rt._hotel)
                                          .ToList();
        return roomTypes;
    }
}
