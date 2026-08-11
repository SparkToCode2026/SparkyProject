using System.ComponentModel.DataAnnotations.Schema;

namespace SparkyProject.API.Models;

// Owner: Aisha Mubarak ALHashmi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class Staff
{
    public int StaffId { get; set; }

[ForeignKey("_user")]
    public int UserId { get; set; }
    public User _user { get; set; } = null!;

    [ForeignKey("_hotel")]
    public int HotelId { get; set; }
    public Hotel _hotel { get; set; } = null!;

    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
}