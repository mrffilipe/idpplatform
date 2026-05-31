(function () {
    var STORAGE_KEY = 'kyvo-account-theme';

    function systemTheme() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    function apply(theme) {
        if (theme !== 'light' && theme !== 'dark') {
            return;
        }

        document.documentElement.setAttribute('data-theme', theme);
        document.documentElement.style.colorScheme = theme;

        try {
            localStorage.setItem(STORAGE_KEY, theme);
        } catch (e) {
            /* private browsing */
        }

        var toggle = document.getElementById('account-theme-toggle');
        if (toggle) {
            var next = theme === 'dark' ? 'light' : 'dark';
            toggle.setAttribute('aria-label', next === 'dark' ? 'Switch to dark mode' : 'Switch to light mode');
            toggle.setAttribute('title', next === 'dark' ? 'Dark mode' : 'Light mode');
        }
    }

    function toggle() {
        var current = document.documentElement.getAttribute('data-theme') || systemTheme();
        apply(current === 'dark' ? 'light' : 'dark');
    }

    window.kyvoAccountTheme = { apply: apply, toggle: toggle };

    document.addEventListener('DOMContentLoaded', function () {
        var stored;
        try {
            stored = localStorage.getItem(STORAGE_KEY);
        } catch (e) {
            stored = null;
        }

        if (stored === 'light' || stored === 'dark') {
            apply(stored);
        } else {
            apply(document.documentElement.getAttribute('data-theme') || systemTheme());
        }

        var button = document.getElementById('account-theme-toggle');
        if (button) {
            button.addEventListener('click', toggle);
        }
    });
})();
