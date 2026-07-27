# 🌐 AppPortal — Blazor Frontend

> Integrated employee data management system built with Blazor Interactive Auto, DevExpress UI Components, and Tailwind CSS / Flowbite.

---

## 📋 Table of Contents

- [Arsitektur & Struktur Proyek](#-arsitektur--struktur-proyek)
- [Tech Stack](#-tech-stack)
- [🛡️ Enterprise Security](#-enterprise-security)
- [🌐 Localization (EN / ID)](#-localization-en--id)
- [📡 HTTP Client Infrastructure](#-http-client-infrastructure)
- [🗃️ Mock Data System](#-mock-data-system)
- [🔁 API Proxy (YARP Reverse Proxy)](#-api-proxy-yarp-reverse-proxy)
- [Base Components (UI Abstraction Layer)](#-base-components-ui-abstraction-layer)
  - [AppButton](#1-appbutton)
  - [AppGrid](#2-appgrid)
  - [AppTextBox](#3-apptextbox)
  - [AppComboBox](#4-appcombobox)
- [Feature-Based Architecture](#-feature-based-architecture)
- [🌐 Localization (EN / ID)](#-localization-en--id)
- [Cara Menjalankan](#-cara-menjalankan)
- [Panduan Pengembangan](#-panduan-pengembangan)
- [Troubleshooting](#-troubleshooting)

---

## 🏗️ Arsitektur & Struktur Proyek

Proyek ini menggunakan **Feature-Based Architecture (Feature Slicing)** — kode diorganisir berdasarkan fitur, bukan berdasarkan tipe file (models, views, controllers).

```
Frontend/
├── run.bat                                # 🚀 Environment launcher (numbered menu)
│
└── src/
    │
    ├── Web.sln                          # Solution file
│
├── Web/                             # 🖥️ Main Project (Server + Shared)
│   ├── Program.cs                        # Entry point, DI, middleware
│   ├── Web.csproj                   # Dependencies + Yarp.ReverseProxy
│   (appsettings.json)                # Proxy routes in "ReverseProxy" section
│   │
│   ├── Components/                       # 🧩 Root components
│   │   ├── App.razor                     # Root HTML template (Tailwind CDN, DevExpress CSS)
│   │   ├── Routes.razor                  # Router (Interactive Auto)
│   │   ├── _Imports.razor                # Global using directives
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor          # Sidebar + Header + Content area
│   │   │   ├── MainLayout.razor.css      # Drawer overrides
│   │   │   ├── NavMenu.razor             # Searchable sidebar navigation
│   │   │   └── Header.razor              # Fixed top header
│   │   └── Pages/
│   │       ├── Home.razor                # Dashboard page
│   │       └── Error.razor               # Error page
│   │
│   ├── Models/
│   │   └── Employee.cs                   # Employee entity + StatusKaryawan enum
│   │
│   ├── Middleware/                        # 🛡️ Security middleware
│   │   ├── SecurityHeadersMiddleware.cs  #   CSP, X-Frame-Options, X-Content-Type-Options
│   │   └── RequestValidationMiddleware.cs #   XSS & SQL injection blocker
│   │
│   ├── Services/
│   │   ├── Http/                          # 📡 API Client infrastructure
│   │   │   ├── IApiClient.cs             #   HTTP client interface
│   │   │   ├── ApiClient.cs              #   Real HTTP implementation (Bearer, API Key, HMAC, Basic auth)
│   │   │   ├── MockApiClient.cs          #   Mock implementation (reads from JSON)
│   │   │   ├── ApiResponse.cs            #   Enterprise response model (RFC 7807 errors + pagination)
│   │   │   └── AuthConfig.cs             #   Auth config (Bearer, API Key, HMAC-SHA256, Basic)
│   │   ├── IEmployeeService.cs           #   Service abstraction
│   │   ├── MockEmployeeService.cs        #   Mock data via IApiClient (reads employees.json)
│   │   └── LocalizationService.cs        #   🌐 Translation engine
│   │
│   ├── UI/                               # 📁 Feature-Based Organization
│   │   ├── _Imports.razor
│   │   │
│   │   ├── Base/                         # 🎯 BASE COMPONENTS (Wrapper)
│   │   │   ├── AppButton.razor           #   Button with variants
│   │   │   ├── AppGrid.razor             #   Data grid wrapper
│   │   │   ├── AppTextBox.razor          #   Text input wrapper
│   │   │   ├── AppComboBox.razor         #   ComboBox wrapper
│   │   │   └── BaseTypes.cs              #   Enums (AppButtonVariant, AppButtonSize)
│   │   │
│   │   └── Features/                     # 📁 Feature Modules
│   │       ├── EmployeeManagement/
│   │       │   ├── Pages/
│   │       │   │   └── EmployeeData.razor           # @page "/employee-data"
│   │       │   └── Components/
│   │       │       ├── FilterBar.razor              # DxComboBox filters + DxSearchBox
│   │       │       ├── EmployeeGrid.razor           # Desktop DxGrid
│   │       │       ├── EmployeeCardList.razor       # Mobile card layout
│   │       │       └── StatusBadge.razor            # Status pill badges
│   │       │
│   │       └── ExampleForm/
│   │           └── Pages/
│   │               └── ExampleForm.razor            # @page "/example-form" (Demo)
│   │
│   └── wwwroot/
│       ├── app.css                       # Global styles
│       ├── css/
│       │   └── app-portal.css          # DevExpress → Tailwind CSS overrides
│       ├── mock-data/                      # 🗃️ Mock API data (JSON)
│       │   └── employees.json            #   Mock employee records
│       └── translations/                  # 🌐 Localization JSON files
│           ├── id.json                    #   Indonesian translations
│           └── en.json                    #   English translations
│
├── Web.Client/                      # 🌐 WebAssembly Client (Auto-generated)
│   ├── Program.cs
│   └── _Imports.razor
│
└── README.md                             # 📘 Dokumentasi ini
```

---

## ⚡ Tech Stack

| Teknologi | Versi | Fungsi |
|-----------|-------|--------|
| **.NET** | 9.0 | Runtime / SDK |
| **Blazor** | Interactive Auto | Rendering mode (Server + WASM) |
| **DevExpress.Blazor** | 25.1.12 | Komponen UI (Grid, ComboBox, Drawer, SearchBox) |
| **Tailwind CSS** | 3.x (CDN) | Utility-first CSS framework |
| **Flowbite** | 2.5.2 (CDN) | Komponen UI berbasis Tailwind |

---

## 🛡️ Enterprise Security

The application implements defense-in-depth security at multiple layers.

### Security Headers (Middleware)

Every HTTP response includes these headers via `SecurityHeadersMiddleware`:

| Header | Value | Purpose |
|--------|-------|---------|
| `Content-Security-Policy` | `default-src 'self'; script-src 'self' https://cdn.tailwindcss.com ...` | Prevents XSS by restricting resource origins |
| `X-Content-Type-Options` | `nosniff` | Prevents MIME-type sniffing |
| `X-Frame-Options` | `DENY` | Prevents clickjacking |
| `X-XSS-Protection` | `1; mode=block` | Enables browser XSS filter |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Controls referrer header |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()...` | Restricts browser API access |
| `Cache-Control` | `no-store, no-cache, must-revalidate` | Prevents sensitive data caching |

### Input Validation (Middleware)

`RequestValidationMiddleware` blocks common attack patterns at the HTTP pipeline level:
- **XSS patterns**: `<script`, `javascript:`, `onerror=`, `onload=`
- **SQL injection patterns**: `UNION SELECT`, `DROP TABLE`, `INSERT INTO`

### Additional Protections

| Mechanism | Status | Description |
|-----------|--------|-------------|
| **HTTPS Redirection** | ✅ `UseHttpsRedirection()` | Forces all traffic to HTTPS |
| **HSTS** | ✅ `UseHsts()` | HTTP Strict Transport Security (prod only) |
| **Anti-CSRF** | ✅ `UseAntiforgery()` | Anti cross-site request forgery tokens |
| **CORS** | ✅ `UseCors("ApiPolicy")` | Cross-Origin Resource Sharing policy |
| **Auth Token** | ✅ `SetAuthToken()` | Bearer token support in `ApiClient` |
| **Exception Handling** | ✅ Developer-friendly in dev, generic error page in prod |

### Future API Security (When Backend is Ready)

```csharp
// In Program.cs — uncomment when API is live
builder.Services.AddScoped<IApiClient, ApiClient>();
```

The `ApiClient` supports:
- Bearer token authentication via `SetAuthToken()`
- Automatic retry with exponential backoff (3 retries)
- Request timeout (30 seconds)
- Structured error handling via `ApiResponse<T>`

---

## 🔁 API Proxy (YARP Reverse Proxy)

The frontend uses **YARP (Yet Another Reverse Proxy)** to hide the backend API URL from the client side. When you inspect network requests in the browser, all API calls appear to go to the frontend domain — the backend URL is never exposed.

```
Browser Network Tab                 Frontend (YARP)                 Backend
┌─────────────────────┐          ┌──────────────────┐          ┌──────────────┐
│ GET /api/v1/employees│ ─────► │ /api/v1/*        │ ─────► │ https://      │
│ Host: app.mycompany │          │ matches proxy    │          │ backend.com/  │
│                      │          │ route            │          │ api/v1/       │
│ (Backend URL hidden!)│          │                  │          │ employees     │
└─────────────────────┘          └──────────────────┘          └──────────────┘
```

### Configuration

Proxy routes are defined in `appsettings.json` under the `"ReverseProxy"` section:

```json
"ReverseProxy": {
  "Routes": {
    "api-v1": {
      "ClusterId": "backend-api",
      "Match": { "Path": "/api/v1/{**catch-all}" }
    }
  },
  "Clusters": {
    "backend-api": {
      "Destinations": {
        "backend": { "Address": "https://your-api-server.com" }
      }
    }
  }
}
```

### How It Works

1. **`ApiClient`** makes requests to `/api/v1/employees` (relative URL — no backend domain exposed)
2. **`IHttpContextAccessor`** resolves the current origin (e.g. `https://app.mycompany.com`)
3. **`HttpClient.BaseAddress`** is set to the current origin so relative URLs resolve correctly
4. **YARP middleware** catches all requests matching `/api/v1/*` and forwards them to the configured backend
5. **Browser Network tab shows**: `https://app.mycompany.com/api/v1/employees` ✅
6. **Backend URL stays hidden**: `https://your-api-server.com` is never sent to the client ❌

### Registration (Program.cs)

```csharp
// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Middleware pipeline
app.MapReverseProxy();
```

### Mock Mode

When using `MockApiClient`, no proxy is needed — it reads directly from `wwwroot/mock-data/*.json`. The `ReverseProxy` config remains in `appsettings.json` and becomes active only when you switch to `ApiClient`.

---

## 📡 HTTP Client Infrastructure

A reusable, enterprise-grade HTTP client wrapper (`IApiClient`) that abstracts all API communication.

### Architecture

```
Components / Services
        │
        ▼
   IApiClient (interface)
        │
        ├── MockApiClient ───── Reads from wwwroot/mock-data/*.json
        │                          (used in development)
        │
        └── ApiClient ────────── Real HTTP calls via IHttpClientFactory
                                   (swap when backend is ready)
```

### IApiClient Interface

```csharp
public interface IApiClient
{
    // ─── Authentication ───
    void SetAuth(AuthConfig config);      // Bearer, API Key, HMAC, or Basic
    void ClearAuth();
    AuthConfig? CurrentAuth { get; }

    // ─── HTTP Methods ───
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null);
    Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);
    Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null);

    string BaseUrl { get; set; }
}
```

### 🔐 Authentication — 4 Methods

`AuthConfig` supports four enterprise authentication methods:

| Method | Code | Headers |
|--------|------|---------|
| **Bearer Token** | `AuthConfig.FromBearer("jwt...")` | `Authorization: Bearer {token}` |
| **API Key** | `AuthConfig.FromApiKey("key...")` | `X-API-Key: {key}` |
| **API Key + HMAC** | `AuthConfig.FromApiKeyWithSecret("key", "secret")` | `X-API-Key` + `X-API-Signature` (HMAC-SHA256) + `X-API-Timestamp` |
| **Basic Auth** | `AuthConfig.FromBasic("user", "pass")` | `Authorization: Basic {base64}` |

```csharp
// Example: API Key + HMAC-SHA256 signing
api.SetAuth(AuthConfig.FromApiKeyWithSecret("my-api-key", "my-api-secret"));

// The signature is computed per-request using:
//   HMAC-SHA256(timestamp + HTTP_METHOD + /path + bodyHash)
// This ensures request integrity — no tampering in transit.
```

### ApiResponse\<T\> — Enterprise Response Format

Every API call returns a standardized response that follows **RFC 7807 (Problem Details)** for errors and **Google-style pagination** for lists.

**Success response:**
```json
{
  "data": { ... },
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "total": 124,
    "totalPages": 13,
    "hasNext": true,
    "hasPrevious": false
  },
  "traceId": "00-abc123def456...",
  "timestamp": "2026-07-24T10:30:00Z",
  "isSuccess": true,
  "httpStatusCode": 200
}
```

**Error response (RFC 7807 Problem Details):**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "The request contains invalid fields.",
    "details": [
      { "field": "email", "message": "Email is required", "code": "REQUIRED" },
      { "field": "name", "message": "Name must be at least 3 characters", "code": "MIN_LENGTH" }
    ],
    "helpUrl": "https://docs.api.com/errors/validation-error"
  },
  "traceId": "00-abc123def456...",
  "timestamp": "2026-07-24T10:30:00Z",
  "isSuccess": false,
  "httpStatusCode": 400
}
```

**Usage in code:**
```csharp
var response = await api.GetAsync<List<Employee>>("/employees");

if (response.IsSuccess)
{
    var employees = response.Data;       // List<Employee>
    var page = response.Pagination;      // PaginationMeta?
}
else
{
    var error = response.Error;           // ApiErrorDetail
    var code = error.Code;                // "NOT_FOUND", "VALIDATION_ERROR", etc.
    var message = error.Message;          // Human-readable
    var details = error.Details;          // Field-level errors (for 400)
}

// TraceId and Timestamp are always available:
var traceId = response.TraceId;           // For debugging / support tickets
var timestamp = response.Timestamp;       // ISO 8601
```

### 🌍 Environment-Aware Error Handling

`ApiClient` and `MockApiClient` auto-detect the runtime environment from `IWebHostEnvironment` and adjust error detail visibility accordingly.

| Aspect | 🛠️ Development | 🚀 Production |
|--------|---------------|---------------|
| **Error Messages** | Full server message (e.g. `"Invalid JSON in 'employees.json': Unexpected token..."`) | Safe generic message (e.g. `"An internal server error occurred"`) |
| **Field Errors** | ✅ Preserved (field name, message, error code) | ❌ Stripped (may leak internal schema) |
| **Help URLs** | ✅ Preserved | ❌ Stripped (may leak architecture) |
| **Status Messages** | Detailed (e.g. `"Unauthorized — token may be expired"`) | Generic (e.g. `"Authentication required"`) |
| **File Paths** | ✅ Visible in error messages | ❌ Never exposed |
| **Stack Traces** | ✅ Visible (if sent by backend) | ❌ Never shown |

**How it works:**
```csharp
// Auto-detected — no manual config needed
EnvironmentMode = environment.IsDevelopment()
    ? ApiEnvironmentMode.Development
    : ApiEnvironmentMode.Production;

// In ExecuteAsync, before returning an error:
if (EnvironmentMode == ApiEnvironmentMode.Production)
    error = error.SanitizeForProduction();
```

**`ApiErrorDetail.SanitizeForProduction()`** maps error codes to safe messages:

```csharp
Code "NOT_FOUND"       → "The requested resource was not found."
Code "SERVER_ERROR"    → "An internal server error occurred. Please try again."
Code "NETWORK_ERROR"   → "A network error occurred. Please check your connection."
Code "VALIDATION_ERROR" → "The request contains invalid fields."
// ... all other codes → "An unexpected error occurred. Please try again."
```

This ensures **no internal server information, file paths, or database schema details** ever leak to the client in production.

### Key Features (ApiClient)

| Feature | Implementation |
|---------|---------------|
| **Retry with Backoff** | 3 retries at 200ms → 500ms → 1s intervals (5xx errors only) |
| **Timeout** | 30 seconds per request |
| **Auth** | Bearer, API Key, API Key+HMAC-SHA256, Basic |
| **Request Signing** | HMAC-SHA256 per-request (timestamp + method + path + body hash) |
| **Error Handling** | RFC 7807 Problem Details with field-level validation errors |
| **Environment Mode** | Auto-detected — Development shows full details, Production sanitizes |
| **Tracing** | Auto-extracts `X-Trace-Id` from response headers |
| **Pagination** | Google-style pagination metadata for list endpoints |
| **Logging** | Full request/response logging via `ILogger` |
| **Proxy Ready** | Uses relative URLs so YARP forwards through frontend |

### Switching to Real API

```csharp
// In Program.cs — change this:
builder.Services.AddScoped<IApiClient, MockApiClient>();

// To this:
builder.Services.AddScoped<IApiClient, ApiClient>();
```

---

## 🗃️ Mock Data System

Mock data is stored as JSON files in `wwwroot/mock-data/` and served by `MockApiClient`.

### How It Works

1. **JSON files** (`employees.json`) contain realistic mock data
2. **MockApiClient** reads these files from disk (not HTTP)
3. **MockEmployeeService** calls `IApiClient.GetAsync<List<Employee>>("/employees")`
4. The service processes the data (filtering, sorting) in memory

### File Structure

```
wwwroot/mock-data/
└── employees.json          # 10 mock employees with realistic Indonesian data
```

### Data Flow

```
Page (EmployeeData.razor)
    │
    ▼
IEmployeeService.GetEmployeesAsync()
    │
    ▼
MockEmployeeService
    │
    ▼
IApiClient.GetAsync<List<Employee>>("/employees")
    │
    ▼
MockApiClient — reads wwwroot/mock-data/employees.json
    │
    ▼
Returns ApiResponse<List<Employee>>
```

### Adding New Mock Data

1. Create a new JSON file in `wwwroot/mock-data/`:
   ```json
   [
     { "id": 1, "name": "Example", ... }
   ]
   ```
2. Load it via `IApiClient`:
   ```csharp
   var response = await _api.GetAsync<List<MyType>>("/my-data");
   ```

### Switching to Real Backend API

```csharp
// 1. Update DI registration in Program.cs
builder.Services.AddScoped<IApiClient, ApiClient>();

// 2. Optionally create ApiEmployeeService that calls the real API
builder.Services.AddScoped<IEmployeeService, ApiEmployeeService>();
```

The `IApiClient` abstraction means **zero changes** to your pages or business services — just swap the implementation in DI.

---

## 🎯 Base Components (UI Abstraction Layer)

Base Components adalah **lapisan abstraksi UI** yang membungkus komponen pihak ketiga (DevExpress) dan elemen HTML standar. Tujuannya:

> 🔒 **Jika terjadi perubahan desain atau migrasi library UI, cukup ubah 1 file (base component), tanpa menyentuh 100+ halaman.**

Setiap base component mendukung:

- ✅ **Attribute Splatting** — `[Parameter(CaptureUnmatchedValues = true)]` — atribut HTML apa pun bisa diteruskan
- ✅ **CssClass** — class CSS tambahan digabungkan secara otomatis dengan class default
- ✅ **Two-way binding** — pattern `@bind-Value` yang kompatibel dengan Blazor

---

### 1. AppButton

**Lokasi:** `UI/Base/AppButton.razor`

Tombol serbaguna dengan 4 variant dan 3 ukuran.

#### Parameter

| Parameter | Tipe | Default | Deskripsi |
|-----------|------|---------|-----------|
| `Text` | `string?` | `null` | Teks tombol |
| `OnClick` | `EventCallback` | - | Event saat diklik |
| `Variant` | `AppButtonVariant` | `Primary` | `Primary`, `Secondary`, `Danger`, `Outline` |
| `Size` | `AppButtonSize` | `Medium` | `Small`, `Medium`, `Large` |
| `IsDisabled` | `bool` | `false` | Nonaktifkan tombol |
| `IsSubmit` | `bool` | `false` | Jadikan `type="submit"` |
| `Icon` | `string?` | `null` | Emoji / SVG icon |
| `CssClass` | `string` | `""` | Class CSS tambahan |
| `ChildContent` | `RenderFragment?` | `null` | Konten kustom |

#### Contoh Penggunaan

```razor
@* Primary button (default) *@
<AppButton Text="Simpan Data"
           OnClick="@HandleSimpan" />

@* Danger button with icon *@
<AppButton Text="Hapus"
           Variant="AppButtonVariant.Danger"
           Icon="🗑️"
           IsDisabled="@(!hasData)"
           OnClick="@HandleHapus" />

@* Secondary button, Small size *@
<AppButton Text="Batal"
           Variant="AppButtonVariant.Secondary"
           Size="AppButtonSize.Small" />

@* Button with ChildContent *@
<AppButton Variant="AppButtonVariant.Outline">
    <span class="font-bold">Kustom</span>
</AppButton>
```

---

### 2. AppGrid

**Lokasi:** `UI/Base/AppGrid.razor`

Wrapper untuk **DxGrid** DevExpress dengan override styling Flowbite/Tailwind.

#### Parameter

| Parameter | Tipe | Default | Deskripsi |
|-----------|------|---------|-----------|
| `Data` | `IEnumerable<object>?` | `null` | Data source |
| `PageSize` | `int` | `10` | Baris per halaman |
| `PageIndex` | `int` | `0` | Halaman aktif |
| `PageIndexChanged` | `EventCallback<int>` | - | Callback saat halaman berubah |
| `TotalCount` | `int` | `0` | Total record (pagination info) |
| `ShowPagination` | `bool` | `true` | Tampilkan pagination bar |
| `CssClass` | `string` | `""` | Class CSS tambahan |
| `ChildContent` | `RenderFragment?` | `null` | Kolom-kolom (DxGridDataColumn) |

#### Contoh Penggunaan

```razor
<AppGrid Data="@dataList"
         PageSize="5"
         @bind-PageIndex="_pageIndex"
         TotalCount="@dataList.Count">
    <DxGridDataColumn Field="Nik" Caption="NIK" Width="130px" />
    <DxGridDataColumn Field="Nama" Caption="Nama" Width="160px" />
    <DxGridDataColumn Field="Status" Caption="Status" Width="100px" />
    <DxGridDataColumn Caption="Aksi" Width="80px">
        <CellDisplayTemplate>
            <button @onclick="() => Hapus(context.VisibleIndex)">
                🗑️
            </button>
        </CellDisplayTemplate>
    </DxGridDataColumn>
</AppGrid>
```

> **Catatan:** Kolom tetap menggunakan `DxGridDataColumn` milik DevExpress melalui `RenderFragment`. Ini memberikan fleksibilitas penuh tanpa mengorbankan abstraksi layout.

---

### 3. AppTextBox

**Lokasi:** `UI/Base/AppTextBox.razor`

Wrapper untuk `<input>` standar dengan styling Flowbite lengkap dengan label, validation message, dan icon.

#### Parameter

| Parameter | Tipe | Default | Deskripsi |
|-----------|------|---------|-----------|
| `InputId` | `string` | (auto) | ID untuk label `for` |
| `Label` | `string?` | `null` | Label di atas input |
| `Value` | `string?` | `null` | Nilai input |
| `ValueChanged` | `EventCallback<string?>` | - | Callback value change |
| `Placeholder` | `string?` | `null` | Placeholder text |
| `InputType` | `string` | `"text"` | Tipe input (`text`, `email`, `password`, `number`) |
| `IsDisabled` | `bool` | `false` | Nonaktifkan input |
| `IsReadOnly` | `bool` | `false` | Read-only |
| `IsRequired` | `bool` | `false` | Tampilkan `*` merah |
| `MaxLength` | `int?` | `null` | Maksimal karakter |
| `LeftIcon` / `RightIcon` | `string?` | `null` | Icon kiri/kanan |
| `ValidationMessage` | `string?` | `null` | Teks error merah |
| `HelperText` | `string?` | `null` | Teks bantuan abu-abu |
| `WrapperCssClass` | `string` | `""` | Class untuk wrapper luar |
| `CssClass` | `string` | `""` | Class untuk input |

#### Contoh Penggunaan

```razor
<AppTextBox Label="NIK"
            @bind-Value="_nik"
            Placeholder="Masukkan NIK"
            IsRequired="true"
            LeftIcon="🔑"
            WrapperCssClass="w-full" />

<AppTextBox Label="Email"
            @bind-Value="_email"
            InputType="email"
            HelperText="Alamat email resmi"
            ValidationMessage="@_emailError"
            WrapperCssClass="w-full" />
```

---

### 4. AppComboBox

**Lokasi:** `UI/Base/AppComboBox.razor`

Wrapper untuk **DxComboBox** DevExpress dengan label dan validasi.

#### Parameter

| Parameter | Tipe | Default | Deskripsi |
|-----------|------|---------|-----------|
| `Label` | `string?` | `null` | Label di atas combobox |
| `Data` | `IEnumerable<string>?` | `null` | Daftar pilihan |
| `SelectedValue` | `string?` | `null` | Nilai yang dipilih |
| `SelectedValueChanged` | `EventCallback<string?>` | - | Callback saat pilihan berubah |
| `NullText` | `string?` | `null` | Placeholder |
| `IsRequired` | `bool` | `false` | Tampilkan `*` merah |
| `ValidationMessage` | `string?` | `null` | Teks error |
| `WrapperCssClass` | `string` | `""` | Class untuk wrapper |
| `CssClass` | `string` | `""` | Class untuk combobox |

#### Contoh Penggunaan

```razor
<AppComboBox Label="Unit Kerja"
             @bind-SelectedValue="_unitKerja"
             Data="@_unitKerjaOptions"
             NullText="Pilih Unit Kerja"
             IsRequired="true"
             WrapperCssClass="w-full" />
```

---

### 📐 AppButtonVariant & AppButtonSize

**Lokasi:** `UI/Base/BaseTypes.cs`

```csharp
public enum AppButtonVariant { Primary, Secondary, Danger, Outline }
public enum AppButtonSize    { Small, Medium, Large }
```

---

## 📁 Feature-Based Architecture

Setiap fitur bisnis dipisahkan ke dalam folder sendiri di `UI/Features/`. Setiap folder fitur berisi:

```
UI/Features/<FeatureName>/
├── Pages/           # Routeable pages (@page "...")
└── Components/      # Supporting components
```

### Existing Features

| Feature | Route | Main File |
|---------|-------|-----------|
| **Dashboard** | `/` | `Components/Pages/Home.razor` |
| **Employee Data** | `/employee-data` | `UI/Features/EmployeeManagement/Pages/EmployeeData.razor` |
| **Example Form** (Demo) | `/example-form` | `UI/Features/ExampleForm/Pages/ExampleForm.razor` |

### Naming Convention

- **Pages/** → One file per page, using `@page "..."`
- **Components/** → Components used by the Page in the same feature
- **Base/** → Global abstraction components usable by all features

---

## 🌐 Localization (EN / ID)

The application supports **Indonesian** (default) and **English** with a toggle button in the header bar.

### Architecture

```
Header (ID/EN toggle)
       │
       ▼
LocalizationService (Scoped DI)
       │
       ├── SetLanguage("en"|"id")
       ├── T("key")                → returns translated string
       ├── T("key", args...)       → returns formatted string
       └── OnLanguageChanged event → triggers component re-render
              │
              ▼
       All subscribing components call StateHasChanged()
```

### How It Works

1. **`LocalizationService`** (`Services/LocalizationService.cs`) is registered as **Scoped** in DI
2. Each component **injects** the service and **subscribes** to `OnLanguageChanged`:
   ```razor
   @implements IDisposable
   @inject LocalizationService _loc

   protected override void OnInitialized()
   {
       _loc.OnLanguageChanged += () => InvokeAsync(StateHasChanged);
   }

   public void Dispose()
   {
       _loc.OnLanguageChanged -= () => InvokeAsync(StateHasChanged);
   }
   ```
3. All UI text uses `@_loc.T("translation.key")` instead of hardcoded strings
4. The **Header** contains an ID/EN toggle that calls `_loc.SetLanguage()`

### Translation Files

All translations live in two JSON files (not in C# code):

| File | Language |
|------|----------|
| [`wwwroot/translations/id.json`](Frontend/src/Web/wwwroot/translations/id.json) | 🇮🇩 Indonesian (default) |
| [`wwwroot/translations/en.json`](Frontend/src/Web/wwwroot/translations/en.json) | 🇬🇧 English |

Structure — simple flat key-value pairs:

```json
{
  "employee.title": "Employee Data",
  "employee.syncManual": "Manual Sync",
  "common.showing": "Showing {0}-{1} of {2} entries"
}
```

Files are loaded once at startup in `Program.cs`:

```csharp
var idPath = Path.Combine(app.Environment.WebRootPath, "translations", "id.json");
var enPath = Path.Combine(app.Environment.WebRootPath, "translations", "en.json");
Translations.Initialize(idPath, enPath);
```

### Adding a New Translation Key

1. Open **both** `wwwroot/translations/id.json` and `en.json`
2. Add the same key with translated values:
   ```json
   // id.json
   "myFeature.myLabel": "Label Bahasa Indonesia"

   // en.json  
   "myFeature.myLabel": "English Label"
   ```
3. Use it in any `.razor` component:
   ```razor
   <h1>@_loc.T("myFeature.myLabel")</h1>
   ```
4. Inject + subscribe to `OnLanguageChanged` if the component hasn't already

### Format Strings

Use `{0}`, `{1}` placeholders with the `T(key, args...)` overload:

```json
// id.json
"common.showing": "Menampilkan {0}-{1} dari {2} entri"

// en.json
"common.showing": "Showing {0}-{1} of {2} entries"
```

```razor
@_loc.T("common.showing", start, end, total)
```

### Language Persistence

Currently the language resets on page reload. For persistence, extend `LocalizationService` to save the choice to `localStorage` via JS interop.

---

## 🚀 Cara Menjalankan

### Prasyarat

- .NET 9 SDK ([download](https://dotnet.microsoft.com/download/dotnet/9.0))
- DevExpress NuGet source (trial / licensed)

### ⚡ Cara Termudah: `run.bat` (Interactive Menu)

Double-click **`run.bat`** di folder `Frontend/` — muncul menu numerik:

```
  =============================================
      A P P P O R T A L
      Select Environment
  =============================================

     1   Development    [hot reload, mock API]
     2   Local          [debug, localhost API]
     3   Staging        [pre-production]
     4   Production     [live deployment]

     0   Exit

  =============================================

  Enter number (0-4) and press Enter:
```

Cukup ketik angka **1-4** lalu Enter untuk menjalankan, atau **0** untuk keluar.

### Environment Tersedia

| Environment | Kegunaan | Logging | Backend API | CORS |
|------------|----------|---------|-------------|------|
| **Development** | Coding sehari-hari | Information+ | Mock (JSON lokal) | AllowAny |
| **Local** | Testing API lokal | Debug+ | `localhost:8080` | Localhost |
| **Staging** | UAT / Pre-production | Information+ | Staging API | Domain terbatas |
| **Production** | Production deployment | Warning+ | Production API | Config-locked |

### Menjalankan Manual via Terminal

```bash
# Masuk ke direktori project
cd Frontend/src/Web

# Restore dependencies
dotnet restore

# Development (hot reload aktif)
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000
dotnet run --no-launch-profile

# Production
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=https://+:443;http://+:80
dotnet run --no-launch-profile --configuration Release
```

### Menjalankan via Visual Studio

Pilih profile dari dropdown ▶️ di toolbar: `dev`, `local`, `staging`, atau `production`.

### Akses

| Environment | URL |
|------------|-----|
| Development / Local | `https://localhost:5001` |
| Staging / Production | `https://your-domain.com` |

### Menambahkan Halaman Baru

1. Buat folder fitur baru: `UI/Features/<FeatureName>/Pages/`
2. Buat file `.razor` dengan `@page "/route-name"`
3. Tambahkan link di `NavMenu.razor` (bagian `AllGroups`)
4. (Opsional) Jika menggunakan base components, lihat panduan di atas

---

## 🧑‍💻 Panduan Pengembangan

### Aturan Utama

1. **Jangan panggil `Dx*` langsung di halaman.** Selalu gunakan Base Components (`AppButton`, `AppGrid`, `AppTextBox`, `AppComboBox`). Jika Base Component belum mendukung kebutuhan tertentu, **perbaiki Base Component-nya**, bukan halamannya.

2. **Gunakan `@bind-Value` untuk two-way binding.** Semua base component mendukung pattern ini.

3. **Manfaatkan `CssClass`** untuk penyesuaian layout per halaman. Class default sudah ditetapkan di Base Component.

4. **Attribute Splatting** — Jika perlu atribut HTML tambahan (seperti `data-*`, `aria-*`, `id`), cukup tulis di komponen dan akan diforward secara otomatis.

### Menambahkan Fitur Baru

```bash
# 1. Create folder structure
mkdir -p UI/Features/<FeatureName>/Pages
mkdir -p UI/Features/<FeatureName>/Components

# 2. Create page
touch UI/Features/<FeatureName>/Pages/<PageName>.razor

# 3. Add route in .razor file:
#    @page "/your-route"

# 4. Add navigation link in Components/Layout/NavMenu.razor
```

### DevExpress → Tailwind Override

Lihat file `wwwroot/css/app-portal.css` untuk mapping override:

| DevExpress Class | Tailwind Override |
|-----------------|-------------------|
| `.dxbl-grid` | Border transparan, header `bg-slate-50`, hover `bg-green-50` |
| `.dxbl-combobox` | `rounded-lg`, green focus ring |
| `.dxbl-searchbox` | `rounded-lg`, green focus ring |
| `.dxbl-drawer` | Z-index, shadow overlay mobile |

---

## 🔧 Troubleshooting

| Masalah | Solusi |
|---------|--------|
| **Build error: namespace `Web.UI.Base` not found** | Pastikan `@using Web.UI.Base` ada di `UI/_Imports.razor` atau `Components/_Imports.razor` |
| **DxComboBox error "type cannot be inferred"** | Tambahkan `TData="string" TValue="string"` atau gunakan `AppComboBox` |
| **Tailwind styles tidak muncul** | CDN mungkin terblokir. Gunakan koneksi internet atau download Tailwind lokal |
| **Font Inter tidak muncul** | Pastikan koneksi ke Google Fonts tidak terblokir. Fallback ke system-ui sudah disediakan |
| **Halaman 404 (Not Found)** | Pastikan route di file `.razor` menggunakan `@page "/..."` dan nama file cocok |

---

## 📚 Referensi

- [DevExpress Blazor Demo](https://demos.devexpress.com/blazor/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Flowbite Components](https://flowbite.com/docs/getting-started/introduction/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

---

> **Dibuat dengan ❤️ untuk Portal Aplikasi** — *Maintainability first, abstraction always.*
