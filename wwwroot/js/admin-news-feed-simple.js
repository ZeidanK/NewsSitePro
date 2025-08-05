/**
 * Simplified AdminNewsFeed - Back to working basics
 * Focuses on: Fetch articles → Select articles → Publish selected/individual
 */
class AdminNewsFeed {
    constructor() {
        this.fetchedArticles = new Map(); // Store fetched articles
        this.selectedArticles = new Set(); // Track selected article IDs
        this.setupEventListeners();
        console.log('AdminNewsFeed initialized (simplified)');
    }

    /**
     * Set up all event listeners
     */
    setupEventListeners() {
        // Fetch button
        const fetchBtn = document.getElementById('fetchNewsBtn');
        if (fetchBtn) {
            fetchBtn.addEventListener('click', () => this.fetchNewsFromAPI());
        }

        // Publish selected button (main)
        const publishSelectedBtn = document.getElementById('publishSelectedBtn');
        if (publishSelectedBtn) {
            publishSelectedBtn.addEventListener('click', () => this.publishSelectedArticles());
        }

        // Publish selected button (sidebar)
        const publishSelectedSidebarBtn = document.getElementById('publishSelectedSidebarBtn');
        if (publishSelectedSidebarBtn) {
            publishSelectedSidebarBtn.addEventListener('click', () => this.publishSelectedArticles());
        }

        // Clear selection button (main)
        const clearSelectionBtn = document.getElementById('clearSelectionBtn');
        if (clearSelectionBtn) {
            clearSelectionBtn.addEventListener('click', () => this.clearSelection());
        }

        // Clear selection button (sidebar) 
        const clearSelectionSidebarBtn = document.getElementById('clearSelectionSidebarBtn');
        if (clearSelectionSidebarBtn) {
            clearSelectionSidebarBtn.addEventListener('click', () => this.clearSelection());
        }

        // Manage sources button
        const manageSourcesBtn = document.getElementById('manageSourcesBtn');
        if (manageSourcesBtn) {
            manageSourcesBtn.addEventListener('click', () => this.showSourceManagement());
        }

        // Background service button
        const backgroundServiceBtn = document.getElementById('backgroundServiceBtn');
        if (backgroundServiceBtn) {
            backgroundServiceBtn.addEventListener('click', () => this.toggleBackgroundService());
        }
    }

    /**
     * Fetch news articles from API
     */
    async fetchNewsFromAPI() {
        try {
            const category = document.getElementById('categorySelect')?.value || 'general';
            const country = document.getElementById('countrySelect')?.value || 'us';
            const count = document.getElementById('articleCountSelect')?.value || '20';

            this.showLoading('Fetching news articles...');

            const response = await fetch('/api/News/fetch-external', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    type: 'top-headlines',
                    category: category,
                    country: country,
                    pageSize: parseInt(count)
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();

            if (data.success && data.articles && data.articles.length > 0) {
                this.processFetchedArticles(data.articles);
                this.showSuccess(`Successfully fetched ${data.articles.length} articles`);
            } else {
                this.showWarning('No articles found for the selected criteria');
                // Clear the container
                const container = document.getElementById('newsArticlesContainer');
                if (container) {
                    container.innerHTML = '<div class="text-center p-4"><p class="text-muted">No articles found. Try a different category or country.</p></div>';
                }
            }

        } catch (error) {
            console.error('Error fetching news:', error);
            this.showError(`Failed to fetch news: ${error.message}`);
        } finally {
            this.hideLoading();
        }
    }

    /**
     * Process and display fetched articles
     */
    processFetchedArticles(articles) {
        this.fetchedArticles.clear();
        this.selectedArticles.clear();

        // Store articles with generated IDs
        articles.forEach((article, index) => {
            const id = `article_${Date.now()}_${index}`;
            this.fetchedArticles.set(id, {
                id: id,
                title: article.Title || article.title || 'Untitled',
                content: article.Content || article.content || article.description || 'No content available',
                imageUrl: article.ImageURL || article.imageUrl || article.urlToImage || '',
                sourceUrl: article.SourceURL || article.sourceUrl || article.url || '',
                sourceName: article.SourceName || article.sourceName || (article.source ? article.source.name : '') || 'Unknown Source',
                publishedAt: article.PublishDate || article.publishedAt || new Date().toISOString(),
                category: document.getElementById('categorySelect')?.value || 'general'
            });
        });

        this.renderArticles();
        this.updateCounts();
    }

    /**
     * Render articles in the container
     */
    renderArticles() {
        const container = document.getElementById('newsArticlesContainer');
        if (!container) return;

        if (this.fetchedArticles.size === 0) {
            container.innerHTML = '<div class="text-center p-4"><p class="text-muted">No articles to display. Click "Fetch News Articles" to get started.</p></div>';
            return;
        }

        container.innerHTML = Array.from(this.fetchedArticles.values()).map(article => 
            this.createArticleCard(article)
        ).join('');

        // Attach event listeners to new elements
        this.attachArticleEventListeners();
    }

    /**
     * Create HTML for individual article card
     */
    createArticleCard(article) {
        const isSelected = this.selectedArticles.has(article.id);
        
        return `
            <div class="card mb-3 article-card ${isSelected ? 'border-primary' : ''}" data-article-id="${article.id}">
                <div class="row g-0">
                    <div class="col-md-3">
                        <img src="${article.imageUrl || '/images/news-placeholder.jpg'}" 
                             class="img-fluid rounded-start article-image" 
                             alt="Article Image"
                             onerror="this.src='/images/news-placeholder.jpg'">
                    </div>
                    <div class="col-md-9">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-start mb-2">
                                <h5 class="card-title">${this.truncateText(article.title, 80)}</h5>
                                <div class="form-check">
                                    <input class="form-check-input article-checkbox" type="checkbox" 
                                           ${isSelected ? 'checked' : ''} data-article-id="${article.id}">
                                </div>
                            </div>
                            <p class="card-text">${this.truncateText(article.content, 150)}</p>
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
                                </div>
                            </div>
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
        // Checkbox change events
        document.querySelectorAll('.article-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const articleId = e.target.dataset.articleId;
                if (e.target.checked) {
                    this.selectedArticles.add(articleId);
                } else {
                    this.selectedArticles.delete(articleId);
                }
                this.updateCounts();
                this.updateCardSelection(articleId, e.target.checked);
            });
        });

        // Preview button events
        document.querySelectorAll('.preview-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const articleId = e.target.closest('.preview-btn').dataset.articleId;
                this.previewArticle(articleId);
            });
        });

        // Individual publish button events
        document.querySelectorAll('.publish-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const articleId = e.target.closest('.publish-btn').dataset.articleId;
                this.publishIndividualArticle(articleId);
            });
        });
    }

    /**
     * Update card visual selection state
     */
    updateCardSelection(articleId, isSelected) {
        const card = document.querySelector(`[data-article-id="${articleId}"]`);
        if (card) {
            if (isSelected) {
                card.classList.add('border-primary');
            } else {
                card.classList.remove('border-primary');
            }
        }
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
                    <a href="${article.sourceUrl}" target="_blank" class="btn btn-outline-primary btn-sm">
                        <i class="fas fa-external-link-alt"></i> View Original
                    </a>
                </div>
            </div>
        `;

        // Set up modal publish button
        const publishBtn = document.getElementById('publishFromModalBtn');
        if (publishBtn) {
            publishBtn.onclick = () => {
                this.publishIndividualArticle(articleId);
                const modal = bootstrap.Modal.getInstance(document.getElementById('articlePreviewModal'));
                modal?.hide();
            };
        }

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('articlePreviewModal'));
        modal.show();
    }

