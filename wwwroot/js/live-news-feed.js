// Real-time news feed with WebSocket integration and live updates
class LiveNewsFeed {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.retryCount = 0;
        this.maxRetries = 5;
        this.retryDelay = 1000;
        this.newsContainer = document.getElementById('newsContainer');
        this.init();
    }

    async init() {
        await this.setupSignalR();
        this.setupAutoRefresh();
        this.setupInfiniteScroll();
        this.setupNewsFilters();
        this.setupBreakingNewsAlert();
    }

    async setupSignalR() {
        try {
            // Initialize SignalR connection for real-time updates
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/newshub")
                .withAutomaticReconnect()
                .build();

            this.connection.on("NewPostAdded", (post) => {
                this.addNewPostToFeed(post);
                this.showNewPostNotification(post);
            });

            this.connection.on("PostUpdated", (post) => {
                this.updatePostInFeed(post);
            });

            this.connection.on("BreakingNews", (news) => {
                this.showBreakingNewsAlert(news);
            });

            await this.connection.start();
            this.isConnected = true;
            console.log("Real-time news feed connected");
        } catch (error) {
            console.error("SignalR connection failed:", error);
            this.fallbackToPolling();
        }
    }

    fallbackToPolling() {
        // Fallback to polling if SignalR fails
        setInterval(() => {
            this.checkForNewPosts();
        }, 30000); // Check every 30 seconds
    }

    async checkForNewPosts() {
        try {
            const lastPostId = this.getLastPostId();
            const response = await fetch(`/api/news/new?since=${lastPostId}`);
            if (response.ok) {
                const newPosts = await response.json();
                newPosts.forEach(post => this.addNewPostToFeed(post));
            }
        } catch (error) {
            console.error("Error checking for new posts:", error);
        }
    }

    addNewPostToFeed(post) {
        const postElement = this.createPostElement(post);
        postElement.style.opacity = '0';
        postElement.style.transform = 'translateY(-20px)';
        
        if (this.newsContainer.firstChild) {
            this.newsContainer.insertBefore(postElement, this.newsContainer.firstChild);
        } else {
            this.newsContainer.appendChild(postElement);
        }

        // Smooth animation
        requestAnimationFrame(() => {
            postElement.style.transition = 'all 0.5s ease-out';
            postElement.style.opacity = '1';
            postElement.style.transform = 'translateY(0)';
        });

        // Add "NEW" badge
        const newBadge = document.createElement('span');
        newBadge.className = 'new-post-badge';
        newBadge.textContent = 'NEW';
        postElement.querySelector('.post-header').appendChild(newBadge);

        // Remove badge after 10 seconds
        setTimeout(() => {
            if (newBadge.parentNode) {
                newBadge.remove();
            }
        }, 10000);
    }

    createPostElement(post) {
        const postDiv = document.createElement('div');
        postDiv.className = 'news-post';
        postDiv.setAttribute('data-post-id', post.id);
        
        postDiv.innerHTML = `
            <div class="post-header">
                <div class="author-info">
                    <img src="${post.authorAvatar || '/images/default-avatar.png'}" alt="${post.authorName}" class="author-avatar">
                    <div class="author-details">
                        <h6>${post.authorName}</h6>
                        <span class="post-time">${this.formatTime(post.createdAt)}</span>
                        ${post.source ? `<span class="source-badge">${post.source}</span>` : ''}
                    </div>
                </div>
                <div class="post-actions">
                    <button class="action-btn bookmark-btn" onclick="toggleBookmark(${post.id})">
                        <i class="far fa-bookmark"></i>
                    </button>
                    <button class="action-btn share-btn" onclick="sharePost(${post.id})">
                        <i class="fas fa-share"></i>
                    </button>
                </div>
            </div>
            
            <div class="post-content">
                <h4 class="post-title">${post.title}</h4>
                <p class="post-summary">${post.summary || post.content.substring(0, 200) + '...'}</p>
                ${post.imageUrl ? `<img src="${post.imageUrl}" alt="${post.title}" class="post-image" loading="lazy">` : ''}
                
                <div class="post-categories">
                    ${post.categories?.map(cat => `<span class="category-tag">${cat}</span>`).join('') || ''}
                </div>
            </div>
            
            <div class="post-footer">
                <div class="engagement-stats">
                    <button class="engagement-btn like-btn ${post.userLiked ? 'liked' : ''}" onclick="handlePostLike(${post.id})">
                        <i class="fas fa-heart"></i>
                        <span class="count">${post.likesCount || 0}</span>
                    </button>
                    <button class="engagement-btn comment-btn" onclick="toggleCommentSection(${post.id})">
                        <i class="fas fa-comment"></i>
                        <span class="nav-text">Comments (${post.commentsCount || 0})</span>
                    </button>
                    <button class="engagement-btn view-btn" onclick="viewFullPost(${post.id})">
                        <i class="fas fa-eye"></i>
                        <span class="count">${post.viewsCount || 0}</span>
                    </button>
                </div>
                
                <div class="reading-time">
                    <i class="fas fa-clock"></i>
                    <span>${this.calculateReadingTime(post.content)} min read</span>
                </div>
            </div>
            
            <div class="comments-section" style="display: none;">
                <div class="comments-container">
                    <!-- Comments will be loaded here dynamically -->
                </div>
            </div>
        `;
        
        return postDiv;
    }

    showNewPostNotification(post) {
        // Create floating notification for new posts
        const notification = document.createElement('div');
        notification.className = 'new-post-notification';
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-newspaper"></i>
                <div class="notification-text">
                    <strong>New Post!</strong>
                    <p>${post.title}</p>
                </div>
                <button onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    }

    showBreakingNewsAlert(news) {
        // Create breaking news banner
        const alertBanner = document.createElement('div');
        alertBanner.className = 'breaking-news-alert';
        alertBanner.innerHTML = `
            <div class="breaking-news-content">
                <span class="breaking-label">🚨 BREAKING</span>
                <span class="breaking-text">${news.title}</span>
                <button class="view-breaking-btn" onclick="viewFullPost(${news.id})">Read More</button>
                <button class="close-breaking-btn" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;
        
        // Insert at top of page
        document.body.insertBefore(alertBanner, document.body.firstChild);
        
        // Auto-dismiss after 30 seconds
        setTimeout(() => {
            if (alertBanner.parentNode) {
                alertBanner.style.transform = 'translateY(-100%)';
                setTimeout(() => alertBanner.remove(), 300);
            }
        }, 30000);
    }

    setupInfiniteScroll() {
        let loading = false;
        let hasMore = true;
        let page = 1;

        const loadMorePosts = async () => {
            if (loading || !hasMore) return;
            
            loading = true;
            this.showLoadingSpinner();
            
            try {
                const response = await fetch(`/api/news/feed?page=${page}&pageSize=10`);
                if (response.ok) {
                    const data = await response.json();
                    if (data.posts.length > 0) {
                        data.posts.forEach(post => {
                            const postElement = this.createPostElement(post);
                            this.newsContainer.appendChild(postElement);
                        });
                        page++;
                        hasMore = data.hasMore;
                    } else {
                        hasMore = false;
                    }
                }
            } catch (error) {
                console.error("Error loading more posts:", error);
            } finally {
                loading = false;
                this.hideLoadingSpinner();
            }
        };

        // Intersection Observer for infinite scroll
        const observer = new IntersectionObserver((entries) => {
            if (entries[0].isIntersecting) {
                loadMorePosts();
            }
        }, { threshold: 0.1 });

        // Create scroll trigger element
        const scrollTrigger = document.createElement('div');
        scrollTrigger.className = 'scroll-trigger';
        scrollTrigger.style.height = '50px';
        this.newsContainer.appendChild(scrollTrigger);
        observer.observe(scrollTrigger);
    }

    setupNewsFilters() {
        const filterContainer = document.createElement('div');
        filterContainer.className = 'news-filters';
        filterContainer.innerHTML = `
            <div class="filter-tabs">
                <button class="filter-tab active" data-filter="all">All News</button>
                <button class="filter-tab" data-filter="trending">🔥 Trending</button>
                <button class="filter-tab" data-filter="breaking">🚨 Breaking</button>
                <button class="filter-tab" data-filter="politics">🏛️ Politics</button>
                <button class="filter-tab" data-filter="technology">💻 Technology</button>
                <button class="filter-tab" data-filter="sports">⚽ Sports</button>
                <button class="filter-tab" data-filter="entertainment">🎬 Entertainment</button>
            </div>
            
            <div class="filter-options">
                <select class="sort-select">
                    <option value="latest">Latest First</option>
                    <option value="popular">Most Popular</option>
                    <option value="trending">Trending</option>
                    <option value="oldest">Oldest First</option>
                </select>
                
                <button class="personalize-btn" onclick="openPersonalizationModal()">
                    <i class="fas fa-user-cog"></i>
                    Personalize Feed
                </button>
            </div>
        `;

        this.newsContainer.parentNode.insertBefore(filterContainer, this.newsContainer);

        // Add filter event listeners
        filterContainer.querySelectorAll('.filter-tab').forEach(tab => {
            tab.addEventListener('click', (e) => {
                this.applyFilter(e.target.dataset.filter);
                filterContainer.querySelectorAll('.filter-tab').forEach(t => t.classList.remove('active'));
                e.target.classList.add('active');
            });
        });

        filterContainer.querySelector('.sort-select').addEventListener('change', (e) => {
            this.applySorting(e.target.value);
        });
    }

    async applyFilter(filter) {
        try {
            this.showLoadingOverlay();
            const response = await fetch(`/api/news/feed?filter=${filter}&page=1&pageSize=20`);
            if (response.ok) {
                const data = await response.json();
                this.newsContainer.innerHTML = '';
                data.posts.forEach(post => {
                    const postElement = this.createPostElement(post);
                    this.newsContainer.appendChild(postElement);
                });
            }
        } catch (error) {
            console.error("Error applying filter:", error);
        } finally {
            this.hideLoadingOverlay();
        }
    }

    formatTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diff = now - date;
        
        if (diff < 60000) return 'Just now';
        if (diff < 3600000) return `${Math.floor(diff / 60000)}m ago`;
        if (diff < 86400000) return `${Math.floor(diff / 3600000)}h ago`;
        return `${Math.floor(diff / 86400000)}d ago`;
    }

    calculateReadingTime(content) {
        const wordsPerMinute = 200;
        const wordCount = content.split(' ').length;
        return Math.ceil(wordCount / wordsPerMinute);
    }

    getLastPostId() {
        const posts = this.newsContainer.querySelectorAll('.news-post');
        return posts.length > 0 ? posts[0].dataset.postId : 0;
    }

    showLoadingSpinner() {
        const spinner = document.createElement('div');
        spinner.className = 'loading-spinner';
        spinner.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Loading more posts...';
        this.newsContainer.appendChild(spinner);
    }

    hideLoadingSpinner() {
        const spinner = this.newsContainer.querySelector('.loading-spinner');
        if (spinner) spinner.remove();
    }

    showLoadingOverlay() {
        const overlay = document.createElement('div');
        overlay.className = 'loading-overlay';
        overlay.innerHTML = '<div class="loading-content"><i class="fas fa-spinner fa-spin"></i><p>Loading news...</p></div>';
        this.newsContainer.appendChild(overlay);
    }

    hideLoadingOverlay() {
        const overlay = this.newsContainer.querySelector('.loading-overlay');
        if (overlay) overlay.remove();
    }
}

// Advanced engagement features
async function toggleLike(postId) {
    try {
        const response = await fetch(`/api/posts/${postId}/like`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                'Content-Type': 'application/json'
            }
        });
        
        if (response.ok) {
            const data = await response.json();
            const likeBtn = document.querySelector(`[data-post-id="${postId}"] .like-btn`);
            likeBtn.classList.toggle('liked', data.liked);
            likeBtn.querySelector('.count').textContent = data.likesCount;
        }
    } catch (error) {
        console.error('Error toggling like:', error);
    }
}

async function sharePost(postId) {
    const post = await getPostData(postId);
    
    if (navigator.share) {
        // Use native sharing API if available
        navigator.share({
            title: post.title,
            text: post.summary,
            url: `${window.location.origin}/post/${postId}`
        });
    } else {
        // Fallback to custom share modal
        showShareModal(post);
    }
}

function showShareModal(post) {
    const modal = document.createElement('div');
    modal.className = 'share-modal';
    modal.innerHTML = `
        <div class="share-modal-content">
            <h3>Share this post</h3>
            <div class="share-options">
                <button onclick="shareToTwitter('${post.title}', '${post.url}')">
                    <i class="fab fa-twitter"></i> Twitter
                </button>
                <button onclick="shareToFacebook('${post.url}')">
                    <i class="fab fa-facebook"></i> Facebook
                </button>
                <button onclick="shareToLinkedIn('${post.title}', '${post.url}')">
                    <i class="fab fa-linkedin"></i> LinkedIn
                </button>
                <button onclick="copyToClipboard('${post.url}')">
                    <i class="fas fa-copy"></i> Copy Link
                </button>
            </div>
            <button class="close-modal" onclick="this.parentElement.parentElement.remove()">×</button>
        </div>
    `;
    
    document.body.appendChild(modal);
}

// Enhanced like functionality with authentication check
async function handlePostLike(postId) {
    const token = localStorage.getItem('jwtToken');
    if (!token) {
        if (window.commentsManager) {
            window.commentsManager.showLoginPrompt('like this post');
        } else {
            alert('Please login to like posts');
        }
        return;
    }

    try {
        // This would connect to your existing like API
        const response = await fetch(`/api/Posts/${postId}/like`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const result = await response.json();
            // Update the like button state
            const likeBtn = document.querySelector(`[data-post-id="${postId}"] .like-btn`);
            const countSpan = likeBtn.querySelector('.count');
            
            if (result.liked) {
                likeBtn.classList.add('liked');
                countSpan.textContent = parseInt(countSpan.textContent) + 1;
            } else {
                likeBtn.classList.remove('liked');
                countSpan.textContent = parseInt(countSpan.textContent) - 1;
            }
        }
    } catch (error) {
        console.error('Error liking post:', error);
    }
}

// Toggle comment section visibility
function toggleCommentSection(postId) {
    const post = document.querySelector(`[data-post-id="${postId}"]`);
    const commentsSection = post.querySelector('.comments-section');
    const commentBtn = post.querySelector('.comment-btn .nav-text');
    
    if (commentsSection.style.display === 'none' || !commentsSection.style.display) {
        commentsSection.style.display = 'block';
        commentBtn.textContent = commentBtn.textContent.replace('Comments', 'Hide Comments');
        
        // Load comments if not already loaded
        if (window.commentsManager) {
            window.commentsManager.currentPostId = postId;
            window.commentsManager.loadComments(postId);
        }
    } else {
        commentsSection.style.display = 'none';
        commentBtn.textContent = commentBtn.textContent.replace('Hide Comments', 'Comments');
    }
}

// Initialize live news feed
document.addEventListener('DOMContentLoaded', () => {
    new LiveNewsFeed();
});
