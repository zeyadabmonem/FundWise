# FundWise AI (FinPilot) 🚀

> **Automation-first Personal Finance Platform for Egypt & MEA**  
> *Track spending effortlessly through Voice, Receipt OCR, QR codes, and Bank SMS.*

---

## 🌟 Features

- **🎙️ Voice Capture:** Speak your expenses naturally in Egyptian Arabic or English (powered by OpenAI Whisper + GPT-4o).
- **📸 Receipt OCR:** Snap a photo of any receipt to extract merchant, total, and items (powered by GPT-4o Vision).
- **📱 Egyptian Bank SMS Auto-Detection:** Regex-based automatic parsing of transaction SMS from CIB, National Bank of Egypt (NBE), Banque Misr, Vodafone Cash, and InstaPay.
- **🔳 QR Code Scanner:** Instant parsing of digital receipt QR codes.
- **💡 Smart Category Memory:** AI auto-categorization into 9 categories + user learning memory.
- **📊 Interactive Dashboard:** Monthly spending breakdown pie chart, recent transactions, and category metrics.

---

## 🏗️ Architecture

```
FundWise/
├── src/                          # ASP.NET Core 8 Web API (Clean Architecture)
│   ├── FundWise.Domain/          # Core Domain Entities & Interfaces
│   ├── FundWise.Application/     # CQRS (MediatR), DTOs, & Services
│   ├── FundWise.Infrastructure/  # OpenAI, Whisper, SMS & Storage Services
│   ├── FundWise.Persistence/     # EF Core, DbContext, & Mock Data Seeders
│   └── FundWise.API/             # Controllers, Swagger UI, & JWT Middleware
├── mobile/                       # Flutter Mobile App (Clean Architecture + BLoC)
│   ├── lib/core/                 # DI (GetIt), Router (go_router), Theme, ApiClient
│   └── lib/features/             # Auth, Dashboard, Transactions, & Capture Features
├── tests/                        # xUnit Unit, Integration & Functional Tests
├── docker-compose.yml            # SQL Server + API Docker setup
└── Dockerfile                    # API Docker image configuration
```

---

## 🚀 Quick Start Guide

### 1️⃣ Backend API (.NET 8)

#### Option A: Local dotnet CLI
```bash
# Run the API server
dotnet run --project src/FundWise.API/FundWise.API.csproj --launch-profile http
```
- Open Swagger UI in browser: **[http://localhost:5207](http://localhost:5207)**

#### Option B: Docker Compose
```bash
docker-compose up --build
```

---

### 2️⃣ Mobile App (Flutter)

```bash
# Navigate to mobile app directory
cd mobile

# Fetch dependencies
flutter pub get

# Run on connected device or emulator
flutter run
```

---

## 🧪 Testing

```bash
# Run all unit and integration tests
dotnet test FundWise.slnx
```

---

## 👥 Authors & Team
- **Zeyad Abdelmonem Abdo**
- **Mohamed Atef**
- **Adam Hamdy**