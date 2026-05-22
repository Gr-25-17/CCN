# Azure Functions Setup Checklist

## Azure resources

- [ ] Create a Storage Account.
- [ ] Create a Function App using .NET Isolated.
- [ ] Connect Application Insights (recommended).

## Application settings

- [ ] AzureWebJobsStorage
- [ ] FUNCTIONS_WORKER_RUNTIME=dotnet-isolated
- [ ] ConnectionStrings__DefaultConnection
- [ ] EmailSettings__SmtpServer
- [ ] EmailSettings__SmtpPort
- [ ] EmailSettings__SmtpUser
- [ ] EmailSettings__SmtpPass
- [ ] EmailSettings__SenderEmail
- [ ] EmailSettings__SenderName

## Deployment

- [ ] Build the solution.
- [ ] Publish NewsSite.Functions.
- [ ] Verify all three functions are visible in the Azure portal.

## Verification

- [ ] Run each function manually from the Azure portal.
- [ ] Confirm emails are sent.
- [ ] Confirm archived articles are updated in the database.
- [ ] Confirm logs appear in Application Insights.
