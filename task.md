# FundWise AI — Task List

## Phase A: Backend (ASP.NET Core) — ✅ COMPLETED

### A1 — Solution Scaffold ✅
- [x] Create .NET solution + 7 src projects + 3 test projects
- [x] Add all NuGet package references
- [x] Set up project-to-project references

### A2 — Domain Layer ✅
- [x] Enums: TransactionCategory, CaptureSource
- [x] Value Objects: Money, Email
- [x] Entities: User, Transaction, MerchantCategoryMemory, AlternativeProduct
- [x] Interfaces: IRepository<T>, IUnitOfWork
- [x] Domain Exceptions

### A3 — Persistence Layer ✅
- [x] FundWiseDbContext + entity configurations
- [x] Generic Repository<T> + UnitOfWork
- [x] Seed data for AlternativeProduct mock dataset (17 Egyptian product pairs)

### A4 — Application Layer ✅
- [x] Result<T> pattern + Error type
- [x] MediatR pipeline behaviors (Validation, Logging)
- [x] External service interfaces (IVoiceService, IOcrService, ISmsParser, IQrParser, ICategorizationService, IAlternativesService, ITokenService, IPasswordHasher)
- [x] Feature: Authentication (Register, Login, RefreshToken, Logout, GetCurrentUser)
- [x] Feature: Transactions (Create, Update, Delete, Confirm, GetAll, GetById)
- [x] Feature: Voice capture (ProcessVoiceCapture)
- [x] Feature: OCR receipt (ProcessReceiptOCR)
- [x] Feature: SMS parse (ParseSmsTransaction)
- [x] Feature: QR parse (ParseQrCode)
- [x] Feature: Categorization (Categorize, CorrectCategory)
- [x] Feature: Dashboard (GetSummary, GetCategoryBreakdown, GetRecentTransactions)
- [x] Feature: Recommendations (GetAlternativeForTransaction)
- [x] Feature: Settings (GetUserSettings, UpdateUserSettings)

### A5 — Infrastructure Layer ✅
- [x] JwtTokenService
- [x] BcryptPasswordHasher
- [x] WhisperVoiceService (OpenAI Whisper + GPT-4o extraction)
- [x] OpenAiOcrService (GPT-4o Vision for receipt OCR)
- [x] OpenAiCategorizationService
- [x] OpenAiAlternativesService (mock dataset lookup + projected savings)
- [x] RegexSmsParser (CIB, Banque Misr, NBE, Vodafone Cash, InstaPay formats)
- [x] QrContentParser
- [x] LocalFileStorageService (for uploaded receipt images)
- [x] DependencyInjection.cs registrations

### A6 — API Layer ✅
- [x] Program.cs setup (Swagger UI with Bearer Auth, CORS, Health checks)
- [x] GlobalExceptionHandlingMiddleware
- [x] AuthController (register, login, refresh, logout, me)
- [x] TransactionsController (CRUD + confirm)
- [x] VoiceController (multipart audio upload)
- [x] OcrController (multipart image upload)
- [x] QrController (QR parse)
- [x] SmsController (SMS parse)
- [x] DashboardController (summary, categories, recent)
- [x] RecommendationsController
- [x] appsettings.json with all config placeholders

### A7 — Docker ✅
- [x] Dockerfile for API
- [x] docker-compose.yml (SQL Server + API)

### A8 — Automated Testing ✅
- [x] xUnit test suite (Passed: 3/3)

---

## Phase B: Flutter App — ✅ COMPLETED

### B1 — Project Scaffold ✅
- [x] Flutter project create + package dependencies (pubspec.yaml)
- [x] Directory structure (core, config, data, domain, features)
- [x] Design system (theme, dark mode, custom palette, typography)

### B2 — Auth Screens ✅
- [x] Splash screen with token routing
- [x] Register screen + AuthBloc
- [x] Login screen + AuthBloc
- [x] Auto token verification

### B3 — Dashboard ✅
- [x] Dashboard screen + DashboardBloc
- [x] Category breakdown pie chart (fl_chart)
- [x] Monthly total spending card
- [x] Recent transactions list with status badges
- [x] Refresh indicator & error handling

### B4 — Capture Screens ✅
- [x] Add Transaction hub screen
- [x] Voice Capture screen + pulse animation + CaptureBloc
- [x] Receipt OCR screen + Camera/Gallery picker + CaptureBloc
- [x] QR Scanner screen + MobileScanner + CaptureBloc
- [x] SMS Import screen + paste & background auto mode + CaptureBloc
- [x] Manual Entry screen + category chip selector & date picker + CaptureBloc
- [x] Confirmation card with AI confidence rating (shared widget)

### B5 — Detail & Recommendations ✅
- [x] Transaction detail screen with category themes and confirm action

### B7 — Data & Infrastructure ✅
- [x] ApiClient (Dio with auth interceptor & refresh)
- [x] Remote datasources for Auth, Dashboard, Transactions
- [x] Repositories & Domain Usecases (Clean Architecture + Dartz Either)
- [x] go_router setup + route guards
- [x] GetIt DI container setup
