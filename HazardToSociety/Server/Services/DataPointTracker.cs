using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HazardToSociety.Shared.Models;

namespace HazardToSociety.Server.Services;

public class DataPointTracker : IEnumerable<Datapoint>
{
    private readonly ConcurrentDictionary<string, List<NoaaData>> _dictionary;

    public DataPointTracker()
    {
        _dictionary = new ConcurrentDictionary<string, List<NoaaData>>();
    }

    public DataPointTracker(IEnumerable<NoaaData> data) : this()
    {
        AddRange(data);
    }

    public void AddRange(IEnumerable<NoaaData> dataSet)
    {
        foreach (var dataPoint in dataSet)
        {
            AddData(dataPoint);
        }
    }

    private void AddData(NoaaData dataPoint)
    {
        _dictionary.AddOrUpdate(dataPoint.DataType, new List<NoaaData> { dataPoint }, (s, lis) =>
        {
            lis.Add(dataPoint);
            return lis;
        });
    }

    public IEnumerable<Datapoint> GetValues()
    {
        foreach (var (dataType, dataList) in _dictionary)
        {
            var dataToAdd = dataList
                .GroupBy(d => d.Date.Date)
                .Select(g =>
                {
                    var values = g.Select(a => a.Value).ToList();
                    return new Datapoint
                    {
                        DataType = dataType,
                        Date = g.Key,
                    };
                });

            foreach (var data in dataToAdd)
            {
                yield return data;
            }
        }
    }

    public void Clear() => _dictionary.Clear();
    
    public IEnumerator<Datapoint> GetEnumerator()
    {
        return GetValues().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}