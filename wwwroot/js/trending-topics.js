/**
 * Trending Topics JavaScript Module
 * Handles trending topics functionality for the sidebar
 */

class TrendingTopics {
    constructor() {
        this.apiBaseUrl = this.getApiUrl();
        this.container = document.getElementById('trendingList');
        this.refreshInterval = null;
        this.init();
    }

    /**
     * Get the correct API URL based on current environment
     */
    getApiUrl() {
        const protocol = window.location.protocol;
        const hostname = window.location.hostname;
        const port = window.location.port;
        const portSuffix = port && port !== '80' && port !== '443' ? `:${port}` : '';
        return `${protocol}//${hostname}${portSuffix}`;
    }

    /**
     * Initialize trending topics functionality
     */
    async init() {
        try {
            await this.loadTrendingTopics();
            this.setupAutoRefresh();
            this.setupEventListeners();
        } catch (error) {
            console.error('Failed to initialize trending topics:', error);
            this.showFallbackTopics();
        }
    }

    /**
     * Load trending topics from the API
     */
    async loadTrendingTopics() {
        try {
            const response = await fetch(`${this.apiBaseUrl}/api/trending/sidebar`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const topics = await response.json();
            this.renderTrendingTopics(topics);
            
            // Update last refresh time
            this.updateRefreshTime();
            
        } catch (error) {
            console.error('Error loading trending topics:', error);
            this.showFallbackTopics();
        }
    }

    /**
     * Render trending topics in the sidebar
     */
    renderTrendingTopics(topics) {
        if (!this.container) {
            console.warn('Trending topics container not found');
            return;
        }

        // Clear existing content
        this.container.innerHTML = '';

        // Render each trending topic
        topics.forEach((topic, index) => {
            const topicElement = this.createTopicElement(topic, index + 1);
            this.container.appendChild(topicElement);
        });
    }

    /**
     * Create a trending topic element
     */
    createTopicElement(topic, rank) {
        const div = document.createElement('div');
        div.className = 'trending-item';
        div.setAttribute('data-category', topic.category);
        div.setAttribute('data-topic', topic.topic);
        
        div.innerHTML = `
            <div class="trending-info">
                <div class="trending-rank">#${rank}</div>
                <span class="trending-category">${this.escapeHtml(topic.category)}</span>
                <span class="trending-topic">${this.escapeHtml(topic.topic)}</span>
                <span class="trending-count">${this.escapeHtml(topic.count)}</span>
                <span class="trending-score" title="Trend Score">${topic.score}</span>
            </div>
        `;

        // Add click handler to view related articles
        div.addEventListener('click', () => {
            this.handleTopicClick(topic);
        });

        return div;
    }

    /**
     * Handle clicking on a trending topic
     */
    handleTopicClick(topic) {
        try {
            // Navigate to search page with trending topic filter
            const searchParams = new URLSearchParams({
                type: 'posts',
                category: topic.category,
                q: topic.topic,
                trending: 'true'
            });

            window.location.href = `/Search?${searchParams.toString()}`;
        } catch (error) {
            console.error('Error handling topic click:', error);
        }
    }

    /**
     * Show fallback trending topics when API fails
     */
    showFallbackTopics() {
        const fallbackTopics = [
            { topic: "AI Technology", category: "Technology", count: "25.4K posts", score: 85.5 },
            { topic: "World Cup 2025", category: "Sports", count: "18.2K posts", score: 78.2 },
            { topic: "Election Updates", category: "Politics", count: "32.1K posts", score: 72.8 },
            { topic: "Climate Action", category: "Environment", count: "12.8K posts", score: 65.3 },
            { topic: "Health Trends", category: "Health", count: "9.5K posts", score: 58.7 }
        ];

        this.renderTrendingTopics(fallbackTopics);
    }

    /**
     * Setup auto-refresh for trending topics
     */
    setupAutoRefresh() {
        // Refresh trending topics every 30 minutes
        this.refreshInterval = setInterval(() => {
            this.loadTrendingTopics();
        }, 30 * 60 * 1000); // 30 minutes
    }

    /**
     * Setup event listeners
     */
    setupEventListeners() {
        // Manual refresh button (if exists)
        const refreshButton = document.querySelector('.trending-refresh-btn');
        if (refreshButton) {
            refreshButton.addEventListener('click', () => {
                this.loadTrendingTopics();
            });
        }

        // Cleanup on page unload
        window.addEventListener('beforeunload', () => {
            if (this.refreshInterval) {
                clearInterval(this.refreshInterval);
            }
        });
    }

    /**
     * Update the last refresh time display
     */
    updateRefreshTime() {
        const refreshTimeElement = document.querySelector('.trending-last-updated');
        if (refreshTimeElement) {
            const now = new Date();
            const timeString = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            refreshTimeElement.textContent = `Last updated: ${timeString}`;
        }
    }

    /**
     * Escape HTML to prevent XSS attacks
     */
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * Manually refresh trending topics
     */
    async refresh() {
        await this.loadTrendingTopics();
    }

    /**
     * Get trending topics data for external use
     */
    async getTrendingTopicsData(count = 10, category = null) {
        try {
            const params = new URLSearchParams({ count: count.toString() });
            if (category) {
                params.append('category', category);
            }

            const response = await fetch(`${this.apiBaseUrl}/api/trending?${params.toString()}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Error getting trending topics data:', error);
            return null;
        }
    }

    /**
     * Cleanup method
     */
    destroy() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }
}

// Initialize trending topics when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize if trending container exists
    if (document.getElementById('trendingList')) {
        window.trendingTopics = new TrendingTopics();
    }
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TrendingTopics;
}
