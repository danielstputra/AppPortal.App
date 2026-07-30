# Frontend Portal Enhance — Panduan Restrukturisasi Blazor AutoInteractive .NET 10

> **Dokumen ini adalah panduan arsitektur lengkap untuk mengubah aplikasi Blazor yang sudah ada menjadi Portal Enterprise Modular.**
>
> **Pengarang:** Senior Blazor Architect — .NET 10 / AutoInteractive
> **Target:** Frontend-only, integrasi API vendor eksternal

---

## ⚠️ PERINGATAN FUNDAMENTAL: BACKEND SUDAH ADA DARI VENDOR

**[!WARNING]**
Backend sudah sepenuhnya dibangun oleh VENDOR EKSTERNAL. Semua API sudah tersedia melalui satu base URL:
**`https://{vendor-api-url}/api/`**

**LARANGAN KERAS — Claude JANGAN PERNAH:**
1. ❌ Membuat / menulis / menggenerate satupun kode backend — Controller, Minimal API, Endpoint, Service, DbContext, Migration, Entity, Middleware backend.
2. ❌ Membuat proyek backend baru (.NET Web API, Node.js, atau lainnya).
3. ❌ Membuat database, migration, atau model entity di sisi backend.
4. ❌ Membuat Gateway API atau APIM baru — semua routing sudah diatur vendor.
5. ❌ Membuat fake/mock backend untuk development — gunakan data dari API vendor langsung.

**Tugas Frontend HANYA:**
- ✅ Membaca dokumentasi API dari vendor (endpoints, request/response contract, auth method)
- ✅ Membuat model/DTO di sisi frontend untuk deserialisasi JSON
- ✅ Memanggil `https://{vendor-api-url}/api/` dengan HttpClient
- ✅ Mengelola autentikasi (login, token, refresh)
- ✅ Menampilkan data dari API vendor dengan state handling (loading, empty, error, success)
- ✅ Menyusun layout portal sebagai Shell tunggal dengan navigasi dinamis

---

## Daftar Isi

