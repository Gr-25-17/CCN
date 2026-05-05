# 📋 Local Testing Guide - Newsletter System

This guide walks you through setting up and testing the entire newsletter system locally before deploying to Azure.

## Prerequisites

Ensure you have the following installed:
- ✅ .NET 10 SDK
- ✅ SQL Server (LocalDB or Express)
- ✅ Azure Storage Emulator (Azurite)
- ✅ Visual Studio 2026 Community
- ✅ SendGrid API Key (free sandbox account at https://sendgrid.com)
- ✅ PowerShell 7+

---

## Part 1: Set Up Local Database

### Step 1.1: Create Local Database
Open PowerShell as Administrator and run:

```powershell
# Navigate to solution root
cd C:\Users\Student\source\repos\NewCCN

# Create database using Entity Framework Core migrations
dotnet ef database update --project NewsSite
```

This will:
- Create the `NewsSiteDb` database (default LocalDB)
- Apply all pending migrations
- Create tables including the new `Newsletters` table

### Step 1.2: Verify Database
You can verify the database was created:

```powershell
# List all LocalDB instances
sqllocaldb info

# Connect to LocalDB and verify tables
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%Newsletter%';"
```

---

## Part 2: Set Up Azure Storage Emulator (Azurite)

### Step 2.1: Install Azurite
```powershell
# Install globally via npm
npm install -g azurite

# Or use Visual Studio's built-in Azurite
# (Visual Studio 2026 includes it)
```

### Step 2.2: Start Azurite
Open a new PowerShell terminal and run:

```powershell
# Start Azurite (listens on http://127.0.0.1:10000)
azurite

# Or if using npm package globally:
azurite --silent --location c:\azurite
```

Keep this terminal open while running tests.

### Step 2.3: Verify Azurite Connection
In another terminal, test the connection:

```powershell
# Test using Azure Storage Explorer or command line
# The connection string for local development is:
$env:AzureWebJobsStorage="UseDevelopmentStorage=true"

Write-Host "Azurite should be running on http://127.0.0.1:10000"
```

---

## Part 3: Configure Local Settings

### Step 3.1: Update NewsletterSender/local.settings.json

Edit `NewsletterSender/local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SendGridApiKey": "SG.YOUR_SENDGRID_API_KEY_HERE",
    "NewsletterApiBaseUrl": "https://localhost:5001",
    "NewsletterBaseUrl": "https://localhost:5001",
    "NewsSiteDbConnection": "Server=(localdb)\\mssqllocaldb;Database=NewsSiteDb;Integrated Security=true;",
    "LOGGING__LOGLEVEL__DEFAULT": "Information"
  }
}
```

**⚠️ IMPORTANT:**
- Replace `YOUR_SENDGRID_API_KEY_HERE` with your actual SendGrid API key
- Ensure the database connection string matches your LocalDB setup
- The URLs should point to your local development environment

### Step 3.2: Get SendGrid API Key

1. Go to https://sendgrid.com/free
2. Sign up for a free account (includes 100 emails/day)
3. Navigate to Settings → API Keys
4. Create a new API key with "Mail Send" permission
5. Copy the key and paste it in `local.settings.json`

---

## Part 4: Seed Test Data

### Step 4.1: Create Admin User

Run the application and:
1. Navigate to `/Identity/Account/Register`
2. Create an account with email: `admin@newssite.local`
3. In SQL Server, update the user to have "Admin" role:

```sql
-- Connect to NewsSiteDb
-- Assign Admin role to the user
DECLARE @UserId NVARCHAR(MAX)
SELECT @UserId = Id FROM AspNetUsers WHERE Email = 'admin@newssite.local'

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT @UserId, Id FROM AspNetRoles WHERE Name = 'Admin'
```

### Step 4.2: Create Newsletter Subscribers

Run this SQL to add test subscribers:

```sql
-- Create newsletter preferences for test users
INSERT INTO NewsletterPreferences (UserId, ReceiveNewsletter, Frequency, SelectedCategoryIds, UpdatedAt)
SELECT Id, 1, 'Weekly', '1,2,3', GETUTCDATE() 
FROM AspNetUsers 
WHERE Email IN ('admin@newssite.local', 'test@newssite.local')
AND NOT EXISTS (SELECT 1 FROM NewsletterPreferences WHERE UserId = AspNetUsers.Id)
```

### Step 4.3: Create Test Articles

Use the Articles admin interface to create test articles with:
- Title: "Test Article 1", "Test Article 2", etc.
- Category: Select different categories
- Summary: "This is a test article for newsletter testing"
- Author: Your admin user
- Mark as "Ready for Publish"

---

## Part 5: Start Services

### Step 5.1: Terminal Setup

Open 4 PowerShell terminals in the solution root:

**Terminal 1: Azurite (already running from Part 2)**
```powershell
azurite
# Keep this running
```

**Terminal 2: NewsSite Application**
```powershell
cd NewsSite
dotnet run --launch-profile https
# Should start on https://localhost:5001 or https://localhost:7xxx
```

**Terminal 3: NewsletterSender Function App**
```powershell
cd NewsletterSender
func start
# Should start on http://localhost:7071
```

**Terminal 4: (Reserve for testing commands)**
```powershell
# For running manual test scripts
cd C:\Users\Student\source\repos\NewCCN
```

---

## Part 6: Manual Testing Workflow

### Test 1: Admin Creates Newsletter

1. **Start NewsSite**: Navigate to https://localhost:5001
2. **Login as Admin**: Use credentials from Part 4.1
3. **Create Newsletter**: 
   - Go to Admin → Newsletters → Create
   - Title: "Weekly Test Newsletter"
   - Select categories
   - Save as Draft
4. **Verify in Database**:

```sql
SELECT Id, Title, Status, CreatedAt FROM Newsletters WHERE Title LIKE 'Weekly%'
```

### Test 2: Preview Newsletter

1. From newsletter list, click "Preview"
2. Verify it shows:
   - Newsletter title and header
   - Sample articles from selected categories
   - Estimated recipient count
   - Unsubscribe link

### Test 3: Schedule Newsletter

1. Edit the newsletter
2. Click "Schedule" and set time to 2 minutes in future
3. Verify `ScheduledSendTime` is set in database

```sql
SELECT Id, Title, Status, ScheduledSendTime FROM Newsletters ORDER BY CreatedAt DESC
```

### Test 4: Test Subscriber Seeding

The `SubscriberSeederFunction` runs on Sunday 02:00 UTC locally. To test immediately:

1. **Manual trigger** (via Azure Storage Explorer):
   - Open Table Storage (Azurite connection)
   - Create "Subscribers" table manually
   - Verify it gets populated after seeding

2. **Or via PowerShell**:

```powershell
# Create test subscriber table
$storageContext = New-AzureStorageContext -Local
$table = Get-AzureStorageTable -Name "Subscribers" -Context $storageContext -ErrorAction SilentlyContinue

if ($null -eq $table) {
    $table = New-AzureStorageTable -Name "Subscribers" -Context $storageContext
    Write-Host "Created Subscribers table"
}

# Verify it exists
Get-AzureStorageTable -Context $storageContext | Where-Object { $_.Name -eq "Subscribers" }
```

### Test 5: Send Newsletter Immediately

1. Edit the test newsletter created in Test 1
2. Click "Send Now"
3. Newsletter status should change to "Scheduled" with `ScheduledSendTime` = now

### Test 6: Monitor Function App Execution

The `WeeklyNewsletterTimer` function runs Monday 08:00 UTC. To trigger it manually:

**Option A: Wait for next Monday 08:00 UTC**
- (Not practical for testing)

**Option B: Modify timer for testing**
- Edit `NewsletterSender/Functions/WeeklyNewsletterTimer.cs`
- Change `[TimerTrigger("0 0 8 * * 1")]` to test schedule, e.g.:
  ```csharp
  [TimerTrigger("*/30 * * * * *")] // Every 30 seconds
  ```
- This is temporary for testing only!

**Option C: Use HTTP trigger for manual testing**
- Create a new function `NewsletterSender/Functions/ManualSendFunction.cs`
- See Part 7 for implementation

### Test 7: Verify Delivery Logs

Check that emails were logged in Azure Table Storage:

```powershell
# List delivery logs (if you create the manual trigger function)
# Or check the Function App logs:

# Navigate to NewsletterSender project
cd NewsletterSender

# View function app logs
func azure functionapp fetch-app-settings <app-name>
```

---

## Part 7: Create Manual Test Function (OPTIONAL)

Create `NewsletterSender/Functions/ManualSendFunction.cs` for manual testing:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NewsletterSender.Services;

namespace NewsletterSender.Functions;

public class ManualSendFunction
{
    private readonly SubscriberRepository _subscriberRepository;
    private readonly NewsletterBuilder _newsletterBuilder;
    private readonly EmailSender _emailSender;
    private readonly DeliveryLogger _deliveryLogger;

    public ManualSendFunction(
        SubscriberRepository subscriberRepository,
        NewsletterBuilder newsletterBuilder,
        EmailSender emailSender,
        DeliveryLogger deliveryLogger)
    {
        _subscriberRepository = subscriberRepository;
        _newsletterBuilder = newsletterBuilder;
        _emailSender = emailSender;
        _deliveryLogger = deliveryLogger;
    }

    [Function("ManualSendNewsletter")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "newsletter/send-manual")] HttpRequestData req,
        ILogger log)
    {
        log.LogInformation("Manual newsletter send triggered");

        try
        {
            var subscribers = await _subscriberRepository.GetActiveSubscribersAsync();
            log.LogInformation($"Found {subscribers.Count} active subscribers");

            if (!subscribers.Any())
            {
                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "No active subscribers" });
                return response;
            }

            var newsletterId = Guid.NewGuid().ToString();
            var sentAt = DateTime.UtcNow;
            int successCount = 0, failureCount = 0;

            foreach (var subscriber in subscribers.Take(5)) // Test with first 5 subscribers
            {
                try
                {
                    var content = await _newsletterBuilder.BuildPersonalizedNewsletterAsync(subscriber);
                    await _emailSender.SendNewsletterAsync(
                        subscriber.Email,
                        subscriber.FirstName,
                        content.HtmlBody,
                        content.Subject);

                    await _deliveryLogger.LogDeliveryAsync(
                        newsletterId, subscriber.Email, subscriber.UserId, sentAt, "Sent", null);
                    successCount++;
                }
                catch (Exception ex)
                {
                    log.LogError($"Failed to send to {subscriber.Email}: {ex.Message}");
                    await _deliveryLogger.LogDeliveryAsync(
                        newsletterId, subscriber.Email, subscriber.UserId, sentAt, "Failed", ex.Message);
                    failureCount++;
                }
            }

            var okResponse = req.CreateResponse(System.Net.HttpStatusCode.Ok);
            await okResponse.WriteAsJsonAsync(new { 
                success = true, 
                totalSubscribers = subscribers.Count,
                testSent = successCount,
                testFailed = failureCount,
                newsletterId = newsletterId
            });
            return okResponse;
        }
        catch (Exception ex)
        {
            log.LogError($"Error in manual send: {ex.Message}");
            var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }
}
```

Test it:
```powershell
# Once function app is running on http://localhost:7071
curl -X POST http://localhost:7071/api/newsletter/send-manual `
  -H "Content-Type: application/json" `
  -H "x-functions-key: admin"
```

