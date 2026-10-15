FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Claims.slnx ./
COPY src/Claims.Domain/Claims.Domain.csproj src/Claims.Domain/
COPY src/Claims.Application/Claims.Application.csproj src/Claims.Application/
COPY src/Claims.Infrastructure/Claims.Infrastructure.csproj src/Claims.Infrastructure/
COPY src/Claims.Api/Claims.Api.csproj src/Claims.Api/
RUN dotnet restore src/Claims.Api/Claims.Api.csproj
COPY src ./src
RUN dotnet publish src/Claims.Api/Claims.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Claims.Api.dll"]
