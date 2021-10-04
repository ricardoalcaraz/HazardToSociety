using System;
using System.ComponentModel.DataAnnotations;

namespace HazardToSociety.Server.Models;

public class UpdateHistory
{
    [Key]
    public string Name { get; set; }
    public DateTime? DateUpdated { get; set; }
    public string DataUpdated { get; set; }
    public bool RequiresUpdates { get; set; }
}