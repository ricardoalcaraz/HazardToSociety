FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ARG Environment
ENV ASPNETCORE_ENVIRONMENT=${Environment}

EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Server/HazardToSociety.Server.csproj"
RUN dotnet restore "Client/HazardToSociety.Client.csproj"
RUN dotnet restore "Shared/HazardToSociety.Shared.csproj"

FROM build AS publish
RUN dotnet publish "Server/HazardToSociety.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HazardToSociety.Server.dll"]
