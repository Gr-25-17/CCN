# Bellos/GPT Implementation Plan

Implemented in this branch:

- Added `RenewalReminderSentAt` to `Subscription`.

Recommended next implementation steps:

1. Extend `ISubscriptionService` with `CreateOrRenewAsync`.
2. Extend `ISubscriptionRepository` with CRUD and expiring queries.
3. Implement `ArticleArchiveService`.
4. Implement weekly newsletter sender.
5. Implement subscription renewal reminder sender.
6. Create a dedicated `NewsSite.Functions` Azure Functions project.
7. Add EF Core migration for `RenewalReminderSentAt`.
8. Refactor all `DateTime.Now` usages to `DateTime.UtcNow`.
9. Remove unused dependencies such as `AngleSharp.Dom` in `NewsletterService` if not used.
10. Optimize `GetAllArticlesSortedByPreferencesAsync` to avoid materializing all articles before sorting.

Refactor recommendations:

- Replace `DateTime.Now` with `DateTime.UtcNow` throughout the solution.
- Move email sending into a dedicated abstraction (`IEmailService`).
- Avoid loading all articles into memory in repository methods.
- Remove unused `using` directives.
- Consider introducing an application layer project for shared services used by MVC and Azure Functions.