- [Arsitektur Sistem](#arsitektur-sistem)
- [Phase 0: Foundation — Understanding the Vendor API](#phase-0-foundation--understanding-the-vendor-api)
- [Phase 1: Authentication & Security Infrastructure](#phase-1-authentication--security-infrastructure)
- [Phase 2: HttpClient Pipeline & Resilience](#phase-2-httpclient-pipeline--resilience)
- [Phase 3: Solution Structure & Modular Architecture](#phase-3-solution-structure--modular-architecture)
- [Phase 4: Routing Architecture — Static vs Dynamic Modules](#phase-4-routing-architecture--static-vs-dynamic-modules)
- [Phase 5: State Management & Cross-Module Communication](#phase-5-state-management--cross-module-communication)
- [Phase 6: UI State Handling Patterns](#phase-6-ui-state-handling-patterns)
- [Phase 7: Base UI Integration & InteractiveAuto](#phase-7-base-ui-integration--interactiveauto)
- [Phase 8: Logging, Monitoring & Error Tracking](#phase-8-logging-monitoring--error-tracking)
- [Phase 9: Configuration & Environment Management](#phase-9-configuration--environment-management)
- [Phase 10: Testing Strategy](#phase-10-testing-strategy)
- [Phase 11: PWA & Service Worker Infrastructure](#phase-11-pwa--service-worker-infrastructure)
- [Phase 12: Network Connectivity Service](#phase-12-network-connectivity-service)
- [Phase 13: IndexedDB Emergency Database](#phase-13-indexeddb-emergency-database)
- [Phase 14: Save Local, Sync Later Engine](#phase-14-save-local-sync-later-engine)
- [Checklist Eksekusi](#checklist-eksekusi)

---

## Arsitektur Sistem

### Diagram Alir Data (Flow)

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    BROWSER (Blazor WASM + PWA)                            │
│                                                                           │
│  ┌──────────────┐   ┌──────────────────────┐   ┌────────────────────────────┐│
│  │ Auth Service  │   │  VendorHttpClient    │   │   App Registry (by Admin) ││
│  │ - Login       │──▶│  - Polly Retry       │──▶│  ┌────────────────────┐   ││
│  │ - Token Store │   │  - Auth Header       │   │  │ CorePortal = BASE  │   ││
│  │ - Refresh     │   │  - Error Map         │   │  │  (Portal App)      │   ││
│  └──────────────┘   └───────────┬───────────┘   │  │ ├ Auth (built-in)  │   ││
│                                 │               │  │ ├ Dashboard        │   ││
│                                 │               │  │ └ EmpManagement    │   ││
│                                 │               │  ├── Legal (App 1)   │   ││
│                                 │               │  │   ├ displayMode:  │   ││
│                                 │               │  │   │ embedded|page │   ││
│                                 │               │  │   └ url/layout    │   ││
│                                 │               │  └── Sparta (App 2) │   ││
│                                 │               │      ├ displayMode: │   ││
│                                 │               │      │ embedded|page │   ││
│                                 │               │      └ url/layout    │   ││
│                                 │               └──────────────────────────┘││
│                                 │                                          │
│  ┌──────────────────────────────▼──────────────────────────────────────┐  │
│  │             OFFLINE LAYER (IndexedDB + Sync Engine)                 │  │
│  │                                                                    │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │  │
│  │  │  IndexedDbService │  │   SyncQueue      │  │  NetworkStatus   │  │  │
│  │  │  - Emergency DB   │  │  - Pending ops   │  │  - Online/Offline │  │  │
│  │  │  - Module Stores  │  │  - FIFO Queue    │  │  - Auto-reconnect │  │  │
│  │  └──────────────────┘  └────────┬─────────┘  └──────────────────┘  │  │
│  │                                                                    │  │
│  └──────────────────────────────────┼──────────────────────────────────┘  │
│                                     │  Online?                            │
│                                     ▼                                     │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │                     Service Worker (Cache Layer)                    │  │
│  │  ┌────────────────────┐  ┌──────────────────┐  ┌────────────────┐  │  │
│  │  │  Assets Cache      │  │  API Cache (RO)  │  │  Offline Page  │  │  │
│  │  │  (Cache-First)     │  │  (Network-First)  │  │  Fallback      │  │  │
│  │  └────────────────────┘  └──────────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────┬──────────────────────────────────┘  │
│                                     │ HTTPS                              │
└─────────────────────────────────────┼────────────────────────────────────┘
                                      │
                                      ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                    VENDOR BACKEND (TIDAK BOLEH DIUBAH)                    │
│                                                                           │
│  https://{vendor-api-url}/api/                       │
│                                                                           │
│  /auth/login          → Authentikasi user (CorePortal)                    │
│  /auth/refresh        → Refresh token (CorePortal)                        │
│  /applications        → Daftar aplikasi dalam portal                      │
│  /portal/*            → Endpoints modul CorePortal                        │
│  /legal/*             → Endpoints modul Legal (App 1)                     │
│  /sparta/*            → Endpoints modul Sparta (App 2)                    │
└──────────────────────────────────────────────────────────────────────────┘
```

### Konsep Desain

| Aspek | Keputusan | Alasan |
|-------|-----------|--------|
| **Render Mode** | `InteractiveAuto` (Server→WASM) | Prerendering cepat, interaktivitas WASM, offline capability |
| **Backend** | Hanya vendor API | Backend sudah ada, tidak perlu dibuat ulang |
| **Auth** | JWT Bearer Token via DelegatingHandler | Security terpusat, otomatis attach ke setiap request |
| **App Hierarchy** | CorePortal (main) + Sub-apps | CorePortal adalah Base App/Portal App yang mengatur semua aplikasi terdaftar. CorePortal berisi Auth/Dashboard/EmployeeManagement sebagai built-in features. Legal & Sparta adalah sub-app yang display modenya (embedded atau dashboard) diatur dari CorePortal. |
| **App Display Mode** | Dinamis per aplikasi | Setiap sub-app bisa ditampilkan sebagai **Embedded** (iframe ke URL eksternal) atau **Dashboard Example** (halaman Blazor sederhana). Konfigurasi display mode diatur dari halaman admin CorePortal dan disimpan di API vendor. |
| **Modul** | Hybrid: Static + Dynamic | Static untuk fitur kompleks, Dynamic untuk aplikasi wrapper |
| **Resilience** | Polly (Retry + Circuit Breaker) | Handle transient failure dari API vendor |
| **State Transfer** | `PersistentComponentState` | Prerendering → WASM state handover |
| **DI Container** | Built-in DI + Module Registration | Setiap modul register service-nya sendiri |
| **PWA** | Progressive Web App + Service Worker | Aplikasi bisa di-install dan offline-capable |
| **Network Awareness** | Online/Offline detection via `navigator.onLine` | Adaptif terhadap koneksi tidak stabil |
| **Database Darurat** | IndexedDB via JSInterop wrapper (typed C#) | Penyimpanan lokal saat offline/gangguan koneksi |
| **Sinkronisasi** | "Save Local, Sync Later" — SyncQueue + SyncEngine | Auto-sync saat koneksi pulih, antrian FIFO, conflict resolution |

---

## Phase 0: Foundation — Understanding the Vendor API

### 🎯 Tujuan
Sebelum menulis kode apapun, harus ada pemahaman penuh tentang API vendor yang akan dikonsumsi.

### ✅ Checklist Discovery API

| Item | Status | Catatan |
|------|--------|---------|
| Base URL | ✅ | `https://{vendor-api-url}/api/` |
| Auth endpoint | ❌ | `/auth/login`, `/auth/refresh` — perlu dikonfirmasi |
| Daftar aplikasi | ❌ | `/applications` — perlu dikonfirmasi |
| Endpoints per modul | ❌ | `/sparta/*`, `/legal/*` — perlu dikonfirmasi |
| Format response | ❌ | `{success, data, message, errors}` — perlu dikonfirmasi |
| HTTP Method | ❌ | GET/POST/PUT/DELETE per endpoint |
| Pagination | ❌ | Apakah response di-paginate? Format `{page, pageSize, total, items}`? |
| API Versioning | ❌ | `/api/v1/...` atau `/api/...`? |
| Auth method | ❌ | JWT? OAuth2? API Key? Brief token expiry? |

### 📋 Langkah

```markdown
1. Minta dokumentasi API/Swagger dari vendor (biasanya di {baseUrl}/swagger atau {baseUrl}/docs)
2. Identifikasi endpoint mana yang dipanggil oleh setiap modul
3. Catat request/response contract setiap endpoint
4. Konfirmasi format error response
5. Konfirmasi mekanisme auth (login body, token format, refresh mechanism)
6. Buat model DTO di `Frontend/src/Web/Models/Vendor/` berdasarkan contract tersebut
```

---

## Phase 1: Authentication & Security Infrastructure

### 🎯 Tujuan
Membangun pipeline autentikasi yang aman dan transparan untuk seluruh komunikasi dengan API vendor.

### 1.1 Authentication Flow

```
┌──────────┐     ┌──────────────┐     ┌─────────────┐     ┌─────────────┐
│  User     │     │  Login Page  │     │  AuthService │     │  Vendor API │
│          │     │              │     │              │     │             │
│  (input) │────▶│ username/pwd │────▶│ POST /auth   │────▶│ /auth/login │
│          │     │              │     │              │     │             │
│          │     │              │◀────│  JWT + Refresh│◀───│ {token,     │
│          │     │              │     │  token       │     │  refresh,   │
│          │     │◀─────────────│     │              │     │  expiresIn} │
│          │     │  redirect to │     │              │     │             │
│          │     │  dashboard   │     │              │     │             │
└──────────┘     └──────────────┘     └─────────────┘     └─────────────┘
```

### 1.2 Token Storage Strategy

| Storage | Kelebihan | Kekurangan | Keputusan |
|---------|-----------|------------|-----------|
| `localStorage` | Persisten di semua tab | Rentan XSS | ✅ Dipilih |
| `sessionStorage` | Hilang saat tab tutup | User harus login ulang | ❌ Tidak nyaman |
| Cookie (HttpOnly) | Aman dari XSS | Butuh backend untuk set | ❌ Backend vendor |

**Keputusan:** Simpan token di `localStorage` dengan nama key `auth_token`.

### 1.3 Auth Service

```csharp
// Lokasi: Frontend/src/Web/Services/Auth/AuthService.cs

using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppPortal.App.Frontend.Web.Services.Auth;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;

    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "auth_refresh_token";

    // State yang di-share ke seluruh aplikasi
    public UserProfile? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public AuthService(IHttpClientFactory httpClientFactory, IJSRuntime js, NavigationManager navigation)
    {
        _httpClient = httpClientFactory.CreateClient("VendorApi");
        _js = js;
        _navigation = navigation;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", new
            {
                username,
                password
            });

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result is null) return false;

            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result.AccessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, result.RefreshToken);

            CurrentUser = result.User;
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task TryRestoreSessionAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            var response = await _httpClient.GetAsync("auth/me");
            if (response.IsSuccessStatusCode)
            {
                CurrentUser = await response.Content.ReadFromJsonAsync<UserProfile>();
            }
        }
        catch
        {
            // Token expired atau invalid — user perlu login ulang
            await LogoutAsync();
        }
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
        _navigation.NavigateTo("/login", forceLoad: true);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _js.InvokeAsync<string>("localStorage.getItem", RefreshTokenKey);
    }

    public async Task<string?> RefreshTokenAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken)) return null;

        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/refresh", new { refreshToken });
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<TokenRefreshResponse>();
            if (result is null) return null;

            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result.AccessToken);
            if (result.RefreshToken is not null)
                await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, result.RefreshToken);

            return result.AccessToken;
        }
        catch
        {
            await LogoutAsync();
            return null;
        }
    }
}

public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn, UserProfile User);
public record TokenRefreshResponse(string AccessToken, string? RefreshToken, int ExpiresIn);
public record UserProfile(string Id, string Username, string Email, string DisplayName, string[] Roles);
```

### 1.4 DelegatingHandler — Auth Header Injector

Ini adalah kunci: **setiap HTTP request otomatis mendapat Bearer token** tanpa perlu manual di setiap service.

```csharp
// Lokasi: Frontend/src/Web/Services/Auth/AuthDelegatingHandler.cs

using System.Net.Http.Headers;

namespace AppPortal.App.Frontend.Web.Services.Auth;

public class AuthDelegatingHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthDelegatingHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Skip auth header untuk endpoint login
        if (request.RequestUri?.AbsolutePath?.Contains("/auth/login") == true ||
            request.RequestUri?.AbsolutePath?.Contains("/auth/refresh") == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await _authService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Jika 401 Unauthorized — coba refresh token
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var newToken = await _authService.RefreshTokenAsync();
            if (newToken is not null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}
```

### 1.5 Program.cs — Registrasi Auth Pipeline

```csharp
// Lokasi: Frontend/src/Web/Program.cs

using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var vendorApiUrl = builder.Configuration["ApiSettings:VendorApiBaseUrl"]
    ?? throw new InvalidOperationException("Vendor API URL not found.");

// 1. Named Client untuk komunikasi internal auth (tanpa auto-auth handler)
builder.Services.AddHttpClient("VendorApi", client =>
{
    client.BaseAddress = new Uri(vendorApiUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// 2. Named Client dengan Auth Handler (untuk seluruh service modul)
builder.Services.AddHttpClient("VendorApi.Authenticated", client =>
{
    client.BaseAddress = new Uri(vendorApiUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<AuthDelegatingHandler>();

// 3. Auth Service — scoped per circuit di server, singleton di WASM
builder.Services.AddScoped<AuthService>();
builder.Services.AddTransient<AuthDelegatingHandler>();
```

---

## Phase 2: HttpClient Pipeline & Resilience

### 🎯 Tujuan
API vendor bisa tidak stabil (timeout, rate limit, downtime). Frontend harus resilient.

### 2.1 Polly Resilience Policy

```csharp
// Lokasi: Frontend/src/Web/Services/Http/ResiliencePipeline.cs

using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;

namespace AppPortal.App.Frontend.Web.Services.Http;

public static class ResiliencePolicies
{
    public static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(
                    attempt * 500 + Random.Shared.Next(0, 200)), // exponential + jitter
                onRetry: (outcome, retryCount, context) =>
                {
                    Console.WriteLine($"[Resilience] Retry #{retryCount} after {outcome.Result?.StatusCode}");
                });

    public static readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> CircuitBreaker =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    Console.WriteLine($"[CircuitBreaker] OPEN for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("[CircuitBreaker] CLOSED — service recovered");
                });
}
```

### 2.2 VendorHttpClient — Typed Client dengan Resilience

```csharp
// Lokasi: Frontend/src/Web/Services/Http/VendorHttpClient.cs

using System.Net.Http.Json;
using System.Text.Json;

namespace AppPortal.App.Frontend.Web.Services.Http;

/// <summary>
/// Typed HttpClient yang digunakan oleh SEMUA modul.
/// Semua request melalui client ini otomatis:
/// 1. Attach Bearer token (via AuthDelegatingHandler)
/// 2. Retry 3x saat transient failure
/// 3. Circuit breaker 30 detik saat 5+ error berturut-turut
/// 4. Deserialize response dengan error handling standar
/// </summary>
public class VendorHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VendorHttpClient> _logger;

    public VendorHttpClient(HttpClient httpClient, ILogger<VendorHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// GET request — return deserialized data atau throw VendorApiException
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET {Endpoint} failed — network error", endpoint);
            throw new VendorApiException("Network error", ex);
        }
    }

    /// <summary>
    /// POST request dengan body
    /// </summary>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint, TRequest body, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, body, ct);
            return await HandleResponseAsync<TResponse>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST {Endpoint} failed — network error", endpoint);
            throw new VendorApiException("Network error", ex);
        }
    }

    private async Task<T> HandleResponseAsync<T>(
        HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await TryReadErrorBodyAsync(response, ct);
            throw new VendorApiException(
                $"API Error: {(int)response.StatusCode} {response.ReasonPhrase}",
                statusCode: (int)response.StatusCode,
                errorBody: errorBody);
        }

        // Asumsi response envelope: { success: true, data: T, message: string, errors: string[] }
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        if (envelope is null || !envelope.Success)
        {
            throw new VendorApiException(
                envelope?.Message ?? "API returned unsuccessful response",
                errorBody: envelope?.Errors is not null ? string.Join("; ", envelope.Errors) : null);
        }

        return envelope.Data!;
    }

    private static async Task<string?> TryReadErrorBodyAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            return null;
        }
    }
}

// Response envelope dari vendor API
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string[]? Errors { get; set; }
}

// Exception khusus API
public class VendorApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorBody { get; }

    public VendorApiException(string message, int statusCode = 0, string? errorBody = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ErrorBody = errorBody;
    }
}
```

### 2.3 Program.cs — Registrasi Lengkap

```csharp
// Frontend/src/Web/Program.cs

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var vendorApiUrl = builder.Configuration["ApiSettings:VendorApiBaseUrl"]
    ?? throw new InvalidOperationException("Vendor API URL not found.");

// ─── HttpClient Factory ────────────────────────────────────────
builder.Services.AddHttpClient("VendorApi", client =>
{
    client.BaseAddress = new Uri(vendorApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// ─── Authenticated Client ──────────────────────────────────────
builder.Services.AddHttpClient("VendorApi.Authenticated", client =>
{
    client.BaseAddress = new Uri(vendorApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<AuthDelegatingHandler>()
.AddPolicyHandler(ResiliencePolicies.RetryPolicy)
.AddPolicyHandler(ResiliencePolicies.CircuitBreaker);

// ─── Typed Client ──────────────────────────────────────────────
builder.Services.AddScoped<VendorHttpClient>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient("VendorApi.Authenticated");
    var logger = sp.GetRequiredService<ILogger<VendorHttpClient>>();
    return new VendorHttpClient(httpClient, logger);
});

// ─── Auth ──────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddTransient<AuthDelegatingHandler>();

// ─── Module Services (akan diregister per modul) ───────────────
builder.Services.AddScoped<SpartaService>();
```

### 2.4 Response Contract dari Vendor — Yang Harus Dikonfirmasi

```markdown
| Aspek | Contoh | Wajib Dikonfirmasi? |
|-------|--------|---------------------|
| Envelope | `{ success, data, message, errors }` | ✅ YA — sesuaikan `ApiResponse<T>` |
| Pagination | `{ page, pageSize, totalCount, totalPages, items }` | ✅ YA — jika API di-paginate |
| Error format | `{ errorCode, message, details }` | ✅ YA — untuk parsing error |
| Date format | `"2026-07-30T14:30:00Z"` (ISO 8601) | ✅ YA — untuk deserialisasi |
| Null handling | `null` fields atau omit? | ✅ YA — untuk nullable DTO props |
```

---

## Phase 3: Solution Structure & Modular Architecture

### 🎯 Tujuan
Menyusun struktur direktori **Vertical Slice Architecture (VSA)** untuk Blazor InteractiveAuto .NET 10, di mana **setiap aplikasi adalah Feature vertical slice** yang berdiri sendiri di bawah `Features/`.

**CorePortal adalah Base App/Portal App** yang memiliki built-in features (Auth, Dashboard, EmployeeManagement). **Legal dan Sparta adalah sub-app features** dengan dashboard masing-masing yang bisa diakses dari halaman utama CorePortal.

### 3.1 Struktur Direktori Final — VSA dengan `Features/` sebagai Single Source of Truth

```
Frontend/src/Web/
│
├── Infrastructure/                    # Cross-cutting concerns (bukan vertical slice)
│   ├── Http/                          # IApiClient, ApiClient, MockApiClient
│   ├── Security/                      # ITokenService, TokenService
│   ├── Localization/                  # LocalizationService + Translations
│   └── Models/                        # AppVersion, TreeNode
│
├── Features/                          # 📁 VERTICAL SLICES — SEMUA APLIKASI DI SINI
│   │
│   ├── CorePortal/                    # 🏠 BASE APP / PORTAL APP
│   │   ├── Auth/                      # 🔐 Login, Logout, AuthGuard, AuthLayout
│   │   │   ├── Models/
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   └── LoginResponse.cs
│   │   │   ├── Services/
│   │   │   │   ├── IAuthService.cs
│   │   │   │   ├── AuthService.cs
│   │   │   │   └── AppAuthenticationStateProvider.cs
│   │   │   └── Components/
│   │   │       ├── AuthGuard.razor
│   │   │       ├── RedirectToLogin.razor
│   │   │       ├── AuthLayout.razor
│   │   │       └── Pages/
│   │   │           ├── Login.razor
│   │   │           └── Logout.razor
│   │   │
│   │   ├── Dashboard/                 # 📊 Halaman utama CorePortal
│   │   │   └── Components/Pages/
│   │   │       └── Home.razor         # Grid aplikasi dinamis dari API
│   │   │
│   │   └── EmployeeManagement/        # 👥 Fitur CRUD employee bawaan CorePortal
│   │       ├── Models/
│   │       │   └── Employee.cs
│   │       ├── Services/
│   │       │   ├── IEmployeeService.cs
│   │       │   └── EmployeeService.cs
│   │       └── Components/
│   │           ├── EmployeeCardList.razor
│   │           ├── EmployeeGrid.razor
│   │           ├── FilterBar.razor
│   │           ├── StatusBadge.razor
│   │           └── Pages/
│   │               ├── EmployeeCreate.razor
│   │               ├── EmployeeData.razor
│   │               ├── EmployeeDetail.razor
│   │               └── EmployeeEdit.razor
│   │
│   ├── Legal/                         # ⚖️ App 1 — Dashboard Example Only
│   │   ├── Components/
│   │   │   └── Pages/
│   │   │       └── Dashboard.razor    # Landing page aplikasi Legal
│   │   ├── _Imports.razor
│   │   └── DependencyInjection.cs
│   │
│   └── Sparta/                        # 🛡️ App 2 — Dashboard Example Only
│       ├── Components/
│       │   └── Pages/
│       │       └── Dashboard.razor    # Landing page aplikasi Sparta
│       ├── _Imports.razor
│       └── DependencyInjection.cs
│
├── UI/
│   ├── Base/                          # ⛔ LOCKED — DILARANG MENGUBAH
│   │   └── (70+ App* base components)
│   └── _Imports.razor
│
├── Components/                        # App shell (App.razor, Routes.razor, Layout/)
├── Middleware/                        # Security headers, validation, exception
└── Program.cs                         # Entry point — registrasi semua service
```

### Alur Navigasi Aplikasi

```
Login (/login) ──▶ CorePortal Dashboard (/dashboard)
                        │
                        ├──▶ Employee Data (/employee-data)   ← Feature CorePortal
                        │
                        ├──▶ Legal App (/legal)               ← Feature Legal
                        │
                        └──▶ Sparta App (/sparta)             ← Feature Sparta
```

### Perbedaan CorePortal vs Sub-App Features

| Aspek | CorePortal (Base App) | Legal / Sparta (Sub-App Feature) |
|-------|----------------------|----------------------------------|
| **Namespace** | `Web.Features.CorePortal.*` | `Web.Features.Legal.*` / `Web.Features.Sparta.*` |
| **Auth** | ✅ Memiliki login/logout | ❌ Inherit dari CorePortal |
| **Layout** | MainLayout + NavMenu | Layout sama / bisa berbeda |
| **Fitur** | Lengkap (Auth, Dashboard, Employee CRUD) | Minimal (dashboard example) |
| **Route** | `/login`, `/dashboard`, `/employee-data/*` | `/legal`, `/sparta` |
| **Service** | AuthService, EmployeeService, dll | Sederhana, bisa tanpa service |
| **Status** | WAJIB ada (base app) | OPSIONAL (sub-app terdaftar) |
```

### 3.2 Convention Over Configuration — Feature Discovery

Setiap feature mengikuti konvensi ini:

| Komponen | Konvensi | Contoh |
|----------|----------|--------|
| Service class | `{FeatureName}Service.cs` di `Features/{FeatureName}/Services/` | `EmployeeService.cs` |
| Models | Folder `Models/` | `Features/CorePortal/EmployeeManagement/Models/` |
| Halaman | `.razor` dengan `@page "/{route}"` | `@page "/employee-data"` |
| Base App (CorePortal) | WAJIB ada, mengandung Auth + fitur bersama | `Features/CorePortal/` |
| Sub-App Feature | OPSIONAL, terdaftar di App Registry API | `Features/Legal/`, `Features/Sparta/` |
| Registrasi DI | Per-feature `DependencyInjection.cs` + dipanggil di `Program.cs` | `services.AddEmployeeServices()` |

### 3.3 FeatureRegistry.cs — Alternatif Registrasi Terpusat

Jika feature bertambah banyak, registry pattern mencegah `Program.cs` membengkak:

```csharp
// Lokasi: Frontend/src/Web/Services/FeatureRegistry.cs

namespace AppPortal.App.Frontend.Web.Services;

public static class FeatureRegistry
{
    private static readonly List<FeatureDefinition> _features = new();

    public static IReadOnlyList<FeatureDefinition> Features => _features.AsReadOnly();

    /// <summary>
    /// Daftarkan feature secara statis (dipanggil dari Program.cs)
    /// </summary>
    public static void Register<TService>(string name, string routePrefix, bool hasComplexFeatures)
        where TService : class
    {
        _features.Add(new FeatureDefinition(
            Name: name,
            RoutePrefix: routePrefix,
            HasComplexFeatures: hasComplexFeatures,
            ServiceType: typeof(TService)));
    }

    /// <summary>
    /// Daftarkan semua service feature ke DI container
    /// </summary>
    public static void RegisterServices(IServiceCollection services)
    {
        foreach (var feature in _features)
        {
            if (feature.ServiceType is not null)
            {
                services.AddScoped(feature.ServiceType);
            }
        }
    }
}

public record FeatureDefinition(
    string Name,
    string RoutePrefix,
    bool HasComplexFeatures,
    Type? ServiceType);
```

**Namun perlu diingat:** Registrasi service tetap manual karena Blazor WASM butuh compile-time registration. Pola di atas hanya membuatnya lebih terorganisir.

### 3.4 Enhanced Structure — Dengan Offline & Sync Support

> Struktur di atas diperluas dengan folder `Offline/` dan `Sync/` di setiap modul untuk mendukung pola "Save Local, Sync Later". **Folder baru ini opsional per modul** — hanya modul dengan fitur CRUD yang membutuhkannya.

```
Frontend/src/Web/
│
├── wwwroot/
│   ├── service-worker.js              # 🆕 Service Worker (PWA)
│   ├── service-worker.published.js    # 🆕 Production SW
│   ├── manifest.json                  # 🆕 Web App Manifest
│   └── offline.html                   # 🆕 Halaman offline fallback
│
├── Services/
│   ├── Auth/
│   ├── Http/
│   ├── Offline/                        # 🆕 SERVICE LAYER OFFLINE GLOBAL
│   │   ├── IndexedDbService.cs         # Wrapper JSInterop ke IndexedDB
│   │   ├── NetworkStatusService.cs     # Online/Offline detector
│   │   ├── SyncEngine.cs               # Proses antrian sinkronisasi
│   │   ├── SyncQueue.cs                # FIFO queue untuk pending changes
│   │   └── ConflictResolver.cs         # Strategi resolusi konflik
│   │
│   └── ModuleRegistry.cs
│
├── Features/
│   ├── Sparta/
│   │   ├── Services/
│   │   │   ├── SpartaService.cs        # Service API online
│   │   │   └── SpartaSyncService.cs    # 🆕 Sync logic spesifik Sparta
│   │   ├── Models/
│   │   ├── Offline/                    # 🆕 IndexedDB store untuk Sparta
│   │   │   ├── SpartaDbStore.cs        # CRUD ke IndexedDB
│   │   │   └── SpartaOfflineSchema.cs  # Schema object store Sparta
│   │   └── Pages/ (atau Components/)
│   │       ├── WbGrading/
│   │       └── MasterData/
│   │
│   ├── Legal/
│   │   ├── Services/
│   │   │   ├── LegalService.cs
│   │   │   └── LegalSyncService.cs     # 🆕 Sync logic spesifik Legal
│   │   ├── Models/
│   │   ├── Offline/                    # 🆕 IndexedDB store untuk Legal
│   │   │   ├── LegalDbStore.cs
│   │   │   └── LegalOfflineSchema.cs
│   │   └── Pages/ (atau Components/)
│   │       └── Contracts/
│   │
│   └── ... (feature lain mengikuti pola yang sama)
│
├── UI/
│   ├── Base/                           # ⛔ LOCKED
│   └── Shared/
│       ├── PortalHeader.razor
│       ├── ConnectivityBanner.razor    # 🆕 Banner offline/online
│       ├── SyncStatusIndicator.razor   # 🆕 Indikator antrian sinkronisasi
│       └── DynamicModulePage.razor
```

### 3.5 Convention untuk Offline Module

Setiap modul yang perlu offline support mengikuti konvensi ini:

| Komponen | Konvensi | Contoh |
|----------|----------|--------|
| IndexedDB Store | `{FeatureName}DbStore.cs` di `Features/{FeatureName}/Offline/` | `SpartaDbStore.cs` |
| Schema Store | `{FeatureName}OfflineSchema.cs` di `Features/{FeatureName}/Offline/` | `SpartaOfflineSchema.cs` |
| Sync Service | `{FeatureName}SyncService.cs` di `Features/{FeatureName}/Services/` | `SpartaSyncService.cs` |
| Object Store Name | `{featureName}_{entityName}` (lowercase) | `sparta_grading`, `sparta_masterdata` |

---

### 3.6 App Display Modes — Embedded vs Dashboard Example

**Konsep Utama:** Karena CorePortal adalah Base App/Portal App untuk semua aplikasi yang terdaftar, maka cara menampilkan setiap sub-app diatur melalui konfigurasi di CorePortal. Setiap aplikasi yang terdaftar di App Registry memiliki properti `DisplayMode` yang menentukan bagaimana aplikasi tersebut ditampilkan.

#### Mode Display

| Mode | Deskripsi | Use Case | Implementasi |
|------|-----------|----------|-------------|
| **Embedded** | Aplikasi eksternal ditampilkan di dalam iframe di halaman CorePortal | Aplikasi yang sudah memiliki UI sendiri (web app eksternal) | `<iframe src="@app.ExternalUrl">` di dalam MainLayout |
| **Dashboard Example** | Aplikasi ditampilkan menggunakan halaman Blazor sederhana di `Features/{AppName}/` | Aplikasi yang belum punya UI atau butuh halaman sederhana | Blazor page di `Features/{AppName}/Components/Pages/Dashboard.razor` |

#### Alur Penentuan Display Mode

```
User klik app card di Dashboard CorePortal
          │
          ▼
   Cek App Registry dari API
          │
          ├── displayMode = "embedded" ──▶ Buka halaman dengan iframe
          │                                  Route: /app/{slug}
          │
          ├── displayMode = "page" ──────▶ Route ke halaman Blazor statis
          │                                  Route: /{slug} (compile-time)
          │
          └── displayMode tidak ditemukan ──▶ Dynamic fallback
                                                 Route: /app/{slug}
```

#### Konfigurasi dari CorePortal (Base App)

CorePortal sebagai Base App menyediakan halaman admin untuk mengatur display mode setiap sub-app:

```csharp
// Model DTO dari API vendor — menentukan display mode aplikasi
public class ApplicationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    
    // ─── Display Mode ───
    public AppDisplayMode DisplayMode { get; set; } = AppDisplayMode.Page;
    
    // ─── Embedded Mode Properties ───
    public string? ExternalUrl { get; set; }        // URL untuk iframe
    public bool RequiresAuth { get; set; }          // Apakah perlu auth token?
    
    // ─── Page Mode Properties ───
    public string? BaseRoute { get; set; }           // Route prefix untuk Blazor page
    public bool HasCustomPage { get; set; }          // Apakah ada halaman Blazor khusus?
    
    public bool IsActive { get; set; }
}

public enum AppDisplayMode
{
    Page,       // Dashboard Example — tampilkan halaman Blazor
    Embedded    // Tampilkan dalam iframe dari URL eksternal
}
```

#### Implementasi DynamicModulePage dengan Display Mode

```razor
@* Lokasi: Frontend/src/Web/UI/Shared/DynamicModulePage.razor *@
@page "/app/{Slug}"
@attribute [Authorize]
@implements IDisposable

@inject VendorHttpClient ApiClient
@inject NavigationManager Navigation
@inject IJSRuntime JS

<PageTitle>@_module?.Name ?? "Loading..."</PageTitle>

@if (_module is null && !_error)
{
    @* Loading state *@
    <div class="flex items-center justify-center min-h-[400px]">
        <div class="text-center">
            <div class="w-10 h-10 border-2 border-gray-300 border-t-green-700 rounded-full animate-spin mx-auto mb-3"></div>
            <p class="text-sm text-gray-500">Memuat aplikasi...</p>
        </div>
    </div>
}
else if (_error)
{
    @* Error state *@
    <div class="flex flex-col items-center justify-center py-20">
        <h1 class="text-6xl font-bold text-gray-200 mb-4">404</h1>
        <p class="text-xl text-gray-500 mb-6">Aplikasi tidak ditemukan atau tidak aktif.</p>
        <button class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                @onclick="() => Navigation.NavigateTo('/dashboard')">
            Kembali ke Dashboard
        </button>
    </div>
}
else if (_module.DisplayMode == AppDisplayMode.Embedded && !string.IsNullOrEmpty(_module.ExternalUrl))
{
    @* ─── EMBEDDED MODE — Tampilkan dalam iframe ─── *@
    <div class="flex flex-col h-[calc(100vh-64px)]">
        @* Toolbar aplikasi embedded *@
        <div class="flex items-center justify-between px-4 py-2 bg-white border-b border-gray-200">
            <div class="flex items-center gap-3">
                <button class="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500"
                        @onclick="() => Navigation.NavigateTo('/dashboard')"
                        title="Kembali ke Dashboard">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                    </svg>
                </button>
                <div class="w-8 h-8 rounded-lg bg-green-100 flex items-center justify-center">
                    <span class="text-sm">@_module.Name[0]</span>
                </div>
                <div>
                    <p class="text-sm font-medium text-gray-900">@_module.Name</p>
                    <p class="text-xs text-gray-500">Aplikasi Eksternal — Embedded</p>
                </div>
            </div>
        </div>
        @* iframe penuh *@
        <iframe src="@_module.ExternalUrl"
                class="flex-1 w-full border-none"
                title="@_module.Name">
        </iframe>
    </div>
}
else
{
    @* ─── PAGE MODE — Tampilkan halaman info (jika tidak punya custom page) ─── *@
    <div class="max-w-4xl mx-auto px-4 py-8">
        <div class="bg-white border border-gray-200 rounded-lg shadow-sm p-8 text-center">
            <div class="w-20 h-20 rounded-full bg-gradient-to-br from-green-50 to-green-100 
                        flex items-center justify-center mx-auto mb-4">
                <span class="text-4xl">📋</span>
            </div>
            <h1 class="text-2xl font-bold text-gray-900 mb-2">@_module.Name</h1>
            <p class="text-gray-500 mb-6 max-w-md mx-auto">@_module.Description</p>
            
            @if (_module.HasCustomPage)
            {
                <p class="text-sm text-gray-400">
                    Aplikasi ini memiliki halaman khusus. Silakan akses melalui menu navigasi.
                </p>
            }
            else
            {
                <p class="text-sm text-gray-400">
                    Aplikasi ini belum memiliki tampilan frontend khusus. 
                    Hubungi administrator untuk informasi lebih lanjut.
                </p>
            }
            
            <button class="mt-6 px-4 py-2 text-sm text-gray-600 border border-gray-300 rounded-lg 
                           hover:bg-gray-50"
                    @onclick="() => Navigation.NavigateTo('/dashboard')">
                Kembali ke Dashboard
            </button>
        </div>
    </div>
}

@code {
    [Parameter] public string Slug { get; set; } = string.Empty;

    private ApplicationDto? _module;
    private bool _error;
    private Action? _langHandler;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var apps = await ApiClient.GetAsync<List<ApplicationDto>>("applications");
            _module = apps.FirstOrDefault(a =>
                a.Slug.Equals(Slug, StringComparison.OrdinalIgnoreCase) && a.IsActive);
            _error = _module is null;
        }
        catch (VendorApiException)
        {
            _error = true;
        }
    }

    public void Dispose() { }
}
```

#### Navigasi dari Dashboard — Menentukan Tujuan Berdasarkan Display Mode

```csharp
// Di Dashboard CorePortal — saat user klik app card
private void NavigateToApp(ApplicationDto app)
{
    switch (app.DisplayMode)
    {
        case AppDisplayMode.Page when app.HasCustomPage && !string.IsNullOrEmpty(app.BaseRoute):
            // Mode Page dengan custom Blazor page → route statis
            Navigation.NavigateTo($"/{app.BaseRoute}");
            break;

        case AppDisplayMode.Page:
            // Mode Page tanpa custom page → dashboard example
            Navigation.NavigateTo($"/app/{app.Slug}");
            break;

        case AppDisplayMode.Embedded when !string.IsNullOrEmpty(app.ExternalUrl):
            // Mode Embedded → dynamic page dengan iframe
            Navigation.NavigateTo($"/app/{app.Slug}");
            break;

        default:
            // External link fallback
            if (!string.IsNullOrEmpty(app.ExternalUrl))
                Navigation.NavigateTo(app.ExternalUrl);
            break;
    }
}
```

---

## Phase 4: Routing Architecture — Static vs Dynamic Modules

### 🎯 Tujuan
Menentukan strategi routing: statis untuk modul dengan fitur kompleks, dinamis untuk aplikasi wrapper.

### 4.1 Masalah Fundamental

**Blazor Router bersifat statis pada compile-time.** Setiap `@page` directive dikompilasi menjadi route table. Tidak bisa menambah route runtime dari API.

### 4.2 Pendekatan Hybrid (Rekomendasi)

```
┌──────────────────────────────────────────────────────────────────┐
│                      BLASOR ROUTER                                │
│                                                                   │
│  ─── CorePortal (Main App) ───                                   │
│  /login               → Features/Auth/Pages/Login.razor          │
│  /                    → Features/Dashboard/Home.razor            │
│  /employee-data       → Features/EmployeeManagement/*            │
│                                                                   │
│  ─── Sub-App Features ───                                        │
│  /legal               → Features/Legal/Pages/Dashboard.razor     │
│  /sparta              → Features/Sparta/Pages/Dashboard.razor     │
│                                                                   │
│  /app/{slug}          → UI/Shared/DynamicModulePage.razor        │
│                          ↑ Route parameter untuk modul dinamis    │
│                                                                   │
└──────────────────────────────────────────────────────────────────┘
```

### 4.3 Static Routes (Compile-time)

| Route | File | Aplikasi |
|-------|------|----------|
| `/login` | `Features/Auth/Components/Pages/Login.razor` | CorePortal |
| `/` | `Features/Dashboard/Components/Pages/Home.razor` | CorePortal |
| `/employee-data` | `Features/EmployeeManagement/Components/Pages/EmployeeData.razor` | CorePortal |
| `/employee-data/new` | `Features/EmployeeManagement/Components/Pages/EmployeeCreate.razor` | CorePortal |
| `/employee-data/{id}` | `Features/EmployeeManagement/Components/Pages/EmployeeDetail.razor` | CorePortal |
| `/employee-data/{id}/edit` | `Features/EmployeeManagement/Components/Pages/EmployeeEdit.razor` | CorePortal |
| `/legal` | `Features/Legal/Components/Pages/Dashboard.razor` | Legal (App 1) |
| `/sparta` | `Features/Sparta/Components/Pages/Dashboard.razor` | Sparta (App 2) |

### 4.4 Dynamic Routes (Runtime — Untuk Sub-App Non-Kompleks)

Sub-app yang tidak memiliki custom Blazor page ditampilkan melalui `DynamicModulePage.razor` dengan route `"/app/{Slug}"`. Halaman ini secara otomatis menyesuaikan display mode berdasarkan konfigurasi dari API vendor.

**Referensi implementasi lengkap ada di [3.6 App Display Modes](#36-app-display-modes--embedded-vs-dashboard-example).**

### 4.5 Dashboard Dinamis — Grid Aplikasi dari API

Dashboard CorePortal membaca daftar aplikasi dari API vendor dan menampilkan grid aplikasi. Saat user mengklik app card, navigasi ditentukan berdasarkan display mode aplikasi tersebut.

```razor
@* Lokasi: Frontend/src/Web/Features/Dashboard/Components/Pages/Home.razor *@
@page "/dashboard"
@attribute [Authorize]

@inject VendorHttpClient ApiClient
@inject NavigationManager Navigation
@inject LocalizationService _loc

<PageTitle>@_loc.T("dashboard.title") — AppPortal</PageTitle>

<div class="max-w-7xl mx-auto px-4 py-8">
    @* Welcome Section *@
    <div class="mb-8">
        <h1 class="text-2xl font-bold text-gray-900">@_loc.T("dashboard.welcome")</h1>
        <p class="text-gray-500 mt-1">@_loc.T("dashboard.welcomeDesc")</p>
    </div>

    @* Loading State *@
    @if (_isLoading)
    {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @for (var i = 0; i < 6; i++)
            {
                <div class="bg-white border border-gray-200 rounded-lg p-6 animate-pulse">
                    <div class="w-12 h-12 bg-gray-200 rounded-lg mb-4"></div>
                    <div class="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                    <div class="h-3 bg-gray-200 rounded w-full"></div>
                </div>
            }
        </div>
    }

    @* Empty State *@
    else if (_applications?.Count == 0)
    {
        <div class="text-center py-20">
            <div class="w-20 h-20 rounded-full bg-gray-100 flex items-center justify-center mx-auto mb-4">
                <svg class="w-10 h-10 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
            </div>
            <h3 class="text-lg font-medium text-gray-900 mb-1">@_loc.T("dashboard.noApps")</h3>
            <p class="text-sm text-gray-500">@_loc.T("dashboard.noAppsDesc")</p>
        </div>
    }

    @* Error State *@
    else if (_error)
    {
        <div class="text-center py-20">
            <p class="text-red-500 mb-4">@_loc.T("common.error")</p>
            <button class="px-4 py-2 bg-green-600 text-white rounded-lg" @onclick="LoadAppsAsync">
                @_loc.T("common.reload")
            </button>
        </div>
    }

    @* App Grid *@
    else
    {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @foreach (var app in _applications!.Where(a => a.IsActive))
            {
                <div class="bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md 
                            transition-shadow duration-200 cursor-pointer p-6"
                     @onclick="() => NavigateToApp(app)">
                    <div class="flex items-start gap-4">
                        <div class="w-12 h-12 rounded-lg bg-gradient-to-br from-green-50 to-green-100 
                                    flex items-center justify-center flex-shrink-0">
                            @if (!string.IsNullOrEmpty(app.IconUrl))
                            {
                                <img src="@app.IconUrl" alt="@app.Name" class="w-7 h-7" />
                            }
                            else
                            {
                                <span class="text-lg font-bold text-green-700">@app.Name[0]</span>
                            }
                        </div>
                        <div class="min-w-0 flex-1">
                            <h3 class="text-sm font-semibold text-gray-900 truncate">@app.Name</h3>
                            <p class="text-xs text-gray-500 mt-1 line-clamp-2">@app.Description</p>
                            @* Display mode badge *@
                            <span class="inline-flex items-center mt-2 px-2 py-0.5 rounded-full text-xs font-medium
                                         @(app.DisplayMode == AppDisplayMode.Embedded 
                                             ? "bg-blue-50 text-blue-700" 
                                             : "bg-green-50 text-green-700")">
                                @(app.DisplayMode == AppDisplayMode.Embedded ? "🔗 Embedded" : "📄 Halaman")
                            </span>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
</div>

@code {
    private List<ApplicationDto>? _applications;
    private bool _isLoading = true;
    private bool _error;

    protected override async Task OnInitializedAsync()
    {
        await LoadAppsAsync();
    }

    private async Task LoadAppsAsync()
    {
        _isLoading = true;
        _error = false;
        try
        {
            _applications = await ApiClient.GetAsync<List<ApplicationDto>>("applications");
        }
        catch (VendorApiException)
        {
            _error = true;
            _applications = new();
        }
        finally { _isLoading = false; }
    }

    private void NavigateToApp(ApplicationDto app)
    {
        switch (app.DisplayMode)
        {
            case AppDisplayMode.Page when app.HasCustomPage && !string.IsNullOrEmpty(app.BaseRoute):
                Navigation.NavigateTo($"/{app.BaseRoute}");
                break;
            case AppDisplayMode.Page:
            case AppDisplayMode.Embedded:
                Navigation.NavigateTo($"/app/{app.Slug}");
                break;
            default:
                if (!string.IsNullOrEmpty(app.ExternalUrl))
                    Navigation.NavigateTo(app.ExternalUrl);
                break;
        }
    }
}
```

### 4.6 Keputusan: Static vs Dynamic vs Display Mode

| Skenario | Static | Dynamic (Page) | Dynamic (Embedded) |
|----------|--------|---------------|-------------------|
| Fitur Blazor kompleks (form, tabel, workflow) | ✅ | ❌ | ❌ |
| Hanya dashboard example | ❌ | ✅ | ❌ |
| Aplikasi eksternal dengan UI sendiri (iframe) | ❌ | ❌ | ✅ |
| Routing | ✅ Compile-time | ✅ Runtime via `{Slug}` | ✅ Runtime via `{Slug}` |
| Display mode diatur dari | Admin CorePortal | Admin CorePortal | Vendor API config |
| Contoh | `/employee-data` | `/app/legal` | `/app/sparta` |

**Kesimpulan:**
1. **CorePortal** = Base App dengan static routes untuk built-in features
2. **Sub-app dengan custom Blazor page** = Static routes (jika ada) + Dynamic fallback
3. **Sub-app tanpa custom page** = Dynamic route `{Slug}` dengan display mode dari API
4. **Display mode (Embedded/Page)** dikonfigurasi dari admin CorePortal dan disimpan di API vendor

---

## Phase 5: State Management & Cross-Module Communication

### 🎯 Tujuan
Mengelola state yang di-share antar modul (auth, notifikasi, dll) dan state lokal per halaman (PersistentComponentState).

### 5.1 Strategi State Management

| Cakupan | Mekanisme | Contoh |
|---------|-----------|--------|
| **Global/Seluruh App** | `AuthService` (DI Scoped) | User profile, token, role |
| **Cross-Module** | Event callback / `IJSRuntime` localStorage | Notifikasi, theme |
| **Per-halaman** | `PersistentComponentState` | Form data, filter |
| **API Cache** | `IMemoryCache` atau manual dictionary | Data master jarang berubah |

### 5.2 PersistentComponentState — Prerendering ↔ WASM

Saat `InteractiveAuto` mode: halaman di-render dulu di server (prerender), lalu WASM mengambil alih. State harus ditransfer via `PersistentComponentState`.

```razor
@* Contoh di setiap halaman yang butuh state transfer *@
@implements IDisposable
@inject PersistentComponentState AppState
@inject VendorHttpClient ApiClient

@code {
    private List<SomeData>? _data;
    private PersistingComponentStateSubscription _subscription;
    private const string StateKey = "Sparta_GradingData";

    protected override async Task OnInitializedAsync()
    {
        _subscription = AppState.RegisterOnPersisting(PersistData);

        // Coba restore dari server-prerendered state
        if (AppState.TryTakeFromJson<List<SomeData>>(StateKey, out var restored))
        {
            _data = restored;
        }
        else
        {
            // Fetch dari API
            _data = await ApiClient.GetAsync<List<SomeData>>("sparta/data");
        }
    }

    private Task PersistData()
    {
        AppState.PersistAsJson(StateKey, _data);
        return Task.CompletedTask;
    }

    public void Dispose() => _subscription.Dispose();
}
```

### 5.3 Auth State — Global untuk Semua Modul

Auth state bukan milik satu modul — milik seluruh aplikasi. Maka `AuthService` didaftarkan sebagai `Scoped` dan di-inject di `MainLayout`:

```razor
@* Lokasi: Frontend/src/Web/Layout/MainLayout.razor *@
@inject AuthService AuthService
@inject NavigationManager Navigation

@code {
    protected override async Task OnInitializedAsync()
    {
        await AuthService.TryRestoreSessionAsync();

        if (!AuthService.IsAuthenticated
            && !Navigation.Uri.Contains("/login"))
        {
            Navigation.NavigateTo("/login");
        }
    }
}
```

### 5.4 Event Bus Sederhana — Untuk Komunikasi Antar Modul

Tanpa library eksternal, pola `Action<T>` event bus sederhana cukup untuk komunikasi antar modul:

```csharp
// Lokasi: Frontend/src/Web/Services/PortalEventBus.cs

namespace AppPortal.App.Frontend.Web.Services;

/// <summary>
/// Event bus sederhana untuk komunikasi antar modul.
/// Contoh: Module Sparta memberitahu Dashboard bahwa data berubah.
/// </summary>
public class PortalEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Publish<T>(T eventData) where T : class
    {
        if (_handlers.TryGetValue(typeof(T), out var handlers))
        {
            foreach (var handler in handlers.OfType<Action<T>>())
            {
                handler(eventData);
            }
        }
    }

    public IDisposable Subscribe<T>(Action<T> handler) where T : class
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Delegate>();

        _handlers[type].Add(handler);

        return new Subscription(() =>
        {
            _handlers[type].Remove(handler);
        });
    }

    private record Subscription(Action Unsubscribe) : IDisposable
    {
        public void Dispose() => Unsubscribe();
    }
}

// Event models
public record NotificationEvent(string Title, string Message, NotificationType Type);
public enum NotificationType { Info, Success, Warning, Error }
```

### 5.5 Offline State Persistence via IndexedDB

Untuk skenario koneksi tidak stabil, state tidak hanya di `PersistentComponentState` (memory) tetapi juga di **IndexedDB** (persistent storage). Pola aksesnya:

```
Flow Akses Data Offline-Aware:
┌─────────┐   Online?   ┌──────────┐   Sukses?   ┌───────────┐
│ Request  │──────────▶  │ API Call  │──────────▶  │ Tampilkan │
│ Data     │      │      │ (Vendor)  │      │      │ Data      │
└─────────┘      │      └──────────┘      │      └───────────┘
                 │ ❌ Offline             │ ❌ Error
                 ▼                        ▼
           ┌──────────────────────────────────┐
           │  Fallback ke IndexedDB           │
           │  (Emergency Database)            │
           │                                  │
           │  Jika ada di cache lokal →       │
           │  tampilkan + banner "Data offline"│
           │                                  │
           │  Jika tidak ada →                │
           │  tampilkan empty state           │
           └──────────────────────────────────┘
```

Prinsipnya: **Online-first, offline-fallback, local-last-resort.**

```csharp
// Pola akses di setiap ModuleService
public class SpartaService
{
    private readonly VendorHttpClient _api;
    private readonly SpartaDbStore _localDb;
    private readonly NetworkStatusService _network;

    public async Task<List<GradingItem>> GetGradingListAsync(CancellationToken ct = default)
    {
        // 1. Coba API dulu (online-first)
        if (_network.IsOnline)
        {
            try
            {
                var data = await _api.GetAsync<List<GradingItem>>("sparta/grading", ct);
                // 2. Simpan ke IndexedDB sebagai cache darurat
                await _localDb.SaveGradingListAsync(data, ct);
                return data;
            }
            catch (VendorApiException) when (!_network.IsOnline)
            {
                // Network putus saat request — fallback
                // fallthrough ke IndexedDB
            }
            catch (VendorApiException)
            {
                // API error meski online — tetap coba local
                // fallthrough ke IndexedDB
            }
        }

        // 3. Fallback: baca dari IndexedDB
        _logger.LogWarning("Offline mode — reading Sparta grading from IndexedDB");
        var localData = await _localDb.GetGradingListAsync(ct);

        return localData ?? new List<GradingItem>();
    }
}
```
```

---

## Phase 6: UI State Handling Patterns

### 🎯 Tujuan
Standarisasi cara setiap halaman menangani 5 state: **Loading → Empty → Error → Success → Revalidation**.

### 6.1 State Machine per Halaman

Setiap halaman mengikuti state flow:

```
OnInitializedAsync
    │
    ▼
┌─────────────┐
│   LOADING   │────▶ Tampilkan BaseSpinner
└──────┬──────┘
       │
       ▼
  ┌────────┐   API sukses, data kosong   ┌───────────┐
  │ FETCH  │─────────────────────────────▶│   EMPTY   │
  │  API   │                              │           │
  │        │─────────────────────┐        │ "Tidak ada│
  └───┬────┘                     │        │  data"    │
      │                          │        └───────────┘
      │ API sukses, data ada     │
      ▼                          │
  ┌────────┐                     │
  │ SUCCESS│                     │
  │ Tampil  │                     │
  │ data   │                     │
  └────────┘                     │
      │                          │
      ▼ API error                ▼
  ┌──────────┐              ┌────────┐
  │  ERROR   │              │  ERROR │
  │ + Retry  │              │+ Retry │
  └──────────┘              └────────┘
```

### 6.2 Base Component untuk Setiap State

```razor
@* Base State Container — reusable di setiap halaman *@

@* ─── LOADING STATE ─── *@
@if (state == PageState.Loading)
{
    <BaseCard Title="@Title">
        <div class="d-flex justify-content-center p-5">
            <BaseSpinner />
            <span class="ms-2">Memuat data...</span>
        </div>
    </BaseCard>
}

@* ─── EMPTY STATE ─── *@
else if (state == PageState.Empty)
{
    <BaseCard Title="@Title">
        <div class="text-center p-5">
            <i class="icon-empty-state" style="font-size:3rem;"></i>
            <p class="mt-3 text-muted">@EmptyMessage</p>
            @if (ShowEmptyAction)
            {
                <BaseButton Color="Primary" OnClick="OnEmptyAction">
                    @EmptyActionLabel
                </BaseButton>
            }
        </div>
    </BaseCard>
}

@* ─── ERROR STATE ─── *@
else if (state == PageState.Error)
{
    <BaseCard Title="Terjadi Kesalahan">
        <div class="text-center p-5">
            <i class="icon-error" style="font-size:3rem; color:var(--danger);"></i>
            <p class="mt-3 text-danger">@ErrorMessage</p>
            <BaseButton Color="Secondary" OnClick="RetryAsync">
                Coba Lagi
            </BaseButton>
        </div>
    </BaseCard>
}

@* ─── SUCCESS STATE ─── *@
else
{
    @ChildContent
}
```

### 6.3 Pattern Implementasi di Setiap Module Page

```razor
@* Template yang DIWAJIBKAN untuk setiap halaman module *@
@rendermode InteractiveAuto
@implements IDisposable
@inject VendorHttpClient ApiClient
@inject PersistentComponentState AppState
@inject ILogger<YourPage> Logger

@code {
    // ─── STATE ────────────────────────────────────────────────
    private PageState _state = PageState.Loading;
    private string _errorMessage = string.Empty;

    // ─── DATA ─────────────────────────────────────────────────
    private List<SomeDto>? _items;
    private const string StateKey = "ModuleName_Feature_Data";

    // ─── LIFECYCLE ────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        var subscription = AppState.RegisterOnPersisting(PersistData);

        if (AppState.TryTakeFromJson<List<SomeDto>>(StateKey, out var restored))
        {
            _items = restored;
            _state = (_items?.Count > 0) ? PageState.Success : PageState.Empty;
        }
        else
        {
            await LoadDataAsync();
        }
    }

    // ─── DATA LOADING ─────────────────────────────────────────
    private async Task LoadDataAsync()
    {
        _state = PageState.Loading;

        try
        {
            _items = await ApiClient.GetAsync<List<SomeDto>>("module/endpoint");

            _state = (_items?.Count > 0) ? PageState.Success : PageState.Empty;
        }
        catch (VendorApiException ex)
        {
            Logger.LogError(ex, "Failed to load module data");
            _errorMessage = ex.Message;
            _state = PageState.Error;
        }
    }

    private Task PersistData()
    {
        AppState.PersistAsJson(StateKey, _items);
        return Task.CompletedTask;
    }

    // ─── RETRY ────────────────────────────────────────────────
    private async Task RetryAsync()
    {
        await LoadDataAsync();
    }

    public void Dispose()
    {
        // cleanup subscription
    }
}

public enum PageState { Loading, Empty, Error, Success }
```

### 6.4 Offline Mode State — Connectivity Banner + Sync Indicator

Karena aplikasi berjalan di lingkungan dengan koneksi tidak stabil, setiap halaman perlu menampilkan indikator status koneksi dan status sinkronisasi.

#### Connectivity Banner

```razor
@* Lokasi: Frontend/src/Web/UI/Shared/ConnectivityBanner.razor *@
@inject NetworkStatusService Network
@implements IDisposable

@if (!Network.IsOnline)
{
    <div class="connectivity-banner offline">
        <span class="icon">📡</span>
        <span>Anda sedang offline. Data disimpan secara lokal dan akan disinkronkan saat koneksi pulih.</span>
    </div>
}
else if (_justRecovered)
{
    <div class="connectivity-banner recovery">
        <span class="icon">✅</span>
        <span>Koneksi pulih. Menyinkronkan data...</span>
    </div>
}

@code {
    private bool _justRecovered;

    protected override void OnInitialized()
    {
        Network.OnStatusChanged += HandleNetworkChange;
    }

    private async void HandleNetworkChange(bool isOnline)
    {
        if (isOnline)
        {
            _justRecovered = true;
            StateHasChanged();
            await Task.Delay(3000); // Tampilkan 3 detik
            _justRecovered = false;
        }
        StateHasChanged();
    }

    public void Dispose()
    {
        Network.OnStatusChanged -= HandleNetworkChange;
    }
}
```

#### Sync Status Indicator

```razor
@* Lokasi: Frontend/src/Web/UI/Shared/SyncStatusIndicator.razor *@
@inject SyncQueue SyncQueue
@inject SyncEngine SyncEngine
@implements IDisposable

<div class="sync-status">
    @if (_pendingCount > 0)
    {
        <span class="sync-badge">
            ⏳ @_pendingCount menunggu sinkronisasi
        </span>
        <button class="btn btn-sm btn-outline-primary"
                @onclick="SyncNowAsync"
                disabled="@_isSyncing">
            @if (_isSyncing)
            {
                <span>Sinkron...</span>
            }
            else
            {
                <span>Sinkron Sekarang</span>
            }
        </button>
    }
    else if (_lastSync is not null)
    {
        <span class="sync-ok text-muted small">
            ✅ Tersinkron @_lastSync.Value.ToLocalTime().ToString("HH:mm")
        </span>
    }
</div>

@code {
    private int _pendingCount;
    private DateTime? _lastSync;
    private bool _isSyncing;

    protected override async Task OnInitializedAsync()
    {
        _pendingCount = await SyncQueue.GetPendingCountAsync();
        _lastSync = await SyncQueue.GetLastSyncTimeAsync();

        SyncEngine.OnSyncCompleted += (count) =>
        {
            _pendingCount = 0;
            _lastSync = DateTime.UtcNow;
            _isSyncing = false;
            InvokeAsync(StateHasChanged);
        };
    }

    private async Task SyncNowAsync()
    {
        _isSyncing = true;
        await SyncEngine.ProcessQueueAsync();
    }

    public void Dispose() { /* cleanup */ }
}
```

#### Integrasi di MainLayout

```razor
@* MainLayout.razor — tambahkan banner + sync indicator *@
<ConnectivityBanner />
<div class="top-bar d-flex justify-content-between">
    <PortalHeader />
    <SyncStatusIndicator />
</div>
```
```

---

## Phase 7: Base UI Integration & InteractiveAuto

### 🎯 Tujuan
Mengintegrasikan Base UI Components ke dalam modul tanpa memodifikasi `UI/Base`, dan memastikan `InteractiveAuto` berjalan optimal.

### 7.1 Aturan Keras

```
⛔ LOCKED — DIREKTORI UI/BASE TIDAK BOLEH DISENTUH:
- Tidak boleh mengubah file .razor yang sudah ada
- Tidak boleh mengubah file .css/.js terkait
- Tidak boleh menambah komponen baru di dalam folder Base
- Hanya boleh CONSUME via @using
```

### 7.2 Konsumsi Base Components

```razor
@* Setiap .razor di Modules/ WAJIB punya ini di awal *@
@using AppPortal.App.Frontend.Web.UI.Base
@using AppPortal.App.Frontend.Web.Modules.Sparta.Features.WbGrading

@* Contoh penggunaan Base Components *@
<BaseCard Title="@_title">
    <BaseTextBox @bind-Value="_searchTerm"
                 Placeholder="Cari data..."
                 DebounceInterval="300" />
    <BaseButton Color="Primary" OnClick="SearchAsync">
        Cari
    </BaseButton>
</BaseCard>
```

### 7.3 InteractiveAuto — Best Practices

```markdown
## Panduan InteractiveAuto .NET 10

### Apa itu InteractiveAuto?
- Halaman di-render di SERVER dulu (prerendering → SEO, fast first paint)
- Kemudian WASM di-download dan mengambil alih (interaktivitas penuh, offline)
- State ditransfer via `PersistentComponentState`

### WAJIB dilakukan di setiap halaman:

1. ✅ SELALU gunakan `@rendermode InteractiveAuto` di setiap halaman module
2. ✅ SELALU implement `PersistentComponentState` untuk transfer state
3. ✅ SELALU handle Loading state (karena prerendering tidak punya data API)
4. ✅ Gunakan `try/catch` di OnInitializedAsync (prerendering tidak bisa handle error otomatis)
5. ❌ JANGAN panggil JSInterop di OnInitializedAsync (server-side akan throw)
6. ❌ JANGAN inject IJSRuntime di constructor — tunggu hingga WASM aktif
7. ❌ JANGAN simpan dependency berat di static fields (tidak akan di-GC saat circuit server tutup)
```

### 7.4 Deteksi Environment: Server vs WASM

```csharp
// Kadang perlu tahu apakah sedang di server (prerender) atau WASM
public static class RenderModeHelper
{
    public static bool IsPrerendering =>
        !OperatingSystem.IsBrowser();

    public static bool IsRunningInWasm =>
        OperatingSystem.IsBrowser();
}

// Penggunaan:
if (RenderModeHelper.IsPrerendering)
{
    // Jangan panggil JSInterop
    _data = new(); // placeholder
}
else
{
    // WASM aktif — aman panggil JS
    await _js.InvokeVoidAsync("someFunction");
}
```

---

## Phase 8: Logging, Monitoring & Error Tracking

### 🎯 Tujuan
Mendapatkan visibilitas penuh terhadap error, performa, dan user behavior di client-side.

### 8.1 Logging Strategy

```csharp
// Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Di production, tambahkan provider ke Application Insights atau Sentry
// builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["AppInsights:ConnectionString"]);
```

| Level | Kapan | Contoh |
|-------|-------|--------|
| `LogTrace` | Developer debug | Parameter request |
| `LogDebug` | Flow informasi | "Fetching X from API" |
| `LogInformation` | User action penting | "User X logged in" |
| `LogWarning` | Anomali tidak fatal | "API returned 429 (rate limited)" |
| `LogError` | Error yang ditangani | "API call failed after 3 retries" |
| `LogCritical` | Tidak bisa recover | "Auth token expired and refresh failed" |

### 8.2 Global Error Boundary

```csharp
// Lokasi: Program.cs atau App.razor

// Di Program.cs — global unhandled exception handler
builder.Services.AddScoped<ErrorBoundary>();

// Di App.razor — bungkus komponen root
<ErrorBoundary>
    <Routes />
</ErrorBoundary>
```

### 8.3 Logging di Setiap Module Service

```csharp
// Contoh — SpartaService (di Features/Sparta/Services/) dengan logging
public class SpartaService
{
    private readonly VendorHttpClient _api;
    private readonly ILogger<SpartaService> _logger;

    public SpartaService(VendorHttpClient api, ILogger<SpartaService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<List<GradingItem>> GetGradingListAsync()
    {
        _logger.LogDebug("Fetching grading list from API");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await _api.GetAsync<List<GradingItem>>("sparta/grading");
            sw.Stop();
            _logger.LogInformation("Fetched {Count} grading items in {Elapsed}ms",
                result.Count, sw.ElapsedMilliseconds);
            return result;
        }
        catch (VendorApiException ex)
        {
            _logger.LogError(ex, "Failed to fetch grading list after {Elapsed}ms",
                sw.ElapsedMilliseconds);
            throw; // Re-throw — UI yang akan handle display
        }
    }
}
```

---

## Phase 9: Configuration & Environment Management

### 🎯 Tujuan
Mengelola konfigurasi untuk berbagai environment (dev/staging/prod).

### 9.1 File Konfigurasi

```json
// Frontend/src/Web/wwwroot/appsettings.json (Development)
{
  "ApiSettings": {
    "VendorApiBaseUrl": "https://{vendor-api-url}/api/",
    "TimeoutSeconds": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "AppPortal": "Debug"
    }
  }
}

// Frontend/src/Web/wwwroot/appsettings.Staging.json
{
  "ApiSettings": {
    "VendorApiBaseUrl": "{vendor-api-url}",
    "TimeoutSeconds": 15
  }
}

// Frontend/src/Web/wwwroot/appsettings.Production.json
{
  "ApiSettings": {
    "VendorApiBaseUrl": "{vendor-api-url}",
    "TimeoutSeconds": 10
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "AppPortal": "Information"
    }
  }
}
```

### 9.2 Blazor WASM — Environment Detection

```csharp
// Blazor WASM tidak punya environment secara otomatis.
// Gunakan appsettings.{environment}.json yang di-load manual:

// Program.cs
var hostEnvironment = builder.HostEnvironment; // IWebAssemblyHostEnvironment
var baseUrl = hostEnvironment.BaseAddress;

// Atau buat service sederhana:
public class AppConfiguration
{
    private readonly IConfiguration _configuration;
    private readonly IWebAssemblyHostEnvironment _env;

    public AppConfiguration(IConfiguration configuration, IWebAssemblyHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public string VendorApiUrl =>
        _configuration[$"ApiSettings:VendorApiBaseUrl"]
        ?? throw new InvalidOperationException("Missing VendorApiBaseUrl");

    public bool IsDevelopment => _env.IsDevelopment();
    public bool IsProduction => _env.IsProduction();
}
```

---

## Phase 10: Testing Strategy

### 🎯 Tujuan
Memastikan setiap modul bisa diuji secara independen tanpa ketergantungan pada API vendor.

### 10.1 Unit Testing — Service Layer

```csharp
// Gunakan bUnit + xUnit untuk test komponen Blazor
// Gunakan Moq atau NSubstitute untuk mock API

[Fact]
public async Task SpartaService_GetGradingList_ReturnsData()
{
    // Arrange
    var mockApi = new Mock<VendorHttpClient>();
    mockApi.Setup(x => x.GetAsync<List<GradingItem>>("sparta/grading"))
           .ReturnsAsync(new List<GradingItem> { new() { Id = 1 } });

    var service = new SpartaService(mockApi.Object, NullLogger<SpartaService>.Instance);

    // Act
    var result = await service.GetGradingListAsync();

    // Assert
    Assert.Single(result);
}
```

### 10.2 Integration Testing — Mock API Vendor

Untuk development tanpa koneksi ke API vendor, buat mock server sederhana:

```csharp
// Program.cs — tambahkan saat development
if (builder.HostEnvironment.IsDevelopment())
{
    // Ganti VendorHttpClient dengan versi mock
    builder.Services.AddScoped<IVendorHttpClient, MockVendorHttpClient>();
}
```

Atau gunakan tool seperti WireMock.NET untuk simulate API vendor secara realistis.

### 10.3 Testing Matrix

| Test Type | Tools | Coverage Target | Frekuensi |
|-----------|-------|----------------|-----------|
| Unit Test (Service) | xUnit + Moq | 90%+ | Setiap commit |
| Component Test (Razor) | bUnit | 80%+ | Setiap PR |
| Integration (Mock API) | WireMock.NET | Critical paths | Daily |
| E2E (Real API) | Playwright | Happy paths | Per release |

---

## Phase 11: PWA & Service Worker Infrastructure

### 🎯 Tujuan
Menjadikan aplikasi Blazor sebagai Progressive Web App (PWA) yang dapat diinstal, memiliki service worker untuk caching asset, dan tetap menampilkan halaman saat offline.

### 11.1 Arsitektur Service Worker

Service worker adalah "proxy jaringan" yang berjalan di browser, terpisah dari thread UI. Di Blazor PWA, SW menangani:

| Tugas | Strategy | File |
|-------|----------|------|
| Static assets (CSS, JS, WASM) | **Cache-First** — Muat dari cache, update background | `service-worker.js` |
| API calls ke vendor | **Network-First** — Coba jaringan dulu, fallback ke cache | `service-worker.js` |
| Offline fallback | **Cache-Only** — Tampilkan halaman offline | `offline.html` |
| Push notifications | **Event Listener** — Terima notifikasi walau app tertutup | `service-worker.js` |

```
Flow Request:
┌─────────┐     ┌──────────────┐
│ Request  │────▶│ Service Worker │
│ Asset/API │     │              │
└─────────┘     └──────┬───────┘
                       │
            ┌──────────┴──────────┐
            │                     │
            ▼                     ▼
    ┌──────────────┐    ┌──────────────┐
    │ Static Asset  │    │  API Call    │
    │ Cache-First   │    │ Network-First│
    │               │    │              │
    │ 1. Cek cache  │    │ 1. Coba net  │
    │ 2. Ada →      │    │ 2. Sukses →  │
    │    return     │    │    return    │
    │ 3. Tidak ada →│    │ 3. Gagal →   │
    │    fetch+save │    │    cache?    │
    └──────────────┘    └──────────────┘
```

### 11.2 Service Worker — Implementasi

```javascript
// Lokasi: Frontend/src/Web/wwwroot/service-worker.js

// Nama cache — increment saat versi berubah
const CACHE_NAME = 'app-portal-v1';
const STATIC_ASSETS = [
  '/',
  '/manifest.json',
  '/css/app-portal.min.css',
  '/css/tailwind.min.css',
  '/css/dex-green.css',
  '/favicon.png',
  '/offline.html'  // Halaman fallback offline
];

// ─── INSTALL: Cache static assets ──────────────────────────────
self.addEventListener('install', (event) => {
  console.log('[SW] Install — caching static assets');
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(STATIC_ASSETS);
    })
  );
  // Activate segera, jangan tunggu close
  self.skipWaiting();
});

// ─── ACTIVATE: Hapus cache lama ───────────────────────────────
self.addEventListener('activate', (event) => {
  console.log('[SW] Activate — cleaning old caches');
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys
          .filter((key) => key !== CACHE_NAME)
          .map((key) => caches.delete(key))
      );
    })
  );
  // Ambil alih halaman yang sudah terbuka
  self.clients.claim();
});

// ─── FETCH: Intercept request ─────────────────────────────────
self.addEventListener('fetch', (event) => {
  const request = event.request;

  // API calls → Network-First
  if (request.url.includes('/api/')) {
    event.respondWith(networkFirstWithFallback(request));
    return;
  }

  // Static assets → Cache-First
  event.respondWith(cacheFirstWithFallback(request));
});

// ─── STRATEGY: Cache-First ────────────────────────────────────
async function cacheFirstWithFallback(request) {
  const cached = await caches.match(request);
  if (cached) return cached;

  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    // Jika gagal dan ini navigation request → tampilkan offline page
    if (request.mode === 'navigate') {
      return caches.match('/offline.html');
    }
    return new Response('Offline', { status: 503 });
  }
}

// ─── STRATEGY: Network-First ──────────────────────────────────
async function networkFirstWithFallback(request) {
  try {
    const response = await fetch(request);
    if (response.ok) {
      const cache = await caches.open(CACHE_NAME);
      cache.put(request, response.clone());
    }
    return response;
  } catch {
    const cached = await caches.match(request);
    if (cached) return cached;

    // API offline dan tidak ada cache → return error
    return new Response(
      JSON.stringify({ success: false, message: 'Anda sedang offline', data: null }),
      { status: 503, headers: { 'Content-Type': 'application/json' } }
    );
  }
}
```

```javascript
// Lokasi: Frontend/src/Web/wwwroot/service-worker.published.js
// Sama dengan service-worker.js tetapi untuk production
// Di-build oleh blazor dengan versi hash otomatis
```

### 11.3 Web App Manifest

```json
// Lokasi: Frontend/src/Web/wwwroot/manifest.json
{
  "name": "Portal Aplikasi Perusahaan",
  "short_name": "Portal App",
  "description": "Portal aplikasi internal perusahaan minyak kelapa sawit",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#1a56db",
  "orientation": "portrait-primary",
  "icons": [
    {
      "src": "icons/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "icons/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any maskable"
    }
  ]
}
```

### 11.4 Offline Fallback Page

```html
<!-- Lokasi: Frontend/src/Web/wwwroot/offline.html -->
<!DOCTYPE html>
<html lang="id">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Offline — Portal App</title>
    <style>
        body {
            font-family: system-ui, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: #f3f4f6;
        }
        .offline-card {
            text-align: center;
            padding: 2rem;
            background: white;
            border-radius: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        .icon { font-size: 4rem; }
        h1 { font-size: 1.5rem; margin: 1rem 0; }
        p { color: #6b7280; }
    </style>
</head>
<body>
    <div class="offline-card">
        <div class="icon">📡</div>
        <h1>Koneksi Terputus</h1>
        <p>Aplikasi akan melanjutkan secara otomatis<br>saat koneksi internet pulih.</p>
        <p class="hint">Data yang sudah tersimpan tetap dapat diakses.</p>
    </div>
</body>
</html>
```

### 11.5 PWA Service Registration di Program.cs

```csharp
// Frontend/src/Web/Program.cs — tambahkan registrasi service worker

// WASM Client (Web.Client/Program.cs)
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ... registrasi lainnya ...

// Service worker registration via JS
var js = builder.Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
await js.InvokeVoidAsync("navigator.serviceWorker.register", "service-worker.js");

// Atau lebih baik, buat service khusus:
// builder.Services.AddScoped<PwaService>();
```

```csharp
// Alternatif: Buat PwaService untuk mengelola lifecycle PWA
// Lokasi: Frontend/src/Web/Services/Offline/PwaService.cs

using Microsoft.JSInterop;

namespace AppPortal.App.Frontend.Web.Services.Offline;

public class PwaService
{
    private readonly IJSRuntime _js;
    private const string SwPath = "service-worker.js";

    public PwaService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task RegisterServiceWorkerAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("navigator.serviceWorker.register", SwPath);
            Console.WriteLine("[PWA] Service Worker registered");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PWA] Failed to register SW: {ex.Message}");
        }
    }

    /// <summary>
    /// Cek apakah ada update SW tersedia
    /// </summary>
    public async Task<bool> CheckForUpdateAsync()
    {
        var registration = await _js.InvokeAsync<IJSObjectReference>(
            "navigator.serviceWorker.getRegistration");
        if (registration is null) return false;

        await registration.InvokeVoidAsync("update");
        return true;
    }
}
```

### 11.6 Update Notification — "New Version Available"

```razor
@* Lokasi: Frontend/src/Web/UI/Shared/UpdateNotification.razor *@
@* Tampil saat ada versi baru aplikasi *@

@if (_updateAvailable)
{
    <div class="update-banner">
        <span>Versi baru aplikasi tersedia.</span>
        <button class="btn btn-sm btn-primary" @onclick="UpdateAsync">
            Perbarui Sekarang
        </button>
    </div>
}

@code {
    private bool _updateAvailable;

    protected override async Task OnInitializedAsync()
    {
        var js = await JSHost.ImportAsync("service-worker-update.js");

        // Listener dari service worker
        await js.InvokeVoidAsync("listenForUpdate", DotNetObjectReference.Create(this));
    }

    [JSInvokable]
    public void NotifyUpdateAvailable()
    {
        _updateAvailable = true;
        StateHasChanged();
    }

    private async Task UpdateAsync()
    {
        // Skip waiting → SW baru activate
        var js = await JSHost.ImportAsync("service-worker-update.js");
        await js.InvokeVoidAsync("skipWaiting");
        // Reload halaman dengan SW baru
        NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
    }
}
```

### 11.7 PWA Lifecycle & Blazor Best Practices

```markdown
## PWA + Blazor AutoInteractive — Best Practices

1. ✅ Service worker hanya aktif di browser (WASM) — tidak di server/prerender
2. ✅ Register SW hanya saat `OperatingSystem.IsBrowser() == true`
3. ❌ JANGAN cache halaman yang memerlukan auth di SW — biarkan Network-First
4. ✅ Cache static assets (WASM files, CSS, JS) dengan Cache-First
5. ✅ Cache API responses read-only (GET) — jangan cache POST/PUT/DELETE
6. ✅ Beri user kontrol untuk update (jangan auto-update tanpa notifikasi)
7. ✅ Sediakan halaman offline.html yang informatif
8. ❌ JANGAN simpan token/auth di SW — SW berbagi storage dengan origin
```

---

## Phase 12: Network Connectivity Service

### 🎯 Tujuan
Mendeteksi status online/offline browser dan memberikan event ke seluruh modul saat status berubah.

### 12.1 NetworkStatusService

```csharp
// Lokasi: Frontend/src/Web/Services/Offline/NetworkStatusService.cs

using Microsoft.JSInterop;

namespace AppPortal.App.Frontend.Web.Services.Offline;

/// <summary>
/// Memantau status koneksi browser via navigator.onLine + online/offline events.
/// Semua modul bisa subscribe ke perubahan status.
/// </summary>
public class NetworkStatusService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly ILogger<NetworkStatusService> _logger;
    private DotNetObjectReference<NetworkStatusService>? _dotNetRef;

    public event Action<bool>? OnStatusChanged;

    public bool IsOnline { get; private set; } = true;

    public NetworkStatusService(IJSRuntime js, ILogger<NetworkStatusService> logger)
    {
        _js = js;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);

        // Baca status awal
        IsOnline = await _js.InvokeAsync<bool>("eval", "navigator.onLine");

        // Daftarkan listener JS → .NET
        await _js.InvokeVoidAsync(
            "networkStatusHelper.listen",
            _dotNetRef,
            "OnNetworkChange");

        _logger.LogInformation("Network status initialized: {Status}", IsOnline ? "Online" : "Offline");
    }

    [JSInvokable]
    public void OnNetworkChange(bool isOnline)
    {
        if (IsOnline == isOnline) return;

        IsOnline = isOnline;
        _logger.LogWarning("Network status changed to: {Status}", isOnline ? "Online" : "Offline");
        OnStatusChanged?.Invoke(isOnline);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        if (_js is not null)
        {
            try
            {
                await _js.InvokeVoidAsync("networkStatusHelper.dispose");
            }
            catch { /* JSInterop mungkin tidak tersedia saat dispose */ }
        }
    }
}
```

### 12.2 JavaScript Helper

```javascript
// Lokasi: Frontend/src/Web/wwwroot/js/network-status.js
// Load di index.html atau _Host.cshtml

window.networkStatusHelper = {
    _dotNetRef: null,
    _methodName: null,
    _onlineHandler: null,
    _offlineHandler: null,

    listen: function (dotNetRef, methodName) {
        this._dotNetRef = dotNetRef;
        this._methodName = methodName;

        this._onlineHandler = () => {
            dotNetRef.invokeMethodAsync(methodName, true);
        };
        this._offlineHandler = () => {
            dotNetRef.invokeMethodAsync(methodName, false);
        };

        window.addEventListener('online', this._onlineHandler);
        window.addEventListener('offline', this._offlineHandler);
    },

    dispose: function () {
        if (this._onlineHandler) {
            window.removeEventListener('online', this._onlineHandler);
            window.removeEventListener('offline', this._offlineHandler);
        }
        this._dotNetRef = null;
    }
};
```

### 12.3 Registrasi di Program.cs

```csharp
// Frontend/src/Web/Program.cs

// ─── Offline / Network ─────────────────────────────────────────
builder.Services.AddScoped<NetworkStatusService>();
builder.Services.AddScoped<PwaService>();

// Di root component (App.razor atau MainLayout.razor), panggil:
// @inject NetworkStatusService Network
// protected override async Task OnInitializedAsync() => await Network.InitializeAsync();
```

---

## Phase 13: IndexedDB Emergency Database

### 🎯 Tujuan
Menyediakan "database darurat" di browser via IndexedDB yang bisa diakses dari C# melalui JSInterop. Digunakan sebagai penyimpanan lokal saat offline dan cache offline untuk data yang sudah pernah di-fetch.

### 13.1 Arsitektur IndexedDB

```
IndexedDB: AppPortalDB
├── Version: 1 (di-increment saat schema berubah)
│
├── Object Stores (setara "tabel")
│   ├── sparta_grading          → Module Sparta - data grading
│   ├── sparta_masterdata       → Module Sparta - master data
│   ├── legal_contracts         → Module Legal - kontrak
│   ├── portal_applications     → Portal - daftar aplikasi
│   ├── sync_queue              → Antrian perubahan yang pending
│   └── sync_metadata           → Metadata sinkronisasi
│
└── Key Path: "id" (semua object store primary key = id)
```

### 13.2 IndexedDbService — JSInterop Wrapper

```csharp
// Lokasi: Frontend/src/Web/Services/Offline/IndexedDbService.cs

using System.Text.Json;
using Microsoft.JSInterop;

namespace AppPortal.App.Frontend.Web.Services.Offline;

/// <summary>
/// Service untuk mengakses IndexedDB dari Blazor WASM via JSInterop.
/// Menyediakan CRUD generik untuk semua object store.
/// </summary>
public class IndexedDbService
{
    private readonly IJSRuntime _js;
    private const string DbName = "AppPortalDB";
    private const int DbVersion = 1;

    public IndexedDbService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Inisialisasi database — buka koneksi, buat object stores jika belum ada.
    /// Panggil sekali di awal aplikasi.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _js.InvokeVoidAsync("indexedDbHelper.openDatabase", DbName, DbVersion);
    }

    // ─── GENERIC CRUD ──────────────────────────────────────────

    /// <summary>
    /// Simpan satu item ke object store. Jika id sudah ada, akan di-update (upsert).
    /// </summary>
    public async Task UpsertAsync<T>(string storeName, T item)
    {
        var json = JsonSerializer.Serialize(item);
        await _js.InvokeVoidAsync("indexedDbHelper.upsert", DbName, storeName, json);
    }

    /// <summary>
    /// Simpan banyak item sekaligus.
    /// </summary>
    public async Task UpsertBatchAsync<T>(string storeName, IEnumerable<T> items)
    {
        var jsonArray = JsonSerializer.Serialize(items);
        await _js.InvokeVoidAsync("indexedDbHelper.upsertBatch", DbName, storeName, jsonArray);
    }

    /// <summary>
    /// Ambil semua data dari object store.
    /// </summary>
    public async Task<List<T>> GetAllAsync<T>(string storeName)
    {
        var json = await _js.InvokeAsync<string>("indexedDbHelper.getAll", DbName, storeName);
        if (string.IsNullOrEmpty(json)) return new List<T>();
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    /// <summary>
    /// Ambil satu item berdasarkan id.
    /// </summary>
    public async Task<T?> GetByIdAsync<T>(string storeName, object id)
    {
        var json = await _js.InvokeAsync<string?>("indexedDbHelper.getById", DbName, storeName, id);
        if (string.IsNullOrEmpty(json)) return default;
        return JsonSerializer.Deserialize<T>(json);
    }

    /// <summary>
    /// Hapus satu item berdasarkan id.
    /// </summary>
    public async Task DeleteAsync(string storeName, object id)
    {
        await _js.InvokeVoidAsync("indexedDbHelper.delete", DbName, storeName, id);
    }

    /// <summary>
    /// Hapus semua data dari object store (clear).
    /// </summary>
    public async Task ClearStoreAsync(string storeName)
    {
        await _js.InvokeVoidAsync("indexedDbHelper.clearStore", DbName, storeName);
    }

    /// <summary>
    /// Hitung jumlah item di object store.
    /// </summary>
    public async Task<int> CountAsync(string storeName)
    {
        return await _js.InvokeAsync<int>("indexedDbHelper.count", DbName, storeName);
    }
}
```

### 13.3 JavaScript IndexedDB Helper

```javascript
// Lokasi: Frontend/src/Web/wwwroot/js/indexeddb-helper.js

window.indexedDbHelper = {
    _db: null,

    // ─── BUKA DATABASE ─────────────────────────────────────────
    openDatabase: function (dbName, version) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(dbName, version);

            request.onupgradeneeded = (event) => {
                const db = event.target.result;

                // Object stores — dibuat otomatis jika belum ada
                const stores = [
                    'sparta_grading',
                    'sparta_masterdata',
                    'legal_contracts',
                    'portal_applications',
                    'sync_queue',
                    'sync_metadata'
                ];

                stores.forEach(storeName => {
                    if (!db.objectStoreNames.contains(storeName)) {
                        db.createObjectStore(storeName, { keyPath: 'id' });
                    }
                });
            };

            request.onsuccess = (event) => {
                this._db = event.target.result;
                console.log('[IndexedDB] Database opened:', dbName);
                resolve();
            };

            request.onerror = (event) => {
                console.error('[IndexedDB] Error:', event.target.error);
                reject(event.target.error);
            };
        });
    },

    // ─── CRUD ──────────────────────────────────────────────────
    upsert: function (dbName, storeName, json) {
        const item = JSON.parse(json);
        return this._transaction(storeName, 'readwrite', (store) => {
            store.put(item);
        });
    },

    upsertBatch: function (dbName, storeName, jsonArray) {
        const items = JSON.parse(jsonArray);
        return this._transaction(storeName, 'readwrite', (store) => {
            items.forEach(item => store.put(item));
        });
    },

    getAll: function (dbName, storeName) {
        return this._transaction(storeName, 'readonly', (store) => {
            const request = store.getAll();
            return new Promise((resolve) => {
                request.onsuccess = () => resolve(JSON.stringify(request.result));
            });
        });
    },

    getById: function (dbName, storeName, id) {
        return this._transaction(storeName, 'readonly', (store) => {
            const request = store.get(id);
            return new Promise((resolve) => {
                request.onsuccess = () => {
                    resolve(request.result ? JSON.stringify(request.result) : null);
                };
            });
        });
    },

    delete: function (dbName, storeName, id) {
        return this._transaction(storeName, 'readwrite', (store) => {
            store.delete(id);
        });
    },

    clearStore: function (dbName, storeName) {
        return this._transaction(storeName, 'readwrite', (store) => {
            store.clear();
        });
    },

    count: function (dbName, storeName) {
        return this._transaction(storeName, 'readonly', (store) => {
            const request = store.count();
            return new Promise((resolve) => {
                request.onsuccess = () => resolve(request.result);
            });
        });
    },

    // ─── TRANSACTION HELPER ────────────────────────────────────
    _transaction: function (storeName, mode, callback) {
        return new Promise((resolve, reject) => {
            if (!this._db) {
                reject(new Error('Database not opened'));
                return;
            }
            const tx = this._db.transaction(storeName, mode);
            const store = tx.objectStore(storeName);

            // Jalankan callback dengan object store
            const result = callback(store);
            if (result instanceof Promise) {
                result.then(resolve).catch(reject);
            } else {
                resolve(result);
            }

            tx.oncomplete = () => resolve();
            tx.onerror = (event) => reject(event.target.error);
        });
    }
};
```

### 13.4 Typed Store per Module — Contoh Sparta

```csharp
// Lokasi: Frontend/src/Web/Modules/Sparta/Offline/SpartaDbStore.cs

namespace AppPortal.App.Frontend.Web.Modules.Sparta.Offline;

/// <summary>
/// Store khusus module Sparta ke IndexedDB.
/// Setiap method adalah typed wrapper untuk operasi IndexedDB.
/// </summary>
public class SpartaDbStore
{
    private readonly IndexedDbService _db;
    private const string GradingStore = "sparta_grading";
    private const string MasterDataStore = "sparta_masterdata";

    public SpartaDbStore(IndexedDbService db)
    {
        _db = db;
    }

    // ─── GRADING ─────────────────────────────────────────────
    public async Task SaveGradingListAsync(List<GradingItem> items, CancellationToken ct = default)
    {
        await _db.UpsertBatchAsync(GradingStore, items);
    }

    public async Task<List<GradingItem>> GetGradingListAsync(CancellationToken ct = default)
    {
        return await _db.GetAllAsync<GradingItem>(GradingStore);
    }

    public async Task SaveGradingItemAsync(GradingItem item)
    {
        await _db.UpsertAsync(GradingStore, item);
    }

    public async Task<GradingItem?> GetGradingItemAsync(int id)
    {
        return await _db.GetByIdAsync<GradingItem>(GradingStore, id);
    }

    // ─── MASTER DATA ──────────────────────────────────────────
    public async Task SaveMasterDataAsync<T>(List<T> items) where T : class
    {
        await _db.UpsertBatchAsync(MasterDataStore, items);
    }

    public async Task ClearAllAsync()
    {
        await _db.ClearStoreAsync(GradingStore);
        await _db.ClearStoreAsync(MasterDataStore);
    }
}
```

### 13.5 Data Expiration & Cache Invalidation

```csharp
// Setiap item yang disimpan di IndexedDB bisa memiliki metadata expiry:

public class CacheItem<T>
{
    public string Id { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string? ETag { get; set; } // Untuk validasi server
}

// Gunakan di store:
public async Task SaveWithExpiryAsync<T>(string storeName, string id, T data, TimeSpan ttl)
{
    var cacheItem = new CacheItem<T>
    {
        Id = id,
        Data = data,
        CachedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.Add(ttl)
    };
    await _db.UpsertAsync(storeName, cacheItem);
}

public async Task<T?> GetWithExpiryAsync<T>(string storeName, string id)
{
    var item = await _db.GetByIdAsync<CacheItem<T>>(storeName, id);
    if (item is null) return default;
    if (DateTime.UtcNow > item.ExpiresAt) return default; // Expired
    return item.Data;
}
```

### 13.6 Registrasi di Program.cs

```csharp
// Frontend/src/Web/Program.cs

// ─── IndexedDB ─────────────────────────────────────────────────
builder.Services.AddScoped<IndexedDbService>();

// ─── Module Stores ────────────────────────────────────────────
builder.Services.AddScoped<SpartaDbStore>();
builder.Services.AddScoped<LegalDbStore>();
```

---

## Phase 14: Save Local, Sync Later Engine

### 🎯 Tujuan
Menyediakan engine sinkronisasi yang memproses antrian perubahan (Create, Update, Delete) yang dibuat saat offline — secara otomatis dikirim ke API vendor saat koneksi pulih.

### 14.1 Arsitektur Sync Engine

```
┌──────────────────────────────────────────────────────────────────┐
│                        SYNC ENGINE ARCHITECTURE                   │
│                                                                   │
│  ┌──────────┐    ┌──────────────┐    ┌──────────────────────┐    │
│  │ Module    │───▶│  SyncQueue   │───▶│     SyncEngine      │    │
│  │ Service   │    │  (IndexedDB) │    │  (Queue Processor)  │    │
│  │           │    │              │    │                      │    │
│  │ Create/   │    │ { id,        │    │ 1. FIFO: ambil      │    │
│  │ Update/   │    │   entity,    │    │    item tertua       │    │
│  │ Delete    │    │   action,    │    │ 2. Panggil API       │    │
│  │ → queue   │    │   payload,   │    │ 3. Sukses → hapus    │    │
│  │           │    │   timestamp, │    │    dari antrian      │    │
│  └──────────┘    │   retryCount }│    │ 4. Gagal → retry     │    │
│                  └──────────────┘    │    nanti              │    │
│                                      │ 5. Jika 5× gagal →   │    │
│  ┌──────────────┐                    │    konflik            │    │
│  │ NetworkStatus│◀───────────────────│                      │    │
│  │ (Auto-trigger│                    └──────────────────────┘    │
│  │  saat online)│                                              │
│  └──────────────┘                                              │
└──────────────────────────────────────────────────────────────────┘
```

### 14.2 Sync Queue — Pending Changes di IndexedDB

```csharp
// Lokasi: Frontend/src/Web/Services/Offline/SyncQueue.cs

using System.Text.Json;

namespace AppPortal.App.Frontend.Web.Services.Offline;

/// <summary>
/// Antrian FIFO untuk perubahan yang dibuat saat offline.
/// Setiap perubahan direkam sebagai SyncQueueItem dan disimpan di IndexedDB.
/// </summary>
public class SyncQueue
{
    private readonly IndexedDbService _db;
    private const string QueueStore = "sync_queue";
    private const string MetadataStore = "sync_metadata";

    public SyncQueue(IndexedDbService db)
    {
        _db = db;
    }

    /// <summary>
    /// Tambahkan perubahan ke antrian sinkronisasi.
    /// </summary>
    public async Task EnqueueAsync(string entityType, SyncAction action, object payload)
    {
        var item = new SyncQueueItem
        {
            Id = Guid.NewGuid().ToString(),
            EntityType = entityType,
            Action = action,
            PayloadJson = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow,
            Status = QueueStatus.Pending
        };

        await _db.UpsertAsync(QueueStore, item);
    }

    /// <summary>
    /// Ambil item berikutnya yang harus diproses (FIFO).
    /// </summary>
    public async Task<SyncQueueItem?> DequeueAsync()
    {
        var items = await _db.GetAllAsync<SyncQueueItem>(QueueStore);
        return items
            .Where(i => i.Status == QueueStatus.Pending)
            .OrderBy(i => i.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Ambil semua item pending (untuk batch processing).
    /// </summary>
    public async Task<List<SyncQueueItem>> GetAllPendingAsync()
    {
        var items = await _db.GetAllAsync<SyncQueueItem>(QueueStore);
        return items
            .Where(i => i.Status == QueueStatus.Pending)
            .OrderBy(i => i.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Tandai item sebagai sukses → hapus dari antrian.
    /// </summary>
    public async Task MarkCompletedAsync(string itemId)
    {
        await _db.DeleteAsync(QueueStore, itemId);
    }

    /// <summary>
    /// Tandai item sebagai gagal — increment retry count.
    /// </summary>
    public async Task MarkFailedAsync(string itemId, string error)
    {
        var item = await _db.GetByIdAsync<SyncQueueItem>(QueueStore, itemId);
        if (item is null) return;

        item.RetryCount++;
        item.LastError = error;
        item.LastAttemptAt = DateTime.UtcNow;

        if (item.RetryCount >= 5)
        {
            item.Status = QueueStatus.Conflict;
        }

        await _db.UpsertAsync(QueueStore, item);
    }

    /// <summary>
    /// Hitung jumlah item pending.
    /// </summary>
    public async Task<int> GetPendingCountAsync()
    {
        var items = await _db.GetAllAsync<SyncQueueItem>(QueueStore);
        return items.Count(i => i.Status == QueueStatus.Pending);
    }

    /// <summary>
    /// Simpan timestamp sinkronisasi terakhir.
    /// </summary>
    public async Task UpdateLastSyncTimeAsync()
    {
        var metadata = new SyncMetadata
        {
            Id = "last_sync",
            LastSyncAt = DateTime.UtcNow
        };
        await _db.UpsertAsync(MetadataStore, metadata);
    }

    public async Task<DateTime?> GetLastSyncTimeAsync()
    {
        var meta = await _db.GetByIdAsync<SyncMetadata>(MetadataStore, "last_sync");
        return meta?.LastSyncAt;
    }
}

// ─── ENUMS & MODELS ─────────────────────────────────────────

public enum SyncAction { Create, Update, Delete }
public enum QueueStatus { Pending, Conflict }

public class SyncQueueItem
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;  // e.g. "sparta_grading"
    public SyncAction Action { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public QueueStatus Status { get; set; } = QueueStatus.Pending;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}

public class SyncMetadata
{
    public string Id { get; set; } = "last_sync";
    public DateTime LastSyncAt { get; set; }
}
```

### 14.3 Sync Engine — Queue Processor

```csharp
// Lokasi: Frontend/src/Web/Services/Offline/SyncEngine.cs

using System.Text.Json;

namespace AppPortal.App.Frontend.Web.Services.Offline;

/// <summary>
/// Engine yang memproses antrian sinkronasi.
/// - Auto-trigger saat koneksi pulih
/// - Bisa dipicu manual oleh user
/// - FIFO: proses satu per satu, urut berdasarkan createdAt
/// - Retry 5× sebelum tandai sebagai konflik
/// - Batch: jika ada banyak item, proses tanpa jeda
/// </summary>
public class SyncEngine
{
    private readonly SyncQueue _queue;
    private readonly VendorHttpClient _api;
    private readonly NetworkStatusService _network;
    private readonly ILogger<SyncEngine> _logger;

    // Event yang bisa di-subscribe UI
    public event Action<int>? OnSyncCompleted;    // parameter: jumlah item di-sync
    public event Action<string>? OnSyncError;     // parameter: pesan error
    public event Action<int>? OnSyncProgress;     // parameter: current / total

    private bool _isProcessing;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public SyncEngine(
        SyncQueue queue,
        VendorHttpClient api,
        NetworkStatusService network,
        ILogger<SyncEngine> logger)
    {
        _queue = queue;
        _api = api;
        _network = network;
        _logger = logger;

        // Auto-sync saat koneksi pulih
        _network.OnStatusChanged += async (isOnline) =>
        {
            if (isOnline) await ProcessQueueAsync();
        };
    }

    /// <summary>
    /// Proses semua item pending di antrian.
    /// </summary>
    public async Task ProcessQueueAsync()
    {
        if (_isProcessing) return;
        if (!_network.IsOnline)
        {
            _logger.LogWarning("Sync skipped — device is offline");
            return;
        }

        await _semaphore.WaitAsync();
        _isProcessing = true;

        try
        {
            var pendingItems = await _queue.GetAllPendingAsync();
            if (pendingItems.Count == 0)
            {
                _logger.LogDebug("No pending items to sync");
                return;
            }

            _logger.LogInformation("Starting sync for {Count} items", pendingItems.Count);
            var successCount = 0;

            for (int i = 0; i < pendingItems.Count; i++)
            {
                var item = pendingItems[i];
                OnSyncProgress?.Invoke(i + 1);

                try
                {
                    await ProcessItemAsync(item);
                    await _queue.MarkCompletedAsync(item.Id);
                    successCount++;
                    _logger.LogInformation("Synced {Entity} - {Action}", item.EntityType, item.Action);
                }
                catch (VendorApiException ex) when (ex.StatusCode == 409)
                {
                    // Conflict — tandai untuk review manual
                    await _queue.MarkFailedAsync(item.Id, $"Conflict: {ex.Message}");
                    OnSyncError?.Invoke($"Konflik data pada {item.EntityType}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    await _queue.MarkFailedAsync(item.Id, ex.Message);
                    _logger.LogError(ex, "Failed to sync item {ItemId}", item.Id);
                }
            }

            await _queue.UpdateLastSyncTimeAsync();
            OnSyncCompleted?.Invoke(successCount);

            _logger.LogInformation("Sync completed: {Success}/{Total} items",
                successCount, pendingItems.Count);
        }
        finally
        {
            _isProcessing = false;
            _semaphore.Release();
        }
    }

    private async Task ProcessItemAsync(SyncQueueItem item)
    {
        var endpoint = MapEntityToEndpoint(item.EntityType, item.Action);
        var payload = JsonDocument.Parse(item.PayloadJson);

        switch (item.Action)
        {
            case SyncAction.Create:
                await _api.PostAsync<JsonElement, JsonElement>(endpoint, payload.RootElement);
                break;

            case SyncAction.Update:
                await _api.PostAsync<JsonElement, JsonElement>($"{endpoint}/update", payload.RootElement);
                break;

            case SyncAction.Delete:
                var id = payload.RootElement.GetProperty("id").GetString();
                // Asumsi DELETE endpoint: {endpoint}/{id}
                // Gunakan HttpClient langsung karena VendorHttpClient hanya GET/POST
                break;
        }
    }

    private static string MapEntityToEndpoint(string entityType, SyncAction action)
    {
        return entityType switch
        {
            "sparta_grading" => "sparta/grading",
            "sparta_masterdata" => "sparta/master-data",
            "legal_contracts" => "legal/contracts",
            _ => entityType.Replace('_', '/')
        };
    }
}
```

### 14.4 Conflict Resolution Strategy

```csharp
// Lokasi: Frontend/src/Web/Services/Offline/ConflictResolver.cs

namespace AppPortal.App.Frontend.Web.Services.Offline;

/// <summary>
/// Strategi resolusi konflik saat sinkronasi.
/// </summary>
public class ConflictResolver
{
    private readonly ILogger<ConflictResolver> _logger;

    public ConflictResolver(ILogger<ConflictResolver> logger)
    {
        _logger = logger;
    }

    public ConflictResolution Resolve(SyncQueueItem item, VendorApiException apiError)
    {
        // Strategy 1: Last-Write-Wins (default)
        // Kirim data lokal → timpa data server
        // Cocok untuk: catatan grading, log aktivitas

        if (apiError.StatusCode == 409)
        {
            _logger.LogWarning("Conflict detected for {Entity} ({Id}) — applying Last-Write-Wins",
                item.EntityType, item.Id);

            return ConflictResolution.RetryWithForce;
        }

        // Strategy 2: Server Wins
        // Abaikan data lokal, ambil data server
        // Cocok untuk: master data yang diupdate admin

        // Strategy 3: Manual Resolution
        // Tandai sebagai konflik, user harus review manual
        // Cocok untuk: data finansial, kontrak legal

        return ConflictResolution.MarkForReview;
    }
}

public enum ConflictResolution
{
    RetryWithForce,   // Kirim ulang dengan force flag
    AcceptServer,     // Ambil data server, buang lokal
    MarkForReview     // User harus review manual
}
```

### 14.5 Integrasi per Vertical Slice — Contoh SpartaSyncService

```csharp
// Lokasi: Frontend/src/Web/Modules/Sparta/Services/SpartaSyncService.cs

namespace AppPortal.App.Frontend.Web.Modules.Sparta.Services;

/// <summary>
/// Service sinkronasi khusus untuk module Sparta.
/// Menggabungkan API online + IndexedDB offline + SyncQueue.
/// Pola: "Save Local, Sync Later"
/// </summary>
public class SpartaSyncService
{
    private readonly SpartaService _onlineService;
    private readonly SpartaDbStore _localDb;
    private readonly SyncQueue _syncQueue;
    private readonly NetworkStatusService _network;
    private readonly ILogger<SpartaSyncService> _logger;

    public SpartaSyncService(
        SpartaService onlineService,
        SpartaDbStore localDb,
        SyncQueue syncQueue,
        NetworkStatusService network,
        ILogger<SpartaSyncService> logger)
    {
        _onlineService = onlineService;
        _localDb = localDb;
        _syncQueue = syncQueue;
        _network = network;
        _logger = logger;
    }

    // ─── SAVE (Create/Update) ──────────────────────────────────
    // Pola: Simpan ke API, lalu simpan ke lokal sebagai backup.
    // Jika offline: simpan ke lokal dulu, lalu queue.

    public async Task SaveGradingAsync(GradingPayload payload)
    {
        if (_network.IsOnline)
        {
            try
            {
                // 1. Kirim ke API vendor
                var result = await _onlineService.SubmitGradingAsync(payload);

                // 2. Simpan hasil ke IndexedDB sebagai backup
                await _localDb.SaveGradingItemAsync(new GradingItem
                {
                    Id = result.Id,
                    Payload = payload,
                    SyncedAt = DateTime.UtcNow
                });

                _logger.LogInformation("Grading saved online, cached locally");
                return;
            }
            catch (VendorApiException ex)
            {
                _logger.LogWarning(ex, "API save failed — falling back to offline");
                // Fallthrough: simpan lokal + queue
            }
        }

        // 3. Simpan ke IndexedDB (emergency)
        var localItem = new GradingItem
        {
            Id = DateTime.UtcNow.Ticks, // ID sementara
            Payload = payload,
            CreatedAt = DateTime.UtcNow,
            IsPendingSync = true
        };

        await _localDb.SaveGradingItemAsync(localItem);

        // 4. Queue untuk sync nanti
        await _syncQueue.EnqueueAsync(
            entityType: "sparta_grading",
            action: SyncAction.Create,
            payload: payload);

        _logger.LogInformation("Grading saved locally — queued for sync");
    }

    // ─── READ ──────────────────────────────────────────────────
    // Pola: Coba API dulu, fallback ke IndexedDB.

    public async Task<List<GradingItem>> GetGradingListAsync()
    {
        if (_network.IsOnline)
        {
            try
            {
                var data = await _onlineService.GetGradingListAsync();
                // Cache ke lokal
                await _localDb.SaveGradingListAsync(data);
                return data;
            }
            catch
            {
                // Fallback ke lokal
            }
        }

        var localData = await _localDb.GetGradingListAsync();
        if (localData.Count == 0)
        {
            _logger.LogWarning("No data available — offline and no cache");
        }
        return localData;
    }
}

// Model lokal untuk Sparta
public class GradingItem
{
    public long Id { get; set; }
    public GradingPayload? Payload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public bool IsPendingSync { get; set; }
}
```

### 14.6 UI Indicators — Pending Sync + Manual Sync

Lihat [Phase 6.4 — SyncStatusIndicator](#64-offline-mode-state--connectivity-banner--sync-indicator) untuk implementasi UI komponen.

### 14.7 Smart Deferral — Batching & Throttling

```csharp
// Di SyncEngine — tambahkan logika smart deferral

public class SyncEngine
{
    // ...

    /// <summary>
    /// Proses antrian dengan batching — jika ada banyak item,
    /// jangan flood API. Kirim dalam batch yang lebih kecil.
    /// </summary>
    private async Task ProcessWithBatchingAsync(List<SyncQueueItem> items)
    {
        const int batchSize = 10;
        var batches = items.Chunk(batchSize);

        foreach (var batch in batches)
        {
            if (!_network.IsOnline)
            {
                _logger.LogWarning("Sync paused — connection lost mid-sync");
                break;
            }

            // Proses batch
            await Task.WhenAll(batch.Select(ProcessItemWithRetryAsync));

            // Jeda antar batch agar tidak overload API
            await Task.Delay(500);
        }
    }

    private async Task ProcessItemWithRetryAsync(SyncQueueItem item)
    {
        // Retry 3× dengan exponential backoff
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                await ProcessItemAsync(item);
                await _queue.MarkCompletedAsync(item.Id);
                return;
            }
            catch (VendorApiException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }

        await _queue.MarkFailedAsync(item.Id, "Failed after 3 retries");
    }
}
```

### 14.8 Token Refresh During Deferred Sync

```csharp
// Saat sync engine berjalan, token mungkin sudah expired.
// AuthDelegatingHandler sudah handle auto-refresh (lihat Phase 1.4),
// tapi pastikan sync engine menunggu jika token sedang di-refresh:

public class SyncEngine
{
    private readonly AuthService _auth;

    // Sebelum proses queue, pastikan token valid
    private async Task EnsureAuthenticatedAsync()
    {
        if (!_auth.IsAuthenticated)
        {
            // Coba restore dari localStorage
            await _auth.TryRestoreSessionAsync();
        }

        if (!_auth.IsAuthenticated)
        {
            throw new InvalidOperationException(
                "Cannot sync — user not authenticated. Silakan login ulang.");
        }
    }
}
```

### 14.9 Full Offline Flow — Skenario Lapangan

Berikut contoh skenario lengkap di lapangan (user di kebun sawit dengan sinyal buruk):

```
1. 📴 User mengisi data grading TBS di form Sparta
2. ❌ Internet tidak stabil → API call gagal
3. 💾 Data disimpan ke IndexedDB (emergency) + SyncQueue
4. 🔄 UI menampilkan "1 menunggu sinkronisasi" di SyncStatusIndicator
5. ...
6. 📶 Beberapa jam kemudian, user mendapat sinyal
7. 🔔 NetworkStatusService mendeteksi online → trigger SyncEngine
8. ⏳ SyncEngine proses antrian FIFO
9. ✅ Data terkirim ke API vendor
10. 🗑️ Item dihapus dari SyncQueue
11. ✅ SyncStatusIndicator: "Tersinkron 14:30"
12. 🟢 User bisa lanjut kerja
```

---

## Checklist Eksekusi

### Pra-Eksekusi (Wajib)

- [ ] Dapatkan dokumentasi API/Swagger dari vendor
- [ ] Konfirmasi auth mechanism (endpoint login/refresh, token format, expiry)
- [ ] Konfirmasi response envelope format (`ApiResponse<T>`)
- [ ] Konfirmasi daftar endpoint per modul
- [ ] Konfirmasi CORS sudah diallow oleh vendor untuk domain aplikasi
- [ ] Test call manual ke API via browser/Postman untuk verifikasi koneksi

### Phase 0 — Foundation

- [ ] Buat folder `Models/Vendor/` dengan DTO sesuai dokumentasi API
- [ ] Buat folder `Services/Http/` untuk HttpClient utilities
- [ ] Buat folder `Services/Auth/` untuk authentication service

### Phase 1 — Authentication

- [ ] Buat `AuthService.cs` (login, logout, restore session, refresh token)
- [ ] Buat `AuthDelegatingHandler.cs` (auto-attach Bearer token)
- [ ] Implementasi login page di `Modules/Auth/Pages/Login.razor`
- [ ] Register semua service di `Program.cs`

### Phase 2 — HttpClient Pipeline

- [ ] Buat `VendorHttpClient.cs` dengan error handling standar
- [ ] Tambahkan Polly retry + circuit breaker policies
- [ ] Test resilience dengan simulate API failure

### Phase 3 — Module Structure

- [ ] Buat folder `Modules/` dengan sub-folder per aplikasi
- [ ] Buat `Modules/Dashboard/` — halaman utama yang panggil `/applications`
- [ ] Buat `Modules/Sparta/`, `Modules/Legal/`, `Modules/CorePortal/`
- [ ] Setiap module punya `Services/`, `Models/`, `Features/` subfolder

### Phase 4 — Routing

- [ ] Konfigurasi statis route untuk modul dengan fitur kompleks
- [ ] Buat `UI/Shared/DynamicModulePage.razor` untuk modul wrapper
- [ ] Dashboard grid membaca daftar aplikasi dari API vendor
- [ ] Navigasi otomatis: static route → fitur kompleks, dynamic → `/app/{slug}`

### Phase 5 — State Management

- [ ] Implementasi `PersistentComponentState` di setiap halaman yang fetch data
- [ ] Integrasi `AuthService` di `MainLayout` untuk global auth state
- [ ] Opsional: `PortalEventBus` untuk komunikasi antar modul

### Phase 6 — UI State Patterns

- [ ] Terapkan 4-state pattern (Loading/Empty/Error/Success) di setiap halaman
- [ ] Buat reusable state container component di `UI/Shared/`
- [ ] Setiap error state punya tombol retry

### Phase 7 — Base UI Integration

- [ ] Verifikasi semua halaman hanya consume `UI/Base`, tidak memodifikasi
- [ ] Pastikan `@rendermode InteractiveAuto` di semua halaman module
- [ ] Test prerendering vs WASM state transfer

### Phase 8 — Logging

- [ ] Konfigurasi logging level per environment
- [ ] Tambahkan logging di setiap service
- [ ] Setup error tracking (Application Insights / Sentry)
- [ ] Global error boundary di `App.razor`

### Phase 9 — Configuration

- [ ] Buat `appsettings.json`, `appsettings.Staging.json`, `appsettings.Production.json`
- [ ] Buat `AppConfiguration.cs` untuk akses konfigurasi terpusat

### Phase 10 — Testing

- [ ] Buat unit test untuk `VendorHttpClient`
- [ ] Buat unit test untuk setiap module service
- [ ] Setup mock API untuk development tanpa vendor

### Phase 11 — PWA

- [ ] Buat `service-worker.js` dengan cache-first asset + network-first API strategy
- [ ] Buat `service-worker.published.js` untuk production
- [ ] Buat `manifest.json` dengan icons, theme, display mode
- [ ] Buat `offline.html` halaman fallback offline
- [ ] Buat folder `wwwroot/icons/` dengan icon 192x192 dan 512x512
- [ ] Buat `PwaService.cs` untuk registrasi SW lifecycle
- [ ] Buat `UpdateNotification.razor` untuk notifikasi versi baru
- [ ] Test install PWA di browser (Lighthouse audit)

### Phase 12 — Network Status

- [ ] Buat `wwwroot/js/network-status.js` helper JS
- [ ] Buat `NetworkStatusService.cs` dengan JSInterop listener
- [ ] Buat `ConnectivityBanner.razor` — banner offline di main layout
- [ ] Integrasi event bus: network change → sync trigger

### Phase 13 — IndexedDB

- [ ] Buat `wwwroot/js/indexeddb-helper.js` — IndexedDB wrapper JS
- [ ] Buat `IndexedDbService.cs` — typed C# CRUD via JSInterop
- [ ] Buat `SpartaDbStore.cs` — typed store untuk module Sparta
- [ ] Buat `LegalDbStore.cs` — typed store untuk module Legal
- [ ] Tentukan schema object store per modul
- [ ] Test inisialisasi database + CRUD di browser DevTools

### Phase 14 — Sync Engine

- [ ] Buat `SyncQueue.cs` — antrian FIFO di IndexedDB
- [ ] Buat `SyncEngine.cs` — processor queue + auto-sync on reconnect
- [ ] Buat `ConflictResolver.cs` — strategi Last-Write-Wins / Server-Wins / Manual
- [ ] Buat `SpartaSyncService.cs` — integrasi Sparta dengan pola "Save Local, Sync Later"
- [ ] Buat `SyncStatusIndicator.razor` — pending count + manual sync button
- [ ] Test offline flow: save data, putus koneksi, sambung lagi, verifikasi sync
- [ ] Test conflict: data berubah di server & lokal → resolusi

---

## Ringkasan Arsitektur

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    PORTAL SHELL (Blazor AutoInteractive + PWA)            │
│                                                                           │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐       ┌─────┐  │
│  │ Auth     │  │ Dashboard│  │ Sparta   │  │ Legal    │  ...  │PWA  │  │
│  │ Module   │  │ Module   │  │ Module   │  │ Module   │       │SW   │  │
│  │          │  │          │  │          │  │          │       │     │  │
│  │ Login    │  │ App Grid │  │ Grading  │  │ Contracts│       │Cache│  │
│  │ Logout   │  │ (Dynamic)│  │ Master   │  │          │       │     │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘       └─────┘  │
│       │             │             │             │                       │
│       ▼             ▼             ▼             ▼                       │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │               OFFLINE AWARE MODULE SERVICE LAYER                  │   │
│  │  ┌─────────────────┐ ┌────────────────┐ ┌──────────────────┐    │   │
│  │  │ SpartaSyncService│ │ LegalSyncService│ │ ModuleRegistry  │    │   │
│  │  │ (Save Local,    │ │ (Save Local,   │ │                  │    │   │
│  │  │  Sync Later)    │ │  Sync Later)   │ │                  │    │   │
│  │  └────────┬────────┘ └───────┬────────┘ └──────────────────┘    │   │
│  │           │                  │                                   │   │
│  │           ▼                  ▼                                   │   │
│  │  ┌──────────────────────────────────────────────────────────┐   │   │
│  │  │   OFFLINE ENGINE (IndexedDB + SyncQueue + SyncEngine)    │   │   │
│  │  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐   │   │   │
│  │  │  │ SpartaDbStore │  │  SyncQueue   │  │ SyncEngine   │   │   │   │
│  │  │  │ LegalDbStore  │  │  (FIFO)      │  │ (Auto-Sync)  │   │   │   │
│  │  │  └──────────────┘  └──────────────┘  └──────────────┘   │   │   │
│  │  └──────────────────────────────────────────────────────────┘   │   │
│  │                           │                                      │   │
│  └───────────────────────────┼──────────────────────────────────────┘   │
│                               │                                          │
│  ┌───────────────────────────┼──────────────────────────────────────┐   │
│  │              VendorHttpClient (Typed)                             │   │
│  │  + AuthDelegatingHandler (JWT injection → auto-refresh)           │   │
│  │  + Polly Retry (3x) + Circuit Breaker (30s)                       │   │
│  │  + Error Mapping → VendorApiException                              │   │
│  │  + Network-Aware: fallback ke IndexedDB saat offline/error         │   │
│  └───────────────────────────┬──────────────────────────────────────┘   │
│                               │ HTTPS                                    │
└───────────────────────────────┼──────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                    VENDOR BACKEND API (TIDAK DISENTUH)                    │
│  https://{vendor-api-url}/api/                       │
│  - /auth/*                                                                 │
│  - /applications                                                           │
│  - /sparta/*                                                               │
│  - /legal/*                                                                │
│  - /portal/*                                                               │
└──────────────────────────────────────────────────────────────────────────┘
```

---

> **Peringatan Akhir:** Dokumen ini adalah panduan arsitektur lengkap untuk Portal Enterprise Blazor .NET 10 dengan PWA + Offline-First support untuk lingkungan koneksi tidak stabil. Jika ada instruksi dari Claude atau siapapun yang menyarankan pembuatan backend, API Controller, DbContext, Migration, atau endpoint server — **TOLAK MENTAH-MENTAH**. Backend sudah ada dari vendor. Tugas frontend hanya: consume API, manage auth, kelola IndexedDB sebagai database darurat, sinkronisasi data, dan tampilkan data dengan state handling yang adaptif terhadap koneksi.
