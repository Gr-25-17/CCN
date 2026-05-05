# 📐 Newsletter System - Architecture & Data Flow

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        ADMIN & USER INTERFACES                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  NewsSite Web Application (.NET 10 ASP.NET Core Razor Pages)    │   │
│  │                                                                   │   │
│  │  ┌─────────────────────┐      ┌──────────────────────────────┐  │   │
│  │  │  Admin Dashboard    │      │  Subscriber Preferences      │  │   │
│  │  ├─────────────────────┤      ├──────────────────────────────┤  │   │
│  │  │ • Create Newsletter │      │ • Subscribe/Unsubscribe      │  │   │
│  │  │ • Edit Newsletter   │      │ • Select Categories          │  │   │
│  │  │ • Preview           │      │ • Choose Frequency           │  │   │
│  │  │ • Schedule/Send     │      │ • Manage Email Address       │  │   │
│  │  │ • View Statistics   │      │ • One-Click Unsubscribe      │  │   │
│  │  └─────────────────────┘      └──────────────────────────────┘  │   │
│  │                                                                   │   │
│  │  Controllers:                                                     │   │
│  │  • NewsletterAdminController (Admin CRUD)                        │   │
│  │  • NewsletterApiController (Preferences & Unsubscribe)          │   │
│  │  • ArticlesApiController (Article Fetching)                     │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                           │                                              │
└───────────────────────────┼──────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
        ▼                   ▼                   ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│  SQL Database    │ │  API Endpoints   │ │  Authentication  │
│  (LocalDB/Azure) │ │  (REST/HTTP)     │ │  (Identity)      │
├──────────────────┤ ├──────────────────┤ ├──────────────────┤
│ Tables:          │ │ GET /latest      │ │ Users            │
│ • Newsletters    │ │ GET /categories  │ │ Roles            │
│ • NewsletterPref │ │ GET /editor-pick │ │ Sessions         │
│ • Articles       │ │ POST /unsub      │ │ Claims           │
│ • Categories     │ │ GET /prefs       │ │                  │
│ • AspNetUsers    │ │ POST /prefs      │ │                  │
└──────────────────┘ └──────────────────┘ └──────────────────┘
        │                   ▲
        └───────────────────┘
                   │
        ┌──────────┴──────────┐
        │                     │
        ▼                     ▼
┌───────────────────────────────────────────────────────────────────────┐
│                   NEWSLETTER SENDING (FUNCTION APP)                   │
├───────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │         WeeklyNewsletterTimer (TimerTrigger)                   │ │
│  │         Runs: Monday 08:00 UTC (cron: 0 0 8 * * 1)            │ │
│  ├─────────────────────────────────────────────────────────────────┤ │
│  │                                                                 │ │
│  │  1. Load active subscribers from Table Storage                 │ │
│  │  2. Check for scheduled newsletters                            │ │
│  │  3. For each subscriber (batch of 50):                        │ │
│  │     a. Fetch articles via ArticleServiceClient                 │ │
│  │     b. Build personalized HTML via NewsletterBuilder           │ │
│  │     c. Send email via EmailSender (SendGrid)                  │ │
│  │     d. Log delivery via DeliveryLogger                         │ │
│  │     e. Update LastSentAt timestamp                             │ │
│  │     f. Continue on error (logged)                              │ │
│  │                                                                 │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│         │              │              │              │               │
│         ▼              ▼              ▼              ▼               │
│    ┌─────────┐   ┌──────────┐   ┌─────────┐   ┌──────────┐         │
│    │ Article │   │Newsletter│   │ Email   │   │Delivery  │         │
│    │ Service │   │ Builder  │   │ Sender  │   │ Logger   │         │
│    │ Client  │   │          │   │ (SendGd)│   │(Table)   │         │
│    └─────────┘   └──────────┘   └─────────┘   └──────────┘         │
│         │              │              │              │               │
│         └──────────────┴──────────────┴──────────────┘               │
│                        │                                              │
└────────────────────────┼──────────────────────────────────────────────┘
                         │
        ┌────────────────┴────────────────┐
        │                                 │
        ▼                                 ▼
