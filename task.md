# FundWise AI — Task List

## Phase A: Backend (ASP.NET Core)

### A1 — Solution Scaffold
- [ ] Create .NET solution + 7 src projects + 3 test projects
- [ ] Add all NuGet package references
- [ ] Set up project-to-project references

### A2 — Domain Layer
- [ ] Enums: TransactionCategory, CaptureSource
- [ ] Value Objects: Money, Email
- [ ] Entities: User, Transaction, MerchantCategoryMemory, AlternativeProduct
- [ ] Interfaces: IRepository<T>, IUnitOfWork
- [ ] Domain Exceptions

### A3 — Persistence Layer
- [ ] FundWiseDbContext + entity configurations
- [ ] Generic Repository<T> + UnitOfWork
- [ ] Initial EF Core migration
- [ ] Seed data for AlternativeProduct mock dataset (15–30 entries)

### A4 — Application Layer
- [ ] Result<T> pattern + Error type
- [ ] MediatR pipeline behaviors (Validation, Logging, Transaction)
- [ ] External service interfaces (IVoiceService, IOcrService, ISmsParser, IQrParser, ICategorizationService, IAlternativesService, ITokenService, IPasswordHasher)
- [ ] Feature: Authentication (Register, Login, RefreshToken, Logout, GetCurrentUser)
- [ ] Feature: Transactions (Create, Update, Delete, Confirm, GetAll, GetById)
- [ ] Feature: Voice capture (ProcessVoiceCapture)
- [ ] Feature: OCR receipt (ProcessReceiptOCR)
- [ ] Feature: SMS parse (ParseSmsTransaction)
- [ ] Feature: QR parse (ParseQrCode)
- [ ] Feature: Categorization (Categorize, CorrectCategory)
- [ ] Feature: Dashboard (GetSummary, GetCategoryBreakdown, GetRecentTransactions)
- [ ] Feature: Recommendations (GetAlternativeForTransaction)
- [ ] Feature: Settings (GetUserSettings, UpdateUserSettings)

### A5 — Infrastructure Layer
- [ ] JwtTokenService
- [ ] BcryptPasswordHasher
- [ ] WhisperVoiceService (OpenAI Whisper + GPT-4o extraction)
- [ ] OpenAiOcrService (GPT-4o Vision for receipt OCR)
- [ ] OpenAiCategorizationService
- [ ] OpenAiAlternativesService (mock dataset lookup + optional GPT enrichment)
- [ ] RegexSmsParser (CIB, Banque Misr, NBE formats)
- [ ] LlmSmsParser (GPT-4o fallback)
- [ ] QrContentParser
- [ ] LocalFileStorageService (for uploaded receipt images)
- [ ] DependencyInjection.cs registrations

### A6 — API Layer
- [ ] Program.cs setup (Swagger, CORS, Auth, Rate limiting, Health checks)
- [ ] GlobalExceptionHandlingMiddleware
- [ ] AuthController (register, login, refresh, logout)
- [ ] TransactionsController (CRUD + confirm)
- [ ] VoiceController (multipart audio upload)
- [ ] OcrController (multipart image upload)
- [ ] QrController (QR parse)
- [ ] SmsController (SMS parse)
- [ ] DashboardController (summary, categories, recent)
- [ ] RecommendationsController
- [ ] appsettings.json with all config placeholders

### A7 — Docker
- [ ] Dockerfile for API
- [ ] docker-compose.yml (SQL Server + API)
- [ ] .env.example file

## Phase B: Flutter App

### B1 — Project Scaffold
- [ ] Flutter project create + package dependencies (pubspec.yaml)
- [ ] Directory structure (core, config, data, domain, features)
- [ ] Design system (theme, colors, typography)

### B2 — Auth Screens
- [ ] Splash screen
- [ ] Onboarding screens (3 slides)
- [ ] Register screen + BLoC
- [ ] Login screen + BLoC
- [ ] Permissions prompt screen

### B3 — Dashboard
- [ ] Dashboard screen + BLoC
- [ ] Category breakdown chart (fl_chart)
- [ ] Recent transactions list
- [ ] Empty/loading/error states

### B4 — Capture Screens
- [ ] Add Transaction hub screen
- [ ] Voice Capture screen + BLoC
- [ ] Receipt OCR screen + BLoC
- [ ] QR Scanner screen + BLoC
- [ ] SMS Import screen + BLoC
- [ ] Manual Entry screen + BLoC
- [ ] Confirmation card (shared widget)

### B5 — Detail & Recommendations
- [ ] Transaction detail/edit screen
- [ ] Recommendations card widget

### B6 — Settings
- [ ] Settings screen + BLoC

### B7 — Infrastructure
- [ ] ApiClient (dio)
- [ ] Auth interceptor (JWT refresh)
- [ ] go_router setup + route guards
- [ ] GetIt DI setup
- [ ] Hive local storage (tokens, cache)

### B8 — Polish
- [ ] Animations (capture channels, loading)
- [ ] Empty states + error handling
- [ ] README with setup instructions
