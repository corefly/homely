# Homely

## Docker Compose Publish

This solution uses Aspire's Docker Compose publisher. The AppHost contains a Docker Compose environment named `compose`.

Prerequisites:

- .NET SDK 10
- Aspire CLI
- Docker

Generate Docker Compose artifacts without building images:

```bash
aspire publish
```

Build images and generate filled environment files:

```bash
aspire do prepare-compose
```

Build and start the generated Docker Compose deployment:

```bash
aspire deploy
```

Generated artifacts are written to `Homely.AppHost/aspire-output/`.

Run the prepared artifacts manually:

```bash
cd Homely.AppHost/aspire-output
docker compose --env-file .env.Production up -d
```
