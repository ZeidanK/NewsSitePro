// Search functionality
class SearchManager {
    constructor() {
        this.searchInput = document.getElementById('mainSearchInput');
        this.searchSuggestions = document.getElementById('searchSuggestions');
        this.filterToggle = document.getElementById('filterToggle');
        this.advancedFilters = document.getElementById('advancedFilters');
        this.searchForm = document.querySelector('.advanced-search-form');
        this.tabButtons = document.querySelectorAll('.tab-btn');
        this.resultsSections = document.querySelectorAll('.results-section');
        
        this.searchTimeout = null;
        this.currentSearchType = 'posts';
        
        this.init();
    }
    
    init() {
        this.setupEventListeners();
        this.setupTabs();
        this.setupSearchSuggestions();
    }
    
    setupEventListeners() {
        // Filter toggle
        if (this.filterToggle) {
            this.filterToggle.addEventListener('click', () => {
                this.toggleAdvancedFilters();
            });
        }
        
        // Search input
        if (this.searchInput) {
            this.searchInput.addEventListener('input', (e) => {
                this.handleSearchInput(e.target.value);
            });
            
            this.searchInput.addEventListener('focus', () => {
                if (this.searchInput.value.length >= 2) {
                    this.showSuggestions();
                }
            });
            
            this.searchInput.addEventListener('blur', () => {
                // Delay hiding suggestions to allow clicking
                setTimeout(() => this.hideSuggestions(), 200);
            });
        }
        
        // Form submission
        if (this.searchForm) {
            this.searchForm.addEventListener('submit', (e) => {
                this.handleFormSubmit(e);
            });
        }
        
        // Follow buttons
        document.querySelectorAll('.follow-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.handleFollowClick(e);
            });
        });
        
        // Close suggestions when clicking outside
        document.addEventListener('click', (e) => {
            if (!this.searchInput?.contains(e.target) && !this.searchSuggestions?.contains(e.target)) {
                this.hideSuggestions();
            }
        });
    }
    
    setupTabs() {
        this.tabButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const tabType = btn.dataset.tab;
                this.switchTab(tabType);
            });
        });
    }
    
    setupSearchSuggestions() {
        // Add keyboard navigation for suggestions
        if (this.searchInput) {
            this.searchInput.addEventListener('keydown', (e) => {
                this.handleKeyboardNavigation(e);
            });
        }
    }
    
    toggleAdvancedFilters() {
        if (this.advancedFilters) {
            const isVisible = this.advancedFilters.style.display !== 'none';
            this.advancedFilters.style.display = isVisible ? 'none' : 'block';
            
            // Update button state
            this.filterToggle.classList.toggle('active', !isVisible);
        }
    }
    
    async handleSearchInput(query) {
        clearTimeout(this.searchTimeout);
        
        if (query.length < 2) {
            this.hideSuggestions();
            return;
        }
        
        this.searchTimeout = setTimeout(async () => {
            await this.fetchSuggestions(query);
        }, 300);
    }
    
    async fetchSuggestions(query) {
        try {
            const typeSelect = document.querySelector('select[name="type"]');
            const searchType = typeSelect ? typeSelect.value : 'posts';
            
            const response = await fetch(`/Search?handler=Suggestions&q=${encodeURIComponent(query)}&type=${searchType}`);
            const data = await response.json();
            
            this.displaySuggestions(data.suggestions);
        } catch (error) {
            console.error('Error fetching suggestions:', error);
            this.hideSuggestions();
        }
    }
    
    displaySuggestions(suggestions) {
        if (!this.searchSuggestions || suggestions.length === 0) {
            this.hideSuggestions();
            return;
        }
        
        let html = '';
        suggestions.forEach(suggestion => {
            if (suggestion.type === 'post') {
                html += `
                    <div class="suggestion-item" data-type="post" data-id="${suggestion.id}">
                        <div class="suggestion-title">${this.highlightMatch(suggestion.title, this.searchInput.value)}</div>
                        <div class="suggestion-meta">
                            <span class="suggestion-category">${suggestion.category}</span> • 
                            <span class="suggestion-author">by ${suggestion.author}</span>
                        </div>
                    </div>
                `;
            } else if (suggestion.type === 'user') {
                html += `
                    <div class="suggestion-item" data-type="user" data-id="${suggestion.id}">
                        <div class="suggestion-title">${this.highlightMatch(suggestion.name, this.searchInput.value)}</div>
                        <div class="suggestion-meta">${suggestion.email}</div>
                    </div>
                `;
            }
        });
        
        this.searchSuggestions.innerHTML = html;
        this.showSuggestions();
        
        // Add click handlers to suggestions
        this.searchSuggestions.querySelectorAll('.suggestion-item').forEach(item => {
            item.addEventListener('click', () => {
                this.selectSuggestion(item);
            });
        });
    }
    
    highlightMatch(text, query) {
        if (!text || !query) return text;
        
        const regex = new RegExp(`(${query})`, 'gi');
        return text.replace(regex, '<strong>$1</strong>');
    }
    
    selectSuggestion(item) {
        const type = item.dataset.type;
        const id = item.dataset.id;
        
        if (type === 'post') {
            window.location.href = `/Post/${id}`;
        } else if (type === 'user') {
            window.location.href = `/UserProfile?userId=${id}`;
        }
    }
    
    showSuggestions() {
        if (this.searchSuggestions) {
            this.searchSuggestions.classList.add('active');
        }
    }
    
    hideSuggestions() {
        if (this.searchSuggestions) {
            this.searchSuggestions.classList.remove('active');
        }
    }
    
    handleKeyboardNavigation(e) {
        const suggestions = this.searchSuggestions?.querySelectorAll('.suggestion-item');
        if (!suggestions || suggestions.length === 0) return;
        
        const currentFocus = this.searchSuggestions.querySelector('.suggestion-item.focused');
        let newFocus = null;
        
        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                if (currentFocus) {
                    newFocus = currentFocus.nextElementSibling;
                    currentFocus.classList.remove('focused');
                } else {
                    newFocus = suggestions[0];
                }
                break;
                
            case 'ArrowUp':
                e.preventDefault();
                if (currentFocus) {
                    newFocus = currentFocus.previousElementSibling;
                    currentFocus.classList.remove('focused');
                } else {
                    newFocus = suggestions[suggestions.length - 1];
                }
                break;
                
            case 'Enter':
                e.preventDefault();
                if (currentFocus) {
                    this.selectSuggestion(currentFocus);
                } else {
                    this.searchForm?.submit();
                }
                break;
                
            case 'Escape':
                this.hideSuggestions();
                this.searchInput?.blur();
                break;
        }
        
        if (newFocus) {
            newFocus.classList.add('focused');
        }
    }
    
    switchTab(tabType) {
        // Update active tab
        this.tabButtons.forEach(btn => {
            btn.classList.toggle('active', btn.dataset.tab === tabType);
        });
        
        // Show/hide result sections
        this.resultsSections.forEach(section => {
            const sectionId = section.id;
            const shouldShow = (tabType === 'posts' && sectionId === 'postsResults') ||
                             (tabType === 'users' && sectionId === 'usersResults');
            section.style.display = shouldShow ? 'block' : 'none';
        });
        
        this.currentSearchType = tabType;
    }
    
    handleFormSubmit(e) {
        const query = this.searchInput?.value?.trim();
        if (!query) {
            e.preventDefault();
            this.searchInput?.focus();
            return;
        }
        
        // Form will submit naturally
        this.hideSuggestions();
    }
    
    async handleFollowClick(e) {
        const btn = e.target;
        const userId = btn.dataset.userId;
        
        if (!userId) return;
        
        try {
            btn.disabled = true;
            const originalText = btn.textContent;
            btn.textContent = 'Following...';
            
            // TODO: Implement actual follow API call
            const response = await fetch(`/api/User/Follow`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
                },
                body: JSON.stringify({ userId: parseInt(userId) })
            });
            
            if (response.ok) {
                btn.textContent = originalText === 'Follow' ? 'Following' : 'Follow';
                btn.classList.toggle('following', originalText === 'Follow');
            } else {
                btn.textContent = originalText;
                throw new Error('Follow request failed');
            }
        } catch (error) {
            console.error('Error following user:', error);
            btn.textContent = btn.textContent.replace('ing...', '');
            this.showToast('Failed to follow user. Please try again.', 'error');
        } finally {
            btn.disabled = false;
        }
    }
    
    showToast(message, type = 'info') {
        // Create toast notification
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        
        // Style the toast
        Object.assign(toast.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            padding: '12px 20px',
            backgroundColor: type === 'error' ? '#e74c3c' : '#2ecc71',
            color: 'white',
            borderRadius: '8px',
            zIndex: '10000',
            opacity: '0',
            transform: 'translateX(100%)',
            transition: 'all 0.3s ease'
        });
        
        document.body.appendChild(toast);
        
        // Animate in
        setTimeout(() => {
            toast.style.opacity = '1';
            toast.style.transform = 'translateX(0)';
        }, 100);
        
        // Remove after delay
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }
}

// Initialize search functionality when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new SearchManager();
});

// Add focused state styles
const style = document.createElement('style');
style.textContent = `
    .suggestion-item.focused {
        background-color: var(--hover-bg) !important;
    }
    
    .follow-btn.following {
        background-color: #657786 !important;
        border-color: #657786 !important;
    }
    
    .toast {
        font-family: var(--font-family);
        font-weight: 600;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }
`;
document.head.appendChild(style);
