using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkyProject.API.Models;

// Owner: Murooj Al Shehaibi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class GuestProfile
{
    [Key]
    public int GuestProfileId { get; set; }
    public string GustPhone { get; set; }
    public string GuestAddress { get; set; }
    public DateTime DateOfBirth { get; set; }

    // Foreign key to User 1 : 1 relationship
    [ForeignKey("_user")]
    public int UserId { get; set; }
    public User _user { get; set; }

}