---

## Part 8: Troubleshooting

### Issue: Database Connection Fails
```
Error: Login failed for user 'NT AUTHORITY\YOUR_COMPUTER'
```

**Solution:**
```sql
-- Enable mixed authentication in LocalDB
-- Use integrated security or create SQL user
sqlcmd -S "(localdb)\mssqllocaldb" -E
> CREATE LOGIN testuser WITH PASSWORD='Test@1234'
> CREATE USER testuser FOR LOGIN testuser
```

### Issue: Azurite Connection Failed
```
Error: Cannot connect to http://127.0.0.1:10000
```

**Solution:**
- Ensure Azurite is running in Terminal 1
- Check that port 10000 is not blocked
- Restart Azurite: `azurite --location c:\azurite`

### Issue: SendGrid Email Fails
```
Error: Invalid SendGrid API Key
```

**Solution:**
- Verify key in `local.settings.json`
- Check key has "Mail Send" permission
- Test key on SendGrid dashboard

### Issue: Newsletter Function Not Triggering
```
Error: Timer trigger not executing
```

**Solution:**
- Check local time matches cron schedule
- For testing, modify the TimerTrigger (see Part 6, Test 6)
- View function logs: `func azure functionapp tail <app-name>`

### Issue: Port Already in Use
```
Error: Address already in use: 5001
```

