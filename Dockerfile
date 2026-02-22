FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала только .csproj — этот слой кэшируется, пока не изменились зависимости
COPY *.csproj .
RUN dotnet restore

# Теперь исходники
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CodifyProjectsBackend.dll"]