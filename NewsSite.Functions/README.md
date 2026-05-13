# NewsSite.Functions

## Local development

1. Copy `local.settings.json.example` to `local.settings.json`.
2. Fill in the SQL Server connection string and SMTP settings.
3. Start Azurite or use a valid Azure Storage connection string.
4. Run:

```bash
dotnet build
func start
```

## Scheduled functions

- `ArchiveArticlesFunction` - Runs daily at 00:00 UTC.
- `SubscriptionReminderFunction` - Runs daily at 00:00 UTC.
- `WeeklyNewsletterFunction` - Runs every Monday at 08:00 UTC.

## Azure deployment settings

Required application settings:

- `AzureWebJobsStorage`
- `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`
- `ConnectionStrings__DefaultConnection`
- `EmailSettings__SmtpServer`
- `EmailSettings__SmtpPort`
- `EmailSettings__SmtpUser`
- `EmailSettings__SmtpPass`
- `EmailSettings__SenderEmail`
- `EmailSettings__SenderName`
