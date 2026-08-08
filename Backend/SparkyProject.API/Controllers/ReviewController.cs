using Microsoft.AspNetCore.Mvc;
using SparkyProject.API.Data;

namespace SparkyProject.API.Controllers;

// Owner: Ibrahim Al Kindi
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
public class ReviewController : ControllerBase
{
    private readonly AppDbContext context;

    public ReviewController(AppDbContext _context)
    {
        context = _context;
    }

    // TODO: implement the 8 cases above
}
