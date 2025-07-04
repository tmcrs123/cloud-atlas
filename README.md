# Cloud-Atlas

## Deployment

`dotnet lambda deploy-function --config-file config.demo.json`

## Migrations
`ASPNETCORE_ENVIRONMENT=Demo dotnet ef database update --context SqlDbContext`