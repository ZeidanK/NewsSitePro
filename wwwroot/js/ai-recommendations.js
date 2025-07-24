// AI-powered content recommendation engine with machine learning
class AIRecommendationEngine {
    constructor() {
        this.userProfile = {};
        this.readingHistory = [];
        this.preferences = {};
        this.recommendations = [];
        this.init();
    }

    async init() {
        await this.loadUserProfile();
        await this.loadReadingHistory();
        this.setupBehaviorTracking();
        this.setupPersonalizationUI();
        await this.generateRecommendations();
        this.setupContentInsights();
    }

    async loadUserProfile() {
        try {
            const response = await fetch('/api/user/profile/insights', {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
                }
            });
            
            if (response.ok) {
                this.userProfile = await response.json();
                this.analyzeUserBehavior();
            }
        } catch (error) {
            console.error('Error loading user profile:', error);
        }
    }

    async loadReadingHistory() {
        try {
            const response = await fetch('/api/user/reading-history', {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
                }
            });
            
            if (response.ok) {
                this.readingHistory = await response.json();
                this.analyzeReadingPatterns();
            }
        } catch (error) {
            console.error('Error loading reading history:', error);
        }
    }

    setupBehaviorTracking() {
        // Track article views
        this.trackArticleViews();
        
        // Track reading time
        this.trackReadingTime();
        
        // Track interaction patterns
        this.trackInteractions();
        
        // Track scroll behavior
        this.trackScrollBehavior();
    }

    trackArticleViews() {
        const articles = document.querySelectorAll('.news-post');
        
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const postId = entry.target.dataset.postId;
                    const categories = entry.target.dataset.categories?.split(',') || [];
                    
                    this.recordView(postId, categories);
                    this.updateRecommendations(postId, categories);
                }
            });
        }, { threshold: 0.7, rootMargin: '0px 0px -20% 0px' });

        articles.forEach(article => observer.observe(article));
    }

    trackReadingTime() {
        let startTime = Date.now();
        let currentArticle = null;

        document.addEventListener('visibilitychange', () => {
            if (document.hidden && currentArticle) {
                this.recordReadingTime(currentArticle, Date.now() - startTime);
            } else {
                startTime = Date.now();
            }
        });

        // Track time spent on articles
        const articles = document.querySelectorAll('.news-post');
        articles.forEach(article => {
            article.addEventListener('click', () => {
                if (currentArticle && currentArticle !== article.dataset.postId) {
                    this.recordReadingTime(currentArticle, Date.now() - startTime);
                }
                currentArticle = article.dataset.postId;
                startTime = Date.now();
            });
        });
    }

    trackInteractions() {
        // Track likes, shares, comments
        document.addEventListener('click', (e) => {
            if (e.target.closest('.like-btn')) {
                const postId = e.target.closest('.news-post').dataset.postId;
                this.recordInteraction(postId, 'like');
            } else if (e.target.closest('.share-btn')) {
                const postId = e.target.closest('.news-post').dataset.postId;
                this.recordInteraction(postId, 'share');
            } else if (e.target.closest('.comment-btn')) {
                const postId = e.target.closest('.news-post').dataset.postId;
                this.recordInteraction(postId, 'comment');
            } else if (e.target.closest('.bookmark-btn')) {
                const postId = e.target.closest('.news-post').dataset.postId;
                this.recordInteraction(postId, 'bookmark');
            }
        });
    }

    trackScrollBehavior() {
        let scrollDepth = 0;
        let maxScroll = 0;

        window.addEventListener('scroll', () => {
            const scrollPercent = (window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100;
            scrollDepth = Math.max(scrollDepth, scrollPercent);
            
            if (scrollPercent > maxScroll) {
                maxScroll = scrollPercent;
                this.updateEngagementScore(scrollPercent);
            }
        });
    }

    recordView(postId, categories) {
        // Send view data to backend
        fetch('/api/analytics/view', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
            },
            body: JSON.stringify({
                postId: postId,
                categories: categories,
                timestamp: new Date().toISOString(),
                source: 'feed'
            })
        });

        // Update local preferences
        categories.forEach(category => {
            this.preferences[category] = (this.preferences[category] || 0) + 1;
        });
    }

    recordReadingTime(postId, timeSpent) {
        if (timeSpent < 1000) return; // Ignore very short times

        fetch('/api/analytics/reading-time', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
            },
            body: JSON.stringify({
                postId: postId,
                timeSpent: timeSpent,
                timestamp: new Date().toISOString()
            })
        });
    }

    recordInteraction(postId, type) {
        fetch('/api/analytics/interaction', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
            },
            body: JSON.stringify({
                postId: postId,
                interactionType: type,
                timestamp: new Date().toISOString()
            })
        });

        // Higher weight for engagement actions
        const weights = { like: 3, share: 5, comment: 4, bookmark: 2 };
        const weight = weights[type] || 1;
        
        const article = document.querySelector(`[data-post-id="${postId}"]`);
        const categories = article?.dataset.categories?.split(',') || [];
        
        categories.forEach(category => {
            this.preferences[category] = (this.preferences[category] || 0) + weight;
        });
    }

    async generateRecommendations() {
        try {
            const response = await fetch('/api/recommendations/generate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
                },
                body: JSON.stringify({
                    preferences: this.preferences,
                    readingHistory: this.readingHistory.slice(-50), // Last 50 articles
                    userProfile: this.userProfile
                })
            });

            if (response.ok) {
                this.recommendations = await response.json();
                this.displayRecommendations();
            }
        } catch (error) {
            console.error('Error generating recommendations:', error);
        }
    }

    displayRecommendations() {
        const container = document.getElementById('recommendationsContainer');
        if (!container) return;

        container.innerHTML = `
            <div class="recommendations-header">
                <h3><i class="fas fa-robot"></i> AI Recommendations for You</h3>
                <button class="refresh-recommendations" onclick="aiEngine.generateRecommendations()">
                    <i class="fas fa-sync-alt"></i> Refresh
                </button>
            </div>
            
            <div class="recommendations-grid">
                ${this.recommendations.map(article => this.createRecommendationCard(article)).join('')}
            </div>
            
            <div class="recommendation-insights">
                <h4>Why these recommendations?</h4>
                <div class="insights-tags">
                    ${Object.entries(this.preferences)
                        .sort((a, b) => b[1] - a[1])
                        .slice(0, 5)
                        .map(([category, score]) => 
                            `<span class="insight-tag">${category} (${score} interactions)</span>`
                        ).join('')}
                </div>
            </div>
        `;
    }

    createRecommendationCard(article) {
        return `
            <div class="recommendation-card" data-post-id="${article.id}">
                <div class="recommendation-badge">
                    <span class="match-score">${article.matchScore}% match</span>
                    <span class="recommendation-reason">${article.reason}</span>
                </div>
                
                ${article.imageUrl ? `<img src="${article.imageUrl}" alt="${article.title}" class="recommendation-image">` : ''}
                
                <div class="recommendation-content">
                    <h5>${article.title}</h5>
                    <p>${article.summary}</p>
                    
                    <div class="recommendation-meta">
                        <span class="category">${article.category}</span>
                        <span class="reading-time">${article.readingTime} min read</span>
                        <span class="popularity">🔥 ${article.popularity}</span>
                    </div>
                    
                    <div class="recommendation-actions">
                        <button onclick="readArticle(${article.id})" class="read-btn">Read Article</button>
                        <button onclick="saveForLater(${article.id})" class="save-btn">Save for Later</button>
                        <button onclick="hideRecommendation(${article.id})" class="hide-btn">Not Interested</button>
                    </div>
                </div>
            </div>
        `;
    }

    setupPersonalizationUI() {
        const personalizationPanel = document.createElement('div');
        personalizationPanel.className = 'personalization-panel';
        personalizationPanel.innerHTML = `
            <div class="personalization-content">
                <h3>Personalize Your Feed</h3>
                
                <div class="interest-categories">
                    <h4>Your Interest Categories</h4>
                    <div class="categories-grid">
                        ${this.getAvailableCategories().map(category => `
                            <label class="category-option">
                                <input type="checkbox" value="${category}" 
                                       ${this.preferences[category] ? 'checked' : ''}>
                                <span class="category-label">${category}</span>
                                <span class="interest-level">${this.preferences[category] || 0}</span>
                            </label>
                        `).join('')}
                    </div>
                </div>
                
                <div class="reading-preferences">
                    <h4>Reading Preferences</h4>
                    <label>
                        Preferred Article Length:
                        <select id="articleLengthPref">
                            <option value="short">Short (1-3 min)</option>
                            <option value="medium" selected>Medium (3-7 min)</option>
                            <option value="long">Long (7+ min)</option>
                            <option value="any">Any Length</option>
                        </select>
                    </label>
                    
                    <label>
                        Update Frequency:
                        <select id="updateFrequencyPref">
                            <option value="realtime">Real-time</option>
                            <option value="hourly" selected>Hourly</option>
                            <option value="daily">Daily</option>
                        </select>
                    </label>
                </div>
                
                <div class="ai-insights">
                    <h4>AI Insights About You</h4>
                    <div class="insights-list">
                        ${this.generateUserInsights().map(insight => `
                            <div class="insight-item">
                                <i class="fas ${insight.icon}"></i>
                                <span>${insight.text}</span>
                            </div>
                        `).join('')}
                    </div>
                </div>
                
                <button onclick="aiEngine.savePersonalizationSettings()" class="save-settings-btn">
                    Save Preferences
                </button>
            </div>
        `;

        // Add to page (or show in modal)
        document.body.appendChild(personalizationPanel);
    }

    setupContentInsights() {
        // Add insights to articles
        document.querySelectorAll('.news-post').forEach(article => {
            const insights = this.generateArticleInsights(article);
            if (insights.length > 0) {
                const insightsContainer = document.createElement('div');
                insightsContainer.className = 'article-insights';
                insightsContainer.innerHTML = `
                    <div class="insights-header">
                        <i class="fas fa-lightbulb"></i>
                        <span>AI Insights</span>
                    </div>
                    <div class="insights-content">
                        ${insights.map(insight => `<span class="insight">${insight}</span>`).join('')}
                    </div>
                `;
                
                article.appendChild(insightsContainer);
            }
        });
    }

    generateUserInsights() {
        const insights = [];
        
        // Analyze reading patterns
        const topCategories = Object.entries(this.preferences)
            .sort((a, b) => b[1] - a[1])
            .slice(0, 3);
            
        if (topCategories.length > 0) {
            insights.push({
                icon: 'fa-chart-pie',
                text: `You're most interested in ${topCategories.map(([cat]) => cat).join(', ')}`
            });
        }
        
        // Reading time analysis
        const avgReadingTime = this.calculateAverageReadingTime();
        if (avgReadingTime > 0) {
            insights.push({
                icon: 'fa-clock',
                text: `You spend an average of ${avgReadingTime} minutes reading articles`
            });
        }
        
        // Engagement pattern
        const engagementLevel = this.calculateEngagementLevel();
        insights.push({
            icon: 'fa-heart',
            text: `Your engagement level: ${engagementLevel}`
        });
        
        return insights;
    }

    generateArticleInsights(article) {
        const insights = [];
        const postId = article.dataset.postId;
        const categories = article.dataset.categories?.split(',') || [];
        
        // Check if article matches user interests
        const matchingCategories = categories.filter(cat => this.preferences[cat] > 0);
        if (matchingCategories.length > 0) {
            insights.push(`Matches your interest in ${matchingCategories.join(', ')}`);
        }
        
        // Trending indicator
        if (article.dataset.trending === 'true') {
            insights.push('🔥 Trending now');
        }
        
        // Reading time recommendation
        const readingTime = parseInt(article.dataset.readingTime);
        if (readingTime <= 3) {
            insights.push('⚡ Quick read');
        } else if (readingTime >= 10) {
            insights.push('📚 In-depth article');
        }
        
        return insights;
    }

    getAvailableCategories() {
        return ['Politics', 'Technology', 'Sports', 'Entertainment', 'Health', 'Business', 'Science', 'World News', 'Local News'];
    }

    calculateAverageReadingTime() {
        // Implementation based on reading history
        return 5; // Placeholder
    }

    calculateEngagementLevel() {
        const totalInteractions = Object.values(this.preferences).reduce((a, b) => a + b, 0);
        if (totalInteractions < 10) return 'Getting Started';
        if (totalInteractions < 50) return 'Active Reader';
        if (totalInteractions < 100) return 'Engaged User';
        return 'Power User';
    }

    async savePersonalizationSettings() {
        const preferences = {};
        document.querySelectorAll('.category-option input').forEach(input => {
            preferences[input.value] = input.checked ? 1 : 0;
        });

        try {
            await fetch('/api/user/preferences', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
                },
                body: JSON.stringify({
                    categories: preferences,
                    articleLength: document.getElementById('articleLengthPref').value,
                    updateFrequency: document.getElementById('updateFrequencyPref').value
                })
            });

            this.showToast('Preferences saved successfully!', 'success');
            await this.generateRecommendations();
        } catch (error) {
            this.showToast('Error saving preferences', 'error');
        }
    }

    showToast(message, type) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        document.body.appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 3000);
    }
}

// Additional AI features
async function readArticle(articleId) {
    // Track that user clicked on recommendation
    await fetch('/api/analytics/recommendation-click', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
        },
        body: JSON.stringify({ articleId, source: 'ai-recommendation' })
    });

    // Navigate to article
    window.location.href = `/post/${articleId}`;
}

async function saveForLater(articleId) {
    try {
        await fetch(`/api/articles/${articleId}/save`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
            }
        });
        
        aiEngine.showToast('Article saved for later', 'success');
    } catch (error) {
        aiEngine.showToast('Error saving article', 'error');
    }
}

function hideRecommendation(articleId) {
    const card = document.querySelector(`[data-post-id="${articleId}"]`);
    if (card) {
        card.style.opacity = '0';
        setTimeout(() => card.remove(), 300);
    }
    
    // Send feedback to improve future recommendations
    fetch('/api/recommendations/feedback', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
        },
        body: JSON.stringify({ articleId, feedback: 'not_interested' })
    });
}

// Initialize AI recommendation engine
let aiEngine;
document.addEventListener('DOMContentLoaded', () => {
    aiEngine = new AIRecommendationEngine();
});
