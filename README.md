# 🏢 AppPortal

> Integrated Enterprise Portal — Modular application platform for internal business applications.

## 📋 Overview

AppPortal is an enterprise-grade portal platform built with **Blazor InteractiveAuto .NET 10** using **Vertical Slice Architecture (VSA)**. It serves as a **Base App (CorePortal)** that hosts multiple sub-applications with dynamic routing and display modes (embedded/page).

### Key Features

🔐 **Authentication** — JWT token-based login with auto-refresh  
📊 **Dashboard** — Dynamic app grid sourced from API  
👥 **Employee Management** — Full CRUD with DevExpress Grid  
⚖️ **Legal (App 1)** — Sub-application dashboard  
🛡️ **Sparta (App 2)** — Sub-application dashboard  
📱 **PWA** — Service worker, manifest, offline fallback  
📡 **Offline-First** — IndexedDB + Sync Engine  
🌐 **Network Aware** — Real-time connectivity monitoring  

## 🏗️ Project Structure

```
├── Frontend/                        # 🖥️ Blazor InteractiveAuto
│   └── src/
│       ├── Web/                     # Server — Razor Components + Host
│       └── Web.Client/              # WASM — Service Layer
│
├── .gitignore
└── README.md
```

## 🚀 Run

```bash
cd Frontend/src/Web
dotnet watch
```

Visit `http://localhost:5000`

## 🧱 Tech Stack

- **.NET 10** — Blazor InteractiveAuto (Server + WASM)
- **DevExpress Blazor 25.2** — 70+ UI Components
- **Polly 8.5** — Resilience pipelines
- **YARP** — API Gateway reverse proxy
- **IndexedDB** — Client-side offline storage
- **Tailwind CSS + Flowbite** — Styling

## 📁 Features (Vertical Slices)

| Feature | Description | Route | Type |
|---------|-------------|-------|------|
| Auth | Login, Logout, AuthGuard | `/login` | CorePortal |
| Dashboard | App grid from API | `/dashboard` | CorePortal |
| Employee Mgmt | Full CRUD with grid | `/employee-data/*` | CorePortal |
| Legal | Dashboard example | `/legal` | Sub-App |
| Sparta | Dashboard example | `/sparta` | Sub-App |
| Dynamic | Runtime modules | `/app/{slug}` | Runtime |

## 🔄 Architecture Highlights

- **InteractiveAuto**: Server prerendering → WASM hydration
- **Vertical Slices**: Each feature is self-contained
- **App Display Modes**: Page (Blazor) or Embedded (iframe)
- **Offline Engine**: Save Local, Sync Later with FIFO queue
- **Resilience**: Polly Retry 3x + Circuit Breaker 30s
- **PWA**: Cache-first assets, network-first API

## 📄 License

Internal use — All rights reserved.
