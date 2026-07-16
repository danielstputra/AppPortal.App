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
- DevExpress Integration (optional)

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
│   ├── ButtonExample.razor
│   ├── BadgeExample.razor
│   └── FormExample.razor
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
<BumiButton OnClick="@HandleClick" Text="Click Me" />
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
@using AppPortal.App.Components.BumitamaKit.Actions
@using AppPortal.App.Components.BumitamaKit.Display
@using AppPortal.App.Components.BumitamaKit.Forms
@using AppPortal.App.Components.BumitamaKit.Layout
@using AppPortal.App.Components.BumitamaKit.Navigation
@using AppPortal.App.Components.BumitamaKit.Indicator
```

### 2. **Use Component**

```razor
@page "/my-page"

<BumiButton OnClick="@HandleClick" Text="Click Me" Color="primary" />

@code {
	private void HandleClick()
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
[Parameter] public string Text { get; set; }           ✅
[Parameter] public string ButtonText { get; set; }      ❌ Redundant
[Parameter] public EventCallback OnClick { get; set; }  ✅
[Parameter] public EventCallback Click { get; set; }    ⚠️ Ambiguous
```

---

## 📦 Component Structure Template

### **Razor File (BumiXxx.razor)**

```razor
@namespace AppPortal.App.Components.BumitamaKit.Actions

<div class="bumi-button @GetCssClass()" @onclick="HandleClick">
	@if (!string.IsNullOrEmpty(Icon))
	{
		<i class="icon @Icon"></i>
	}
	<span>@Text</span>
</div>

@code {
	[Parameter] public string Text { get; set; } = "";
	[Parameter] public string? Icon { get; set; }
	[Parameter] public string Color { get; set; } = "default";
	[Parameter] public bool Disabled { get; set; }
	[Parameter] public EventCallback OnClick { get; set; }

	private string GetCssClass() => $"bumi-button--{Color} {(Disabled ? "bumi-button--disabled" : "")}";

	private async Task HandleClick()
	{
		if (!Disabled)
			await OnClick.InvokeAsync();
	}
}
```

### **CSS File (BumiXxx.razor.css)**

```css
.bumi-button {
	display: inline-flex;
	align-items: center;
	gap: 0.5rem;
	padding: 0.5rem 1rem;
	border: 1px solid var(--bumi-border-color);
	border-radius: var(--bumi-border-radius);
	cursor: pointer;
	transition: all 0.2s ease;
}

.bumi-button:hover:not(.bumi-button--disabled) {
	background-color: var(--bumi-primary-hover);
}

.bumi-button--primary {
	background-color: var(--bumi-primary);
	color: white;
}

.bumi-button--disabled {
	opacity: 0.5;
	cursor: not-allowed;
}
```

### **Code-Behind (BumiXxx.razor.cs) - Optional**

Jika logic kompleks, buat file `.cs` terpisah:

```csharp
namespace AppPortal.App.Components.BumitamaKit.Actions;

public partial class BumiButton
{
	// Complex logic here
	public void ValidateInputs() { }
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

### **Example 1: Simple Button**

```razor
<BumiButton Text="Submit" Color="primary" OnClick="@HandleSubmit" />

@code {
	private void HandleSubmit()
	{
		// Handle submission
	}
}
```

### **Example 2: Button Group**

```razor
<BumiButtonGroup>
	<BumiButton Text="Save" Color="primary" />
	<BumiButton Text="Reset" Color="secondary" />
	<BumiButton Text="Cancel" Color="danger" />
</BumiButtonGroup>
```

### **Example 3: Dropdown Button**

```razor
<BumiDropDownButton Text="Menu" Icon="chevron-down">
	<DxDropDownMenuItem Text="Item 1" OnClick="@(() => HandleMenu(1))" />
	<DxDropDownMenuItem Text="Item 2" OnClick="@(() => HandleMenu(2))" />
	<DxDropDownMenuItem Text="Item 3" OnClick="@(() => HandleMenu(3))" />
</BumiDropDownButton>
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
