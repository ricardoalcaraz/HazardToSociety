using System;
using System.Collections;
using System.Collections.Generic;

namespace HazardToSociety.Shared.Models;

public class Datapoint
{
    public long Id { get; set; }
    public int LocationId { get; set; }
    public virtual Location Location { get; set; }
    public DateTime Date { get; set; }
    public string DataType { get; set; }
    public string Station { get; set; }
    public string Attributes { get; set; }
    public double Value { get; set; }
}

public class LocationDataPointComparer : IEqualityComparer<Datapoint>
{
    public bool Equals(Datapoint x, Datapoint y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.LocationId == y.LocationId && x.Date.Equals(y.Date) && x.DataType == y.DataType;
    }

    public int GetHashCode(Datapoint obj)
    {
        return HashCode.Combine(obj.LocationId, obj.Date, obj.DataType);
    }
}