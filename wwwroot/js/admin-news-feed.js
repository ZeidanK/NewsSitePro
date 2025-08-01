/*
    admin-news-feed.js - Specialized JavaScript for admin news management
    Extends live news feed functionality with admin-specific features like fetching, approval workflow, and source management
*/

/**
 * AdminNewsFeed class - Handles admin-specific news management functionality
 * Extends the base LiveNewsFeed with administrative controls and news API integration
 * Features: External news fetching, approval workflow, source management, bulk operations
 */
class AdminNewsFeed {
    constructor() {
        // Core properties for admin news management
        this.currentFilter = 'pending';
        this.currentPage = 1;
        this.pageSize = 20;
        this.isLoading = false;
        this.hasMore = true;
        
        // News API configuration - can be moved to settings modal
        this.newsApiConfig = {
            apiKey: '', // Will be loaded from user settings or entered via modal
            baseUrl: 'https://newsapi.org/v2',
            defaultCountry: 'us',
            defaultCategory: 'general'
        };

        // Article management state
        this.pendingArticles = new Map(); // Store fetched articles pending review
        this.selectedArticles = new Set(); // Track selected articles for bulk operations
        this.sources = []; // Available news sources
        
        // DOM element references for performance
        this.elements = {
            container: null,
            loadingIndicator: null,
            filterTabs: null,
            loadMoreBtn: null
        };
    }

    /**
     * Initialize admin news feed system
     * Sets up event listeners, loads initial data, and configures UI components
     */
    async init() {
        try {
            // Cache DOM elements for better performance
            this.cacheElements();
            
            // Set up all event listeners for admin interface
            this.setupEventListeners();
            
            // Load initial news data based on default filter
            await this.loadNewsArticles();
            
            // Initialize news sources list
            await this.loadNewsSources();
            
            // Set up auto-refresh for real-time updates (every 5 minutes)
            this.setupAutoRefresh();
            
            console.log('Admin News Feed initialized successfully');
        } catch (error) {
            console.error('Error initializing Admin News Feed:', error);
            this.showErrorMessage('Failed to initialize news management system');
        }
    }

    /**
     * Cache frequently used DOM elements to improve performance
     * Called once during initialization to avoid repeated queries
     */
    cacheElements() {
        this.elements = {
            container: document.getElementById('newsArticlesContainer'),
            loadingIndicator: document.getElementById('loadingIndicator'),
            filterTabs: document.getElementById('newsFilterTabs'),
            loadMoreBtn: document.getElementById('loadMoreBtn'),
            // Form elements for news fetching
            categorySelect: document.getElementById('categorySelect'),
            countrySelect: document.getElementById('countrySelect'),
            sourcesList: document.getElementById('sourcesList')
        };
    }

    /**
     * Set up all event listeners for admin news interface
     * Handles clicks, form submissions, and other user interactions
     */
    setupEventListeners() {
        // Filter tab navigation
        if (this.elements.filterTabs) {
            this.elements.filterTabs.addEventListener('click', (e) => {
                if (e.target.matches('[data-filter]')) {
                    this.switchFilter(e.target.dataset.filter);
                }
            });
        }

        // News fetching buttons
        document.getElementById('fetchLatestBtn')?.addEventListener('click', () => {
            this.fetchNewsFromAPI('latest');
        });

        document.getElementById('fetchTopBtn')?.addEventListener('click', () => {
            this.fetchNewsFromAPI('top-headlines');
        });

        document.getElementById('fetchBreakingBtn')?.addEventListener('click', () => {
            this.fetchNewsFromAPI('breaking');
        });

        // Bulk operation buttons
        document.getElementById('publishAllBtn')?.addEventListener('click', () => {
            this.performBulkOperation('publish');
        });

        document.getElementById('rejectAllBtn')?.addEventListener('click', () => {
            this.performBulkOperation('reject');
        });

        document.getElementById('autoModeBtn')?.addEventListener('click', () => {
            this.toggleAutoPublishMode();
        });

        // Source management
        document.getElementById('manageSourcesBtn')?.addEventListener('click', () => {
            this.openSourceManagementModal();
        });

        // Load more button
        this.elements.loadMoreBtn?.addEventListener('click', () => {
            this.loadMoreArticles();
        });

        // Modal action buttons
        this.setupModalEventListeners();
    }

