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
public class UserController : ControllerBase
{
    private readonly AppDbContext context;

    public UserController(AppDbContext _context)
    {
        context = _context;
    }

    // Case 1: POST Create
    [HttpPost("CreateUser")]
    public IActionResult CreateUser(User user)
    {
        context.Users.Add(user);
        context.SaveChanges();

        return Ok("User created successfully.");
    }

    // Case 2: PUT Update
    [HttpPut("UpdateUser")]
    public IActionResult UpdateUser(int id, User updatedUser)
    {
        User user = context.Users.FirstOrDefault(u => u.UserId == id);

        if (user == null)
        {
            return NotFound("User not found");
        }
        else
        {

            user.UserName = updatedUser.UserName;
            user.UserEmail = updatedUser.UserEmail;
            user.PasswordHash = updatedUser.PasswordHash;
            user.Role = updatedUser.Role;
            context.SaveChanges();

            return Ok("User updated successfully");
        }
    }

    // Case 3: PUT/PATCH Second distinct update (status change / update via related FK)
    [HttpPatch("UpdateUserRole")]
    public IActionResult UpdateUserRole(int id, string newRole)
    {
        User user = context.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null)
        {
            return NotFound("User not found");
        }
        else
        {
            user.Role = newRole;
            context.SaveChanges();

            return Ok("User role updated successfully to " + newRole);

        }
    }

    // Case 4: DELETE User (consider soft-delete)
    [HttpDelete("DeleteUser")]
    public IActionResult DeleteUser(int id)
    {
        User user = context.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null)
        {
            return NotFound("User not found");
        }
        else
        {
            context.Users.Remove(user);
            context.SaveChanges();
            return Ok("User deleted successfully");
        }

    }

    // Case 5: GET (list) Include() a related navigation property
    [HttpGet("GetAllUsers")]
    public IActionResult GetAllUsers()
    {
        List<User> users = context.Users
                                  .Include(u => u._GuestProfile)
                                  .ToList();

        return Ok(users);
    }

    // Case 6: GET Find User by ID
    [HttpGet("GetUserById")]
    public IActionResult GetUserById(int id)
    {
        User user = context.Users
                           .Include(u => u._GuestProfile)
                           .FirstOrDefault(u => u.UserId == id);
        if (user == null)
        {
            return NotFound("User not found");
        }
        else
        {
            return Ok(user);
        }
    }


}