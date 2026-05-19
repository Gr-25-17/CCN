# PR Review – branch `work`

## Scope reviewed
- Head commit: `159741d` (`-Published on Azure`)
- Files changed in commit:
  - `CCNLetter/CCNLetter/Properties/serviceDependencies.json`
  - `CCNLetter/CCNLetter/Properties/serviceDependencies.ccnewsfunctions - Zip Deploy.json`
  - `CCNLetter/CCNLetter/Properties/ServiceDependencies/ccnewsfunctions - Zip Deploy/appInsights1.arm.json`
  - `CCNLetter/CCNLetter/Properties/ServiceDependencies/ccnewsfunctions - Zip Deploy/storage1.arm.json`

## Overall assessment
**Status: Approve with follow-up items**

The commit is deployment-oriented and improves environment binding clarity by wiring Azure dependencies (`APPLICATIONINSIGHTS_CONNECTION_STRING`, `AzureWebJobsStorage`) and ARM templates for reproducible provisioning.

## Strengths
1. **No secrets committed**
   - Uses configuration keys and ARM placeholders rather than hardcoded credentials.
2. **Infra is codified**
   - App Insights and Storage are represented as deployable templates, reducing manual setup drift.
3. **Consistent dependency mapping**
   - Connection IDs are aligned between local dependency map and zip deploy dependency map.

## Risks / observations
1. **Hardcoded environment defaults in ARM templates**
   - `resourceGroupName = Gr25-17RG`, location `swedencentral`, storage account `ccnstorage`, and app insights name `CCNFunctionsApp` are fixed defaults.
   - Risk: non-portable deployment and name collisions across environments/subscriptions.
2. **Old API versions in templates**
   - Uses `Microsoft.Storage/storageAccounts` apiVersion `2017-10-01` and Insights `2015-05-01`.
   - Risk: reduced access to newer capabilities and future compatibility issues.
3. **Potential source-control noise**
   - `serviceDependencies*.json` files are often IDE-generated and may churn frequently.
   - Risk: noisy PR history if not intentionally managed.

## Recommended follow-ups
1. Parameterize names/locations per environment (dev/test/prod) and avoid fixed global names (`ccnstorage`).
2. Upgrade ARM resource `apiVersion` values to currently supported stable versions.
3. Add a short deployment README section explaining expected parameter overrides.
4. Optionally define policy for generated dependency files (track intentionally or ignore where appropriate).

## Suggested PR verdict text
"Infrastructure alignment looks good and no secrets are exposed. Approving this deployment wiring change, with a recommendation to parameterize environment-specific defaults and modernize ARM API versions in a follow-up PR."
