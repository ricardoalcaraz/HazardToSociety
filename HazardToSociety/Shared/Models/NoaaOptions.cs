using System;

namespace HazardToSociety.Shared.Models
{
    public record NoaaOptions
    {
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public SortField? SortField { get; init; }
        public SortOrder? SortOrder { get; init; }
        public int Limit { get; init; } = 1000;
        public int Offset { get; init; }
    }
    
    public record NoaaLocationOptions : NoaaOptions
    {
        public string DataSetId { get; init; }
        public string LocationCategoryId { get; init; }
        public string DataCategoryId { get; init; }
    }

    public record NoaaDatasetOptions : NoaaOptions
    {
        public string DataTypeId { get; init; }
        public string LocationId { get; init; }
        public string StationId { get; init; }
    }

    public record NoaaDataOptions : NoaaOptions
    {
        public string DataSetId { get; init; }
        public string DataTypeId { get; init; }
        public string LocationId { get; init; }
        public string StationId { get; init; }
        public TempUnit? Units { get; init; }
    }

    public record NoaaDataTypeOptions : NoaaOptions
    {
        public string DataSetId { get; init; }
        public string LocationId { get; init; }
        public string StationId { get; init; }
        public string DataCategoryId { get; init; }
    }

    public record NoaaDataCategoryOptions : NoaaOptions
    {
        public string DataSetId { get; init; }
        public string LocationId { get; init; }
        public string StationId { get; init; }
    }
    
    public enum TempUnit
    {
        Standard,
        Metric
    }

    public enum SortField
    {
        Id,
        Name,
        MinDate,
        MaxDate,
        DataCoverage
    }

    public enum SortOrder
    {
        Desc, 
        Asc
    }
}