    /**
     * Set up event listeners for modal interactions
     * Handles article preview, approval, and source management modals
     */
    setupModalEventListeners() {
        // Article preview modal actions
        document.getElementById('approveArticleBtn')?.addEventListener('click', () => {
            this.approveCurrentArticle();
        });

        document.getElementById('rejectArticleBtn')?.addEventListener('click', () => {
            this.rejectCurrentArticle();
        });

        document.getElementById('editArticleBtn')?.addEventListener('click', () => {
            this.editCurrentArticle();
        });

        document.getElementById('publishNowBtn')?.addEventListener('click', () => {
            this.publishCurrentArticle();
        });

        // Source management modal
        document.getElementById('saveSourcesBtn')?.addEventListener('click', () => {
            this.saveSourceSettings();
        });
    }

    /**
     * Switch between different article filters (pending, approved, published, rejected)
     * Updates UI state and reloads articles based on selected filter
     */
    async switchFilter(filter) {
        // Update active tab styling
        this.elements.filterTabs.querySelectorAll('.nav-link').forEach(tab => {
            tab.classList.remove('active');
        });
        document.querySelector(`[data-filter="${filter}"]`).classList.add('active');

        // Update current filter and reset pagination
        this.currentFilter = filter;
        this.currentPage = 1;
        this.hasMore = true;

        // Clear current articles and load new ones
        this.elements.container.innerHTML = '';
        await this.loadNewsArticles();
    }

    /**
     * Load news articles based on current filter and pagination
     * Fetches from database for published articles or displays pending articles
     */
    async loadNewsArticles() {
        if (this.isLoading) return;

        this.isLoading = true;
        this.showLoading();

        try {
            let articles = [];

            switch (this.currentFilter) {
                case 'pending':
                    // Show articles fetched from API but not yet published
                    articles = Array.from(this.pendingArticles.values());
                    break;
                
                case 'published':
                    // Fetch published articles from database
                    articles = await this.fetchPublishedArticles();
                    break;
                
                case 'approved':
                case 'rejected':
                    // TODO: Implement when approval system is added to database
                    articles = [];
                    break;
                
                default:
                    articles = [];
            }

            // Render articles in the container
            this.renderArticles(articles);
            
            // Update load more button visibility
            this.updateLoadMoreButton(articles.length);

        } catch (error) {
            console.error('Error loading news articles:', error);
            this.showErrorMessage('Failed to load news articles');
        } finally {
            this.isLoading = false;
            this.hideLoading();
        }
    }