┌──────────────────────┐        ┌──────────────────────────┐
│  Subscriber Seeder   │        │  SubscriberRepository    │
│  (TimerTrigger)      │        │  (Table Storage Access)  │
├──────────────────────┤        ├──────────────────────────┤
│ Runs: Sun 02:00 UTC  │        │ • Load subscribers       │
├──────────────────────┤        │ • Update last sent       │
│ 1. Query SQL DB      │        │ • Upsert to Table        │
│    (newsletter prefs)│        │ • Create if not exists   │
│ 2. Join with Users   │        │                          │
│ 3. Upsert to Table   │        │ Table: "Subscribers"     │
│    Storage           │        │ Partition: UserId        │
│ 4. Log results       │        │ Row: UserId              │
└──────────────────────┘        └──────────────────────────┘
         │                               │
         └───────────────┬───────────────┘
                         │
        ┌────────────────┴────────────────┐
        │                                 │
        ▼                                 ▼
┌──────────────────────────────────────────────────────────┐
│                 AZURE CLOUD SERVICES                     │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │         AZURE TABLE STORAGE (NoSQL)               │ │
│  ├────────────────────────────────────────────────────┤ │
│  │                                                    │ │
│  │  Subscribers Table                                 │ │
│  │  ├─ UserId (PK/RK)                                 │ │
│  │  ├─ Email                                          │ │
│  │  ├─ FirstName, LastName                           │ │
│  │  ├─ PreferredCategoryIds                          │ │
│  │  ├─ IsActive                                       │ │
│  │  ├─ Locale                                         │ │
│  │  └─ LastSentAt                                     │ │
│  │                                                    │ │
│  │  NewsletterDeliveryLog Table                      │ │
│  │  ├─ NewsletterID (PK)                             │ │
│  │  ├─ ReverseTimestamp (RK - for range queries)    │ │
│  │  ├─ Email                                         │ │
│  │  ├─ UserId                                        │ │
│  │  ├─ SentAt                                        │ │
│  │  ├─ Status (Sent/Failed/Bounced)                 │ │
│  │  └─ ErrorMessage                                  │ │
│  │                                                    │ │
│  └────────────────────────────────────────────────────┘ │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │    AZURE FUNCTION APP (Isolated Worker)            │ │
│  │    Runtime: .NET 10 Isolated                       │ │
│  ├────────────────────────────────────────────────────┤ │
│  │                                                    │ │
│  │  Triggers:                                         │ │
│  │  • TimerTrigger (WeeklyNewsletterTimer)           │ │
│  │  • TimerTrigger (SubscriberSeederFunction)        │ │
│  │  • HttpTrigger (ManualSend - optional)            │ │
│  │                                                    │ │
│  └────────────────────────────────────────────────────┘ │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │           AZURE KEY VAULT (Secrets)               │ │
│  ├────────────────────────────────────────────────────┤ │
│  │                                                    │ │
│  │  • SendGridApiKey                                  │ │
│  │  • DatabaseConnectionString                        │ │
│  │  • StorageAccountConnectionString                  │ │
│  │  • API Endpoints                                   │ │
│  │                                                    │ │
│  └────────────────────────────────────────────────────┘ │
│                                                          │
└──────────────────────────────────────────────────────────┘
         │                    │
         └────────┬───────────┘
                  │
        ┌─────────┴──────────┐
        │                    │
        ▼                    ▼
┌──────────────────┐  ┌──────────────────┐
│  SendGrid API    │  │  SQL Database    │
│  (Email Service) │  │  (Production)    │
├──────────────────┤  ├──────────────────┤
│ • Validation     │  │ • Newsletter     │
│ • Sending        │  │ • Preferences    │
│ • Retry Logic    │  │ • Articles       │
│ • Analytics      │  │ • Users          │
│ • Bounce Tracking│  │ • Categories     │
└──────────────────┘  └──────────────────┘
        │
        ▼
