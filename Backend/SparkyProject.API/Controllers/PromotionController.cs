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
    public void CreatePromotion(Promotion promotion)
    {
        context.Promotions.Add(promotion);
        context.SaveChanges();
    }



    /////////////////// Case 2: PUT/PATCH Update Promotion ///////////////////
    public void UpdatePromotion(int id, Promotion updatedPromotion)
    {
        Promotion promotion = context.Promotions.FirstOrDefault(p => p.PromotionId == id);

        if (promotion == null)
        {

        }
        else
        {
            promotion.PromotionCode = updatedPromotion.PromotionCode;
            promotion.DiscountPercentage = updatedPromotion.DiscountPercentage;
            promotion.ExpiryDate = updatedPromotion.ExpiryDate;
            promotion.HotelId = updatedPromotion.HotelId;
            context.SaveChanges();
        }
    }



    /////////////////// Case 3: PUT/PATCH Update Hotel ID for Promotion ///////////////////
    public void UpdatePromotionHotel(int id, int hotelId, int newHotelId)
    {
        Promotion promotion = context.Promotions.FirstOrDefault(p => p.PromotionId == id);

        if (promotion == null)
        {

        }

        Hotel hotel = context.Hotels.FirstOrDefault(h => h.HotelId == hotelId);

        if (hotel == null)
        {

        }

        promotion.HotelId = newHotelId;
        context.SaveChanges();
    }



    /////////////////// Case 4: DELETE Remove Promotion ///////////////////
    public void RemovePromotion(int id)
    {
        Promotion promotion = context.Promotions.FirstOrDefault(p => p.PromotionId == id);

        if (promotion == null)
        {

        }
        else
        {
            context.Promotions.Remove(promotion);
            context.SaveChanges();
        }
    }



    /////////////////// Case 5: GET List Promotions with Hotel Information ///////////////////
    public List<Promotion> GetAllPromotions()
    {
        List<Promotion> promotions = context.Promotions
                                            .Include(r => r._Hotels)
                                            .ToList();

        return promotions;
    }
}
