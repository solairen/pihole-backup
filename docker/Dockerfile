# Build dotnet application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

ARG app_version=1.0.0
ENV AZURE_TELEMETRY_DISABLED=true

WORKDIR /app

COPY src/pihole-backup/*.csproj ./

RUN dotnet restore

COPY src/pihole-backup/. ./

RUN dotnet publish -c release -p:Version=${app_version} -o out \
    && chown -R app:app /app/out

# Final image
FROM mcr.microsoft.com/dotnet/runtime:10.0.5-azurelinux3.0

ENV AZURE_TELEMETRY_DISABLED=true

LABEL org.opencontainers.image.source="https://github.com/solairen/pihole-backup"
LABEL org.opencontainers.image.description="Create PiHole backup and send it to S3 Compatible object or Azure Blob Storage"
LABEL org.opencontainers.image.authors="Michał Oleszek michal@michaloleszek.com"
LABEL org.opencontainers.image.licenses=GPL
LABEL release=production

USER app

WORKDIR /app

COPY --from=builder /app/out .

ENTRYPOINT ["dotnet", "pihole-backup.dll"]
