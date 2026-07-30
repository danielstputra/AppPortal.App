# 🌐 AppPortal — Blazor Frontend

> Enterprise Portal Platform built with **Blazor InteractiveAuto .NET 10**, DevExpress UI Components, and Tailwind CSS.

## 🏗️ Architecture

```
Frontend/
└── src/
    ├── Web/                         # 🖥️ Server — Razor Components + Host
    │   ├── Features/                # 📁 Vertical Slices (Auth, Dashboard, Employee, Legal, Sparta)
    │   ├── Infrastructure/          # 🔧 Cross-cutting (Http, Security, Localization)
    │   ├── UI/Base/                 # ⛔ LOCKED — DevExpress App* components
    │   ├── UI/Shared/               # 🔄 Portal components (ConnectivityBanner, SyncStatus, DynamicModule)
    │   ├── Components/              # 🏠 App.razor, Routes.razor, Layout
    │   ├── Middleware/              # 🛡️ Server middleware (Security, Validation, Exception)
    │   └── wwwroot/                 # 📦 Static assets + PWA files
    │
    └── Web.Client/                  # 🌐 WASM — Service Layer
        ├── Infrastructure/          # Auth, Http, Offline services
        ├── Features/                # Module services (Sparta, Legal)
        └── Models/                  # Vendor DTOs
```

## 🚀 Run the Project

```bash
cd Frontend/src/Web
dotnet watch
```

- **HTTP:** `http://localhost:5000` (development)
- **HTTPS:** `https://localhost:5001` (production only)

## 🧱 Tech Stack

| Technology | Purpose |
|-----------|---------|
| .NET 10 | Framework |
| Blazor InteractiveAuto | Server + WASM dual rendering |
| DevExpress Blazor 25.2 | UI Component Library |
| Tailwind CSS + Flowbite | Styling |
| Polly 8.5 | Resilience (Retry + Circuit Breaker) |
| YARP Reverse Proxy | API Gateway |
| IndexedDB (JSInterop) | Client-side offline storage |

## 📁 Application Features

| Feature | Route | App |
|---------|-------|-----|
| Auth (Login/Logout) | `/login` | CorePortal (Base) |
| Dashboard | `/dashboard` | CorePortal (Base) |
| Employee Management | `/employee-data/*` | CorePortal (Base) |
| Legal Dashboard | `/legal` | App 1 |
| Sparta Dashboard | `/sparta` | App 2 |
| Dynamic Module | `/app/{slug}` | Runtime |

## 🔄 Offline-First Architecture

- **IndexedDB** — Emergency database for offline data storage
- **Sync Engine** — FIFO queue with auto-sync on reconnect
- **Network Monitor** — Real-time online/offline detection
- **Connectivity Banner** — Visual indicator for offline status
- **Conflict Resolver** — Last-Write-Wins strategy

## 📦 NuGet Packages

- `DevExpress.Blazor` 25.2.5
- `Polly` 8.5.2
- `Yarp.ReverseProxy` 2.3.0
- `Microsoft.Extensions.Http` 10.0.10

## ✅ Build

```bash
dotnet build
# Build succeeded. 0 Error(s), 0 Warning(s)
```
