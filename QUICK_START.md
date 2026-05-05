# 🚀 Newsletter System - Quick Start Guide

## 5-Minute Overview

You now have a **complete newsletter system** with:

✅ **Admin Interface** - Create & manage newsletters  
✅ **Azure Function App** - Automated weekly sending  
✅ **SendGrid Integration** - Reliable email delivery  
✅ **Subscriber Management** - Preferences & unsubscribe  
✅ **Local Testing Ready** - Test everything locally first  
✅ **Database Migrations** - Ready to apply  

---

## 🎯 What You Can Do Right Now

### 1️⃣ **Apply Database Migration** (1 minute)
```powershell
cd NewsSite
dotnet ef database update
```
This creates the `Newsletters` table.

### 2️⃣ **Configure Local Settings** (2 minutes)
Edit `NewsletterSender/local.settings.json`:
```json
{
  "Values": {
    "SendGridApiKey": "SG.YOUR_KEY_HERE",
    "NewsletterApiBaseUrl": "https://localhost:5001",
    "NewsSiteDbConnection": "Server=(localdb)\\mssqllocaldb;Database=NewsSiteDb;Integrated Security=true;"
  }
}
```

### 3️⃣ **Start Local Services** (5 minutes)
```powershell
# Terminal 1: Azurite storage emulator
azurite

# Terminal 2: NewsSite web app
cd NewsSite && dotnet run --launch-profile https

# Terminal 3: Newsletter Function App
cd NewsletterSender && func start
```

---

## 📰 Creating Your First Newsletter

1. Navigate to: https://localhost:5001/Admin/Newsletters
2. Click **"Create New Newsletter"**
3. Fill in:
   - **Title**: "My First Newsletter"
   - **Select Categories**: Choose any categories
   - Leave other fields default
4. Click **"Preview"** - see how it looks
5. Click **"Send Now"** - or click **"Schedule"** for later

---

## 📊 Key Features

### For Admins
- Create unlimited newsletters
- Preview before sending
- Schedule for specific date/time
- Track delivery statistics
- Target specific categories
- Custom HTML headers/footers
- View all send history

### For Subscribers
- One-click unsubscribe
- Manage preferences
- Choose article categories
- Set frequency (Weekly/Monthly)
- Unsubscribe via email link

### For System
- Automatic weekly sending (Monday 08:00 UTC)
- Automatic subscriber syncing (Sunday 02:00 UTC)
- Batch processing (50 emails at a time)
- SendGrid retry logic (3 attempts)
- Full delivery audit trail
- Error logging & recovery

---

## 🗂️ Important Files

### **To Manage Newsletters**
- `NewsSite/Controllers/NewsletterAdminController.cs` - Admin UI
- `NewsSite/Models/Entities/Newsletter.cs` - Database model
- `NewsSite/Models/ViewModels/NewsletterManagementViewModel.cs` - Edit form

### **To Change How Newsletters Are Sent**
- `NewsletterSender/Functions/WeeklyNewsletterTimer.cs` - Sending schedule & logic
- `NewsletterSender/Services/NewsletterBuilder.cs` - Email HTML generation
- `NewsletterSender/Services/EmailSender.cs` - SendGrid configuration

### **To Change Subscriber Seeding**
- `NewsletterSender/Functions/SubscriberSeederFunction.cs` - Sync schedule
- `NewsletterSender/Services/SubscriberSeeder.cs` - Sync logic

### **To Change API Endpoints**
- `NewsSite/Controllers/ArticlesApiController.cs` - Article fetching
- `NewsSite/Controllers/NewsletterApiController.cs` - Unsubscribe & preferences

---

## 📍 Key URLs

| URL | Purpose |
|-----|---------|
| `/Admin/Newsletters` | Admin newsletter list |
| `/Admin/Newsletters/Create` | Create newsletter |
| `/Admin/Newsletters/Edit/{id}` | Edit newsletter |
| `/Admin/Newsletters/Preview/{id}` | Preview newsletter |
| `/api/newsletter/unsubscribe?token=...` | Unsubscribe via email |
| `/api/newsletter/preferences` | Get/update subscriber preferences |

---

## 🔍 Testing Checklist

Before deploying to Azure, verify:

- [ ] Database migration applied successfully
- [ ] Local.settings.json configured with SendGrid key
- [ ] Azurite storage emulator running
- [ ] NewsSite app starts on https://localhost:5001
- [ ] Function app starts on http://localhost:7071
- [ ] Can login as admin
- [ ] Can create draft newsletter
- [ ] Preview shows articles correctly
- [ ] Estimated recipient count is correct
- [ ] Can send newsletter (check SendGrid dashboard for email)
- [ ] Newsletter marked as "Sent" in database

---

## ⚙️ Customization

### Change Send Schedule
Edit `NewsletterSender/Functions/WeeklyNewsletterTimer.cs`:
```csharp
[TimerTrigger("0 0 8 * * 1")] // Current: Monday 08:00 UTC
// Change to:
[TimerTrigger("0 9 * * 1")] // Every Monday at 09:00 UTC
```

### Change Subscriber Sync Schedule
Edit `NewsletterSender/Functions/SubscriberSeederFunction.cs`:
```csharp
[TimerTrigger("0 0 2 * * 0")] // Current: Sunday 02:00 UTC
// Change to:
[TimerTrigger("0 0 1 * * *")] // Daily at 01:00 UTC
```

