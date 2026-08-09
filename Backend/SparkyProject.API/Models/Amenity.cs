// Models/Amenity.cs - Ahmed
namespace SparkyProject.API.Models;

public class Amenity
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public Hotel? Hotel { get; set; }
}