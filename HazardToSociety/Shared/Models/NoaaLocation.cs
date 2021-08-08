using System;

namespace HazardToSociety.Server
{
    public class NoaaLocation
    {
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public string Name { get; set; }
        public float DataCoverage { get; set; }
        public string Id { get; set; }
    }
}