# Implementation Plan — Fix Integration & Align API Contracts

Review of system requirements (SRS, Architecture, PRD, Sprint Plan, UI/UX docs) revealed a critical route mismatch between Flutter data sources and ASP.NET Core API endpoints, which caused the `Failed to load dashboard data.` error on app startup.

## User Review Required

> [!IMPORTANT]
> **Git Branching Strategy**: All changes will be made on feature branches, committed incrementally, and merged into `develop` using non-fast-forward (`--no-ff`) merges. Merge to `main` will happen after user approval and completion of the testing plan.

> [!NOTE]
> **Root Cause identified**: `DashboardRemoteDatasource.dart` called `/transactions/dashboard` instead of the actual ASP.NET Core API route `/dashboard/summary`.

---

## Proposed Changes

### Mobile App (Flutter)

#### [MODIFY] [dashboard_remote_datasource.dart](file:///c:/Users/User/Documents/GitHub/FundWise/mobile/lib/features/dashboard/data/datasources/dashboard_remote_datasource.dart)
- Update endpoint path from `/transactions/dashboard` to `AppConfig.baseUrl + '/dashboard/summary'` (`/dashboard/summary`).
- Fix JSON parsing for category breakdown and recent transactions to safely parse enum integers/strings and DateTime formats returned by .NET API.

#### [MODIFY] [app_config.dart](file:///c:/Users/User/Documents/GitHub/FundWise/mobile/lib/core/config/app_config.dart)
- Verify and update all `ApiEndpoints` definitions to match backend controllers:
  - `dashboardSummary` = `/dashboard/summary`
  - `recommendations` = `/recommendations`

#### [MODIFY] [capture_bloc.dart](file:///c:/Users/User/Documents/GitHub/FundWise/mobile/lib/features/capture/presentation/bloc/capture_bloc.dart)
- Update endpoint paths to use `ApiEndpoints` constants (`/voice/capture`, `/ocr/receipt`, `/sms/parse`, `/qr/parse`).

---

### Backend API (.NET 8)

#### [MODIFY] [Program.cs](file:///c:/Users/User/Documents/GitHub/FundWise/src/FundWise.API/Program.cs)
- Ensure CORS policy allows requests from all origins (including Flutter Web dev ports).
- Ensure JWT Bearer authentication scheme and error handling return consistent JSON problem details.

---

## Git Workflow Execution Plan

1. **Branch Creation**: Create `feature/fix-dashboard-integration` from `develop`.
2. **Implementation & Commits**: Apply fixes with atomic commits.
3. **Merge to Develop**: Merge `feature/fix-dashboard-integration` into `develop` with `--no-ff`.
4. **Verification**: Run `dotnet run` (Backend) and `flutter run -d chrome` (Mobile) and test full login -> dashboard flow.

---

## Verification Plan

### Automated Tests
- `dotnet test` to ensure all 22 Unit/Integration backend tests pass.
- `flutter analyze` to ensure zero compilation errors in Flutter mobile project.

### Manual Verification
- Test registration/login using `zeyad@fundwise.ai` / `FundWise@2026`.
- Verify Dashboard loads successfully with real data from backend without `Failed to load dashboard data.` error.
- Verify adding a manual transaction updates dashboard totals automatically.