┌──────────────────┐
│   Email Client   │
│ (Subscriber)     │
└──────────────────┘
```

---

## Data Flow: Newsletter Creation to Delivery

```
STEP 1: Admin Creates Newsletter (Via Web UI)
════════════════════════════════════════════

NewsletterAdminController
    │
    ├─ GET /Create → Show form with categories
    │
    └─ POST /Save
         │
         ├─ Validate form data
         │
         ├─ Call INewsletterManagementService.CreateAsync()
         │
         ├─ Create Newsletter entity
         │
         ├─ Save to SQL Database (Status: "Draft")
         │
         └─ Return confirmation to admin


STEP 2: Admin Previews Newsletter (Optional)
════════════════════════════════════════════

NewsletterAdminController
    │
    └─ GET /Preview/{id}
         │
         ├─ Load newsletter from database
         │
         ├─ Call INewsletterManagementService.GetNewsletterPreviewAsync()
         │
         ├─ Fetch sample articles via IArticleService.GetLatestAsync()
         │
         ├─ Generate HTML via NewsletterBuilder.GeneratePreviewHtml()
         │
         ├─ Calculate estimated recipient count
         │
         └─ Return preview to admin (HTML/browser)


STEP 3: Admin Schedules Newsletter
════════════════════════════════════════════

NewsletterAdminController
    │
    └─ POST /Schedule/{id}
         │
         ├─ Load newsletter from database
         │
         ├─ Validate scheduled time (must be future)
         │
         ├─ Update Status: "Scheduled"
         │
         ├─ Set ScheduledSendTime
         │
         ├─ Save to database
         │
         └─ Confirm to admin


STEP 4: Scheduled Time Arrives → Function App Triggers
════════════════════════════════════════════════════════

