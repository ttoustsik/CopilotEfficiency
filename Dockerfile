FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["app/CopilotEfficiency.sln", "./"]
COPY ["app/CopilotEfficiency.Console/CopilotEfficiency.Console.csproj", "CopilotEfficiency.Console/"]
COPY ["app/CopilotEfficiency.Business/CopilotEfficiency.Business.csproj", "CopilotEfficiency.Business/"]
COPY ["app/CopilotEfficiency.Core/CopilotEfficiency.Core.csproj", "CopilotEfficiency.Core/"]
COPY ["app/CopilotEfficiency.Infrastructure/CopilotEfficiency.Infrastructure.csproj", "CopilotEfficiency.Infrastructure/"]

RUN dotnet restore
COPY . ./

RUN dotnet publish "CopilotEfficiency.Console/CopilotEfficiency.Console.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CopilotEfficiency.Console.dll"]