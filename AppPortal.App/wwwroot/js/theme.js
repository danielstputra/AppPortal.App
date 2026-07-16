window.themeInterop = {
    // Dipanggil saat toggle dark/light
    setDark: function (isDark) {
        // Simpan ke cookie (dibaca server saat render)
        const val = isDark ? 'dark' : 'light';
        document.cookie = `app-theme=${val}; path=/; max-age=${60 * 60 * 24 * 365}; SameSite=Lax`;
        // Reload agar server re-render dengan DevExpress theme yang benar
        window.location.reload();
    },

    // Dipanggil saat halaman load — baca cookie untuk sinkron state Blazor
    isDark: function () {
        return document.cookie.split(';').some(c => c.trim() === 'app-theme=dark');
    },

    // Simpan pilihan ukuran komponen ke localStorage
    setSize: function (size) {
        localStorage.setItem('app-size', size);
    },

    // Baca pilihan ukuran komponen dari localStorage
    getSize: function () {
        return localStorage.getItem('app-size');
    }
};