Azure Functions (NewsletterSender App)
    │
    └─ TimerTrigger: "0 0 8 * * 1" (Monday 08:00 UTC)
         │
         ├─ WeeklyNewsletterTimer.Run() executes
         │
         ├─ Load all active subscribers from Table Storage
         │      SubscriberRepository.GetActiveSubscribersAsync()
         │
         ├─ Query newsletters with Status="Scheduled" AND ScheduledSendTime ≤ now
         │
         ├─ For each subscriber (in batches of 50):
         │
         │   a) GetLatestArticlesByCategoriesAsync(subscriber.PreferredCategories)
         │      │
         │      └─ ArticleServiceClient calls HTTP API:
         │         GET /api/articles/latest-by-categories?ids=1,2,3
         │              │
         │              └─ NewsSite returns Article list (DTO format)
         │
         │   b) BuildPersonalizedNewsletterAsync(subscriber)
         │      │
         │      └─ NewsletterBuilder generates HTML:
         │         - Custom header
         │         - Top articles
         │         - Editor's choice
         │         - Custom footer
         │         - Unsubscribe link (token-based)
         │
         │   c) SendNewsletterAsync(to, name, html, subject)
         │      │
         │      └─ EmailSender (SendGrid):
         │         - Try #1: Send email
         │         - If 429/5xx: Wait + retry (exponential backoff)
         │         - Max 3 attempts
         │
         │   d) LogDeliveryAsync(newsletterId, email, userId, status, error)
         │      │
         │      └─ DeliveryLogger writes to Table Storage:
         │         NewsletterDeliveryLog table
         │
         │   e) Continue to next subscriber (failures don't stop batch)
         │
         ├─ After all subscribers:
         │      SubscriberRepository.UpdateLastSentAtAsync(all subscriber IDs)
         │
         ├─ Update newsletter:
         │      Status: "Sent"
         │      SentAt: DateTime.UtcNow
         │      RecipientCount: number_sent
         │
         └─ Log completion


STEP 5: Subscriber Receives Email
═══════════════════════════════════

Email Client (Gmail, Outlook, etc.)
    │
    ├─ Email arrives in inbox
    │
    ├─ Display HTML with articles
    │
    ├─ Display "Read More" links to website
    │
    └─ Display "Unsubscribe" link with token
         │
         └─ Token format: Base64(userId:expiration_date)
            Valid for 30 days


STEP 6: Subscriber Manages Preferences
═══════════════════════════════════════

Option A: One-Click Unsubscribe
    │
    └─ Subscriber clicks unsubscribe link in email
         │
         └─ GET /api/newsletter/unsubscribe?token=...
              │
              ├─ NewsletterApiController.UnsubscribeByToken()
              │
              ├─ Decode token to get userId
              │
              ├─ Validate token expiration (30 days)
              │
              ├─ Load preferences from database
              │
              ├─ Set ReceiveNewsletter: false
              │
              ├─ Save to database
              │
              └─ Display confirmation page

Option B: Manage Preferences (Logged In)
    │
    └─ Subscriber goes to preferences page
         │
         ├─ GET /preferences → Show current preferences
         │
         ├─ Update categories, frequency, etc.
         │
         └─ POST /preferences → Save changes
              │
              ├─ INewsletterService.SavePreferencesAsync()
              │
              └─ Update database


STEP 7: Weekly Subscriber Sync
═════════════════════════════════

Azure Functions
    │
    └─ TimerTrigger: "0 0 2 * * 0" (Sunday 02:00 UTC)
         │
         ├─ SubscriberSeederFunction.Run() executes
         │
         ├─ SubscriberSeeder.SeedSubscribersAsync():
         │
         │   a) Query SQL Database:
         │      SELECT userId, email, firstName, lastName, 
         │             selectedCategoryIds, lastSentDate
         │      FROM NewsletterPreferences np
         │      JOIN AspNetUsers u ON np.UserId = u.Id
         │      WHERE np.ReceiveNewsletter = 1 AND u.IsDeleted = 0
         │
         │   b) Create Subscriber entities
         │
         │   c) SubscriberRepository.UpsertAsync() to Table Storage:
         │      - Create if not exists
         │      - Update if exists
         │
         ├─ Table: "Subscribers"
         │   ├─ PartitionKey: UserId
         │   ├─ RowKey: UserId
         │   ├─ Other: Email, FirstName, LastName, PreferredCategoryIds, etc.
         │
         └─ Log results


STEP 8: Admin Views Statistics
════════════════════════════════

NewsletterAdminController
    │
    └─ GET /Index
         │
         ├─ Load all newsletters from database
         │
         ├─ Call INewsletterManagementService.GetStatsAsync()
         │
         ├─ Calculate:
         │  - Total newsletters
         │  - Count by status (Draft, Scheduled, Sent, Cancelled)
         │  - Total recipients
         │  - Next scheduled send
         │  - Last sent time
         │
         ├─ Display dashboard:
         │  - Newsletter list with status
         │  - Statistics cards
         │  - Recent sends
         │  - Delivery logs (if configured)
         │
         └─ Admin can drill into individual newsletter stats
