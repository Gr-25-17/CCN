# Azure Deployment Checklist

## Existing SMTP Configuration

The solution already contains an SMTP implementation (`EmailSender`) that reads these settings:

- `EmailSettings:SmtpServer`
- `EmailSettings:SmtpPort`
- `EmailSettings:SmtpUser`
- `EmailSettings:SmtpPass`
- `EmailSettings:SenderEmail`
- `EmailSettings:SenderName`

No new email provider is required.

## Production Database

Use the external SQL Server via environment variable:

```text
ConnectionStrings__DefaultConnection=Data Source=dreammaker-it.se;Initial Catalog=carrotdb;User ID=carrotadmin;Password=<REPLACE>;Trust Server Certificate=True;
```

## Azure Resources Required

1. Azure App Service (MVC application)
2. Azure Function App (.NET isolated)
3. Azure Storage Account (required by Function App)
4. Application Insights (optional but recommended)

## App Service Environment Variables

```text
ConnectionStrings__DefaultConnection=<external SQL Server connection string>
EmailSettings__SmtpServer=sandbox.smtp.mailtrap.io
EmailSettings__SmtpPort=2525
EmailSettings__SmtpUser=<smtp username>
EmailSettings__SmtpPass=<smtp password>
EmailSettings__SenderEmail=noreply@newssite.com
EmailSettings__SenderName=NewsSite Support
```

## Function App Environment Variables

```text
AzureWebJobsStorage=<storage connection string>
FUNCTIONS_WORKER_RUNTIME=dotnet-isolated
ConnectionStrings__DefaultConnection=<external SQL Server connection string>
EmailSettings__SmtpServer=sandbox.smtp.mailtrap.io
EmailSettings__SmtpPort=2525
EmailSettings__SmtpUser=<smtp username>
EmailSettings__SmtpPass=<smtp password>
EmailSettings__SenderEmail=noreply@newssite.com
EmailSettings__SenderName=NewsSite Support
```

## Local Development

Continue using SQLite:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=app.db"
}
```

## Schema Migration to SQL Server

Run when the project is ready for production:

```bash
dotnet ef database update --connection "Data Source=dreammaker-it.se;Initial Catalog=carrotdb;User ID=carrotadmin;Password=<PASSWORD>;Trust Server Certificate=True;"
```

## Data Migration Strategy

1. Choose one SQLite database as the master dataset.
2. Back up the file.
3. Run all EF migrations against SQL Server.
4. Import the master data into SQL Server.
5. Point all environments to SQL Server.

## Publish Order

1. Publish MVC application to Azure App Service.
2. Publish Azure Functions to Function App.
3. Verify timer triggers are enabled.
4. Test SMTP email delivery.
5. Validate scheduled jobs.

## Security

Do not commit production passwords to Git.
Rotate any passwords that have been shared outside secure channels.
