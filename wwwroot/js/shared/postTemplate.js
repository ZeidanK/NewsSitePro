/**
 * Unified Post Template System
 * This module provides a consistent post rendering system across all pages
 */

class PostTemplate {
    constructor(options = {}) {
        this.options = {
            showLikeButton: true,
            showShareButton: true,
            showSaveButton: true,
            showReportButton: true,
            showCommentButton: false,
            showEditButton: false,
            showDeleteButton: false,
            showReadMoreButton: false,
            isOwnPost: false,
            isProfileView: false,
            enableInteractions: true,
            ...options
        };
    }

    /**
     * Create a unified post element
     * @param {Object} post - The post data object
     * @param {Object} customOptions - Override default options for this specific post
     * @returns {HTMLElement} - The created post element
     */
    createPostElement(post, customOptions = {}) {
        const options = { ...this.options, ...customOptions };
        
        const postBlock = document.createElement('div');
        postBlock.className = 'post-card unified-post';
        postBlock.setAttribute('data-post-id', post.articleID || post.ArticleID);
        
        // Determine if this is a live/API post
        const isLivePost = post.isLivePost || post.sourceName === 'NewsBot' || post.user?.username === 'NewsBot';
        
        if (isLivePost) {
            postBlock.classList.add('live-post');
        }

        postBlock.innerHTML = this.generatePostHTML(post, options, isLivePost);
        
        // Add event listeners
        this.attachEventListeners(postBlock, post, options);
        
        // Add animation
        postBlock.classList.add('new-post');
        
        return postBlock;
    }

    /**
     * Generate the HTML content for a post
     */
    generatePostHTML(post, options, isLivePost) {
        const postId = post.articleID || post.ArticleID;
        const user = post.user || { username: post.username || 'Unknown User' };
        const publishDate = post.publishDate || post.PublishDate || new Date().toISOString();
        
        return `
            ${isLivePost ? this.generateLiveIndicator(post) : ''}
            
            <div class="post-header">
                <div class="post-user">
                    <div class="user-avatar ${isLivePost ? 'newsbot-avatar' : ''}">
                        ${isLivePost ? '🤖' : (user.username?.charAt(0).toUpperCase() || 'U')}
                    </div>
                    <div class="user-info">
                        ${options.isProfileView ? 
                            `<span class="username">${user.username}</span>` : 
                            `<a href="/userprofile/${user.userId || user.UserID || postId}" class="username">${user.username}</a>`
                        }
                        <span class="post-time">${this.formatTimeAgo(publishDate)}</span>
                        ${isLivePost ? `<span class="live-source">Live from ${post.sourceName || post.SourceName}</span>` : ''}
                    </div>
                </div>
                <div class="post-category">
                    <span class="category-badge ${isLivePost ? 'live' : ''}">${post.category || post.Category}</span>
                </div>
                ${options.showEditButton || options.showDeleteButton ? this.generateOwnerActions(postId, options) : ''}
            </div>
            
            ${this.generatePostImage(post)}
            
            <div class="post-content">
                <h3 class="post-title">${post.title || post.Title}</h3>
                <p class="post-text">${post.content || post.Content}</p>
            </div>
            
            ${this.generatePostSource(post)}
            
            <div class="post-footer">
                ${this.generatePostStats(post, postId)}
                ${this.generatePostActions(post, postId, options, isLivePost)}
            </div>
        `;
    }

    /**
     * Generate live post indicator
     */
    generateLiveIndicator(post) {
        return `
            <div class="live-indicator">
                <span class="live-badge">🔴 LIVE</span>
                <span class="source-time">From ${post.sourceName || post.SourceName} • ${this.formatTimeAgo(post.publishDate || post.PublishDate)}</span>
            </div>
        `;
    }

    /**
     * Generate owner actions (edit/delete)
     */
    generateOwnerActions(postId, options) {
        if (!options.showEditButton && !options.showDeleteButton) return '';
        
        return `
            <div class="post-owner-actions">
                ${options.showEditButton ? `
                    <button class="btn btn-sm btn-outline-primary edit-post" data-post-id="${postId}">
                        <i class="fas fa-edit"></i>
                    </button>
                ` : ''}
                ${options.showDeleteButton ? `
                    <button class="btn btn-sm btn-outline-danger delete-post" data-post-id="${postId}">
                        <i class="fas fa-trash"></i>
                    </button>
                ` : ''}
            </div>
        `;
    }