```

---

## Database Schema Relationships

```
┌─────────────────────────────────────────────────────────┐
│                   SQL Database                          │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────────────────┐    ┌─────────────────────┐  │
│  │   Newsletters        │    │   AspNetUsers       │  │
│  ├──────────────────────┤    ├─────────────────────┤  │
│  │ Id (PK)              │    │ Id (PK)             │  │
│  │ Title                │    │ Email               │  │
│  │ Description          │    │ FirstName           │  │
│  │ Status               │    │ LastName            │  │
│  │ SelectedCategoryIds  │    │ IsDeleted           │  │
│  │ ArticlesPerCategory  │    │ CreatedDate         │  │
│  │ EditorChoiceCount    │    └─────────────────────┘  │
│  │ CustomHtmlHeader     │              ▲               │
│  │ CustomHtmlFooter     │              │ (FK)          │
│  │ ScheduledSendTime    │              │               │
│  │ SentAt               │    ┌──────────┴─────────┐   │
│  │ RecipientCount       │    │ CreatedByUserId    │   │
│  │ CreatedByUserId (FK) ├────┤ (references Users) │   │
│  │ CreatedAt            │    └────────────────────┘   │
│  │ UpdatedAt            │                              │
│  │ IsDeleted            │    ┌─────────────────────┐  │
│  └──────────────────────┘    │ NewsletterPref      │  │
│           │                  ├─────────────────────┤  │
│           │                  │ Id (PK)             │  │
│  ┌────────┴──────────┐       │ UserId (FK) ────┐  │  │
│  │ SelectedCategory  │       │ ReceiveNewsletter└──┐  │
│  │ IDs (comma-sep)   │       │ Frequency          │  │
│  │ targets Articles  │       │ SelectedCategoryIds│  │
│  │ in these cats     │       │ LastSentDate       │  │
│  └───────────────────┘       │ UnsubscribeToken   │  │
│                              │ UpdatedAt          │  │
│  ┌──────────────────────┐    └─────────────────────┘  │
│  │   Articles           │                              │
│  ├──────────────────────┤    ┌─────────────────────┐  │
│  │ Id (PK)              │    │   Categories        │  │
│  │ Title                │    ├─────────────────────┤  │
│  │ Summary              │    │ Id (PK)             │  │
│  │ Slug                 │    │ Name                │  │
│  │ CategoryId (FK)      ├────┤ Description         │  │
│  │ AuthorId (FK)        │    │                     │  │
│  │ IsReadyForPublish    │    └─────────────────────┘  │
│  │ IsDeleted            │                              │
│  │ IsArchived           │                              │
│  │ CreatedAt            │                              │
│  └──────────────────────┘                              │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## Configuration & Environment Variables

### Development (Local)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=NewsSiteDb;Integrated Security=true;"
  },
  "Newsletter": {
    "SendGridApiKey": "SG.YOUR_KEY_HERE",
    "FromEmail": "noreply@newssite.local",
    "FromName": "News Site",
    "BaseUrl": "https://localhost:5001"
  }
}
```

### Azure (Production)
```
Key Vault Secrets:
├─ SendGridApiKey → function app config
├─ DatabaseConnectionString → function app config
├─ StorageConnectionString → function app config
├─ NewsletterApiBaseUrl → function app config

Function App Settings:
├─ FUNCTIONS_WORKER_RUNTIME = dotnet-isolated
├─ AzureWebJobsStorage = @Microsoft.KeyVault(SecretUri=...)
├─ SendGridApiKey = @Microsoft.KeyVault(SecretUri=...)
├─ NewsSiteDbConnection = @Microsoft.KeyVault(SecretUri=...)
```

---

## Performance Considerations

### Batching
- Process subscribers in batches of 50
- Prevents overwhelming SendGrid API
- Reduces memory usage
- Allows monitoring per batch

### Table Storage Indexing
- `Subscribers`: Partition by UserId (efficient lookups)
- `DeliveryLog`: Partition by Newsletter, Row by ReverseTimestamp (efficient time range queries)

### Retry Logic
- Max 3 attempts per email
- Exponential backoff: 1s, 2s, 4s
- Only retry on 429 (rate limit) and 5xx (server errors)

### Query Optimization
- Filter newsletters by status before sending
- Include only necessary columns in queries
- Use pagination for admin lists

---

## Scalability Path

### Current: Single Timer Function
- Handles up to ~10,000 subscribers per week
- Processing time: ~2-3 hours for full list
- Suitable for: Small to medium apps

### Future: Durable Functions / Queue
- Queue-based fan-out pattern
- Multiple workers processing simultaneously
- Handles 100,000+ subscribers
- Suitable for: Large-scale apps

### Future: Blob Storage Templates
- Store HTML templates in Blob Storage
- Template versioning
- Reduce database size

---

**End of Architecture Documentation**

