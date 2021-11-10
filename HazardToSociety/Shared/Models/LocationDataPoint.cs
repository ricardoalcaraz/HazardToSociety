using System;

namespace HazardToSociety.Shared.Models;

public class LocationDataPoint
{
    public long Id { get; set; }
    public int LocationId { get; set; }
    public virtual Location Location { get; set; }
    public float Average { get; set; }
    public float Max { get; set; }
    public float Min { get; set; }
    public DateTime Date { get; set; }
}