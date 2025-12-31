FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the csproj to leverage layer caching for nuget restore
COPY ["Home.Automation.Api.csproj", "./"]
RUN dotnet restore "Home.Automation.Api.csproj"

# Copy remaining source files and publish
COPY . .
RUN dotnet publish "Home.Automation.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 5672

# Copy published output from build stage
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Home.Automation.Api.dll"]