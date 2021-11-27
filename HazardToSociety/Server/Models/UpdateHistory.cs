using System;
using System.ComponentModel.DataAnnotations;

namespace HazardToSociety.Server.Models;

public class UpdateHistory
{
    public UpdateHistory()
    {
    }
    
    public UpdateHistory(UpdateType updateType)
    {
        UpdateType = updateType;
        RequiresUpdates = true;
    }
    
    [Key]
    public UpdateType UpdateType { get; set; }
    public DateTime? DateUpdated { get; set; }
    public string DataUpdated { get; set; }
    public bool RequiresUpdates { get; set; }
}

public enum UpdateType
{
    Invalid = 0,
    InitialSeeding,
    LocationSeeding,
}