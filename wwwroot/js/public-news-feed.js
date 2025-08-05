/**
 * PublicNewsFeed - Public news browsing interface
 * Focuses on: Browse news → Preview articles → Read full articles
 * No publishing or selection capabilities - read-only for users
 */
class PublicNewsFeed {
    constructor() {
        this.fetchedArticles = new Map(); // Store fetched articles
        this.currentPage = 1;
        this.pageSize = 20;
        this.isLoading = false;
        this.hasMoreArticles = true;
        this.currentFilters = {};
        this.setupEventListeners();
        console.log('PublicNewsFeed initialized');
        
        // Auto-load latest news when page loads
        setTimeout(() => {
            this.fetchNewsFromAPI(true); // true = reset to first page
        }, 500);
    }

    /**
     * Set up all event listeners
     */
    setupEventListeners() {
        // Refresh button
        const refreshBtn = document.getElementById('refreshBtn');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.fetchNewsFromAPI(true));
        }

        // Clear filters button
        const clearFiltersBtn = document.getElementById('clearFiltersBtn');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', () => this.clearFilters());
        }

        // Load more button
        const loadMoreBtn = document.getElementById('loadMoreBtn');
        if (loadMoreBtn) {
            loadMoreBtn.addEventListener('click', () => this.loadMoreArticles());
        }

        // Auto-fetch when filters change
        const categorySelect = document.getElementById('categorySelect');
        if (categorySelect) {
            categorySelect.addEventListener('change', () => {
                this.fetchNewsFromAPI(true); // Reset to first page when category changes
            });
        }

        // Infinite scroll
        window.addEventListener('scroll', () => {
            if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight - 1000) {
                if (!this.isLoading && this.hasMoreArticles) {
                    this.loadMoreArticles();
                }
            }
        });
    }

    /**
     * Fetch news articles from API
     */
    async fetchNewsFromAPI(resetPage = false) {
        if (this.isLoading) return;
        
        try {
            this.isLoading = true;
            
            if (resetPage) {
                this.currentPage = 1;
                this.hasMoreArticles = true;
            }

            const category = document.getElementById('categorySelect')?.value || '';

            // Store current filters for pagination
            this.currentFilters = { category };

            this.showLoading('Loading articles...');

            // Build request for top headlines with category filter
            let requestBody = {
                category: category || 'general',
                country: 'us', // Default to US
                pageSize: this.pageSize,
                type: 'top-headlines' // Always use top headlines
            };

            const response = await fetch('/api/News/browse-external', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(requestBody)
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();

            if (data.success && data.articles && data.articles.length > 0) {
                this.processFetchedArticles(data.articles, resetPage);
                
                // Show load more button if we got a full page
                this.hasMoreArticles = data.articles.length >= this.pageSize;
                this.updateLoadMoreButton();
            } else {
                if (resetPage) {
                    this.showWarning('No articles found for the selected criteria');
                    // Clear the container
                    const container = document.getElementById('newsArticlesContainer');
                    if (container) {
                        container.innerHTML = `
                            <div class="text-center p-5">
                                <i class="fas fa-search fa-3x text-muted mb-3"></i>
                                <h4 class="text-muted">No Articles Found</h4>
                                <p class="text-muted">Try selecting a different category or refresh the page.</p>
                            </div>
                        `;
                    }
                }
                this.hasMoreArticles = false;
                this.updateLoadMoreButton();
            }

        } catch (error) {
            console.error('Error fetching news:', error);
            this.showError(`Failed to fetch news: ${error.message}`);
            this.hasMoreArticles = false;
            this.updateLoadMoreButton();
        } finally {
            this.isLoading = false;
            this.hideLoading();
        }
    }

    /**
     * Load more articles (pagination)
     */
    async loadMoreArticles() {
        if (this.isLoading || !this.hasMoreArticles) return;
        
        this.currentPage++;
        await this.fetchNewsFromAPI(false); // false = don't reset, append to existing
    }

    /**
     * Update load more button visibility
     */
    updateLoadMoreButton() {
        const loadMoreSection = document.getElementById('loadMoreSection');
        const loadMoreBtn = document.getElementById('loadMoreBtn');
        
        if (loadMoreSection && loadMoreBtn) {
            if (this.hasMoreArticles && this.fetchedArticles.size > 0) {
                loadMoreSection.style.display = 'block';
                loadMoreBtn.disabled = this.isLoading;
                loadMoreBtn.innerHTML = this.isLoading 
                    ? '<i class="fas fa-spinner fa-spin"></i> Loading...'
                    : '<i class="fas fa-plus"></i> Load More Articles';
            } else {
                loadMoreSection.style.display = 'none';
            }
        }
    }

    /**
     * Process and display fetched articles
     */
    processFetchedArticles(articles, resetPage = false) {
        if (resetPage) {
            this.fetchedArticles.clear();
        }

        // Store articles with generated IDs
        articles.forEach((article, index) => {
            const id = `article_${Date.now()}_${index}`;
            this.fetchedArticles.set(id, {
                id: id,
                title: article.Title || article.title || 'Untitled',
                description: article.Description || article.description || '',
                content: article.Content || article.content || article.description || 'No content available',
                imageUrl: article.UrlToImage || article.urlToImage || '',
                sourceUrl: article.Url || article.url || '',
                sourceName: article.Source?.Name || article.source?.name || 'Unknown Source',
                publishedAt: article.PublishedAt || article.publishedAt || new Date().toISOString(),
                category: document.getElementById('categorySelect')?.value || 'general'
            });
        });

        this.renderArticles(resetPage);
    }

    /**
     * Render articles in the container
     */
    renderArticles(resetPage = false) {
        const container = document.getElementById('newsArticlesContainer');
        if (!container) return;

        if (this.fetchedArticles.size === 0) {
            container.innerHTML = `
                <div class="text-center p-5">
                    <i class="fas fa-newspaper fa-3x text-muted mb-3"></i>
                    <h4 class="text-muted">No Articles to Display</h4>
                    <p class="text-muted">Select a category or refresh to discover the latest articles.</p>
                </div>
            `;
            return;
        }

        if (resetPage) {
            // Replace all content
            container.innerHTML = Array.from(this.fetchedArticles.values()).map(article => 
                this.createArticleCard(article)
            ).join('');
        } else {
            // Append new articles (for pagination)
            const newArticles = Array.from(this.fetchedArticles.values()).slice(-20); // Get last 20 articles
            const newHtml = newArticles.map(article => this.createArticleCard(article)).join('');
            container.insertAdjacentHTML('beforeend', newHtml);
        }

        // Attach event listeners to new elements
        this.attachArticleEventListeners();
    }

    /**
     * Create HTML for individual article card (text-only, no images)
     */
    createArticleCard(article) {
        return `
            <div class="card mb-3 article-card" data-article-id="${article.id}">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <h5 class="card-title">${this.truncateText(article.title, 100)}</h5>
                    </div>
                    <p class="card-text">${this.truncateText(article.description || article.content, 200)}</p>
                    <div class="d-flex justify-content-between align-items-center">
                        <small class="text-muted">
                            <i class="fas fa-globe"></i> ${article.sourceName}
                            <span class="ms-2">
                                <i class="fas fa-clock"></i> ${this.formatDate(article.publishedAt)}
                            </span>
                        </small>
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary btn-sm preview-btn" data-article-id="${article.id}">
                                <i class="fas fa-eye"></i> Preview
                            </button>
                            <button class="btn btn-success btn-sm publish-btn" data-article-id="${article.id}">
                                <i class="fas fa-rocket"></i> Publish
                            </button>
                            <a href="${article.sourceUrl}" target="_blank" class="btn btn-outline-secondary btn-sm">
                                <i class="fas fa-external-link-alt"></i> Read Full
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Attach event listeners to article elements
     */
    attachArticleEventListeners() {
        // Preview button events
        document.querySelectorAll('.preview-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const articleId = e.target.closest('.preview-btn').dataset.articleId;
                this.previewArticle(articleId);
            });
        });

        // Publish button events
        document.querySelectorAll('.publish-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const articleId = e.target.closest('.publish-btn').dataset.articleId;
                this.publishArticle(articleId);
            });
        });
    }

    /**
     * Preview individual article
     */
    previewArticle(articleId) {
        const article = this.fetchedArticles.get(articleId);
        if (!article) return;

        const modalContent = document.getElementById('articlePreviewContent');
        if (!modalContent) return;

        modalContent.innerHTML = `
            <div class="article-preview">
                <h4>${article.title}</h4>
                <div class="mb-3">
                    <small class="text-muted">
                        <i class="fas fa-globe"></i> ${article.sourceName} | 
                        <i class="fas fa-clock"></i> ${this.formatDate(article.publishedAt)}
                    </small>
                </div>
                ${article.imageUrl ? `<img src="${article.imageUrl}" class="img-fluid mb-3" alt="Article Image">` : ''}
                <p>${article.content}</p>
                <div class="mt-3">
                    <span class="badge bg-secondary">${article.category}</span>
                </div>
            </div>
        `;

        // Set up modal read full article button
        const readFullBtn = document.getElementById('readFullArticleBtn');
        if (readFullBtn) {
            readFullBtn.href = article.sourceUrl;
        }

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('articlePreviewModal'));
        modal.show();
    }

    /**
     * Publish individual article
     */
    async publishArticle(articleId) {
        const article = this.fetchedArticles.get(articleId);
        if (!article) return;

        try {
            const publishBtn = document.querySelector(`[data-article-id="${articleId}"].publish-btn`);
            if (publishBtn) {
                publishBtn.disabled = true;
                publishBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Publishing...';
            }

            const response = await fetch('/api/News/publish', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    articles: [{
                        title: article.title,
                        content: article.content,
                        imageUrl: article.imageUrl,
                        sourceUrl: article.sourceUrl,
                        sourceName: article.sourceName,
                        category: article.category
                    }]
                })
            });

            const result = await response.json();
            
            if (response.status === 401 && result.requiresLogin) {
                // User not logged in - show login prompt
                this.showLoginPrompt();
                return;
            }

            if (!response.ok) {
                throw new Error(result.message || `HTTP ${response.status}: ${response.statusText}`);
            }
            
            if (result.success) {
                this.showSuccess(`Article "${this.truncateText(article.title, 50)}" published successfully!`);
                
                // Remove the article from display
                const articleCard = document.querySelector(`[data-article-id="${articleId}"]`);
                if (articleCard) {
                    articleCard.style.transition = 'opacity 0.3s ease';
                    articleCard.style.opacity = '0.5';
                    setTimeout(() => {
                        articleCard.remove();
                        this.fetchedArticles.delete(articleId);
                    }, 300);
                }
            } else {
                throw new Error(result.message || 'Unknown error');
            }
        } catch (error) {
            console.error('Error publishing article:', error);
            this.showError(`Failed to publish article: ${error.message}`);
        } finally {
            // Re-enable the publish button
            const publishBtn = document.querySelector(`[data-article-id="${articleId}"].publish-btn`);
            if (publishBtn) {
                publishBtn.disabled = false;
                publishBtn.innerHTML = '<i class="fas fa-rocket"></i> Publish';
            }
        }
    }

    /**
     * Show login prompt for unauthenticated users
     */
    showLoginPrompt() {
        if (confirm('You need to be logged in to publish articles. Would you like to go to the login page?')) {
            window.location.href = '/login';
        }
    }

    /**
     * Clear category filter and reset to show all categories
     */
    clearFilters() {
        // Reset category dropdown to show all categories
        const categorySelect = document.getElementById('categorySelect');
        if (categorySelect) categorySelect.value = '';

        // Auto-fetch latest news after clearing category filter
        setTimeout(() => {
            this.fetchNewsFromAPI(true);
        }, 100);
    }

    /**
     * Utility functions
     */
    truncateText(text, maxLength) {
        if (!text) return '';
        return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
    }

    formatDate(dateString) {
        try {
            return new Date(dateString).toLocaleDateString('en-US', {
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch {
            return 'Unknown date';
        }
    }

    /**
     * Show notification methods
     */
    showNotification(type, message) {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 350px;';
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 5000);
    }

    showSuccess(message) { this.showNotification('success', message); }
    showError(message) { this.showNotification('danger', message); }
    showWarning(message) { this.showNotification('warning', message); }

    showLoading(message = 'Loading...') {
        console.log('Loading:', message);
    }

    hideLoading() {
        console.log('Loading complete');
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.publicNewsFeed = new PublicNewsFeed();
});