    /**
     * Generate post image section
     */
    generatePostImage(post) {
        const imageUrl = post.imageURL || post.ImageURL;
        if (!imageUrl) return '';
        
        return `
            <div class="post-image">
                <img src="${imageUrl}" alt="${post.title || post.Title}" loading="lazy" />
            </div>
        `;
    }

    /**
     * Generate post source section
     */
    generatePostSource(post) {
        const sourceUrl = post.sourceURL || post.SourceURL;
        if (!sourceUrl) return '';
        
        const sourceName = post.sourceName || post.SourceName || 'External Link';
        
        return `
            <div class="post-source">
                <i class="fas fa-external-link-alt"></i>
                <span>Source: <a href="${sourceUrl}" target="_blank" rel="noopener">${sourceName}</a></span>
            </div>
        `;
    }

    /**
     * Generate post statistics section
     */
    generatePostStats(post, postId) {
        const views = post.views || post.ViewsCount || Math.floor(Math.random() * 1000);
        const likes = post.likes || post.LikesCount || 0;
        const comments = post.comments || post.CommentsCount || 0;
        
        return `
            <div class="post-stats">
                <span class="stat-item">
                    <span class="stat-icon">👁️</span>
                    <span class="stat-count">${views}</span>
                </span>
                <span class="stat-item">
                    <span class="stat-icon">❤️</span>
                    <span class="stat-count" id="likes-${postId}">${likes}</span>
                </span>
                <span class="stat-item">
                    <span class="stat-icon">💬</span>
                    <span class="stat-count">${comments}</span>
                </span>
            </div>
        `;
    }

    /**
     * Generate post actions section
     */
    generatePostActions(post, postId, options, isLivePost) {
        if (!options.enableInteractions) return '';
        
        const isLiked = post.isLiked || false;
        
        let actions = '<div class="post-actions">';
        
        if (options.showLikeButton && !isLivePost) {
            actions += `
                <button class="btn btn-success ${isLiked ? 'liked' : ''}" onclick="window.postInteractions.likePost(${postId})" id="like-btn-${postId}">
                    <span class="btn-icon">${isLiked ? '❤️' : '🤍'}</span>
                    <span class="btn-text">Like</span>
                </button>
            `;
        }
        
        if (options.showShareButton) {
            actions += `
                <button class="btn btn-primary" onclick="window.postInteractions.sharePost(${postId}, '${(post.title || post.Title).replace(/'/g, "\\'")}', '${post.sourceURL || ''}')">
                    <span class="btn-icon">🔄</span>
                    <span class="btn-text">Share</span>
                </button>
            `;
        }
        
        if (options.showSaveButton) {
            if (isLivePost) {
                actions += `
                    <button class="btn btn-info" onclick="window.postInteractions.saveLiveArticle(${JSON.stringify(post).replace(/"/g, '&quot;')})">
                        <span class="btn-icon">💾</span>
                        <span class="btn-text">Save to Feed</span>
                    </button>
                `;
            } else {
                actions += `
                    <button class="btn btn-secondary" onclick="window.postInteractions.savePost(${postId})">
                        <span class="btn-icon">🔖</span>
                        <span class="btn-text">Save</span>
                    </button>
                `;
            }
        }
        
        if (options.showCommentButton) {
            actions += `
                <button class="btn btn-info comment-post" data-post-id="${postId}">
                    <span class="btn-icon">💬</span>
                    <span class="btn-text">Comment</span>
                </button>
            `;
        }
        
        if (options.showReadMoreButton) {
            if (isLivePost && (post.sourceURL || post.SourceURL)) {
                actions += `
                    <button class="btn btn-info" onclick="window.open('${post.sourceURL || post.SourceURL}', '_blank')">
                        <span class="btn-icon">📖</span>
                        <span class="btn-text">Read Full</span>
                    </button>
                `;
            } else {
                actions += `
                    <a href="/Post/${postId}" class="btn btn-outline-primary">
                        <span class="btn-icon">📖</span>
                        <span class="btn-text">Read More</span>
                    </a>
                `;
            }
        }
        
        if (options.showReportButton && !isLivePost && !options.isOwnPost) {
            actions += `
                <button class="btn btn-danger" onclick="window.postInteractions.reportPost(${postId})">
                    <span class="btn-icon">⚠️</span>
                    <span class="btn-text">Report</span>
                </button>
            `;
        }
        
        actions += '</div>';
        
        return actions;
    }

