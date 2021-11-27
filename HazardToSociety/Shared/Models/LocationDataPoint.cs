using System;
using System.Collections;
using System.Collections.Generic;

namespace HazardToSociety.Shared.Models;

public class LocationDataPoint
{
    public long Id { get; set; }
    public int LocationId { get; set; }
    public virtual Location Location { get; set; }
    public double Average { get; set; }
    public double Max { get; set; }
    public double Min { get; set; }
    public DateTime Date { get; set; }
    public int NumRecords { get; set; }
    public string DataType { get; set; }
}

public class LocationDataPointComparer : IComparer<LocationDataPoint>, IComparer
{
    public int Compare(LocationDataPoint x, LocationDataPoint y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        var averageComparison = x.Average.CompareTo(y.Average);
        if (averageComparison != 0) return averageComparison;
        var maxComparison = x.Max.CompareTo(y.Max);
        if (maxComparison != 0) return maxComparison;
        var minComparison = x.Min.CompareTo(y.Min);
        if (minComparison != 0) return minComparison;
        var dateComparison = x.Date.CompareTo(y.Date);
        if (dateComparison != 0) return dateComparison;
        var numRecordsComparison = x.NumRecords.CompareTo(y.NumRecords);
        if (numRecordsComparison != 0) return numRecordsComparison;
        return string.Compare(x.DataType, y.DataType, StringComparison.Ordinal);
    }

    public int Compare(object? x, object? y)
        => Compare(x as LocationDataPoint, y as LocationDataPoint);
}