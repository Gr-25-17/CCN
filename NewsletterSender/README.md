# NewsletterSender Azure Function App

## Overview
A .NET 10 Azure Function App that sends personalized newsletters to subscribers weekly.

### Features
- **Weekly Timer Trigger**: Runs every Monday at 08:00 UTC (`0 0 8 * * 1`)
- **Subscriber Management**: Loads active subscribers and preferences from Azure Table Storage
- **Personalized Content**: Builds newsletters based on subscriber category preferences
- **Email Sending**: Sends emails via SendGrid with retry and rate-limiting support
- **Delivery Logging**: Tracks all delivery attempts, successes, and failures in Table Storage
- **Batching**: Processes subscribers in batches of 50 to avoid overwhelming email providers
- **Error Handling**: Logs errors and continues processing remaining subscribers

## Architecture

### Components

1. **WeeklyNewsletterTimer** (TimerTrigger Function)
   - Orchestrates the newsletter sending process
   - Loads subscribers and builds personalized content
   - Manages batching and throttling

2. **SubscriberRepository**
   - Loads active subscribers from `Subscribers` table
   - Manages subscriber data and preferences
   - Updates LastSentAt timestamps

3. **NewsletterBuilder**
   - Builds personalized HTML newsletter content
   - Fetches relevant articles based on category preferences
   - Generates unsubscribe tokens

4. **EmailSender**
   - Sends emails via SendGrid
   - Implements retry logic with exponential backoff
   - Handles rate limiting and transient errors

5. **DeliveryLogger**
   - Logs all delivery attempts to `NewsletterDeliveryLog` table
   - Tracks delivery statistics (sent, failed, bounced)
   - Provides audit trail

### Data Models

#### Subscribers Table
- **PartitionKey**: "Subscribers"
- **RowKey**: UserId
- Fields: Email, FirstName, LastName, PreferredCategoryIds (comma-separated), IsActive, Locale, LastSentAt, TemplateVersion

#### NewsletterDeliveryLog Table
- **PartitionKey**: NewsleterId
- **RowKey**: Reverse timestamp (for efficient time-based queries)
- Fields: Email, UserId, SentAt, Status (Sent/Failed/Bounced), ErrorMessage

## Configuration

Required settings in `local.settings.json` or Azure Function App settings:

```json
{
  "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...",
  "SendGridApiKey": "SG.xxxxx",
  "NewsletterFromEmail": "noreply@newssite.com",
  "NewsletterFromName": "NewsletterSender",
  "NewsletterBaseUrl": "https://newssite.com",
  "ApplicationInsightsKey": "..." // Optional
}
```

## Local Development

### Prerequisites
- Azure Storage Emulator (Azurite) running locally
- SendGrid sandbox API key or test account
- .NET 10 SDK

### Setup

1. **Start Azurite** (Azure Storage Emulator):
   ```bash
   azurite
   ```

2. **Set local secrets**:
   ```bash
   # Update local.settings.json with your SendGrid API key
   ```

3. **Run the function**:
   ```bash
   func start
   ```

4. **Create test subscribers** in the local `Subscribers` table:
   - Use Azure Storage Explorer to add test records
   - Ensure IsActive = true

## Deployment

### To Azure

1. **Create resources**:
   ```bash
   # Create resource group
   az group create --name NewsletterGroup --location eastus

   # Create storage account
   az storage account create --name newsletterstorage --resource-group NewsletterGroup --location eastus

   # Create function app
   az functionapp create --resource-group NewsletterGroup --consumption-plan-location eastus \
     --runtime dotnet-isolated --runtime-version 8.0 --functions-version 4 \
     --name NewsletterSenderFunction --storage-account newsletterstorage
   ```

2. **Store secrets in Key Vault**:
   ```bash
   az keyvault secret set --vault-name MyKeyVault --name SendGridApiKey --value "SG.xxxxx"
   ```

3. **Configure Function App settings**:
   - Link Key Vault secrets to Function App settings
   - Set SendGridApiKey, NewsletterFromEmail, etc.

4. **Deploy**:
   ```bash
   func azure functionapp publish NewsletterSenderFunction
   ```

## Next Steps

### Integration with NewsSite

1. **Seed Subscribers table** from existing subscription data:
   - Create a migration or seed script to populate subscribers from the NewsSite database

2. **Integrate IArticleService**:
   - Add reference to NewsSite.Services assembly
   - Implement NewsletterBuilder to call IArticleService methods (GetLatestByCategoryIdsAsync, GetEditorChoiceByCategoryIdsAsync)

3. **Add unsubscribe endpoint** to NewsSite:
   - Create an API endpoint to validate and process unsubscribe tokens

4. **Monitor and audit**:
   - Use Application Insights to track function performance
   - Query DeliveryLog table for reports

## Customization

### Change Schedule
Edit the TimerTrigger CRON expression in `WeeklyNewsletterTimer.cs`:
- `"0 0 8 * * 1"` = Mondays 08:00 UTC
- `"0 0 8 * * MON"` = Same (alternative syntax)
- `"0 0 8 1 * *"` = Monthly (1st of each month)

### Change Batch Size
Modify the `batchSize` variable in the timer function (default: 50)

### Change Email Provider
Replace SendGrid implementation in `EmailSender.cs` with SMTP or another provider

### Add Template Support
Store HTML templates in Blob Storage and fetch them in `NewsletterBuilder`

## Troubleshooting

### "Subscribers table not found"
- Ensure the table exists or let the function create it (enabled in code)
- Check storage connection string

### "SendGrid API key invalid"
- Verify SendGridApiKey in settings
- Check Key Vault access (if using managed identity)

### "No articles fetched"
- Ensure IArticleService integration is complete
- Check article filtering logic (IsReadyForPublish, IsArchived, etc.)

### "High failure rate"
- Check email validity in subscriber data
- Monitor SendGrid rate limits and quota
- Review error logs in Application Insights

## Monitoring

Query delivery statistics:
```csharp
var stats = await deliveryLogger.GetDeliveryStatsAsync(newsletterId);
Console.WriteLine($"Sent: {stats.Sent}, Failed: {stats.Failed}, Bounced: {stats.Bounced}");
```

## License
See parent repository
