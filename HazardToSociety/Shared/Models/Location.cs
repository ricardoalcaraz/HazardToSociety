using System;
using Microsoft.EntityFrameworkCore;
namespace HazardToSociety.Shared.Models;

[Index(nameof(Name), IsUnique = true)]
public class Location
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NoaaId { get; set; }
    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
}