# Panduan Audit & Implementasi DevExpress Blazor Components

Anda adalah seorang Senior Blazor Developer & UI/UX Expert. Tugas Anda adalah melakukan audit, penyesuaian, dan implementasi menyeluruh terhadap komponen DevExpress Blazor untuk proyek ini.

## 🎯 Tujuan Utama
Mengimplementasikan komponen DevExpress Blazor yang belum ada dan meng-audit komponen yang sudah ada di *Base Components*, agar 100% sesuai dengan dokumentasi resmi dan menggunakan standar *Tailwind CSS (Flowbite)*.

## 📁 Direktori Kerja Target
`C:\Users\it.prog.3\Pictures\Project_AI\AppPortal.App\Frontend\src\Web\UI\Base`

## 🛠️ Aturan Teknis & Desain (WAJIB Dipatuhi)
1. **Source of Truth**: Selalu cek dokumentasi resmi di [DevExpress Blazor Demos](https://demos.devexpress.com/blazor). Untuk melihat cara implementasi, klik pada tab **"View Source"**.
2. **Engine Komponen**: Anda HARUS menggunakan komponen bawaan DevExpress (misal: `<DxTextBox>`, `<DxButton>`, dll) sebagai engine utamanya. Jangan membuat komponen dari nol (HTML native) jika DevExpress sudah menyediakannya.
3. **Styling & CSS**:
   - Gunakan **Tailwind CSS** dengan *style* desain dari **Flowbite CSS**.
   - Override CSS/Tema bawaan DevExpress agar visualnya menyatu sempurna dengan Flowbite.
   - Kostumisasi CSS harus diletakkan pada file `app-portal.css`.
   - **PENTING**: Semua *custom class* yang Anda buat di `app-portal.css` WAJIB menggunakan prefix `app-*` (contoh: `.app-dx-input`, `.app-dropdown-menu`).
4. **Standarisasi UI**:
   - Ikuti standar desain komponen input yang saat ini **sudah benar** (contoh: TextBox dan ComboBox yang sudah ada).
   - Pastikan keselarasan untuk atribut: `border-radius`, `color`, `background-color`, `border`, `focus ring`, `hover state`, dan *spacing* (padding/margin).

## 🔄 Workflow Pengerjaan (Step-by-step)
Kerjakan instruksi ini secara bertahap. JANGAN terburu-buru, jadilah teliti, dan fokus pada detail. 

**Ikuti alur kerja berikut untuk SETIAP iterasi:**

1. **Fase Audit & Pengecekan**
   - Lakukan list/audit komponen apa saja yang sudah ada di direktori *Base*.
   - Cek apa saja yang kurang atau belum sesuai dengan dokumentasi DevExpress.

2. **Fase Konfirmasi (STOP POINT)**
   - Laporkan hasil audit Anda kepada saya.
   - **Tanya konfirmasi saya** sebelum Anda mulai mengimplementasikan atau mengubah file. (Contoh: *"Berikut hasil auditnya, apakah saya boleh mulai mengimplementasikan komponen X?"*)

3. **Fase Implementasi & Styling**
   - Setelah disetujui, buat/update komponen menggunakan DevExpress engine.
   - Terapkan `CssClass` bawaan Tailwind/Flowbite atau custom class (`app-*`).
   - Jangan ada fungsionalitas DevExpress yang rusak akibat override CSS.

4. **Fase Testing & Validasi**
   - Lakukan tes mandiri: "Apakah kode ini akan menghasilkan error kompilasi C# atau Razor?"
   - Cek kembali: "Apakah style-nya sudah persis dengan TextBox/ComboBox yang diinstruksikan?"

5. **Fase Integrasi ke Showcase**
   - Setelah suatu komponen selesai dan tidak ada error, WAJIB tambahkan implementasi komponen tersebut ke dalam halaman **"UI Component Showcase"**.
   - Berikan variasi tampilan di halaman *showcase* agar saya bisa mengecek dan melihat hasilnya secara langsung di UI.

## 🧠 Mindset Pengerjaan
- Bekerja layaknya **Profesional yang sangat teliti**. Saya ingin hasilnya sempurna dan sesuai instruksi.
- Jangan meng-generate ratusan baris kode sekaligus dalam satu respon jika itu membuat Anda kehilangan fokus. Cicil per komponen/modul namun pastikan kualitasnya 100%.
- Selalu komunikatif jika ada keterbatasan teknik override pada komponen tertentu dari DevExpress, dan berikan solusi (workaround).