    /**
     * Publish individual article
     */
    async publishIndividualArticle(articleId) {
        const article = this.fetchedArticles.get(articleId);
        if (!article) return;

        try {
            await this.publishArticles([article]);
            
            // Remove from fetched articles
            this.fetchedArticles.delete(articleId);
            this.selectedArticles.delete(articleId);
            
            // Re-render
            this.renderArticles();
            this.updateCounts();
            
        } catch (error) {
            console.error('Error publishing article:', error);
            this.showError(`Failed to publish article: ${error.message}`);
        }
    }

    /**
     * Publish selected articles
     */
    async publishSelectedArticles() {
        if (this.selectedArticles.size === 0) {
            this.showWarning('Please select articles to publish');
            return;
        }

        const selectedArticleData = Array.from(this.selectedArticles)
            .map(id => this.fetchedArticles.get(id))
            .filter(article => article);

        try {
            await this.publishArticles(selectedArticleData);
            
            // Remove published articles
            this.selectedArticles.forEach(id => {
                this.fetchedArticles.delete(id);
            });
            this.selectedArticles.clear();
            
            // Re-render
            this.renderArticles();
            this.updateCounts();
            
        } catch (error) {
            console.error('Error publishing articles:', error);
            this.showError(`Failed to publish articles: ${error.message}`);
        }
    }

    /**
     * Publish articles to database
     */
    async publishArticles(articles) {
        const response = await fetch('/api/News/publish-articles', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                articles: articles.map(article => ({
                    title: article.title,
                    content: article.content,
                    imageUrl: article.imageUrl,
                    sourceUrl: article.sourceUrl,
                    sourceName: article.sourceName,
                    category: article.category
                }))
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();
        
        if (result.success) {
            this.showSuccess(`Successfully published ${result.published || articles.length} articles`);
        } else {
            throw new Error(result.message || 'Unknown error');
        }
    }

    /**
     * Clear all selections
     */
    clearSelection() {
        this.selectedArticles.clear();
        
        // Uncheck all checkboxes
        document.querySelectorAll('.article-checkbox').forEach(checkbox => {
            checkbox.checked = false;
        });
        
        // Remove visual selection
        document.querySelectorAll('.article-card').forEach(card => {
            card.classList.remove('border-primary');
        });
        
        this.updateCounts();
    }

    /**
     * Update counts in UI
     */
    updateCounts() {
        const fetchedCountEl = document.getElementById('fetchedCount');
        if (fetchedCountEl) {
            fetchedCountEl.textContent = this.fetchedArticles.size;
        }

        const selectedCountEl = document.getElementById('selectedCount');
        if (selectedCountEl) {
            selectedCountEl.textContent = this.selectedArticles.size;
        }

        const publishSelectedBtn = document.getElementById('publishSelectedBtn');
        if (publishSelectedBtn) {
            publishSelectedBtn.disabled = this.selectedArticles.size === 0;
        }
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
        // Simple loading - you can enhance this
        console.log('Loading:', message);
    }

    hideLoading() {
        console.log('Loading complete');
    }

    /**
     * Show source management modal
     */
    showSourceManagement() {
        const modal = new bootstrap.Modal(document.getElementById('sourceManagementModal'));
        modal.show();
    }

    /**
     * Toggle background service
     */
    toggleBackgroundService() {
        // Simple toggle for now
        const textEl = document.getElementById('backgroundServiceText');
        if (textEl) {
            const isOn = textEl.textContent.includes('ON');
            textEl.textContent = isOn ? 'Auto Sync: OFF' : 'Auto Sync: ON';
            this.showSuccess(isOn ? 'Background sync disabled' : 'Background sync enabled');
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.adminNewsFeed = new AdminNewsFeed();
});
