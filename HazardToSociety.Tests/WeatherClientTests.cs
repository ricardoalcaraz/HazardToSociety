using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HazardToSociety.Server;
using HazardToSociety.Shared;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HazardToSociety.Tests;

[TestClass]
public class WeatherClientTests
{
    [TestMethod]
    [Ignore]
    public async Task GetDataAsList()
    {
        var client = new HttpClient();
        //todo: remove this
        var token = "UVBvEBhxblJStSVXhgGwwMUWsMcTjJFr";
        client.DefaultRequestHeaders.Add("token", token);
        client.BaseAddress = new Uri("https://www.ncdc.noaa.gov/cdo-web/api/v2/");
        
        var moqLogger = Mock.Of<ILogger<WeatherClient>>();
        var weatherClient = new WeatherClient(client, moqLogger);

        var noaaDataOptions = new NoaaDataOptions()
        {
            LocationId = "CITY:US060013",
            Units = TempUnit.Standard,
            StartDate = DateTime.Today - TimeSpan.FromDays(45),
            EndDate = DateTime.Today,
            DataSetId = "GHCND"
        };
        var data = await weatherClient.GetAllData(noaaDataOptions);
        Assert.IsNotNull(data);
    }

    [TestMethod]
    public void GetDataOptionFromLocation()
    {
        var timeChunk = TimeSpan.FromDays(30);
        var locations = new Location
        {
            NoaaId = "Test",
            MinDate = DateTime.Today - timeChunk,
            MaxDate = DateTime.Today,
        };

        var expectedDataOption = new NoaaDataOptions()
        {
            LocationId = locations.NoaaId,
            StartDate = locations.MinDate,
            EndDate = locations.MaxDate,
            Units = TempUnit.Standard
        };

        var dataOption = locations.ToDataOptionsDesc(timeChunk).ToList();

        Assert.AreEqual(1, dataOption.Count);
        Assert.AreEqual(expectedDataOption, dataOption.Single());
    }
    
    [TestMethod]
    public void GetDataOptionFromLocation_ShouldReturnMultiple()
    {
        var days = 15;
        var today = DateTime.Today;
        
        var timeChunk = TimeSpan.FromDays(days);
        var locations = new Location
        {
            NoaaId = "Test",
            MinDate = DateTime.Today - TimeSpan.FromDays(days * 2),
            MaxDate = DateTime.Today,
        };

        var expectedDataOption = new List<NoaaDataOptions>
            {
                new()
                {
                    LocationId = locations.NoaaId,
                    StartDate = locations.MaxDate - TimeSpan.FromDays(days),
                    EndDate = locations.MaxDate,
                    Units = TempUnit.Standard
                },
                new()
                {
                    LocationId = locations.NoaaId,
                    StartDate = locations.MaxDate - TimeSpan.FromDays(days * 2),
                    EndDate = locations.MaxDate - TimeSpan.FromDays(days),
                    Units = TempUnit.Standard
                },
            };

        var dataOption = locations.ToDataOptionsDesc(timeChunk).ToList();

        Assert.AreEqual(2, dataOption.Count);
        CollectionAssert.AreEqual(expectedDataOption, dataOption);
    }
    
    [TestMethod]
    public void GetDataOptionFromLocation_UnevenDays_ShouldReturnMultiple()
    {
        var days = 2;
        var timeChunk = TimeSpan.FromDays(days);
        var remainderOfDays = TimeSpan.FromDays(1);
        var location = new Location
        {
            NoaaId = "Test",
            MinDate = DateTime.Today - (TimeSpan.FromDays(days) + remainderOfDays),
            MaxDate = DateTime.Today
        };

        var expectedDataOption = new List<NoaaDataOptions>
        {
            new()
            {
                LocationId = location.NoaaId,
                StartDate = location.MinDate,
                EndDate = location.MinDate + remainderOfDays,
                Units = TempUnit.Standard
            },
            new()
            {
                LocationId = location.NoaaId,
                StartDate = location.MaxDate - timeChunk,
                EndDate = location.MaxDate,
                Units = TempUnit.Standard
            }
        };

        var dataOption = location.ToDataOptionsDesc(timeChunk)
            .OrderBy(l => l.StartDate)
            .ToList();

        Assert.AreEqual(2, dataOption.Count);
        CollectionAssert.AreEqual(expectedDataOption, dataOption);
    }
    
    [TestMethod]
    public void GetDataOptionPages_PagesWithinLimit_ShouldReturnNothing()
    {
        var endDate = DateTime.Today;
        var startDate = endDate - TimeSpan.FromDays(7);

        const int limit = 1000;
        const int totalCount = limit / 2;

        var pagedData = new NoaaPagedData<NoaaData, NoaaDataOptions>
        {
            Metadata = new NoaaMetadata()
            {
                ResultSet = new NoaaResultSet()
                {
                    Count = totalCount,
                    Limit = limit,
                    Offset = 0
                }
            },
            Options = new NoaaDataOptions()
            {
                Limit = limit,
                StartDate = startDate,
                EndDate = endDate
            }
        };

        var dataOption = pagedData.GetRemainingDataOptions();
        Assert.IsFalse(dataOption.Any());
    }
    
    [TestMethod]
    public void GetDataOptionPages_UnevenDays_ShouldReturnTwoOptions()
    {
        var endDate = DateTime.Today;
        var startDate = endDate - TimeSpan.FromDays(30);
        var limit = 1000;
        var baseOptions = new NoaaDataOptions()
        {
            StartDate = startDate,
            EndDate = endDate,
            Limit = limit
        };
        var totalCount = limit * 3;

        var pagedData = new NoaaPagedData<NoaaData, NoaaDataOptions>
        {
            Metadata = new NoaaMetadata()
            {
                ResultSet = new NoaaResultSet()
                {
                    Count = totalCount,
                    Limit = limit,
                    Offset = 0
                },
            },
            Options = baseOptions
        };

        var remainingOptions = pagedData.GetRemainingDataOptions()
            .OrderBy(o => o.StartDate)
            .ToList();
        
        
        var expectedDataOptions = new List<NoaaOptions>
        {
            baseOptions with {Offset = baseOptions.Limit},
            baseOptions with {Offset = baseOptions.Limit * 2}
        };
        CollectionAssert.AreEqual(expectedDataOptions, remainingOptions);
    }
    
    [TestMethod]
    public void GetDataOptionPages_UnevenDays_ShouldReturnSingleOption()
    {
        var endDate = DateTime.Today;
        var startDate = endDate - TimeSpan.FromDays(30);
        var limit = 1000;
        var baseOptions = new NoaaDataOptions()
        {
            StartDate = startDate,
            EndDate = endDate,
            Limit = limit
        };
        var totalCount = limit * 2;

        var pagedData = new NoaaPagedData<NoaaData, NoaaDataOptions>
        {
            Metadata = new NoaaMetadata()
            {
                ResultSet = new NoaaResultSet()
                {
                    Count = totalCount,
                    Limit = limit,
                    Offset = 0
                },
            },
            Options = baseOptions
        };

        var remainingOptions = pagedData.GetRemainingDataOptions()
            .OrderBy(o => o.StartDate)
            .ToList();
        
        
        var expectedDataOptions = new List<NoaaOptions>
        {
            baseOptions with {Offset = baseOptions.Limit}
        };
        CollectionAssert.AreEqual(expectedDataOptions, remainingOptions);
    }
}