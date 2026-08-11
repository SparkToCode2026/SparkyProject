using System.ComponentModel.DataAnnotations;

namespace SparkyProject.API.Models;

// Owner: Aisha Mubarak ALHashmi
public class Staff
{
    [Key]
    public int StaffId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Position { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime HireDate { get; set; } = DateTime.Now;

    public int? UserId { get; set; }

    public User? User { get; set; }
}