using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HazardToSociety.Shared.Models;

namespace HazardToSociety.Server.Services;

public class DataPointTracker
{
    private readonly Dictionary<string, List<NoaaData>> _dictionary;

    public DataPointTracker()
    {
        _dictionary = new Dictionary<string, List<NoaaData>>();
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
        if (_dictionary.TryGetValue(dataPoint.DataType, out var dataList))
        {
            dataList.Add(dataPoint);
        }
        else
        {
            _dictionary.Add(dataPoint.DataType, new List<NoaaData> {dataPoint});
        }
    }

    public IEnumerable<LocationDataPoint> GetValues()
    {
        var data = new List<LocationDataPoint>();
        foreach (var (dataType, dataList) in _dictionary)
        {
            var dataToAdd = dataList.GroupBy(d => d.Date.Date)
                .Select(g =>
                {
                    var values = g.Select(a => a.Value).ToList();
                    return new LocationDataPoint
                    {
                        DataType = dataType,
                        Date = g.Key,
                        Average = values.Average(),
                        Max = values.Max(),
                        Min = values.Min(),
                        NumRecords = values.Count
                    };
                });

            data.AddRange(dataToAdd);
        }
        return data;

    }
}