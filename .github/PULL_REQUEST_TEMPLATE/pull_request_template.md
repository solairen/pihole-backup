
# Pull Request

Thank you for contributing! Use this template to describe your change and make it easy for reviewers to verify and merge.

## Description
- What changed? (one or two sentences)
- Why this change is needed (motivation / context)

## Type of change
- Bugfix (non-breaking change which fixes an issue)
- New feature (non-breaking change which adds functionality)
- Breaking change (fix or feature that would cause existing behavior to change)
- Documentation only

## Related issues
- Resolves / relates to: #(issue number) — link the issue if applicable

## Checklist for reviewers and CI
- [ ] The code builds: `dotnet build src/pihole-backup/pihole-backup.csproj`
- [ ] No new warnings in build output
- [ ] Code follows established style and conventions
- [ ] Tests added or updated (if applicable)
- [ ] Documentation updated (README, TODOs, or other relevant docs)
- [ ] Changelog entry added if this affects end users

## How to test locally (Reviewer guide)
Include minimal steps so reviewers can reproduce the behavior. Provide commands and exact env values where appropriate.

1. Build the project:

```bash
dotnet build src/pihole-backup/pihole-backup.csproj
```

1. Create a local `.env` (or export env vars) with only non-sensitive example values. Omit secrets if posting publicly. Example:

    ```text
    PIHOLE_URL=https://pihole.local
    PIHOLE_PASSWORD=secret
    PROVIDER=S3
    S3_BUCKET=example-bucket
    BACKUP_CRON=""
    ```

2. Run locally:

    ```bash
    dotnet run --project src/pihole-backup/pihole-backup.csproj
    ```

3. Or build the Docker image and run (recommended for environment parity):

    ```bash
    docker build -f docker/Dockerfile -t local/pihole-backup:dev .
    docker run --rm --env-file .env local/pihole-backup:dev
    ```

Notes:

- For storage testing choose one backend: S3-compatible (AWS/Linode/Garage/Other) or Azure Blob.
  If testing Azure, ensure `AZURE_*` env vars are set. If testing S3-compatible, set `S3_ACCESS_KEY`,
  `S3_SECRET_KEY`, `S3_BUCKET`, and `S3_ENDPOINT` if using a non-AWS provider.

## CI / Expected checks

- CI should run `dotnet build` and any unit tests (none currently). Verify Docker build if a workflow exists.

## Security considerations

- Do not commit secrets. Use CI secrets or runtime-only environment variables.

- If a change affects credentials handling, document how secrets are stored and rotated.

## Backwards compatibility / Migration

- If this PR introduces breaking behavior, describe migration steps and list affected consumers.

## Release notes / Changelog

- Add a short entry under CHANGELOG.md for user-visible changes (bugfix, feature, breaking change).

## Additional context / Screenshots

- Add any logs, screenshots, or other information needed to review this change.

---

Small PR checklist for contributors:
- [ ] Branch name follows project conventions
- [ ] Commit messages are clear and atomic
- [ ] PR description explains intent and testing steps
- [ ] Assign appropriate reviewers
