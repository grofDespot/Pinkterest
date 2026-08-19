# Pinkterest

Photo upload and browsing application. ASP.NET Core MVC on .NET 10, EF Core, PostgreSQL.

Built for two courses on one codebase:

| Course | Scope | Points |
|---|---|---|
| **AADBDT** | The application + 12 design patterns | 86 |
| **APP** | Tests, AOP, functional programming, metrics, Docker, SOLID | 82 |

---

## Run

```bash
docker compose up -d                             # PostgreSQL + MinIO
dotnet run --project Web --launch-profile https  # https://localhost:7061
```

Migrations and seed data (roles, packages, administrator) are applied on startup.

Administrator: `admin@pinkterest.local` — password in `Web/appsettings.Development.json`.

Fully containerised:

```bash
docker compose --profile full up -d --build      # http://localhost:8080
```

## Tests

```bash
dotnet test tests/UnitTests           # 40 tests, no dependencies
dotnet test tests/IntegrationTests    # 53 tests, needs Docker
dotnet test tests/UiTests --no-build  # 7 tests, needs the app running
```

## Benchmarks

```bash
dotnet run --project Benchmarks -c Release -- --filter *
```

---

## Structure

```
Domain/          entities, domain events            — no dependencies
App/             use cases, contracts, Result<T>    — depends on Domain only
CrossCutting/    [Audited] and [Measured]           — no dependencies
Infrastructure/  EF Core, Identity, storage         — implements App interfaces
Web/             controllers, views, DI wiring      — the only project that sees both
```

---

## AADBDT — features

| Requirement | Where |
|---|---|
| Anonymous / registered / administrator | `Domain/Constants/Roles.cs`, `Policies.cs` |
| Local registration + Google + GitHub | `Infrastructure/Identity/AccountService.cs`, `ExternalAuthenticationService.cs` |
| FREE / PRO / GOLD packages, usage tracking | `Infrastructure/Usage/UsageQuery.cs` |
| Package change once a day, effective next day | `App/Packages/State/`, `Infrastructure/Packages/` |
| Upload with hashtags, description, processing | `Infrastructure/Photos/PhotoUploadService.cs` |
| Gallery, 10 latest thumbnails, detail view | `Web/Controllers/GalleryController.cs` |
| Search: hashtag, author, size, date range | `App/Photos/Search/PhotoSearchBuilder.cs` |
| Download original or with filters + presets | `Infrastructure/Photos/PhotoDownloadService.cs`, `App/Photos/Presets/` |
| Administration: users, packages, photos, stats | `Web/Controllers/AdminController.cs`, `Infrastructure/Admin/` |
| Audit log — 15 action types | `Infrastructure/Auditing/AuditLog.cs` |
| Local **or** S3 storage, by configuration | `Infrastructure/Storage/PhotoStorageFactory.cs` |

Switch storage with one setting: `"Storage": { "Provider": "S3" }`

## AADBDT — design patterns

| Layer | Pattern | Where |
|---|---|---|
| Business | Strategy | `App/Photos/Processing/IImageFilter.cs` |
| Business | Decorator | `App/Photos/Processing/FilterPipelineDecorator.cs` |
| Business | Chain of Responsibility | `App/Photos/Validation/UploadValidationChain.cs` |
| Business | Specification | `App/Common/Specifications/Specification.cs` |
| Business | State | `App/Packages/State/` |
| Data / service | Abstract Factory | `Infrastructure/Storage/PhotoStorageFactory.cs` |
| Data / service | Repository + Unit of Work | `App/Photos/IPhotoRepository.cs` |
| Data / service | Proxy | `Infrastructure/Storage/CachingPhotoStorageProxy.cs` |
| Data / service | Observer | `Infrastructure/Events/DomainEventDispatcher.cs` |
| Presentation | Mediator | `Infrastructure/Mediation/Sender.cs` |
| Presentation | Template Method | `Web/Controllers/AuditedController.cs` |
| Presentation | Builder | `App/Photos/Search/PhotoSearchBuilder.cs` |

---

## APP — outcomes

| # | Outcome | Delivered |
|---|---|---|
| O1 | Testing concepts | Advanced concepts, in the presentation |
| O2/O3 | Unit + integration + UI tests | 100 tests, 3 categories, 3 architectural parts |
| O4 | Reduce time and memory | 15 benchmarks — see below |
| O5 | Functional programming | 10 examples across 3 projects |
| O6 | Aspects | 2 aspects, 15 usages, 8 intercepted services |
| O7 | Version control | 3 branches, conventional commits |
| O8 | SOLID + Docker | All 5 principles; multi-stage image, non-root |
| O9 | Metrics | 9 at `/metrics`, 7 custom |

