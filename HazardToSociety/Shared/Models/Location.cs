using Microsoft.EntityFrameworkCore;
namespace HazardToSociety.Shared.Models;

[Index(nameof(Name), IsUnique = true)]
public class Location
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NoaaId { get; set; }
}