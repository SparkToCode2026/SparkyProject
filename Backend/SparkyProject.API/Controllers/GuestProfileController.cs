using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;


namespace SparkyProject.API.Controllers;

// Owner: Murooj Al Shehaibi
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
public class GuestProfileController : ControllerBase
{
    private readonly AppDbContext context;

    public GuestProfileController(AppDbContext _context)
    {
        context = _context;
    }

    //Case 1: POST Create GuestProfile
    [HttpPost("CreateGuestProfile")]
    public IActionResult CreateGuestProfile(GuestProfile guestProfile)
    {
        context.GuestProfiles.Add(guestProfile);
        context.SaveChanges();

        return Ok("Guest profile created successfully.");
    }

    //Case 2: PUT/PATCH Update GuestProfile
    [HttpPut("UpdateGuestProfile")]
    public IActionResult UpdateGuestProfile(int id, GuestProfile updatedGuestProfile)
    {
        GuestProfile guestProfile = context.GuestProfiles.FirstOrDefault(gp => gp.GuestProfileId == id);
        if (guestProfile == null)
        {
            return NotFound("Guest profile not found");
        }
        else
        {
            guestProfile.GustPhone = updatedGuestProfile.GustPhone;
            guestProfile.GuestAddress = updatedGuestProfile.GuestAddress;
            guestProfile.DateOfBirth = updatedGuestProfile.DateOfBirth;
            context.SaveChanges();

            return Ok("Guest profile updated successfully");
        }
    }

    //Case 3: PUT/PATCH Update GuestProfile via related FK (UserId)
    [HttpPatch("UpdateAddressForProfile")]
    public IActionResult UpdateAddressForProfile(int id, string newAddress)
    {
        GuestProfile guestProfile = context.GuestProfiles.FirstOrDefault(gp => gp.GuestProfileId == id);
        if (guestProfile == null)
        {
            return NotFound("Guest profile not found");
        }
        else
        {
            guestProfile.GuestAddress = newAddress;
            context.SaveChanges();

            return Ok("Guest profile address updated successfully to " + newAddress);
        }
    }

    //Case 4: DELETE GuestProfile
    [HttpDelete("DeleteGuestProfile")]
    public IActionResult DeleteGuestProfile(int id)
    {
        GuestProfile guestProfile = context.GuestProfiles.FirstOrDefault(gp => gp.GuestProfileId == id);
        if (guestProfile == null)
        {
            return NotFound("Guest profile not found");
        }
        else
        {
            context.GuestProfiles.Remove(guestProfile);
            context.SaveChanges();

            return Ok("Guest profile deleted successfully");
        }
    }


}
