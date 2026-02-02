# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CvEvaluator.Api/CvEvaluator.Api.csproj", "CvEvaluator.Api/"]
COPY ["CvEvaluator.Application/CvEvaluator.Application.csproj", "CvEvaluator.Application/"]
COPY ["CvEvaluator.Domain/CvEvaluator.Domain.csproj", "CvEvaluator.Domain/"]
COPY ["CvEvaluator.Infrastructure/CvEvaluator.Infrastructure.csproj", "CvEvaluator.Infrastructure/"]

RUN dotnet restore "CvEvaluator.Api/CvEvaluator.Api.csproj"

COPY . .
WORKDIR "/src/CvEvaluator.Api"
RUN dotnet publish -c Release -o /app/publish

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CvEvaluator.Api.dll"]