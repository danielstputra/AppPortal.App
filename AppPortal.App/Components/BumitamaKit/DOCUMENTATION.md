# 🎨 BumitamaKit - Component Library

**BumitamaKit** adalah sebuah custom component library untuk Blazor yang menyediakan reusable UI components yang modern, responsive, dan mudah digunakan. Semua components diorganisir berdasarkan kategori dan mengikuti best practices Blazor development.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Folder Structure](#-folder-structure)
- [Component Categories](#-component-categories)
- [Getting Started](#-getting-started)
- [Naming Conventions](#-naming-conventions)
- [Component Structure](#-component-structure)
- [Available Components](#-available-components)
- [Usage Examples](#-usage-examples)
- [Contribution Guidelines](#-contribution-guidelines)

---

## Overview

BumitamaKit dirancang untuk:
- ✅ Mempercepat development dengan reusable components
- ✅ Menjaga consistency UI di seluruh aplikasi
- ✅ Memudahkan maintenance dan dokumentasi
- ✅ Mendukung multiple themes dan customization

**Technology Stack:**
- Blazor Server/WebAssembly
- .NET 10
- CSS Scoping
- **DevExpress Blazor Components** (required for UI rendering)

---

## 📁 Folder Structure

```
BumitamaKit/
├── Actions/                      ← Interactive components
│   ├── BumiButton/
│   ├── BumiButtonGroup/
│   ├── BumiDropDownButton/
│   └── BumiSplitButton/
├── Display/                      ← Visual/Display components
│   ├── BumiBadge/
│   ├── BumiCard/
│   └── BumiAlert/
├── Forms/                        ← Form input components
│   ├── BumiInput/
│   ├── BumiSelect/
│   └── BumiTextarea/
├── Layout/                       ← Layout components
│   ├── BumiGrid/
│   ├── BumiContainer/
│   └── BumiFlexbox/
├── Navigation/                   ← Navigation components
│   ├── BumiTabs/
│   ├── BumiDrawer/
│   └── BumiBreadcrumb/
├── Indicator/                    ← Indicator components
│   ├── BumiProgress/
│   ├── BumiSteps/
│   └── BumiBadge/
├── Shared/
│   ├── Models/                   ← Shared data classes
│   │   └── ComponentBase.cs
│   ├── Utilities/                ← Helper functions
│   │   ├── ThemeHelper.cs
│   │   ├── StyleHelper.cs
│   │   └── ValidationHelper.cs
│   └── Themes/                   ← CSS variables & themes
│       ├── Variables.css
│       ├── Light.css
│       └── Dark.css
├── Examples/                     ← Component demos
│   ├── ButtonPage.razor
│   ├── BadgePage.razor
│   └── FormPage.razor
├── README.md                     ← This file
└── CHANGELOG.md                  ← Version history
```

---

## 🏷️ Component Categories

### **Actions/** 
Interactive components yang memicu action atau event.

| Component | Purpose |
|-----------|---------|
| `BumiButton` | Basic button dengan multiple variants |
| `BumiButtonGroup` | Grouped buttons |
| `BumiDropDownButton` | Button dengan dropdown menu |
| `BumiSplitButton` | Button dengan split action |

**Usage:**
```razor
<BumiButton Text="Click Me" Click="@HandleClick" />
<BumiButtonGroup>
	<BumiButton Text="Save" />
	<BumiButton Text="Cancel" />
</BumiButtonGroup>
```

---

### **Display/**
Components untuk menampilkan informasi/konten.

| Component | Purpose |
|-----------|---------|
| `BumiBadge` | Badge untuk status/labels |
| `BumiCard` | Card container |
| `BumiAlert` | Alert/notification message |

**Usage:**
```razor
<BumiBadge Text="Active" Color="green" />
<BumiCard>
	<BumiCard.Header>Title</BumiCard.Header>
	<BumiCard.Body>Content here</BumiCard.Body>
</BumiCard>
```

---

### **Forms/**
Components untuk form input dan validation.

| Component | Purpose |
|-----------|---------|
| `BumiInput` | Text input field |
| `BumiSelect` | Dropdown select |
| `BumiTextarea` | Multi-line text input |

**Usage:**
```razor
<BumiInput @bind-Value="@Name" Placeholder="Enter name" />
<BumiSelect @bind-Value="@SelectedOption" Options="@Options" />
```

---

### **Layout/**
Components untuk mengatur layout halaman.

| Component | Purpose |
|-----------|---------|
| `BumiGrid` | Grid layout system |
| `BumiContainer` | Container wrapper |
| `BumiFlexbox` | Flexbox utilities |

**Usage:**
```razor
<BumiContainer>
	<BumiGrid Columns="3" Gap="1rem">
		<BumiGrid.Item>Content 1</BumiGrid.Item>
		<BumiGrid.Item>Content 2</BumiGrid.Item>
	</BumiGrid>
</BumiContainer>
```

---

### **Navigation/**
Components untuk navigasi dan menu.

| Component | Purpose |
|-----------|---------|
| `BumiTabs` | Tab navigation |
| `BumiDrawer` | Slide-out drawer menu |
| `BumiBreadcrumb` | Breadcrumb navigation |

**Usage:**
```razor
<BumiTabs>
	<BumiTabs.Tab Name="Tab 1">Content 1</BumiTabs.Tab>
	<BumiTabs.Tab Name="Tab 2">Content 2</BumiTabs.Tab>
</BumiTabs>
```

---

### **Indicator/**
Components untuk menampilkan progress atau status.

| Component | Purpose |
|-----------|---------|
| `BumiProgress` | Progress bar |
| `BumiSteps` | Step indicator |
| `BumiBadge` | Status badge |

**Usage:**
```razor
<BumiProgress Value="75" />
<BumiSteps Current="2">
	<BumiSteps.Step Title="Step 1" />
	<BumiSteps.Step Title="Step 2" />
</BumiSteps>
```

---

## 🚀 Getting Started

### 1. **Import BumitamaKit Components**

Di file `_Imports.razor`:
```razor
@using AppPortal.App.Components.BumitamaKit.Actions.BumiButton
@using AppPortal.App.Components.BumitamaKit.Actions.BumiButtonGroup
@using AppPortal.App.Components.BumitamaKit.Display
@using AppPortal.App.Components.BumitamaKit.Forms
@using AppPortal.App.Components.BumitamaKit.Layout
@using AppPortal.App.Components.BumitamaKit.Navigation
@using AppPortal.App.Components.BumitamaKit.Indicator
```

### 2. **Use Component**

```razor
@page "/my-page"
@using AppPortal.App.Components.BumitamaKit.Actions.BumiButton

<BumiButton Text="Click Me" Color="ButtonColor.Primary" Click="@HandleClick" />

@code {
	private async Task HandleClick()
	{
		Console.WriteLine("Button clicked!");
	}
}
```

### 3. **Apply Themes (Optional)**

Di `App.razor` atau `index.html`:
```html
<link rel="stylesheet" href="_framework/bulma/shared/themes/light.css" />
```

---

## 🎨 Naming Conventions

### **File Naming**

Semua components mengikuti pattern: `Bumi[ComponentName].razor`

```
✅ GOOD:
- BumiButton.razor
- BumiButtonGroup.razor
- BumiDropDownButton.razor

❌ BAD:
- Button.razor
- CustomButton.razor
- MyButton.razor
```

### **Component File Structure**

Setiap component memiliki struktur:

```
BumiButton/
├── BumiButton.razor          ← Markup & template
├── BumiButton.razor.cs       ← Code-behind logic
└── BumiButton.razor.css      ← Scoped styles
```

### **CSS Class Naming**

Menggunakan BEM (Block Element Modifier):

```css
.bumi-button { }              /* Block */
.bumi-button__icon { }        /* Element */
.bumi-button--primary { }     /* Modifier */
.bumi-button--disabled { }
```

### **Parameter Naming**

```csharp
[Parameter] public string? Text { get; set; }              ✅ Descriptive
[Parameter] public string? ButtonText { get; set; }       ❌ Redundant
[Parameter] public EventCallback Click { get; set; }      ✅ Clear & concise
[Parameter] public ButtonColor Color { get; set; }        ✅ Enum-based
[Parameter] public bool Loading { get; set; }            ✅ Clear purpose
```

---

## 📦 Component Structure Template

### **Razor File (BumiXxx.razor)**

```razor
@namespace AppPortal.App.Components.BumitamaKit.Actions.BumiButton
@using DevExpress.Blazor

<div class="bumi-button-theme bumi-theme-@Theme.ToString().ToLower()" 
	 style="@GetWrapperStyle()"
	 @onmouseenter="@OnMouseEnter"
	 @onmouseleave="@OnMouseLeave">
	<DxButton IconCssClass="@IconCssClass"
			  Enabled="@(!Disabled && !Loading)"
			  RenderStyle="@DxRenderStyle"
			  RenderStyleMode="@DxRenderStyleMode"
			  SizeMode="@DxSizeMode"
			  CssClass="@GetDynamicCssClass()"
			  Click="@HandleClick"
			  style="@GetButtonStyle()">
		@Text
		@if (Loading)
		{
			<span class="bumi-spinner"></span>
		}
	</DxButton>
</div>
```

### **CSS File (BumiXxx.razor.css)**

```css
.bumi-button-theme {
	display: inline-block;
}

/* Light Theme */
.bumi-theme-light ::deep .dx-button {
	background-color: #ffffff;
	border-color: #d0d0d0;
	box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
	color: #333333;
	transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.bumi-theme-light ::deep .dx-button:hover {
	background-color: #f8f8f8;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
}

/* Dark Theme */
.bumi-theme-dark ::deep .dx-button {
	background-color: #3a3a3a;
	border-color: #505050;
	color: #e8e8e8;
	box-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
}

/* Loading Spinner */
.bumi-spinner {
	display: inline-block;
	width: 14px;
	height: 14px;
	border: 2px solid currentColor;
	border-radius: 50%;
	animation: bumi-spin 0.8s linear infinite;
}

@keyframes bumi-spin {
	to { transform: rotate(360deg); }
}
```

### **Code-Behind (BumiXxx.razor.cs)**

File `.cs` terpisah untuk logic dan parameters:

```csharp
using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Actions.BumiButton;

public partial class BumiButton
{
	[Parameter]
	public string? Text { get; set; }

	[Parameter]
	public string? IconCssClass { get; set; }

	[Parameter]
	public ButtonColor Color { get; set; } = ButtonColor.Primary;

	[Parameter]
	public ButtonVariant Variant { get; set; } = ButtonVariant.Filled;

	[Parameter]
	public ButtonSize Size { get; set; } = ButtonSize.Medium;

	[Parameter]
	public ButtonTheme Theme { get; set; } = ButtonTheme.Light;

	[Parameter]
	public bool Disabled { get; set; }

	[Parameter]
	public bool Loading { get; set; }

	[Parameter]
	public EventCallback Click { get; set; }

	protected async Task HandleClick()
	{
		if (Disabled || Loading) return;
		await Click.InvokeAsync();
	}
}
```

---

## 📚 Available Components

### **Status: Implemented ✅**

- [x] BumiButton
- [x] BumiButtonGroup
- [x] BumiDropDownButton
- [x] BumiSplitButton

### **Status: Planned 🔄**

- [ ] BumiBadge
- [ ] BumiCard
- [ ] BumiAlert
- [ ] BumiInput
- [ ] BumiSelect
- [ ] BumiTabs
- [ ] BumiDrawer
- [ ] BumiBreadcrumb
- [ ] BumiProgress
- [ ] BumiSteps

---

## 💡 Usage Examples

### **Example 1: Different Colors**

```razor
@using AppPortal.App.Components.BumitamaKit.Actions.BumiButton

<BumiButton Text="Primary" Color="ButtonColor.Primary" Click="@HandleClick" />
<BumiButton Text="Success" Color="ButtonColor.Success" Click="@HandleClick" />
<BumiButton Text="Danger" Color="ButtonColor.Danger" Click="@HandleClick" />
<BumiButton Text="Brand" Color="ButtonColor.Brand" Click="@HandleClick" />
<BumiButton Text="Custom" Color="ButtonColor.Custom" CustomColor="#FF6B6B" Click="@HandleClick" />

@code {
	private async Task HandleClick()
	{
		await Task.Delay(500);
	}
}
```

### **Example 2: Variants and Sizes**

```razor
<!-- Size Variants -->
<BumiButton Text="Small" Size="ButtonSize.Small" Variant="ButtonVariant.Filled" />
<BumiButton Text="Medium" Size="ButtonSize.Medium" Variant="ButtonVariant.Filled" />
<BumiButton Text="Large" Size="ButtonSize.Large" Variant="ButtonVariant.Filled" />

<!-- Style Variants -->
<BumiButton Text="Filled" Variant="ButtonVariant.Filled" />
<BumiButton Text="Outline" Variant="ButtonVariant.Outline" />
<BumiButton Text="Text" Variant="ButtonVariant.Text" />
```

### **Example 3: Loading State**

```razor
<BumiButton 
	Text="@(isLoading ? "Loading..." : "Submit")" 
	Loading="@isLoading" 
	Disabled="@isLoading"
	Click="@HandleSubmitAsync" />

@code {
	private bool isLoading = false;

	private async Task HandleSubmitAsync()
	{
		isLoading = true;
		await Task.Delay(2000);
		isLoading = false;
	}
}
```

### **Example 4: Themes**

```razor
<BumiButton Text="Light Theme" Theme="ButtonTheme.Light" />
<BumiButton Text="Dark Theme" Theme="ButtonTheme.Dark" />
```

### **Example 5: Button Group**

```razor
<BumiButtonGroup>
	<BumiButton Text="Save" Color="ButtonColor.Primary" />
	<BumiButton Text="Reset" Color="ButtonColor.Secondary" />
	<BumiButton Text="Cancel" Color="ButtonColor.Danger" />
</BumiButtonGroup>
```

---

## 👥 Contribution Guidelines

### **Adding New Component**

1. **Create folder** di kategori yang sesuai:
   ```
   Components/BumitamaKit/Display/BumiNewComponent/
   ```

2. **Create files**:
   - `BumiNewComponent.razor`
   - `BumiNewComponent.razor.css`
   - `BumiNewComponent.razor.cs` (jika perlu)

3. **Follow naming conventions** sesuai dokumentasi ini

4. **Add documentation** di `Examples/` folder

5. **Update README.md** dengan component baru

### **Code Standards**

- ✅ Use scoped CSS (`.razor.css`)
- ✅ Follow C# naming conventions
- ✅ Add XML comments untuk public members
- ✅ Support accessibility (ARIA labels, semantic HTML)
- ✅ Responsive design

---

## 📝 License & Credits

**BumitamaKit** - Custom Component Library  
Built with ❤️ for AppPortal Application

---

## 📞 Support

Untuk pertanyaan atau issue dengan BumitamaKit, silakan buat issue di repository atau hubungi tim development.

**Last Updated:** January 2026  
**Version:** 1.0.0
