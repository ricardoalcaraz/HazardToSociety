using System;
using System.Collections.Generic;
using System.Linq;
using HazardToSociety.Server.Services;
using HazardToSociety.Shared.Models;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HazardToSociety.Tests;

[TestClass]
public class WeatherServiceTests
{
    private readonly DateTime _today;
    private readonly DateTime _tomorrow;

    public WeatherServiceTests()
    {
        _today = DateTime.Now.Date;
        _tomorrow = _today + TimeSpan.FromDays(1);
    }

    [TestMethod]
    public void ShouldCalculateRunningAverage_SingleReturn()
    {
        var dataSet = new List<NoaaData>
        {
            new(){ Date = _today, DataType = "PRCP", Value = 1 },
            new(){ Date = _today, DataType = "PRCP", Value = 3 },
        };
        var dataPointTracker = new DataPointTracker();

        dataPointTracker.AddRange(dataSet);
        var finalList = dataPointTracker.GetValues().ToList();
        var expectedList = new List<LocationDataPoint>()
        {
            new()
            {
                DataType = "PRCP",
                Date = _today,
                Average = 2.0,
                Max = 3.0,
                Min = 1.0,
                NumRecords = 2
            }
        };
        CollectionAssert.AreEqual(expectedList, finalList, new LocationDataPointComparer());
        
        dataSet = new List<NoaaData>
        {
            new(){ Date = _today, DataType = "OTHER", Value = 4 },
            new(){ Date = _today, DataType = "OTHER", Value = 6 },
        };
        dataPointTracker.AddRange(dataSet);
        finalList = dataPointTracker.GetValues().ToList();
        expectedList = new List<LocationDataPoint>()
        {
            new()
            {
                DataType = "PRCP",
                Date = _today,
                Average = 2.0,
                Max = 3.0,
                Min = 1.0,
                NumRecords = 2
            },
            new()
            {
                DataType = "OTHER",
                Date = _today,
                Average = 5,
                Max = 6,
                Min = 4,
                NumRecords = 2
            }
        };
        CollectionAssert.AreEqual(expectedList, finalList, new LocationDataPointComparer());
    }

    [TestMethod]
    public void BiggerListShouldCalculateCorrectly()
    {
        var dataSet = new List<NoaaData>
        {
            new(){ Date = _today, DataType = "PRCP", Value = 1 },
            new(){ Date = _today, DataType = "PRCP", Value = 3 },
            new(){ Date = _today, DataType = "OTHER", Value = 4 },
            new(){ Date = _today, DataType = "OTHER", Value = 6 },
            new(){ Date = _today, DataType = "PRCP", Value = 4 },
            new(){ Date = _today, DataType = "PRCP", Value = 6 },
            new(){ Date = _tomorrow, DataType = "PRCP", Value = 6 },
            new(){ Date = _tomorrow, DataType = "PRCP", Value = 3 },
        };
        var dataPointTracker = new DataPointTracker();

        dataPointTracker.AddRange(dataSet);
        var finalList = dataPointTracker.GetValues().ToList();
        var expectedList = new List<LocationDataPoint>()
        {
            new LocationDataPoint()
            {
                DataType = "PRCP",
                Date = _today,
                Average = 14.0/4.0,
                Max = 6,
                Min = 1,
                NumRecords = 4
            },
            new LocationDataPoint()
            {
                Date = _tomorrow,
                DataType = "PRCP",
                Average = 4.5,
                Max = 6,
                Min = 3,
                NumRecords = 2
            },
            new LocationDataPoint()
            {
                DataType = "OTHER",
                Date = _today,
                Average = 5,
                Max = 6,
                Min = 4,
                NumRecords = 2
            }
        };
        CollectionAssert.AreEqual(expectedList, finalList, new LocationDataPointComparer());
        
    }
}