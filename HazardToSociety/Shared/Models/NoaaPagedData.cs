using System.Collections.Generic;
using System.Linq;

namespace HazardToSociety.Server
{
    public record NoaaPagedData<T>
    {
        public NoaaMetadata Metadata { get; set; }
        public IEnumerable<T> Results { get; set; } = Enumerable.Empty<T>();
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