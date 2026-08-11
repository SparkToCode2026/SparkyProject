using Microsoft.AspNetCore.Authorization;
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
[Authorize]
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

    //Case 5: GET List Guest Profiles with User Information
    [HttpGet("GetAllGuestProfiles")]
    [AllowAnonymous]
    public IActionResult GetAllGuestProfiles()
    {
        List<GuestProfile> guestProfiles = context.GuestProfiles
                                                  .Include(gp => gp._user)
                                                  .ToList();
        return Ok(guestProfiles);
    }

    //Case 6: GET GuestProfile By Id
    [HttpGet("GetGuestProfileById")]
    [AllowAnonymous]
    public IActionResult GetGuestProfileById(int id)
    {
        GuestProfile guestProfile = context.GuestProfiles
                                           .Include(gp => gp._user)
                                           .FirstOrDefault(gp => gp.GuestProfileId == id);
        if (guestProfile == null)
        {
            return NotFound("Guest profile not found");
        }
        else
        {
            return Ok(guestProfile);
        }
    }

    //Case 7: GET Filter GuestProfiles by Address
    [HttpGet("GetGuestProfilesByAddress")]
    [AllowAnonymous]
    public IActionResult GetGuestProfilesByAddress(string address)
    {
        List<GuestProfile> guestProfiles = context.GuestProfiles
                                                  .Include(gp => gp._user)
                                                  .Where(gp => gp.GuestAddress == address)
                                                  .ToList();
        return Ok(guestProfiles);
    }

    //Case 8: GET Sort GuestProfiles by DateOfBirth
    [HttpGet("GetGuestProfilesByDateOfBirth")]
    [AllowAnonymous]
    public IActionResult GetGuestProfilesByDateOfBirth()
    {
        List<GuestProfile> guestProfiles = context.GuestProfiles
                                                  .Include(gp => gp._user)
                                                  .OrderBy(gp => gp.DateOfBirth)
                                                  .ToList();
        return Ok(guestProfiles);
    }


}
