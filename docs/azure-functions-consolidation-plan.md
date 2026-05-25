# Azure Functions-konsolidering i CCN

## Kort svar
Ja: en Azure Function App kan hosta **många functions** så länge de ligger i **samma deployade Function Worker-projekt** (samma host/process). I ert repo innebär det praktiskt att ni behöver slå ihop functions som ska dela App till ett gemensamt Function-projekt (eller skapa ett nytt gemensamt projekt och flytta in dem).

## Nuvarande läge (repo-analys)
Ni har idag minst sex separata Function-projekt:

1. `NewsSite.Functions` – timerjobb för arkivering, nyhetsbrev och abonnemangs-påminnelser.
2. `API_Weather` – väderjobb var 15:e minut.
3. `GoldApIUpdater` – guldprisjobb var 6:e timme.
4. `ImageProcessor` – blob-trigger + HTTP repair-endpoint för bildprocessning.
5. `NewsArchive` – arkiveringsjobb via anrop till NewsSite API + Table Storage.
6. `CCNLetter` – veckonyhetsbrev via API-anrop.

Alla är .NET isolated worker (Function v4) och net10, men de är uppdelade i olika host-processer/projekt.

## Rekommenderad målbild
Skapa **2 Function Apps** (inte 1), med tydlig workload-separation:

### Function App A: `ccn-jobs-func`
Samla **timer-/batch-jobb**:
- `WeeklyNewsletterFunction`, `SubscriptionReminderFunction`, `ArchiveArticlesFunction` (från `NewsSite.Functions`)
- `WeatherFunction` (från `API_Weather`)
- `GoldMarketTimer` (från `GoldApIUpdater`)

### Function App B: `ccn-media-func`
Samla **bild/IO-intensiv pipeline**:
- `ProcessArticleImage`
- `RepairExistingImages`

## Varför 2 appar är bättre än 1
1. **Skalning**: Blob-trigger med bildkonvertering kan ge CPU/minnes-toppar och påverka timerjobb om allt ligger i samma app.
2. **Driftfönster**: Media-pipeline kan behöva andra timeout/concurrency-inställningar än schemalagda jobb.
3. **Säkerhet/konfig**: Media-funktioner behöver andra storage-behörigheter än rena timerjobb.
4. **Release-isolering**: Mindre risk att bildkod påverkar affärskritiska e-post/arkivjobb vid deploy.

## Vilka funktioner bör avvecklas/slås ihop logiskt
- Behåll **en** implementation för veckonyhetsbrev. Ni har både `CCNLetter` och `NewsSite.Functions/WeeklyNewsletterFunction`.
- Behåll **en** arkiveringskedja. Ni har både `NewsArchive/ArchiveFunction` och `NewsSite.Functions/ArchiveArticlesFunction`.

Rekommendation:
- Standardisera på `NewsSite.Functions`-spåret (det använder redan NewsSite-tjänstelager via DI och blir enklare att underhålla).
- Flytta in weather + gold där, och avveckla duplicerade projekt stegvis.

## Konkreta tekniska steg (låg risk)

### Steg 1 – Skapa nytt gemensamt projekt för timerjobb
Skapa t.ex. `CCN.Jobs.Functions`:
- Kopiera `Program.cs` från `NewsSite.Functions` som bas.
- Registrera beroenden för weather + gold + befintliga NewsSite-tjänster i samma DI-container.
- Flytta funktionsklasserna in i detta projekt (timer-jobben först).

### Steg 2 – Extrahera delad domän/infrastruktur
För att undvika tight coupling mot MVC-projekt:
- Flytta återanvändbara interfaces/services till ett delat class library, t.ex. `CCN.Shared` eller `CCN.Workloads`.
- Function-projekten refererar class library, inte web-UI direkt.

### Steg 3 – Konsolidera konfiguration
I `app settings` / Key Vault:
- Samla alla keys per app (`AzureWebJobsStorage`, API-URL:er, API-nycklar, SQL-connstring).
- Prefixa settings per domän (`Weather__`, `Gold__`, `Newsletter__`).

### Steg 4 – Migrera och verifiera i ordning
1. Deploy `ccn-jobs-func` med endast en migrerad function.
2. Kör smoke test på trigger + logs.
3. Lägg till nästa function.
4. När parity är verifierad: stäng av gammal function app.

### Steg 5 – Flytta ImageProcessor separat
- Skapa/behåll `ccn-media-func`.
- Flytta bara `ImageProcessor`-funktionerna dit.
- Sätt separata scale/concurrency-inställningar.

## Risker ni ska hantera innan merge
1. **Function name-kollisioner**: `[Function("...")]` måste vara unika inom samma app.
2. **host.json-konflikter**: en app har en `host.json`; sammanslagning kräver gemensamma värden.
3. **NuGet-versioner**: harmonisera Worker/ApplicationInsights/Extensions-versioner innan flytt.
4. **Storage-tabeller**: säkerställ att partition/row-key-strategier inte krockar mellan workloads.

## Praktisk tumregel framåt
- **Samma Function App**: funktioner med liknande driftprofil (timer/batch tillsammans).
- **Separat Function App**: olika resursprofil, säkerhetsyta eller release-cadence (t.ex. media pipeline).

## Beslut
Bästa vägen för er: 
1) Konsolidera till två appar (jobs + media), 
2) ta bort duplicerade funktionella flöden, 
3) bygg gemensamt jobs-projekt först och migrera inkrementellt.

Det ger lägre driftkostnad, enklare förvaltning och mindre operativ risk än att pressa in allt i en enda app direkt.
