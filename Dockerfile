FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim as build

ARG version="0.0.0"
WORKDIR /sln

COPY . .

RUN dotnet restore
RUN dotnet build /p:Version=$version -c Release --no-restore

FROM build AS test

ENTRYPOINT ["dotnet", "test", "-c", "Release", "--no-restore", "--no-build"]

FROM build AS push-nuget

ARG version 

RUN dotnet pack /p:Version=$version -c Release --no-restore --no-build -o /sln/artifacts 
ENTRYPOINT ["dotnet", "nuget", "push", "/sln/artifacts/*.nupkg", "--source", "https://api.nuget.org/v3/index.json"]
