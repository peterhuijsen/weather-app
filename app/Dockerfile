FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["weather-app/weather-app.csproj", "weather-app/"]
RUN dotnet restore "weather-app/weather-app.csproj"
COPY . .
WORKDIR "/src/weather-app"
RUN dotnet build "weather-app.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "weather-app.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "weather-app.dll"]