**Solution:**
```powershell
# Find process using port 5001
Get-NetTCPConnection -LocalPort 5001

# Kill the process
Stop-Process -Id <PID> -Force
```

---

## Part 9: Testing Checklist

Complete this checklist to verify all functionality:

- [ ] **Database**: Database created, migrations applied
- [ ] **Azurite**: Storage emulator running and accessible
- [ ] **NewsSite**: Application runs on https://localhost:5001
- [ ] **Admin Login**: Can login with admin account
- [ ] **Create Newsletter**: Admin can create draft newsletter
- [ ] **Edit Newsletter**: Can edit title, description, categories
- [ ] **Preview**: Newsletter preview displays articles and unsubscribe link
- [ ] **Schedule**: Can schedule newsletter for future send
- [ ] **Subscriber Seeding**: Subscribers table populated from SQL
- [ ] **Manual Send**: Manual send function works (if implemented)
- [ ] **Delivery Logging**: Emails logged to delivery log table
- [ ] **Error Handling**: Failures logged without stopping batch processing

---

## Part 10: Next Steps - Ready for Azure Deployment

Once all tests pass:

1. ✅ Code is tested and working
2. ✅ All functionality verified locally
3. ✅ Ready to deploy to Azure
4. ⏭️ **See: [AZURE_DEPLOYMENT_GUIDE.md](./AZURE_DEPLOYMENT_GUIDE.md)**

For manual Azure deployment steps, follow the guide to:
- Create Azure Function App
- Set up Storage Account and Key Vault
- Configure managed identity
- Deploy and verify

---

## Quick Reference: Service URLs

| Service | URL | Purpose |
|---------|-----|---------|
| NewsSite Web App | https://localhost:5001 | Main web application |
| Function App | http://localhost:7071 | Newsletter functions |
| Azurite (Blob/Table) | http://127.0.0.1:10000 | Local storage emulator |
| Database | (localdb)\mssqllocaldb | Local SQL Server |

## Quick Reference: Important Files

| File | Purpose |
|------|---------|
| `NewsletterSender/local.settings.json` | Function app configuration |
| `NewsSite/Models/Entities/Newsletter.cs` | Newsletter database model |
| `NewsSite/Controllers/NewsletterAdminController.cs` | Admin newsletter management |
| `NewsletterSender/Functions/WeeklyNewsletterTimer.cs` | Main newsletter send function |
| `NewsSite/Data/ApplicationDbContext.cs` | Database context |
