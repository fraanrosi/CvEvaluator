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
FROM ubuntu:22.04 AS final

# ---- System dependencies ----
RUN apt-get update && \
    apt-get install -y \
        curl \
        ca-certificates \
        zstd \
        bash \
        libicu-dev && \
    rm -rf /var/lib/apt/lists/*

# ---- Install Ollama ----
RUN curl -fsSL https://ollama.com/install.sh | sh

# ---- Install .NET runtime (ASP.NET Core) ----
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- \
    --channel 8.0 \
    --runtime aspnetcore \
    --install-dir /usr/share/dotnet

ENV PATH="/usr/share/dotnet:${PATH}"

# ---- App ----
WORKDIR /app
COPY --from=build /app/publish .

# ---- Startup script ----
COPY start.sh /start.sh
RUN chmod +x /start.sh

# ---- Environment ----
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV OLLAMA_BASE_URL=http://localhost:11434

EXPOSE 8080
EXPOSE 11434

# ---- Entrypoint ----
ENTRYPOINT ["/start.sh"]
