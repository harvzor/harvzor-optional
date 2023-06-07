FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["tests/Harvzor.Optional.Console/Harvzor.Optional.Console.csproj", "tests/Harvzor.Optional.Console/"]
RUN dotnet restore "tests/Harvzor.Optional.Console/Harvzor.Optional.Console.csproj"
COPY . .
WORKDIR "/src/tests/Harvzor.Optional.Console"
RUN dotnet build "Harvzor.Optional.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Harvzor.Optional.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Harvzor.Optional.Console.dll"]