    /**
     * Fetch news from external News API based on specified type
     * Supports latest, top-headlines, and breaking news categories
     */
    async fetchNewsFromAPI(type) {
        const categorySelect = document.getElementById('categorySelect');
        const countrySelect = document.getElementById('countrySelect');
        
        const category = categorySelect ? categorySelect.value : 'general';
        const country = countrySelect ? countrySelect.value : 'us';

        // Show loading state
        this.showLoading();
        this.showStatusMessage('Fetching news from external sources...', 'info');

        try {
            // Call our NewsController endpoint instead of directly calling external API
            // This provides better error handling and logging
            const response = await fetch('/api/News/fetch-external', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.getAuthToken()}`
                },
                body: JSON.stringify({
                    type: type,
                    category: category,
                    country: country,
                    pageSize: 20
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            if (data.success && data.articles) {
                // Process and store fetched articles as pending
                this.processFetchedArticles(data.articles, type);
                this.showStatusMessage(`Successfully fetched ${data.articles.length} articles`, 'success');
                
                // Switch to pending tab to show new articles
                if (this.currentFilter !== 'pending') {
                    this.switchFilter('pending');
                }
            } else {
                throw new Error(data.message || 'Failed to fetch articles');
            }

        } catch (error) {
            console.error('Error fetching news from API:', error);
            this.showErrorMessage('Failed to fetch news. Please check your API configuration.');
        } finally {
            this.hideLoading();
        }
    }

    /**
     * Process articles fetched from external API
     * Converts external format to internal format and stores as pending
     */
    processFetchedArticles(articles, sourceType) {
        articles.forEach(article => {
            // Convert external article format to internal format
            const processedArticle = {
                id: `temp_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`, // Temporary ID
                title: article.title,
                content: article.description || article.content || '',
                summary: article.description || '',
                imageUrl: article.urlToImage,
                sourceUrl: article.url,
                sourceName: article.source?.name || 'External Source',
                publishDate: new Date(article.publishedAt || Date.now()),
                category: this.elements.categorySelect.value,
                status: 'pending',
                fetchedAt: new Date(),
                fetchType: sourceType,
                // Admin review fields
                needsReview: true,
                isApproved: false,
                isRejected: false
            };

            // Store in pending articles map
            this.pendingArticles.set(processedArticle.id, processedArticle);
        });

        // Update pending count badge
        this.updateFilterBadges();
    }

    /**
     * Render articles in the main container
     * Creates article cards with admin action buttons
     */
    renderArticles(articles) {
        const container = this.elements.container;
        
        // Clear container if starting fresh
        if (this.currentPage === 1) {
            container.innerHTML = '';
        }

        articles.forEach(article => {
            const articleElement = this.createAdminArticleCard(article);
            container.appendChild(articleElement);
        });

        // Show empty state if no articles
        if (articles.length === 0 && this.currentPage === 1) {
            this.showEmptyState();
        }
    }

    /**
     * Create article card with admin controls
     * Includes preview, approve, reject, edit, and publish buttons
     */
    createAdminArticleCard(article) {
        const card = document.createElement('div');
        card.className = 'admin-article-card';
        card.dataset.articleId = article.id;

        // Determine card styling based on article status
        const statusClass = this.getStatusClass(article.status);
        card.classList.add(statusClass);

        card.innerHTML = `
            <div class="article-card-header">
                <div class="article-meta">
                    <span class="source-name">${article.sourceName || 'Unknown Source'}</span>
                    <span class="article-date">${this.formatDate(article.publishDate)}</span>
                    <span class="status-badge ${statusClass}">${article.status || 'pending'}</span>
                </div>
                <div class="article-actions">
                    <button class="btn btn-sm btn-outline-primary" onclick="adminNewsFeed.previewArticle('${article.id}')">
                        <i class="fas fa-eye"></i> Preview
                    </button>
                    ${this.getActionButtons(article)}
                </div>
            </div>
            
            <div class="article-card-content">
                <h5 class="article-title">${article.title}</h5>
                <p class="article-summary">${article.summary || article.content.substring(0, 150) + '...'}</p>
                
                ${article.imageUrl ? `
                    <div class="article-image">
                        <img src="${article.imageUrl}" alt="${article.title}" loading="lazy">
                    </div>
                ` : ''}
                
                <div class="article-metadata">
                    <span class="category-tag">${article.category || 'general'}</span>
                    ${article.sourceUrl ? `<a href="${article.sourceUrl}" target="_blank" class="source-link">View Original</a>` : ''}
                </div>
            </div>
            
            <div class="article-card-footer">
                <div class="bulk-select">
                    <input type="checkbox" class="article-checkbox" value="${article.id}" 
                           onchange="adminNewsFeed.toggleArticleSelection('${article.id}')">
                    <label>Select for bulk action</label>
                </div>
                ${article.fetchedAt ? `<small class="text-muted">Fetched: ${this.formatDate(article.fetchedAt)}</small>` : ''}
            </div>
        `;

        return card;
    }

    /**
     * Get appropriate action buttons based on article status
     * Different statuses show different available actions
     */
    getActionButtons(article) {
        const status = article.status || 'pending';
        
        switch (status) {
            case 'pending':
                return `
                    <button class="btn btn-sm btn-success" onclick="adminNewsFeed.approveArticle('${article.id}')">
                        <i class="fas fa-check"></i> Approve
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="adminNewsFeed.rejectArticle('${article.id}')">
                        <i class="fas fa-times"></i> Reject
                    </button>
                `;
            
            case 'approved':
                return `
                    <button class="btn btn-sm btn-primary" onclick="adminNewsFeed.publishArticle('${article.id}')">
                        <i class="fas fa-rocket"></i> Publish
                    </button>
                    <button class="btn btn-sm btn-warning" onclick="adminNewsFeed.editArticle('${article.id}')">
                        <i class="fas fa-edit"></i> Edit
                    </button>
                `;
            
            case 'published':
                return `
                    <button class="btn btn-sm btn-info" onclick="adminNewsFeed.viewPublishedPost('${article.id}')">
                        <i class="fas fa-external-link-alt"></i> View Post
                    </button>
                `;
                
            default:
                return '';
        }
    }

    /**
     * Preview article in modal for detailed review
     * Shows full content with all metadata and action buttons
     */
    async previewArticle(articleId) {
        const article = this.pendingArticles.get(articleId) || await this.fetchArticleById(articleId);
        
        if (!article) {
            this.showErrorMessage('Article not found');
            return;
        }

        // Populate modal with article data
        const modalContent = document.getElementById('articlePreviewContent');
        modalContent.innerHTML = `
            <div class="article-preview">
                <div class="preview-header">
                    <h3>${article.title}</h3>
                    <div class="preview-meta">
                        <span class="source">Source: ${article.sourceName}</span>
                        <span class="date">Date: ${this.formatDate(article.publishDate)}</span>
                        <span class="category">Category: ${article.category}</span>
                    </div>
                </div>
                
                ${article.imageUrl ? `
                    <div class="preview-image">
                        <img src="${article.imageUrl}" alt="${article.title}" class="img-fluid">
                    </div>
                ` : ''}
                
                <div class="preview-content">
                    <h5>Summary:</h5>
                    <p>${article.summary || 'No summary available'}</p>
                    
                    <h5>Full Content:</h5>
                    <div class="content-text">${article.content || 'No content available'}</div>
                </div>
                
                ${article.sourceUrl ? `
                    <div class="preview-source">
                        <a href="${article.sourceUrl}" target="_blank" class="btn btn-outline-primary">
                            <i class="fas fa-external-link-alt"></i> View Original Article
                        </a>
                    </div>
                ` : ''}
            </div>
        `;

        // Store current article ID for modal actions
        this.currentPreviewArticle = articleId;
        
        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('articlePreviewModal'));
        modal.show();
    }

    /**
     * Approve article for publishing
     * Updates article status and moves to approved filter
     */
    async approveArticle(articleId) {
        try {
            const article = this.pendingArticles.get(articleId);
            if (article) {
                article.status = 'approved';
                article.approvedAt = new Date();
                article.isApproved = true;
                article.needsReview = false;
                
                this.showStatusMessage('Article approved successfully', 'success');
                this.updateFilterBadges();
                this.refreshCurrentView();
            }
        } catch (error) {
            console.error('Error approving article:', error);
            this.showErrorMessage('Failed to approve article');
        }
    }

    /**
     * Reject article with optional reason
     * Removes from pending and adds to rejected list
     */
    async rejectArticle(articleId, reason = '') {
        try {
            const article = this.pendingArticles.get(articleId);
            if (article) {
                article.status = 'rejected';
                article.rejectedAt = new Date();
                article.rejectionReason = reason;
                article.isRejected = true;
                article.needsReview = false;
                
                this.showStatusMessage('Article rejected', 'warning');
                this.updateFilterBadges();
                this.refreshCurrentView();
            }
        } catch (error) {
            console.error('Error rejecting article:', error);
            this.showErrorMessage('Failed to reject article');
        }
    }

    /**
     * Publish approved article to the main news feed
     * Converts to NewsArticle format and saves to database
     */
    async publishArticle(articleId) {
        try {
            const article = this.pendingArticles.get(articleId);
            if (!article) {
                throw new Error('Article not found');
            }

            // Prepare article data for publication
            const publishData = {
                title: article.title,
                content: article.content,
                summary: article.summary,
                imageUrl: article.imageUrl,
                sourceUrl: article.sourceUrl,
                sourceName: article.sourceName,
                category: article.category,
                publishDate: new Date().toISOString()
            };

            // Call NewsController to publish article
            const response = await fetch('/api/News/publish-article', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.getAuthToken()}`
                },
                body: JSON.stringify(publishData)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();

            if (result.success) {
                // Update article status
                article.status = 'published';
                article.publishedAt = new Date();
                article.publishedId = result.articleId;
                
                this.showStatusMessage('Article published successfully!', 'success');
                this.updateFilterBadges();
                this.refreshCurrentView();
            } else {
                throw new Error(result.message || 'Failed to publish article');
            }

        } catch (error) {
            console.error('Error publishing article:', error);
            this.showErrorMessage('Failed to publish article: ' + error.message);
        }
    }

    // Utility methods for UI management and helper functions

    /**
     * Update filter badge counts based on current article status
     */
    updateFilterBadges() {
        const articles = Array.from(this.pendingArticles.values());
        
        document.getElementById('pendingBadge').textContent = 
            articles.filter(a => a.status === 'pending').length;
        document.getElementById('approvedBadge').textContent = 
            articles.filter(a => a.status === 'approved').length;
        document.getElementById('rejectedBadge').textContent = 
            articles.filter(a => a.status === 'rejected').length;
    }

    /**
     * Show loading indicator
     */
    showLoading() {
        if (this.elements.loadingIndicator) {
            this.elements.loadingIndicator.style.display = 'block';
        }
    }

    /**
     * Hide loading indicator
     */
    hideLoading() {
        if (this.elements.loadingIndicator) {
            this.elements.loadingIndicator.style.display = 'none';
        }
    }

    /**
     * Show status message to user
     */
    showStatusMessage(message, type = 'info') {
        // Create toast notification
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        toast.style.top = '20px';
        toast.style.right = '20px';
        toast.style.zIndex = '9999';
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(toast);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 5000);
    }

    /**
     * Show error message to user
     */
    showErrorMessage(message) {
        this.showStatusMessage(message, 'danger');
    }

    /**
     * Get authentication token for API calls
     */
    getAuthToken() {
        // Get JWT token from cookie or localStorage
        const cookieToken = document.cookie
            .split('; ')
            .find(row => row.startsWith('jwtToken='))
            ?.split('=')[1];
            
        return cookieToken || localStorage.getItem('jwtToken') || '';
    }

    /**
     * Format date for display
     */
    formatDate(date) {
        if (!date) return 'Unknown';
        return new Date(date).toLocaleString();
    }

    /**
     * Get CSS class for article status
     */
    getStatusClass(status) {
        switch (status) {
            case 'pending': return 'status-pending';
            case 'approved': return 'status-approved';
            case 'published': return 'status-published';
            case 'rejected': return 'status-rejected';
            default: return 'status-unknown';
        }
    }

    /**
     * Refresh current view after status changes
     */
    refreshCurrentView() {
        // Simply re-render current articles
        this.loadNewsArticles();
    }

    /**
     * Setup auto-refresh for real-time updates
     */
    setupAutoRefresh() {
        // Refresh every 5 minutes to check for new articles
        setInterval(() => {
            if (this.currentFilter === 'published') {
                this.loadNewsArticles();
            }
        }, 5 * 60 * 1000); // 5 minutes
    }

    /**
     * Show empty state when no articles are available
     */
    showEmptyState() {
        const container = this.elements.container;
        container.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">
                    <i class="fas fa-newspaper fa-3x text-muted"></i>
                </div>
                <h4 class="text-muted">No articles found</h4>
                <p class="text-muted">
                    ${this.currentFilter === 'published' 
                        ? 'No published articles yet. Fetch some news from external sources to get started.' 
                        : 'No articles available for this filter. Try fetching news from external sources.'}
                </p>
                <button class="btn btn-primary" onclick="adminNewsFeed.fetchNewsFromAPI('latest', 'general')">
                    <i class="fas fa-sync"></i> Fetch News
                </button>
            </div>
        `;
    }

    /**
     * Load news sources for management
     */
    async loadNewsSources() {
        try {
            // For now, use static list of sources
            // This could be enhanced to fetch from an API
            this.sources = [
                { id: 'bbc-news', name: 'BBC News', enabled: true },
                { id: 'cnn', name: 'CNN', enabled: true },
                { id: 'reuters', name: 'Reuters', enabled: true },
                { id: 'associated-press', name: 'Associated Press', enabled: false }
            ];
        } catch (error) {
            console.error('Error loading news sources:', error);
            this.sources = [];
        }
    }

    /**
     * Open source management modal
     */
    openSourceManagementModal() {
        // Create and show modal for managing news sources
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.innerHTML = `
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Manage News Sources</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p>Configure which news sources to fetch articles from:</p>
                        <div id="sourcesList">
                            ${this.sources.map(source => `
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" 
                                           id="source-${source.id}" ${source.enabled ? 'checked' : ''}>
                                    <label class="form-check-label" for="source-${source.id}">
                                        ${source.name}
                                    </label>
                                </div>
                            `).join('')}
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        <button type="button" class="btn btn-primary" onclick="adminNewsFeed.saveSourceSettings()">Save Changes</button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
        
        // Remove modal from DOM when hidden
        modal.addEventListener('hidden.bs.modal', () => {
            modal.remove();
        });
    }

    /**
     * Save source settings
     */
    saveSourceSettings() {
        try {
            // Initialize sources if not exists
            if (!this.sources) {
                this.sources = [];
            }
            
            // Update source settings based on checkboxes
            this.sources.forEach(source => {
                const checkbox = document.getElementById(`source-${source.id}`);
                if (checkbox) {
                    source.enabled = checkbox.checked;
                }
            });
            
            this.showMessage('Source settings saved successfully', 'success');
            
            // Close modal
            const modal = document.querySelector('.modal.show');
            if (modal) {
                const modalInstance = bootstrap.Modal.getInstance(modal);
                if (modalInstance) {
                    modalInstance.hide();
                }
            }
        } catch (error) {
            console.error('Error saving source settings:', error);
            this.showMessage('Failed to save source settings', 'error');
        }
    }

    /**
     * Fetch published articles from database
     */
    async fetchPublishedArticles() {
        try {
            const response = await fetch('/api/News/admin/published?page=1&pageSize=20', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${this.getAuthToken()}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return data.articles || [];
        } catch (error) {
            console.error('Error fetching published articles:', error);
            this.showMessage('Failed to load published articles', 'error');
            return [];
        }
    }

    /**
     * Perform bulk operations on selected articles
     */
    async performBulkOperation(operation) {
        const selectedCards = document.querySelectorAll('.admin-article-card input[type="checkbox"]:checked');
        
        if (selectedCards.length === 0) {
            this.showMessage('Please select articles first', 'warning');
            return;
        }

        const articleIds = Array.from(selectedCards).map(checkbox => 
            checkbox.closest('.admin-article-card').dataset.articleId
        );

        try {
            switch (operation) {
                case 'publish':
                    for (const articleId of articleIds) {
                        await this.publishArticle(articleId);
                    }
                    this.showMessage(`Published ${articleIds.length} articles`, 'success');
                    break;
                    
                case 'reject':
                    for (const articleId of articleIds) {
                        await this.rejectArticle(articleId);
                    }
                    this.showMessage(`Rejected ${articleIds.length} articles`, 'success');
                    break;
                    
                default:
                    this.showMessage('Unknown operation', 'error');
                    return;
            }
            
            // Refresh the view
            this.loadNewsArticles();
            
        } catch (error) {
            console.error('Error performing bulk operation:', error);
            this.showMessage(`Failed to ${operation} articles`, 'error');
        }
    }

    /**
     * Toggle auto-publish mode
     */
    toggleAutoPublishMode() {
        this.autoPublishMode = !this.autoPublishMode;
        
        const button = document.querySelector('[onclick*="toggleAutoPublishMode"]');
        if (button) {
            button.innerHTML = this.autoPublishMode 
                ? '<i class="fas fa-pause"></i> Disable Auto-Publish'
                : '<i class="fas fa-play"></i> Enable Auto-Publish';
                
            button.className = this.autoPublishMode
                ? 'btn btn-warning'
                : 'btn btn-success';
        }
        
        this.showMessage(
            this.autoPublishMode 
                ? 'Auto-publish mode enabled' 
                : 'Auto-publish mode disabled', 
            'info'
        );
    }

    /**
     * Reject an article
     */
    async rejectArticle(articleId) {
        try {
            // For now, just remove from UI
            // This could be enhanced to call a rejection API
            const card = document.querySelector(`[data-article-id="${articleId}"]`);
            if (card) {
                card.remove();
            }
        } catch (error) {
            console.error('Error rejecting article:', error);
            throw error;
        }
    }

    /**
     * Update load more button visibility and state
     */
    updateLoadMoreButton(articlesCount) {
        const loadMoreBtn = document.getElementById('loadMoreBtn');
        if (loadMoreBtn) {
            if (articlesCount < this.pageSize || !this.hasMore) {
                loadMoreBtn.style.display = 'none';
            } else {
                loadMoreBtn.style.display = 'block';
                loadMoreBtn.disabled = this.isLoading;
            }
        }
    }

    /**
     * Show status message to user
     */
    showMessage(message, type = 'info') {
        // Create or update message container
        let messageContainer = document.getElementById('admin-message-container');
        if (!messageContainer) {
            messageContainer = document.createElement('div');
            messageContainer.id = 'admin-message-container';
            messageContainer.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 1060;
                max-width: 300px;
            `;
            document.body.appendChild(messageContainer);
        }

        // Create message element
        const messageId = 'message-' + Date.now();
        const messageElement = document.createElement('div');
        messageElement.id = messageId;
        messageElement.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show`;
        messageElement.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        messageContainer.appendChild(messageElement);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (messageElement.parentNode) {
                messageElement.remove();
            }
        }, 5000);
    }

    /**
     * Show error message (alias for showMessage with error type)
     */
    showErrorMessage(message) {
        this.showMessage(message, 'error');
    }

    /**
     * Show status message (alias for showMessage with info type)
     */
    showStatusMessage(message, type = 'info') {
        this.showMessage(message, type);
    }
}

// Global instance for easy access from HTML onclick handlers
window.adminNewsFeed = null;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.adminNewsFeed = new AdminNewsFeed();
});
