FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim as build

ARG version="0.0.0"
WORKDIR /sln

COPY . .

RUN dotnet restore
RUN dotnet build /p:Version=$version -c Release --no-restore

FROM build AS test

ENTRYPOINT ["dotnet", "test", "-c", "Release", "--no-restore", "--no-build", "/p:CollectCoverage=true", "/p:CoverletOutputFormat=opencover"]

FROM build as codecov-uploader

RUN dotnet test -c Release --no-restore --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:Exclude="[*Tests]*"

RUN curl -Os https://uploader.codecov.io/latest/linux/codecov

RUN chmod +x codecov

ENTRYPOINT ["./codecov", "-t"]

FROM build AS push-nuget

ARG version

RUN dotnet pack /p:Version=$version -c Release --no-restore --no-build -o /sln/artifacts 
ENTRYPOINT ["dotnet", "nuget", "push", "/sln/artifacts/*.nupkg", "--source", "https://api.nuget.org/v3/index.json"]
