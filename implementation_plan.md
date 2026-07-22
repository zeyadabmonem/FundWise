# FundWise AI — Implementation Plan (Phase 1 MVP)

This plan covers the complete scaffolding of the FundWise AI project: a Flutter Android app + ASP.NET Core backend, following Clean Architecture as defined in the blueprint.

---

## Background

The repository (`c:\Users\User\Documents\GitHub\FundWise`) is currently empty (just `.gitignore` + `README.md`). We are building from scratch.

The full system consists of:
- **Backend:** ASP.NET Core (.NET 9), Clean Architecture, SQL Server, EF Core, MediatR, CQRS, OpenAI (GPT-4o + Whisper + Vision)
- **Frontend:** Flutter (Android-first), Clean Architecture, BLoC state management

---

## User Review Required

> [!IMPORTANT]
> **Scope decision needed before we start:** The full project (backend + Flutter app) is very large for a single session. I recommend we scaffold both in phases:
> 1. **Backend first** — full solution structure, all layers, all features (F1–F9), API running and documented.
> 2. **Flutter app second** — full app structure, screens, BLoC, API integration.
>
> Do you want me to proceed with this order, or do you have a different preference?

> [!WARNING]
> **API Keys required at runtime** — the app will need the following in `appsettings.json` (we'll leave placeholders):
> - `OpenAI:ApiKey` — for GPT-4o (categorization, voice NLP, alternatives)
> - `OpenAI:WhisperApiKey` — for speech-to-text (can be same key)
> - `OpenAI:VisionApiKey` — for receipt OCR (can be same key)
> - `ConnectionStrings:DefaultConnection` — SQL Server connection string
>
> You'll fill these in before running the backend.

> [!IMPORTANT]
> **Database:** We'll use SQL Server. If you want to use a different DB (PostgreSQL, SQLite for local dev), let me know before we start.

---

## Open Questions

> [!IMPORTANT]
> 1. **State Management (Flutter):** The blueprint mentions BLoC/Provider/GetX. The blueprint leans BLoC (mentions `bloc/` folders). Confirm: **BLoC (flutter_bloc)?**
> 2. **OCR Service:** The PRD says "Azure Document Intelligence OR Google Vision — pick one." Which do you prefer? (Recommendation: **Azure Document Intelligence** — better Arabic support, integrates natively with .NET.)
> 3. **Local Flutter DB:** The blueprint mentions Hive, SharedPreferences, or SQLite for offline. For MVP, do you want **Hive** (fast, NoSQL-style) or **SQLite via drift** (relational, closer to backend model)?
> 4. **Do you want me to set up Docker + docker-compose** for local dev (SQL Server container + API container), or will you run SQL Server locally yourself?
> 5. **Flutter routing:** Blueprint mentions `go_router` or `auto_route`. Recommendation: **go_router** (Flutter-team maintained). Confirm?

---

## Proposed Changes

### Phase A — Backend Solution (ASP.NET Core)

---

#### [NEW] Solution & Project Structure

Scaffold the full .NET solution with 7 projects:

| Project | Type |
|---|---|
| `FundWise.API` | ASP.NET Core Web API |
| `FundWise.Application` | Class Library |
| `FundWise.Domain` | Class Library |
| `FundWise.Infrastructure` | Class Library |
| `FundWise.Persistence` | Class Library |
| `FundWise.Shared` | Class Library |
| `FundWise.Contracts` | Class Library |

And 3 test projects:
| Project | Type |
|---|---|
| `FundWise.UnitTests` | xUnit |
| `FundWise.IntegrationTests` | xUnit |
| `FundWise.FunctionalTests` | xUnit |

---

#### Domain Layer (`FundWise.Domain`)

**Entities:**
- `User` — Id (Guid), Name, Email, PasswordHash, Currency (default "EGP"), CreatedAt, RefreshToken, RefreshTokenExpiry
- `Transaction` — Id, UserId, Merchant, Amount, Currency, Category (enum), Source (enum: Voice/OCR/QR/SMS/Manual), CaptureDate, Notes, IsConfirmed, ConfidenceScore, CreatedAt
- `MerchantCategoryMemory` — Id, UserId, MerchantName, Category — for per-user learned corrections
- `AlternativeProduct` — Id, ProductName, Category, CurrentPrice, AlternativeName, AlternativePrice, SavingAmount (mock dataset)

**Enums:**
- `TransactionCategory`: FoodAndDrink, Groceries, Transport, BillsAndUtilities, Shopping, Entertainment, Health, Education, Transfer, Other
- `CaptureSource`: Voice, ReceiptOCR, QRCode, SMS, Manual

**Interfaces (in Domain):**
- `IRepository<T>`, `IUnitOfWork`

**Value Objects:**
- `Money` — Amount + Currency
- `Email`

---

#### Application Layer (`FundWise.Application`)

**Features / Use Cases (Commands + Queries):**

| Feature | Commands | Queries |
|---|---|---|
| Authentication | RegisterUser, LoginUser, RefreshToken, LogoutUser | GetCurrentUser |
| Transactions | CreateTransaction, UpdateTransaction, DeleteTransaction, ConfirmTransaction | GetTransactions, GetTransactionById |
| Voice | ProcessVoiceCapture | — |
| OCR | ProcessReceiptOCR | — |
| SMS | ParseSmsTransaction | — |
| QR | ParseQrCode | — |
| Categorization | CategorizeTransaction, CorrectTransactionCategory | — |
| Dashboard | — | GetDashboardSummary, GetCategoryBreakdown, GetRecentTransactions |
| Recommendations | — | GetAlternativeForTransaction |
| Settings | UpdateUserSettings | GetUserSettings |

**Common:**
- `Result<T>` pattern
- `ValidationBehavior` (FluentValidation pipeline)
- `LoggingBehavior`
- `TransactionBehavior`
- Custom domain exceptions

**External Service Interfaces (Application defines, Infrastructure implements):**
- `IVoiceService` — transcribe audio → extract structured transaction
- `IOcrService` — image → extract structured transaction
- `ISmsParser` — SMS text + sender → extract structured transaction
- `IQrParser` — QR content → extract structured transaction
- `ICategorizationService` — merchant + description + amount → category + confidence
- `IAlternativesService` — transaction → alternative product recommendation
- `ITokenService` — JWT generate/validate/refresh
- `IPasswordHasher`

---

#### Infrastructure Layer (`FundWise.Infrastructure`)

**AI Services (all use OpenAI SDK):**
- `WhisperVoiceService` implements `IVoiceService` — audio → Whisper STT → GPT-4o extraction prompt → structured JSON
- `OpenAiOcrService` implements `IOcrService` — image → GPT-4o Vision → structured JSON (this doubles as OCR avoiding a separate Azure/Google dependency)
- `OpenAiCategorizationService` implements `ICategorizationService` — merchant name prompt → category enum
- `OpenAiAlternativesService` implements `IAlternativesService` — looks up mock dataset, optionally enriches with GPT

**SMS Parsers:**
- `RegexSmsParser` — bank-specific regex patterns (CIB, Banque Misr, NBE as starting point)
- `LlmSmsParser` — GPT-4o fallback for unrecognized bank formats

**QR Parser:**
- `QrContentParser` — URL fetch + structured parse for known formats

**Security:**
- `JwtTokenService` implements `ITokenService`
- `BcryptPasswordHasher` implements `IPasswordHasher`

**Storage:**
- `AzureBlobStorageService` or `LocalFileStorageService` (configurable) for uploaded receipt images

---

#### Persistence Layer (`FundWise.Persistence`)

- `FundWiseDbContext` (EF Core)
- Entity configurations (IEntityTypeConfiguration<T> for each entity)
- Generic `Repository<T>` implementation
- `UnitOfWork` implementation
- Initial migration
- Seed data for `AlternativeProduct` mock dataset (15–30 entries)

---

#### API Layer (`FundWise.API`)

**Controllers:**
- `AuthController` — POST /api/auth/register, POST /api/auth/login, POST /api/auth/refresh, POST /api/auth/logout
- `TransactionsController` — GET/POST/PUT/DELETE /api/transactions
- `VoiceController` — POST /api/voice/capture (multipart audio upload)
- `OcrController` — POST /api/ocr/receipt (multipart image upload)
- `QrController` — POST /api/qr/parse
- `SmsController` — POST /api/sms/parse
- `DashboardController` — GET /api/dashboard/summary, GET /api/dashboard/categories, GET /api/dashboard/recent
- `RecommendationsController` — GET /api/recommendations/{transactionId}

**Middleware:**
- `GlobalExceptionHandlingMiddleware` — maps domain exceptions → HTTP status codes
- `RequestLoggingMiddleware`

**Setup:**
- Swagger/OpenAPI with JWT bearer support
- CORS policy (allow all origins for dev)
- Rate limiting (basic, per user)
- Health check endpoint

---

### Phase B — Flutter App

---

#### Project Structure (per blueprint)

Full `lib/` structure as defined in the blueprint under section 2.2.

**Packages:**
- `flutter_bloc` — state management
- `go_router` — navigation/routing
- `dio` — HTTP client
- `hive` / `hive_flutter` — local storage (offline cache, tokens)
- `flutter_sound` or `record` — audio recording for voice capture
- `camera` + `image_picker` — receipt photo
- `mobile_scanner` — QR scanning
- `fl_chart` — dashboard charts
- `permission_handler` — SMS/camera/mic permissions
- `flutter_sms_inbox` — read SMS on Android
- `freezed` + `json_serializable` — immutable models
- `get_it` — dependency injection
- `injectable` — DI code generation
- `intl` — date/currency formatting (EGP)

**Screens (Pages):**
1. Splash / Onboarding (3 screens explaining value + permissions)
2. Register / Login
3. Home Permissions Prompt (mic, camera, SMS)
4. Dashboard (main screen)
5. Add Transaction (hub: Voice, OCR, QR, SMS, Manual tabs)
6. Voice Capture (record → confirm card)
7. Receipt Scanner (camera → confirm card)
8. QR Scanner (camera overlay → confirm card)
9. SMS Import (list of detected SMS → confirm)
10. Transaction Detail / Edit
11. Recommendations Card (shown inline on transaction detail)
12. Settings

**Design system:**
- Dark-mode-first, EGP-localized
- Primary color: a deep indigo/violet gradient (premium, modern)
- Typography: Inter / Plus Jakarta Sans from Google Fonts
- Glassmorphism cards for dashboard
- Micro-animations (capture channels, loading states)

---

## Verification Plan

### Automated Tests
```bash
# Backend unit tests
dotnet test tests/FundWise.UnitTests/

# Backend integration tests (requires SQL Server)
dotnet test tests/FundWise.IntegrationTests/

# Flutter tests
flutter test
```

### Manual Verification
1. Run backend via `dotnet run` from `src/FundWise.API/`
2. Open Swagger UI at `https://localhost:7xxx/swagger` — verify all endpoints documented
3. Register a user, login, get JWT, call dashboard endpoint
4. Run Flutter app on Android emulator — verify auth flow, dashboard, capture screens
5. Test voice capture end-to-end (requires OpenAI key)
6. Test receipt OCR with a sample receipt image
7. Test SMS parsing with a mock bank SMS string

---

## Implementation Order

1. Backend scaffold (solution + all projects + NuGet packages)
2. Domain entities + enums + interfaces
3. Persistence (DbContext + migrations + seed data)
4. Application layer (Result pattern + behaviors + all commands/queries)
5. Infrastructure (JWT, password hashing, AI services, SMS parsers)
6. API controllers + middleware + Swagger
7. Flutter app scaffold (packages + structure)
8. Flutter: Auth screens + BLoC
9. Flutter: Dashboard screen + BLoC
10. Flutter: Capture screens (Voice, OCR, QR, SMS, Manual)
11. Flutter: Recommendations screen
12. Flutter: Settings screen
13. Polish: animations, empty states, error handling
14. README update with setup instructions
