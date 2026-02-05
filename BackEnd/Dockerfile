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
# Runtime stage - OPTIMIZADO
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# ---- Minimal system dependencies (solo lo necesario para .NET) ----
RUN apt-get update && \
    apt-get install -y \
        libicu-dev \
        curl \
    && rm -rf /var/lib/apt/lists/*

# ---- App ----
WORKDIR /app
COPY --from=build /app/publish .

# ---- Environment ----
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
# NOTA: OLLAMA__BaseUrl se configura como variable de entorno en Render
# NO se define aquí para mayor flexibilidad

EXPOSE 8080

# ---- Entrypoint optimizado ----
ENTRYPOINT ["dotnet", "CvEvaluator.Api.dll"]