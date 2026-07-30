# 🏢 AppPortal

> Integrated Enterprise Portal — Internal employee data management system.

## 📁 Struktur Project

```
AppPortal.App/
├── Frontend/         # 🖥️ Blazor Web App (Server + WASM)
│   ├── README.md     # Dokumentasi teknis Frontend
│   └── src/
│       ├── Web/          # Host/Server project (.NET 10, DevExpress, YARP)
│       └── Web.Client/   # Blazor WebAssembly Client
├── Backend/          # 📡 API Backend (coming soon)
├── LICENSE           # © Hak cipta dilindungi
└── README.md         # ← File ini (overview project)
```

## 🚀 Cara Menjalankan

### ⚡ Via Batch File (Windows)

Jalankan **`AppPortal - Run.bat`** di root folder, lalu pilih environment:

1. **Development** — hot reload, mock API
2. **Local** — debug, localhost API
3. **Staging** — pre-production
4. **Production** — live deployment

### 🧑‍💻 Manual (Terminal)

```bash
cd Frontend/src/Web
dotnet restore
dotnet run --no-launch-profile
```

> Dokumentasi teknis lengkap ada di [`Frontend/README.md`](Frontend/README.md).

## 🛠️ Tech Stack

| Layer | Teknologi |
|-------|-----------|
| **Frontend** | Blazor Interactive Auto (.NET 10) |
| **UI** | DevExpress Blazor 25.2 + Tailwind CSS + Flowbite |
| **API Proxy** | YARP ReverseProxy |
| **Backend** | _Coming soon_ |

## 📄 Lisensi

**All Rights Reserved** — © 2026 Bumitama Gunajaya Agro. Lihat file [LICENSE](LICENSE).

---

**Last Update:** July 28, 2026 by Daniel
