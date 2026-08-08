namespace SparkyProject.API.Models;

// Owner: Khalid Al Hashemi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class RoomType
{
    public int RoomTypeId { get; set; }
    public string RoomName { get; set; }
    public double BasePrice { get; set; }
    public string Capacity { get; set; }



}
