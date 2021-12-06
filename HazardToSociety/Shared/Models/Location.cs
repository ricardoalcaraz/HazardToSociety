using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HazardToSociety.Shared.Models;

[Index(nameof(NoaaId), IsUnique = true)]
[Index(nameof(City), nameof(Country))]
public class Location
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public string NoaaId { get; set; }
    
    [Required]
    public DateTime MinDate { get; set; }
    
    [Required]
    public DateTime MaxDate { get; set; }
    
    public string City { get; set; }
    
    public string State { get; set; }
    
    public string Country { get; set; }
}