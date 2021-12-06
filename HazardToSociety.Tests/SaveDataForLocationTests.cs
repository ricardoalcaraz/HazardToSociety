using System.Threading.Tasks;
using HazardToSociety.Server.Mediatr.Command;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HazardToSociety.Tests;

[Collection(nameof(SliceFixture))]
public class SaveDataForLocationTests
{
    private readonly SliceFixture _sliceFixture;

    public SaveDataForLocationTests(SliceFixture sliceFixture)
    {
        _sliceFixture = sliceFixture;
    }

    [Fact]
    public async Task ShouldSaveDataForLocation()
    {
        var locations = await _sliceFixture.ExecuteDbContextAsync(context => context.Locations.ToListAsync());
        
        var saveDataCommand = new SaveDataForLocationFromNoaa()
        {
            Locations = locations
        };
        
        await _sliceFixture.SendAsync(saveDataCommand);

        var data = await _sliceFixture.ExecuteDbContextAsync(context => context.Datapoints.ToListAsync());
        
        Assert.Single(data);
    }
}