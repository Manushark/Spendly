// ===================================
// DARK MODE TOGGLE SYSTEM
// ===================================

(function () {
    'use strict';

    const DarkMode = {
        // Get current theme from localStorage or system preference
        getTheme: function () {
            const stored = localStorage.getItem('theme');
            if (stored) return stored;

            // Check system preference
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                return 'dark';
            }
            return 'light';
        },

        // Set theme
        setTheme: function (theme) {
            document.documentElement.setAttribute('data-theme', theme);
            localStorage.setItem('theme', theme);
            this.updateToggleIcon(theme);
        },

        // Toggle between themes
        toggle: function () {
            const current = this.getTheme();
            const newTheme = current === 'dark' ? 'light' : 'dark';
            this.setTheme(newTheme);
        },

        // Update toggle button icon
        updateToggleIcon: function (theme) {
            const toggleBtn = document.getElementById('theme-toggle');
            if (!toggleBtn) return;

            const icon = toggleBtn.querySelector('i');
            if (!icon) return;

            if (theme === 'dark') {
                icon.className = 'bi bi-sun-fill';
                toggleBtn.setAttribute('aria-label', 'Switch to light mode');
            } else {
                icon.className = 'bi bi-moon-fill';
                toggleBtn.setAttribute('aria-label', 'Switch to dark mode');
            }
        },

        // Initialize
        init: function () {
            // Set initial theme
            const theme = this.getTheme();
            this.setTheme(theme);

            // Add toggle button event listener
            document.addEventListener('DOMContentLoaded', () => {
                const toggleBtn = document.getElementById('theme-toggle');
                if (toggleBtn) {
                    toggleBtn.addEventListener('click', () => this.toggle());
                }

                // Listen for system theme changes
                if (window.matchMedia) {
                    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                        if (!localStorage.getItem('theme')) {
                            this.setTheme(e.matches ? 'dark' : 'light');
                        }
                    });
                }
            });
        }
    };

    // Initialize dark mode
    DarkMode.init();

    // Export to window for external access
    window.DarkMode = DarkMode;
})();