### Metrics

`/metrics`, Prometheus format. OpenTelemetry with ASP.NET Core and runtime instrumentation, plus seven of our own on the `Pinkterest` meter.

| Instrument | Type | Source |
|---|---|---|
| `pinkterest.photos.uploaded` | counter | `Infrastructure/Events/PhotoUploadMetricsHandler.cs` |
| `pinkterest.photos.uploaded.bytes` | histogram | `Infrastructure/Events/PhotoUploadMetricsHandler.cs` |
| `pinkterest.operation.duration` | histogram | `Infrastructure/Interception/MetricsInterceptor.cs` |
| `pinkterest.operation.outcomes` | counter | `Infrastructure/Interception/MetricsInterceptor.cs` |
| `pinkterest.storage.bytes.used` | observable gauge | `Infrastructure/Observability/StorageMetrics.cs` |
| `pinkterest.photos.total` | observable gauge | `Infrastructure/Observability/StorageMetrics.cs` |
| `pinkterest.users.total` | observable gauge | `Infrastructure/Observability/StorageMetrics.cs` |

The two operation instruments are emitted by the aspect, so every `[Measured]` method is timed and counted by name and outcome without touching the method. The gauges query the database behind a 30-second cache and serve the previous values if the query fails.

### Benchmarks

15 benchmarks in four suites: storage reads through the caching proxy, download buffering versus streaming, image processing, and specification composition.

| Change | Result |
|---|---|
| Caching proxy on a gallery page | 775 µs → 27 µs |
| Streaming instead of buffering a 4 MB download | 8,259,268 B → 946 B |
| Skipping decode/encode when no filters requested | 12.2 ms → 25.9 ns |
| Hoisting a `ToLower()` out of a specification | 361 ns → 348 ns |

### Functional programming

| Example | Where | Coupling removed |
|---|---|---|
| Fold filters into a decorator chain | `App/Photos/Processing/ImagePipeline.cs` | Callers no longer know how many filters exist, or the order |
| Identity element instead of a null check | `App/Photos/Processing/EmptyPipeline.cs` | No branch for "no filters" anywhere |
| `Result<T>` with `Map`, `Bind`, `Ensure`, `Match` | `App/Common/Results/` | Callers stop depending on which exceptions a service throws |
| Specifications as composable expression trees | `App/Common/Specifications/Specification.cs` | The same predicate runs in memory and as SQL |
| Absent input returns the builder unchanged | `App/Photos/Search/PhotoSearchBuilder.cs` | Optional criteria need no conditionals at the call site |
| Pure pipeline of small functions | `App/Photos/HashtagNormalizer.cs` | Parsing is testable without a database or a request |
| Immutable record narrowed with `with` | `App/Photos/Presets/FilterPresetDefinition.cs` | A stored preset cannot smuggle in an unknown filter |
| Name-to-filter mapping as a function | `App/Photos/Processing/ImageFilterCatalog.cs` | Unknown names drop out instead of throwing |
| Operations passed as `Func` to a base controller | `Web/Controllers/AuditedController.cs` | One audit-and-redirect path serves every write action |
| Gauge callbacks passed to the meter | `Infrastructure/Observability/StorageMetrics.cs` | The exporter pulls values; nothing pushes to it |

### Aspects

```csharp
[Audited(AuditActions.PhotoUpload, EntityType = nameof(Photo))]
[Measured(AuditActions.PhotoUpload)]
public async Task<Result<Guid>> UploadAsync(...)
```

Castle DynamicProxy interceptors in `Infrastructure/Interception/`. Auditing and timing are not handwritten in any service.

### SOLID

| Principle | Where | What it buys |
|---|---|---|
| Single responsibility | `App/Photos/Validation/` | Five upload rules, five classes, one reason to change each |
| Open/closed | `App/Photos/Processing/IImageFilter.cs` | A new filter is a new class and one catalog entry; the pipeline is untouched |
| Liskov | `tests/IntegrationTests/PhotoStorageContractTests.cs` | One test body, run against local storage and the caching proxy |
| Interface segregation | `IPhotoUploadService`, `IPhotoEditService`, `IPhotoDownloadService`, `IPhotoRepository` | Each caller depends on the one operation it uses |
| Dependency inversion | `App/` declares the interfaces, `Infrastructure/` implements them | `App` has no reference to `Infrastructure`; storage swaps by configuration |

---

## Notes

- OAuth needs credentials in user secrets; without them the buttons simply do not render.
- `RefreshToken` is modelled but unused.
