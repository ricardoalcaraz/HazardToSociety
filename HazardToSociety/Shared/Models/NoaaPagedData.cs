using System.Collections.Generic;
using System.Linq;

namespace HazardToSociety.Shared.Models
{
    public record NoaaPagedData<T, TOption> where TOption : NoaaOptions
    {
        public NoaaMetadata Metadata { get; init; }
        public IEnumerable<T> Results { get; init; } = Enumerable.Empty<T>();
        public TOption Options { get; init; }

        public IEnumerable<TOption> GetRemainingQueries()
        {
            var totalCount = Metadata.ResultSet.Count;
            var limit = Metadata.ResultSet.Limit;
            var offset = Metadata.ResultSet.Offset + limit;
            while (offset < totalCount)
            {
                yield return Options with {Offset = offset};
                offset += limit;
            }
        }
    }

    public record NoaaMetadata
    {
        public NoaaResultSet ResultSet { get; set; }
    }

    public record NoaaResultSet
    {
        public int Offset { get; set; }
        public int Count { get; set; }
        public int Limit { get; set; }
    }
}