    /**
     * Attach event listeners to the post element
     */
    attachEventListeners(postElement, post, options) {
        // Edit button
        const editBtn = postElement.querySelector('.edit-post');
        if (editBtn) {
            editBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.handleEditPost(post.articleID || post.ArticleID);
            });
        }
        
        // Delete button
        const deleteBtn = postElement.querySelector('.delete-post');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.handleDeletePost(post.articleID || post.ArticleID);
            });
        }
        
        // Comment button
        const commentBtn = postElement.querySelector('.comment-post');
        if (commentBtn) {
            commentBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.handleCommentPost(post.articleID || post.ArticleID);
            });
        }
    }

    /**
     * Handle edit post action
     */
    async handleEditPost(postId) {
        if (window.editPost && typeof window.editPost === 'function') {
            window.editPost(postId);
        } else {
            console.log('Edit post:', postId);
            // Default implementation or redirect to edit page
        }
    }

    /**
     * Handle delete post action
     */
    async handleDeletePost(postId) {
        if (!confirm('Are you sure you want to delete this post?')) return;
        
        if (window.deletePost && typeof window.deletePost === 'function') {
            window.deletePost(postId);
        } else {
            console.log('Delete post:', postId);
            // Default implementation
        }
    }

    /**
     * Handle comment post action
     */
    handleCommentPost(postId) {
        if (window.commentPost && typeof window.commentPost === 'function') {
            window.commentPost(postId);
        } else {
            console.log('Comment on post:', postId);
            // Default implementation or redirect to post detail
        }
    }

    /**
     * Format time ago utility
     */
    formatTimeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffInSeconds = Math.floor((now - date) / 1000);
        
        if (diffInSeconds < 60) return 'Just now';
        if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m`;
        if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h`;
        if (diffInSeconds < 604800) return `${Math.floor(diffInSeconds / 86400)}d`;
        
        return date.toLocaleDateString();
    }

    /**
     * Update post stats (likes, views, etc.)
     */
    updatePostStats(postId, stats) {
        const likesElement = document.getElementById(`likes-${postId}`);
        if (likesElement && stats.likes !== undefined) {
            likesElement.textContent = stats.likes;
        }
        
        // Update other stats as needed
    }

    /**
     * Toggle like button state
     */
    toggleLikeButton(postId, isLiked, newCount) {
        const likeBtn = document.getElementById(`like-btn-${postId}`);
        if (likeBtn) {
            const icon = likeBtn.querySelector('.btn-icon');
            const likesCount = document.getElementById(`likes-${postId}`);
            
            if (isLiked) {
                likeBtn.classList.add('liked');
                icon.textContent = '❤️';
            } else {
                likeBtn.classList.remove('liked');
                icon.textContent = '🤍';
            }
            
            if (likesCount && newCount !== undefined) {
                likesCount.textContent = newCount;
            }
        }
    }
}

// Create global instances for different contexts
window.PostTemplate = PostTemplate;

// Default instances
window.postTemplates = {
    feed: new PostTemplate({
        showLikeButton: true,
        showShareButton: true,
        showSaveButton: true,
        showReportButton: true,
        showCommentButton: false,
        enableInteractions: true
    }),
    
    profile: new PostTemplate({
        showLikeButton: false,
        showShareButton: false,
        showSaveButton: false,
        showReportButton: false,
        showCommentButton: true,
        showEditButton: true,
        showDeleteButton: true,
        showReadMoreButton: true,
        isProfileView: true,
        enableInteractions: true
    }),
    
    ownProfile: new PostTemplate({
        showLikeButton: false,
        showShareButton: true,
        showSaveButton: false,
        showReportButton: false,
        showCommentButton: true,
        showEditButton: true,
        showDeleteButton: true,
        showReadMoreButton: true,
        isProfileView: true,
        isOwnPost: true,
        enableInteractions: true
    }),
    
    liveNews: new PostTemplate({
        showLikeButton: false,
        showShareButton: true,
        showSaveButton: true,
        showReportButton: false,
        showCommentButton: false,
        showReadMoreButton: true,
        enableInteractions: true
    })
};
