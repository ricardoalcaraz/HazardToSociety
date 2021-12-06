using System;

namespace HazardToSociety.Shared.Models;

public record NoaaDataSet
{
    public string Uid { get; init; }
    public DateTime MinDate { get; init; }
    public DateTime MaxDate { get; init; }
    public string Name { get; init; }
    public float DataCoverage { get; init; }
    public string Id { get; init; }
}

public record NoaaLocation
{
    public string Id { get; init; }
    public float DataCoverage { get; init; }
    public string Name { get; init; }
    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }
}

public record NoaaData
{
    public DateTime Date { get; init; }
    public string DataType { get; init; }
    public string Station { get; init; }
    public string Attributes { get; init; }
    public double Value { get; init; }
}

public record NoaaDataType
{
    public DateTime MinDate { get; init; }
    public DateTime MaxDate { get; init; }
    public string Name { get; init; }
    public float DataCoverage { get; init; }
    public string Id { get; init; }
}

public record NoaaDataCategories
{
    public string Name { get; set; }
    public string Id { get; set; }
}