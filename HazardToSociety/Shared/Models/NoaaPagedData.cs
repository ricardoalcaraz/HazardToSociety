using System.Collections.Generic;

namespace HazardToSociety.Server
{
    public class NoaaPagedData<T>
    {
        public NoaaMetadata Metadata { get; set; }
        public IEnumerable<T> Results { get; set; }
    }

    public class NoaaMetadata
    {
        public NoaaResultSet ResultSet { get; set; }
    }

    public class NoaaResultSet
    {
        public int Offset { get; set; }
        public int Count { get; set; }
        public int Limit { get; set; }
    }
}