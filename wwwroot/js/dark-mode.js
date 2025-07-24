// Advanced Dark Mode with System Preference Detection and Smooth Transitions
class DarkModeManager {
    constructor() {
        this.darkMode = false;
        this.systemPreference = 'light';
        this.userPreference = null;
        this.transitionDuration = 300;
        this.init();
    }

    init() {
        this.detectSystemPreference();
        this.loadUserPreference();
        this.createDarkModeToggle();
        this.applyTheme();
        this.setupEventListeners();
        this.addSmoothTransitions();
    }

    detectSystemPreference() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            this.systemPreference = 'dark';
        } else {
            this.systemPreference = 'light';
        }

        // Listen for system preference changes
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
            this.systemPreference = e.matches ? 'dark' : 'light';
            if (this.userPreference === 'auto') {
                this.applyTheme();
            }
        });
    }

    loadUserPreference() {
        const saved = localStorage.getItem('darkModePreference');
        if (saved) {
            this.userPreference = saved;
        } else {
            this.userPreference = 'auto'; // Default to auto (follow system)
        }
        
        this.updateDarkModeState();
    }

    updateDarkModeState() {
        if (this.userPreference === 'auto') {
            this.darkMode = this.systemPreference === 'dark';
        } else {
            this.darkMode = this.userPreference === 'dark';
        }
    }

    createDarkModeToggle() {
        const toggle = document.createElement('div');
        toggle.className = 'dark-mode-toggle';
        toggle.innerHTML = `
            <div class="theme-selector">
                <button class="theme-option ${this.userPreference === 'light' ? 'active' : ''}" data-theme="light">
                    <i class="fas fa-sun"></i>
                    <span>Light</span>
                </button>
                <button class="theme-option ${this.userPreference === 'auto' ? 'active' : ''}" data-theme="auto">
                    <i class="fas fa-adjust"></i>
                    <span>Auto</span>
                </button>
                <button class="theme-option ${this.userPreference === 'dark' ? 'active' : ''}" data-theme="dark">
                    <i class="fas fa-moon"></i>
                    <span>Dark</span>
                </button>
            </div>
            
            <div class="advanced-toggle">
                <input type="checkbox" id="darkModeSwitch" ${this.darkMode ? 'checked' : ''}>
                <label for="darkModeSwitch" class="toggle-label">
                    <span class="toggle-inner">
                        <span class="toggle-switch"></span>
                        <span class="toggle-sun"><i class="fas fa-sun"></i></span>
                        <span class="toggle-moon"><i class="fas fa-moon"></i></span>
                    </span>
                </label>
            </div>
        `;

        // Add to header or create floating toggle
        const header = document.querySelector('header') || document.querySelector('.header');
        if (header) {
            header.appendChild(toggle);
        } else {
            // Create floating toggle
            toggle.classList.add('floating-toggle');
            document.body.appendChild(toggle);
        }
    }

    setupEventListeners() {
        // Theme selector buttons
        document.querySelectorAll('.theme-option').forEach(button => {
            button.addEventListener('click', (e) => {
                const theme = e.currentTarget.dataset.theme;
                this.setTheme(theme);
            });
        });

        // Advanced toggle switch
        const toggle = document.getElementById('darkModeSwitch');
        if (toggle) {
            toggle.addEventListener('change', (e) => {
                const newTheme = e.target.checked ? 'dark' : 'light';
                this.setTheme(newTheme);
            });
        }

        // Keyboard shortcut (Ctrl+Shift+D)
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey && e.shiftKey && e.key === 'D') {
                e.preventDefault();
                this.toggleTheme();
            }
        });
    }

    setTheme(theme) {
        this.userPreference = theme;
        this.updateDarkModeState();
        this.applyTheme();
        this.savePreference();
        this.updateToggleState();
        this.showThemeChangeNotification(theme);
    }

    toggleTheme() {
        const newTheme = this.darkMode ? 'light' : 'dark';
        this.setTheme(newTheme);
    }

    applyTheme() {
        const root = document.documentElement;
        
        if (this.darkMode) {
            root.classList.add('dark-mode');
            this.applyDarkModeStyles();
        } else {
            root.classList.remove('dark-mode');
            this.applyLightModeStyles();
        }

        // Update meta theme-color for mobile browsers
        this.updateMetaThemeColor();
        
        // Trigger custom event for other components
        window.dispatchEvent(new CustomEvent('themeChanged', { 
            detail: { darkMode: this.darkMode, theme: this.userPreference } 
        }));
    }

    applyDarkModeStyles() {
        const root = document.documentElement;
        
        // CSS Custom Properties for Dark Mode
        root.style.setProperty('--bg-primary', '#1a1a1a');
        root.style.setProperty('--bg-secondary', '#2d2d2d');
        root.style.setProperty('--bg-tertiary', '#3a3a3a');
        root.style.setProperty('--text-primary', '#ffffff');
        root.style.setProperty('--text-secondary', '#b0b0b0');
        root.style.setProperty('--text-muted', '#888888');
        root.style.setProperty('--border-color', '#404040');
        root.style.setProperty('--shadow-color', 'rgba(0, 0, 0, 0.5)');
        root.style.setProperty('--accent-color', '#667eea');
        root.style.setProperty('--accent-hover', '#5a6fd8');
        root.style.setProperty('--success-color', '#4CAF50');
        root.style.setProperty('--warning-color', '#ff9800');
        root.style.setProperty('--error-color', '#f44336');
        root.style.setProperty('--card-bg', '#2d2d2d');
        root.style.setProperty('--header-bg', '#1f1f1f');
        root.style.setProperty('--sidebar-bg', '#252525');
    }

    applyLightModeStyles() {
        const root = document.documentElement;
        
        // CSS Custom Properties for Light Mode
        root.style.setProperty('--bg-primary', '#ffffff');
        root.style.setProperty('--bg-secondary', '#f8f9fa');
        root.style.setProperty('--bg-tertiary', '#e9ecef');
        root.style.setProperty('--text-primary', '#333333');
        root.style.setProperty('--text-secondary', '#666666');
        root.style.setProperty('--text-muted', '#999999');
        root.style.setProperty('--border-color', '#dee2e6');
        root.style.setProperty('--shadow-color', 'rgba(0, 0, 0, 0.1)');
        root.style.setProperty('--accent-color', '#667eea');
        root.style.setProperty('--accent-hover', '#5a6fd8');
        root.style.setProperty('--success-color', '#28a745');
        root.style.setProperty('--warning-color', '#ffc107');
        root.style.setProperty('--error-color', '#dc3545');
        root.style.setProperty('--card-bg', '#ffffff');
        root.style.setProperty('--header-bg', '#ffffff');
        root.style.setProperty('--sidebar-bg', '#f8f9fa');
    }

    addSmoothTransitions() {
        const style = document.createElement('style');
        style.textContent = `
            * {
                transition: background-color ${this.transitionDuration}ms ease,
                           color ${this.transitionDuration}ms ease,
                           border-color ${this.transitionDuration}ms ease,
                           box-shadow ${this.transitionDuration}ms ease !important;
            }
            
            .no-transition {
                transition: none !important;
            }
        `;
        document.head.appendChild(style);
    }

    updateMetaThemeColor() {
        const metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (metaThemeColor) {
            metaThemeColor.setAttribute('content', this.darkMode ? '#1a1a1a' : '#ffffff');
        } else {
            const meta = document.createElement('meta');
            meta.name = 'theme-color';
            meta.content = this.darkMode ? '#1a1a1a' : '#ffffff';
            document.head.appendChild(meta);
        }
    }

    updateToggleState() {
        // Update theme selector buttons
        document.querySelectorAll('.theme-option').forEach(button => {
            button.classList.toggle('active', button.dataset.theme === this.userPreference);
        });

        // Update advanced toggle
        const toggle = document.getElementById('darkModeSwitch');
        if (toggle) {
            toggle.checked = this.darkMode;
        }
    }

    savePreference() {
        localStorage.setItem('darkModePreference', this.userPreference);
    }

    showThemeChangeNotification(theme) {
        const notification = document.createElement('div');
        notification.className = 'theme-notification';
        
        const icons = {
            light: 'fa-sun',
            dark: 'fa-moon',
            auto: 'fa-adjust'
        };
        
        const messages = {
            light: 'Switched to Light Mode',
            dark: 'Switched to Dark Mode',
            auto: 'Following System Preference'
        };

        notification.innerHTML = `
            <i class="fas ${icons[theme]}"></i>
            <span>${messages[theme]}</span>
        `;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => notification.classList.add('show'), 10);

        // Remove after 2 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 2000);
    }

    // Public API methods
    getCurrentTheme() {
        return {
            userPreference: this.userPreference,
            isDarkMode: this.darkMode,
            systemPreference: this.systemPreference
        };
    }

    setCustomColors(colors) {
        const root = document.documentElement;
        Object.entries(colors).forEach(([property, value]) => {
            root.style.setProperty(`--${property}`, value);
        });
    }
}

