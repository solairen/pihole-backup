# Pi-hole Backup

Automated backup tool for [Pi-hole](https://pi-hole.net/) v6+. It uses the Pi-hole Teleporter API to export your configuration and uploads the backup archive to S3-compatible object storage or Azure Blob Storage on a cron schedule.

Built with .NET 10, packaged as a lightweight Docker container for `linux/amd64` and `linux/arm64`.

## Features

- Scheduled backups using standard cron expressions
- One-shot mode (run once, no schedule) when `BACKUP_CRON` is omitted
- Supports multiple storage providers:
  - **AWS S3**
  - **Linode Object Storage**
  - **Garage** (self-hosted S3-compatible)
  - **Azure Blob Storage**
- Multi-architecture Docker image (`amd64` / `arm64`)

## Quick Start

### 1. Create an environment file

Copy the example and fill in your values:

```sh
cp .env.example .env
```

### 2. Run with Docker Compose

Using the pre-built image from GitHub Container Registry:

```yaml
# compose.yml
services:
  pihole-backup:
    container_name: pihole-backup
    image: ghcr.io/solairen/pihole-backup:latest
    env_file: .env
```

```sh
docker compose up -d
```

The image is also available on Docker Hub:

```text
moleszek/pihole-backup:latest
```

## Configuration

All configuration is done through environment variables. Create a `.env` file based on [.env.example](.env.example).

### Required Variables

| Variable | Description |
| --- | --- |
| `PROVIDER` | Storage provider: `AWS`, `Linode`, `Garage`, or `Azure` |
| `PIHOLE_URL` | Full URL to your Pi-hole instance (e.g. `https://pihole.local`) |
| `PIHOLE_PASSWORD` | Pi-hole admin password |

### Optional Variables

| Variable | Default | Description |
| --- | --- | --- |
| `BACKUP_CRON` | *(none)* | Cron expression for backup schedule (e.g. `*/2 * * * *`). If omitted, runs once and exits |
| `LOG_LEVEL` | `Information` | Serilog log level: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal` |

### S3 Provider Variables (AWS, Linode, Garage)

| Variable | Default | Description |
| --- | --- | --- |
| `S3_ACCESS_KEY` | **required** | S3 access key |
| `S3_SECRET_KEY` | **required** | S3 secret key |
| `S3_BUCKET` | **required** | Target bucket name |
| `S3_REGION` | `eu-central-1` | AWS region |
| `S3_ENDPOINT` | `https://s3.<region>.amazonaws.com` | Custom endpoint. Required for Garage and Linode |

**Endpoint examples:**

- **AWS** -- leave `S3_ENDPOINT` empty; it is derived from `S3_REGION`
- **Linode** -- `https://eu-central-1.linodeobjects.com`
- **Garage** -- `http://<garage-host>:3900`

### Azure Provider Variables

| Variable | Description |
| --- | --- |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_CLIENT_ID` | Service principal client ID |
| `AZURE_CLIENT_SECRET` | Service principal client secret |
| `AZURE_STORAGE_ACCOUNT` | Storage account name |
| `AZURE_CONTAINER` | Blob container name |

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (for container builds)

### Build the Application

```sh
dotnet restore src/pihole-backup/pihole-backup.csproj
dotnet build src/pihole-backup/pihole-backup.csproj -c Release
```

### Run Locally

```sh
# Export required environment variables first (see Configuration above)
dotnet run --project src/pihole-backup/pihole-backup.csproj
```

### Build the Docker Image

```sh
docker compose -f compose.build.yml build
```

Or directly with Docker:

```sh
docker build -f docker/Dockerfile -t pihole-backup:local .
```

Pass a custom version number at build time:

```sh
docker build -f docker/Dockerfile --build-arg app_version=1.2.0 -t pihole-backup:1.2.0 .
```

## Project Structure

```text
.
├── docker/
│   ├── Dockerfile            # Production multi-stage build
│   └── Dockerfile-dev        # Development container
├── src/pihole-backup/
│   ├── Config/
│   │   └── AppConfig.cs      # Environment variable bindings
│   ├── Helpers/
│   │   └── ValidateConfig.cs # Startup configuration validation
│   ├── Models/
│   │   ├── AzureBlobOptions.cs
│   │   └── S3Options.cs
│   ├── Providers/
│   │   └── Providers.cs      # Provider enum (AWS, Linode, Garage, Azure)
│   ├── Services/
│   │   ├── AWSS3Service.cs          # S3-compatible upload
│   │   ├── AzureBlobStorageService.cs # Azure Blob upload
│   │   ├── IStorageService.cs       # Storage interface
│   │   └── PiHoleService.cs         # Pi-hole Teleporter API client
│   ├── Program.cs            # Entry point and cron scheduler
│   └── pihole-backup.csproj
├── .devcontainer/            # VS Code Dev Container configuration
├── .github/
│   └── workflows/
│       ├── push-image.yml    # Build and push on milestone close
│       └── test-docker.yml   # Build test on pull requests
├── compose.yml               # Run with pre-built image
├── compose.build.yml         # Build image locally
├── .env.example              # Environment variable template
├── global.json               # .NET SDK version pin
├── CONTRIBUTING.md
├── CODE_OF_CONDUCT.md
├── CHANGELOG.md
└── LICENSE                   # GPL-3.0
```

## How It Works

1. The application authenticates against the Pi-hole v6 API (`/api/auth`) using the configured password
2. It requests a Teleporter export (`/api/teleporter`) which returns a `.tar.gz` archive of the Pi-hole configuration
3. The archive is uploaded to the configured storage provider under the `pihole/` prefix with a timestamped filename (e.g. `pihole/teleporter-2025-01-15_14-30.tar.gz`)
4. The local file is deleted after a successful upload
5. If a cron expression is set, the process repeats on schedule; otherwise it exits after a single run

## Development

### Dev Container

The project includes a [Dev Container](.devcontainer/devcontainer.json) configuration for VS Code. Open the project in VS Code and select **Reopen in Container** to get a fully configured development environment with:

- .NET 10 SDK
- Pre-commit hooks
- Hadolint (Dockerfile linter)

### Pre-commit Hooks

The project uses [pre-commit](https://pre-commit.com/) for code quality checks. Install the hooks after cloning:

```sh
pip install pre-commit
pre-commit install
```

## CI/CD

- **Pull requests** trigger a Docker build test across `amd64` and `arm64` platforms (no push)
- **Milestone close** triggers a production build, pushes to both GitHub Container Registry and Docker Hub, and creates a GitHub Release with auto-generated release notes

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on reporting bugs, suggesting features, and submitting pull requests.

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE).
