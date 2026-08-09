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
public class PromotionController : ControllerBase
{
    private readonly AppDbContext context;

    public PromotionController(AppDbContext _context)
    {
        context = _context;
    }



    /////////////////// Case 1: POST Create Promotion ///////////////////
    [HttpPost("CreatePromotion")]
    public IActionResult CreatePromotion(Promotion promotion)
    {
        context.Promotions.Add(promotion);
        context.SaveChanges();
        return Ok("Promotion created successfully");
    }



    /////////////////// Case 2: PUT/PATCH Update Promotion ///////////////////
    [HttpPut("UpdatePromotion")]
    public IActionResult UpdatePromotion(int id, Promotion updatedPromotion)
    {
        Promotion promotion = context.Promotions.FirstOrDefault(p => p.PromotionId == id);

        if (promotion == null)
        {
            return NotFound("Promotion not found");
        }
        else
        {
            promotion.PromotionCode = updatedPromotion.PromotionCode;
            promotion.DiscountPercentage = updatedPromotion.DiscountPercentage;
            promotion.ExpiryDate = updatedPromotion.ExpiryDate;
            promotion.HotelId = updatedPromotion.HotelId;
            context.SaveChanges();
            return Ok("Promotion updated successfully");
        }
    }



    /////////////////// Case 3: PUT/PATCH Update Hotel ID for Promotion ///////////////////
    [HttpPatch("UpdatePromotionHotel")]
    public IActionResult UpdatePromotionHotel(int id, int hotelId, int newHotelId)
    {
        Promotion promotion = context.Promotions.FirstOrDefault(p => p.PromotionId == id);

        if (promotion == null)
        {
            return NotFound("Promotion not found");
        }

        Hotel hotel = context.Hotels.FirstOrDefault(h => h.HotelId == hotelId);

        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        promotion.HotelId = newHotelId;
        context.SaveChanges();
        return Ok("Promotion hotel updated successfully");
    }



    /////////////////// Case 4: DELETE Remove Promotion ///////////////////
    [HttpDelete("RemovePromotion")]
    public IActionResult RemovePromotion(int id)
    {
        Promotion promotion = context.Promotions.FirstOrDefault(p => p.PromotionId == id);

        if (promotion == null)
        {
            return NotFound("Promotion not found");
        }
        else
        {
            context.Promotions.Remove(promotion);
            context.SaveChanges();
            return Ok("Promotion removed successfully" + promotion);
        }
    }



    /////////////////// Case 5: GET List Promotions with Hotel Information ///////////////////
    [HttpGet("GetAllPromotions")]
    public IActionResult GetAllPromotions()
    {
        List<Promotion> promotions = context.Promotions
                                            .Include(r => r._Hotels)
                                            .ToList();

        return Ok(promotions);
    }



    /////////////////// Case 6: GET Find Promotion By Id ///////////////////
    [HttpGet("GetPromotionById")]
    public IActionResult GetPromotionById(int id)
    {
        Promotion promotion = context.Promotions.FirstOrDefault(p => p.PromotionId == id);

        if (promotion == null)
        {
            return NotFound("Promotion not found");
        }

        return Ok(promotion);
    }



    /////////////////// Case 7: GET Filter Promotions By Expiry Date ///////////////////
    [HttpGet("GetPromotionsByExpiryDate")]
    public IActionResult GetPromotionsByExpiryDate(DateTime expiryDate)
    {
        List<Promotion> promotions = context.Promotions
                                            .Where(p => p.ExpiryDate >= expiryDate)
                                            .ToList();

        return Ok(promotions);
    }



    /////////////////// Case 8: GET Find Promotions Sorted By Discount Percentage ///////////////////
    [HttpGet("GetPromotionsSortedByDiscount")]
    public IActionResult GetPromotionsSortedByDiscount()
    {
        List<Promotion> promotions = context.Promotions
                                            .OrderByDescending(p => p.DiscountPercentage)
                                            .ToList();

        return Ok(promotions);
    } 
}
