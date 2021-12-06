using AutoMapper;
using HazardToSociety.Shared.Models;

namespace HazardToSociety.Server.Profiles;

public class NoaaProfile : Profile
{
    public NoaaProfile()
    {
        CreateMap<NoaaData, Datapoint>();
    }
}