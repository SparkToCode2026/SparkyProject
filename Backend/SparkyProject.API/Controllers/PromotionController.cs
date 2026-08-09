using Microsoft.AspNetCore.Mvc;
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




}