### Change Newsletter Template
Edit `NewsletterSender/Services/NewsletterBuilder.cs`:
- Modify the `BuildHtmlContent()` method
- Add custom CSS, colors, layout
- Include additional sections

### Change Email From Address
Edit `NewsletterSender/Program.cs` and look for:
```csharp
var fromEmail = "noreply@yoursite.com"; // Change this
```

---

## 🐛 Quick Troubleshooting

**Problem**: "Database table not found"  
**Solution**: Run `dotnet ef database update` in NewsSite folder

**Problem**: "SendGrid API key invalid"  
**Solution**: Check key in `local.settings.json`, regenerate if needed

**Problem**: "No subscribers found"  
**Solution**: Create newsletter preferences for users in database, or wait for seeder to run

**Problem**: "Function app not starting"  
**Solution**: Ensure Azure Functions Core Tools installed (`func --version`)

**Problem**: "Azurite connection failed"  
**Solution**: Run `azurite` command in terminal, verify port 10000 not blocked

---

## 📈 Next Steps

### Immediate (Today)
1. ✅ Apply database migration
2. ✅ Configure local settings
3. ✅ Test locally end-to-end
4. ✅ Create test newsletter

### Short Term (This Week)
1. ✅ Test with real SendGrid key
2. ✅ Create admin guide for team
3. ✅ Train team on admin interface
4. ✅ Document custom HTML examples

### Medium Term (This Month)
1. ⏳ Deploy to Azure
2. ⏳ Configure Key Vault
3. ⏳ Set up monitoring & alerts
4. ⏳ Plan first real newsletter send

### Future (Optional)
1. 🔜 Email template storage (Blob)
2. 🔜 A/B testing
3. 🔜 Advanced analytics
4. 🔜 Scheduled auto-sends
5. 🔜 SMS notifications

---

## 📚 Full Documentation

For detailed information, see:

- **[NEWSLETTER_SYSTEM_SUMMARY.md](./NEWSLETTER_SYSTEM_SUMMARY.md)** - Complete overview
- **[LOCAL_TESTING_GUIDE.md](./LOCAL_TESTING_GUIDE.md)** - Detailed local setup
- **[ADMIN_NEWSLETTER_GUIDE.md](./ADMIN_NEWSLETTER_GUIDE.md)** - Admin user manual
- **Code Comments** - Each class has XML documentation

---

## 🎓 How It Works (5-Minute Explanation)

### The Flow

```
1. Admin creates newsletter
   ↓
2. Admin previews & schedules
   ↓
3. Monday 08:00 UTC: WeeklyNewsletterTimer function runs
   ↓
4. Function loads subscribers from Table Storage
   ↓
5. For each subscriber:
   - Fetches their preferred articles
   - Generates HTML email
   - Sends via SendGrid
   - Logs delivery result
   ↓
6. Admin can view statistics
   ↓
7. Subscriber receives email with unsubscribe link
   ↓
8. Subscriber clicks unsubscribe → preference saved
   ↓
9. Next week: Seeder syncs preferences, updates subscriber list
```

### Technologies Used

- **C# .NET 10** - Backend logic
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - Database access
- **Azure Functions** - Scheduled sends
- **Azure Table Storage** - Subscriber data & logs
- **SendGrid** - Email delivery
- **SQL Server** - Newsletter & preference storage

---

## ❓ FAQ

**Q: Can I change the send time?**  
A: Yes, edit the TimerTrigger in `WeeklyNewsletterTimer.cs`

**Q: Can admins create multiple newsletters per week?**  
A: Yes, create as many as you want (recommended max 2-3/week)

**Q: Do unsubscribed users still get emails?**  
A: No, they're excluded during subscriber seeding

**Q: Can I use a different email provider?**  
A: Yes, replace SendGrid with your provider in `EmailSender.cs`

**Q: How do I customize the email design?**  
A: Edit the HTML template in `NewsletterBuilder.cs`

**Q: Can subscribers see email list?**  
A: Yes, add a page showing sent newsletters in user dashboard

---

## 🎯 Success Criteria

Your newsletter system is working when:

1. ✅ Admins can create newsletters via web interface
2. ✅ Newsletters can be previewed before sending
3. ✅ Newsletters can be scheduled or sent immediately
4. ✅ Function app automatically sends on schedule
5. ✅ Subscribers receive emails with correct content
6. ✅ Unsubscribe links work
7. ✅ Subscriber preferences are respected
8. ✅ Delivery logs show all sends
9. ✅ Statistics dashboard works
10. ✅ Errors are logged and don't stop batch processing

---

## 📞 Support

### Quick Links
- SendGrid Dashboard: https://app.sendgrid.com
- Azure Portal: https://portal.azure.com
- Entity Framework Docs: https://docs.microsoft.com/ef/core
- Azure Functions: https://docs.microsoft.com/en-us/azure/azure-functions

### Common Commands
```powershell
# Apply migrations
dotnet ef database update --project NewsSite

# Run tests
dotnet test

# Run build
dotnet build

# Run from solution root
dotnet run --project NewsSite

# Check function app status
func status
```

---

**You're all set!** 🎉

Start with the local testing guide and you'll have a working newsletter system in under an hour.

**Happy newsletter sending!** 📬