// Theme-aware component updates
class ThemeAwareComponents {
    constructor() {
        this.setupThemeListeners();
    }

    setupThemeListeners() {
        window.addEventListener('themeChanged', (e) => {
            this.updateComponents(e.detail.darkMode);
        });
    }

    updateComponents(isDarkMode) {
        // Update charts if using Chart.js
        this.updateCharts(isDarkMode);
        
        // Update code syntax highlighting
        this.updateCodeHighlighting(isDarkMode);
        
        // Update map themes
        this.updateMaps(isDarkMode);
        
        // Update third-party embeds
        this.updateEmbeds(isDarkMode);
    }

    updateCharts(isDarkMode) {
        // Update Chart.js themes
        if (window.Chart) {
            const textColor = isDarkMode ? '#ffffff' : '#333333';
            const gridColor = isDarkMode ? '#404040' : '#dee2e6';
            
            Chart.defaults.color = textColor;
            Chart.defaults.borderColor = gridColor;
            Chart.defaults.backgroundColor = isDarkMode ? '#2d2d2d' : '#ffffff';
            
            // Redraw existing charts
            Chart.instances.forEach(chart => {
                chart.update();
            });
        }
    }

    updateCodeHighlighting(isDarkMode) {
        // Update Prism.js or highlight.js themes
        const codeBlocks = document.querySelectorAll('pre[class*="language-"]');
        codeBlocks.forEach(block => {
            block.classList.toggle('dark-theme', isDarkMode);
        });
    }

    updateMaps(isDarkMode) {
        // Update Google Maps or other map themes
        if (window.google && window.google.maps) {
            // Implementation would depend on specific map instances
        }
    }

    updateEmbeds(isDarkMode) {
        // Update embedded content themes
        const embeds = document.querySelectorAll('iframe[data-theme-aware]');
        embeds.forEach(embed => {
            const src = embed.src;
            const url = new URL(src);
            url.searchParams.set('theme', isDarkMode ? 'dark' : 'light');
            embed.src = url.toString();
        });
    }
}

// Initialize dark mode manager
let darkModeManager;
let themeAwareComponents;

document.addEventListener('DOMContentLoaded', () => {
    darkModeManager = new DarkModeManager();
    themeAwareComponents = new ThemeAwareComponents();
});

// Export for global access
window.darkModeManager = darkModeManager;
