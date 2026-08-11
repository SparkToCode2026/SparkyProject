using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;

namespace SparkyProject.API.Controllers;

// Owner: Ibrahim Al Kindi
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext context;

    public ReviewController(AppDbContext _context)
    {
        context = _context;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Review>> CreateReview(Review review)
    {
        context.Reviews.Add(review);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetReview), new { id = review.ReviewId }, review);
    }

    // 2. PUT - full update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id, Review updated)
    {
        var review = await context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        review.Rating = updated.Rating;
        review.Comment = updated.Comment;
        review.HotelId = updated.HotelId;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PATCH - second distinct update (rating only)
    [HttpPatch("{id}/rating")]
    public async Task<IActionResult> UpdateRating(int id, int newRating)
    {
        var review = await context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        review.Rating = newRating;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        context.Reviews.Remove(review);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET (list) - includes related User and Hotel
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Review>>> GetAllReviews()
    {
        return await context.Reviews
            .Include(r => r.User)
            .Include(r => r.Hotel)
            .ToListAsync();
    }

    // 6. GET (find) - by Id
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Review>> GetReview(int id)
    {
        var review = await context.Reviews
            .Include(r => r.User)
            .Include(r => r.Hotel)
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        if (review == null) return NotFound();
        return review;
    }

    // 7. GET (filter) - by rating
    [HttpGet("by-rating/{rating}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Review>>> GetByRating(int rating)
    {
        return await context.Reviews
            .Where(r => r.Rating == rating)
            .ToListAsync();
    }

    // 8. GET (sort/aggregate) - sorted by rating and average rating
    [HttpGet("sorted-by-rating")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Review>>> GetSortedByRating()
    {
        return await context.Reviews
            .OrderByDescending(r => r.Rating)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    [HttpGet("average-rating")]
    [AllowAnonymous]
    public async Task<ActionResult<double>> GetAverageRating()
    {
        if (!await context.Reviews.AnyAsync()) return 0;
        return await context.Reviews.AverageAsync(r => r.Rating);
    }
}