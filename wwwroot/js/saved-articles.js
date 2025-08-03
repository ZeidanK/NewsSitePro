/**
 * SavedArticles.js
 * Handles the saved articles page functionality using the NewsController API
 * Provides search, filtering, and pagination for saved articles
 */

class SavedArticlesManager {
    constructor() {
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.currentCategory = 'all';
        this.currentSearch = '';
        this.isLoading = false;
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadSavedArticles();
    }

    bindEvents() {
        // Search functionality
        const searchBtn = document.getElementById('searchBtn');
        const searchInput = document.getElementById('searchInput');
        const clearSearchBtn = document.getElementById('clearSearchBtn');

        if (searchBtn) {
            searchBtn.addEventListener('click', () => this.handleSearch());
        }

        if (searchInput) {
            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.handleSearch();
                }
            });
        }

        if (clearSearchBtn) {
            clearSearchBtn.addEventListener('click', () => this.clearSearch());
        }

        // Category filter
        const categoryFilter = document.getElementById('categoryFilter');
        if (categoryFilter) {
            categoryFilter.addEventListener('change', (e) => {
                this.currentCategory = e.target.value;
                this.currentPage = 1;
                this.loadSavedArticles();
            });
        }

        // Sort filter
        const sortFilter = document.getElementById('sortFilter');
        if (sortFilter) {
            sortFilter.addEventListener('change', () => {
                this.currentPage = 1;
                this.loadSavedArticles();
            });
        }

        // Display type toggles
        document.querySelectorAll('.display-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const displayType = e.target.dataset.display;
                this.changeDisplayType(displayType);
            });
        });

        // Pagination
        this.bindPaginationEvents();
    }

    async loadSavedArticles() {
        if (this.isLoading) return;
        
        this.isLoading = true;
        this.showLoadingState();

        try {
            const params = new URLSearchParams({
                page: this.currentPage,
                limit: this.itemsPerPage,
                category: this.currentCategory === 'all' ? '' : this.currentCategory,
                search: this.currentSearch
            });

            const url = `/api/News/posts/saved?${params}`;
            console.log('[SavedArticles] Loading saved articles from:', url);

            const response = await fetch(url, {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Accept': 'text/html'
                }
            });

            if (response.ok) {
                const html = await response.text();
                this.renderArticles(html);
                this.updateResultsCount();
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            console.error('[SavedArticles] Error loading saved articles:', error);
            this.showErrorState('Failed to load saved articles. Please try again.');
        } finally {
            this.isLoading = false;
            this.hideLoadingState();
        }
    }

    renderArticles(html) {
        const articlesContainer = document.getElementById('articlesContainer');
        if (articlesContainer) {
            articlesContainer.innerHTML = html;
            this.initializePostInteractions();
        }
    }

    initializePostInteractions() {
        // Post interactions will be handled by the existing post-interactions.js
        // Just trigger any necessary initialization
        if (window.PostCardInteractions) {
            window.PostCardInteractions.initialize?.();
        }
        console.log('[SavedArticles] Post interactions initialized');
    }

    handleSearch() {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            this.currentSearch = searchInput.value.trim();
            this.currentPage = 1;
            this.loadSavedArticles();
            this.updateSearchUI();
        }
    }

    clearSearch() {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.value = '';
            this.currentSearch = '';
            this.currentPage = 1;
            this.loadSavedArticles();
            this.updateSearchUI();
        }
    }

    updateSearchUI() {
        const clearBtn = document.getElementById('clearSearchBtn');
        if (clearBtn) {
            clearBtn.style.display = this.currentSearch ? 'block' : 'none';
        }
    }

    changeDisplayType(displayType) {
        const articlesContainer = document.getElementById('articlesContainer');
        if (articlesContainer) {
            // Remove existing display classes
            articlesContainer.className = articlesContainer.className
                .replace(/\s*(grid|list|compact)-view/g, '');
            
            // Add new display class
            articlesContainer.classList.add(`${displayType}-view`);
        }

        // Update active button
        document.querySelectorAll('.display-btn').forEach(btn => {
            btn.classList.remove('active');
        });
        document.querySelector(`[data-display="${displayType}"]`)?.classList.add('active');
    }

    bindPaginationEvents() {
        // Handle pagination clicks
        document.addEventListener('click', (e) => {
            if (e.target.matches('.pagination-btn[data-page]')) {
                e.preventDefault();
                const page = parseInt(e.target.dataset.page);
                if (page !== this.currentPage && page > 0) {
                    this.currentPage = page;
                    this.loadSavedArticles();
                }
            }

            // Handle prev/next buttons
            if (e.target.matches('.pagination-prev')) {
                e.preventDefault();
                if (this.currentPage > 1) {
                    this.currentPage--;
                    this.loadSavedArticles();
                }
            }

            if (e.target.matches('.pagination-next')) {
                e.preventDefault();
                this.currentPage++;
                this.loadSavedArticles();
            }
        });
    }

    updateResultsCount() {
        // Count the actual articles displayed
        const articleCards = document.querySelectorAll('.post-card');
        const countBadge = document.querySelector('.count-badge');
        const resultsText = document.querySelector('.page-header p');
        
        if (countBadge) {
            countBadge.textContent = articleCards.length;
        }
        
        if (resultsText && articleCards.length === 0) {
            resultsText.textContent = 'No saved articles found';
        } else if (resultsText) {
            resultsText.textContent = 'Your personal collection of bookmarked articles';
        }
    }

    showLoadingState() {
        const articlesContainer = document.getElementById('articlesContainer');
        if (articlesContainer) {
            articlesContainer.innerHTML = `
                <div class="loading-state">
                    <div class="loading-spinner">
                        <i class="fas fa-spinner fa-spin"></i>
                    </div>
                    <p>Loading your saved articles...</p>
                </div>
            `;
        }
    }

    hideLoadingState() {
        // Loading state will be replaced by actual content
    }

    showErrorState(message) {
        const articlesContainer = document.getElementById('articlesContainer');
        if (articlesContainer) {
            articlesContainer.innerHTML = `
                <div class="error-state">
                    <div class="error-icon">⚠️</div>
                    <h3>Oops! Something went wrong</h3>
                    <p>${message}</p>
                    <button class="btn btn-primary retry-btn" onclick="savedArticlesManager.loadSavedArticles()">
                        <i class="fas fa-redo"></i> Try Again
                    </button>
                </div>
            `;
        }
    }

    // Public method to refresh articles (can be called from other scripts)
    refresh() {
        this.loadSavedArticles();
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.savedArticlesManager = new SavedArticlesManager();
});
