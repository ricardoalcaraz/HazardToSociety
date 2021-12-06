using System;
using System.Collections.Generic;
using System.Linq;
using HazardToSociety.Shared.Models;

namespace HazardToSociety.Shared
{
    public static class LinqExtensions
    {
        public static IEnumerable<NoaaDataOptions> ToDataQueries(this IEnumerable<Location> locations, TimeSpan timeChunk)
        {
            return locations.SelectMany(location => location.ToDataOptionsDesc(timeChunk));
        }
        
        public static IEnumerable<NoaaDataOptions> ToDataQueries(this Location location, TimeSpan timeChunk)
        {
            return location.ToDataOptionsDesc(timeChunk);
        }
    
        public static IEnumerable<NoaaDataOptions> ToDataOptionsDesc(this Location location, TimeSpan timeChunks)
        {
            var startDate = location.MaxDate.GetClampedDate(location.MinDate, timeChunks);
            var endDate = location.MaxDate;
            while (endDate > startDate)
            {
                yield return new NoaaDataOptions()
                {
                    LocationId = location.NoaaId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Units = TempUnit.Standard
                };
                startDate = startDate.GetClampedDate(location.MinDate, timeChunks);
                endDate = endDate.GetClampedDate(location.MinDate, timeChunks);
            }
        }

        private static DateTime GetClampedDate(this DateTime date, DateTime minDate, TimeSpan timeChunks)
        {
            //data is uninitialized so return something that makes sense
            if(date == default || minDate == default)
                return DateTime.Today - timeChunks;
            
            var pastDate = date - timeChunks;
            var ticks = pastDate.Ticks;
            var startDate = Math.Max(ticks, minDate.Ticks);

            return new DateTime(startDate);
        }

        public static IEnumerable<TOptions> GetRemainingDataOptions<T, TOptions>(this NoaaPagedData<T, TOptions> pagedData)
        where TOptions : NoaaOptions
        {
            var totalCount = pagedData.Metadata.ResultSet.Count;
            var limit = pagedData.Metadata.ResultSet.Limit;
            var offset = pagedData.Metadata.ResultSet.Offset + limit;
            while (offset < totalCount)
            {
                yield return pagedData.Options with {Offset = offset};
                offset += limit;
            }
        }
    }
}