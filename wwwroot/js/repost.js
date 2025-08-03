/**
 * repost.js
 * 
 * JavaScript functions for repost functionality.
 * Handles creating reposts, toggling repost likes, and managing repost comments.
 * Integrates with RepostController and NotificationManager.
 * 
 * Author: System
 * Date: 2025-01-27
 */

// Repost management functions
window.RepostManager = {
    
    // Configuration
    config: {
        apiBaseUrl: '/Repost',
        maxRepostTextLength: 500
    },

    // Initialize repost functionality
    init: function() {
        console.log('[RepostManager] Initializing...');
        this.bindEvents();
    },

    // Bind event handlers
    bindEvents: function() {
        // Repost buttons
        document.addEventListener('click', (e) => {
            if (e.target.matches('.repost-btn') || e.target.closest('.repost-btn')) {
                e.preventDefault();
                const button = e.target.closest('.repost-btn');
                const articleId = button.getAttribute('data-article-id');
                this.showRepostModal(articleId);
            }
        });

        // Repost like buttons
        document.addEventListener('click', (e) => {
            if (e.target.matches('.repost-like-btn') || e.target.closest('.repost-like-btn')) {
                e.preventDefault();
                const button = e.target.closest('.repost-like-btn');
                const repostId = button.getAttribute('data-repost-id');
                this.toggleRepostLike(repostId);
            }
        });

        // Repost comment forms
        document.addEventListener('submit', (e) => {
            if (e.target.matches('.repost-comment-form')) {
                e.preventDefault();
                this.submitRepostComment(e.target);
            }
        });
    },

    // Show repost modal
    showRepostModal: function(articleId) {
        if (!window.authService || !window.authService.isAuthenticated()) {
            window.showToast('Please log in to repost articles', 'warning');
            return;
        }

        const modalHtml = `
            <div id="repostModal" class="modal fade show" style="display: block;">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Repost Article</h5>
                            <button type="button" class="btn-close" onclick="RepostManager.closeRepostModal()"></button>
                        </div>
                        <div class="modal-body">
                            <form id="repostForm">
                                <div class="mb-3">
                                    <label for="repostText" class="form-label">Add your thoughts (optional)</label>
                                    <textarea id="repostText" class="form-control" rows="3" 
                                              maxlength="${this.config.maxRepostTextLength}"
                                              placeholder="What do you think about this article?"></textarea>
                                    <div class="form-text">
                                        <span id="charCount">0</span>/${this.config.maxRepostTextLength} characters
                                    </div>
                                </div>
                                <input type="hidden" id="articleId" value="${articleId}">
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" onclick="RepostManager.closeRepostModal()">Cancel</button>
                            <button type="button" class="btn btn-primary" onclick="RepostManager.submitRepost()">Repost</button>
                        </div>
                    </div>
                </div>
            </div>
            <div class="modal-backdrop fade show"></div>
        `;

        document.body.insertAdjacentHTML('beforeend', modalHtml);

        // Character counter
        const textarea = document.getElementById('repostText');
        const charCount = document.getElementById('charCount');
        
        textarea.addEventListener('input', function() {
            charCount.textContent = this.value.length;
        });
    },

    // Close repost modal
    closeRepostModal: function() {
        const modal = document.getElementById('repostModal');
        const backdrop = document.querySelector('.modal-backdrop');
        
        if (modal) modal.remove();
        if (backdrop) backdrop.remove();
    },

    // Submit repost
    submitRepost: async function() {
        const articleId = document.getElementById('articleId').value;
        const repostText = document.getElementById('repostText').value.trim();

        if (!articleId) {
            window.showToast('Invalid article ID', 'error');
            return;
        }

        try {
            const response = await fetch(`${this.config.apiBaseUrl}/Create`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                credentials: 'include',
                body: JSON.stringify({
                    OriginalArticleID: parseInt(articleId),
                    RepostText: repostText || null
                })
            });

            const result = await response.json();
            
            if (result.success) {
                window.showToast('Article reposted successfully!', 'success');
                this.closeRepostModal();
                
                // Update UI
                this.updateRepostButton(articleId, true);
                
                // Update notification count if needed
                if (window.NotificationManager) {
                    setTimeout(() => window.NotificationManager.updateNotificationBadge(), 1000);
                }
            } else {
                window.showToast(result.message || 'Failed to repost article', 'error');
            }
        } catch (error) {
            console.error('Error reposting article:', error);
            window.showToast('Error reposting article', 'error');
        }
    },

    // Toggle repost like
    toggleRepostLike: async function(repostId) {
        if (!window.authService || !window.authService.isAuthenticated()) {
            window.showToast('Please log in to like reposts', 'warning');
            return;
        }

        try {
            const response = await fetch(`${this.config.apiBaseUrl}/ToggleLike`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                credentials: 'include',
                body: `repostId=${repostId}`
            });

            const result = await response.json();
            
            if (result.success) {
                this.updateRepostLikeButton(repostId, result.liked, result.likeCount);
            } else {
                window.showToast(result.message || 'Failed to like repost', 'error');
            }
        } catch (error) {
            console.error('Error toggling repost like:', error);
            window.showToast('Error processing like', 'error');
        }
    },

    // Submit repost comment
    submitRepostComment: async function(form) {
        const formData = new FormData(form);
        const repostId = formData.get('repostId');
        const content = formData.get('content');

        if (!content.trim()) {
            window.showToast('Please enter a comment', 'warning');
            return;
        }

        try {
            const response = await fetch(`${this.config.apiBaseUrl}/AddComment`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                credentials: 'include',
                body: JSON.stringify({
                    RepostID: parseInt(repostId),
                    Content: content
                })
            });

            const result = await response.json();
            
            if (result.success) {
                window.showToast('Comment added successfully!', 'success');
                form.reset();
                
                // Reload comments section or update UI
                setTimeout(() => window.location.reload(), 1000);
            } else {
                window.showToast(result.message || 'Failed to add comment', 'error');
            }
        } catch (error) {
            console.error('Error adding repost comment:', error);
            window.showToast('Error adding comment', 'error');
        }
    },

    // Update repost button UI
    updateRepostButton: function(articleId, isReposted) {
        const buttons = document.querySelectorAll(`[data-article-id="${articleId}"].repost-btn`);
        buttons.forEach(button => {
            if (isReposted) {
                button.classList.add('reposted');
                button.innerHTML = '<i class="fas fa-retweet"></i> Reposted';
                button.disabled = true;
            }
        });
    },

    // Update repost like button UI
    updateRepostLikeButton: function(repostId, isLiked, likeCount) {
        const buttons = document.querySelectorAll(`[data-repost-id="${repostId}"].repost-like-btn`);
        buttons.forEach(button => {
            const icon = button.querySelector('i');
            const countSpan = button.querySelector('.like-count');
            
            if (isLiked) {
                button.classList.add('liked');
                if (icon) icon.className = 'fas fa-heart';
            } else {
                button.classList.remove('liked');
                if (icon) icon.className = 'far fa-heart';
            }
            
            if (countSpan) {
                countSpan.textContent = likeCount;
            }
        });
    },

    // Get anti-forgery token
    getAntiForgeryToken: function() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.RepostManager.init();
});

// CSS for repost functionality
const repostStyles = `
    .repost-btn {
        color: #1da1f2;
        border: 1px solid #1da1f2;
        background: transparent;
        transition: all 0.2s ease;
    }
    
    .repost-btn:hover {
        background: #1da1f2;
        color: white;
    }
    
    .repost-btn.reposted {
        background: #1da1f2;
        color: white;
        cursor: not-allowed;
    }
    
    .repost-like-btn {
        color: #e91e63;
        border: none;
        background: transparent;
        transition: all 0.2s ease;
    }
    
    .repost-like-btn:hover {
        background: rgba(233, 30, 99, 0.1);
    }
    
    .repost-like-btn.liked {
        color: #e91e63;
    }
    
    .repost-like-btn.liked i {
        color: #e91e63 !important;
    }
    
    .repost-modal .modal-content {
        border-radius: 10px;
    }
    
    .repost-modal .modal-header {
        border-bottom: 1px solid #e1e8ed;
        padding: 1rem 1.5rem;
    }
    
    .repost-modal .modal-body {
        padding: 1.5rem;
    }
    
    .repost-modal .modal-footer {
        border-top: 1px solid #e1e8ed;
        padding: 1rem 1.5rem;
    }
    
    .repost-item {
        border: 1px solid #e1e8ed;
        border-radius: 10px;
        margin-bottom: 1rem;
        background: white;
    }
    
    .repost-header {
        display: flex;
        align-items: center;
        padding: 1rem;
        border-bottom: 1px solid #e1e8ed;
    }
    
    .repost-avatar {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        margin-right: 0.75rem;
    }
    
    .repost-user-info h6 {
        margin: 0;
        font-weight: 600;
    }
    
    .repost-user-info small {
        color: #657786;
    }
    
    .repost-content {
        padding: 0 1rem 1rem;
    }
    
    .repost-text {
        margin-bottom: 1rem;
        font-size: 1rem;
        line-height: 1.4;
    }
    
    .repost-original-article {
        border: 1px solid #e1e8ed;
        border-radius: 8px;
        padding: 1rem;
        background: #f8f9fa;
    }
    
    .repost-actions {
        display: flex;
        gap: 1rem;
        padding: 0.75rem 1rem;
        border-top: 1px solid #e1e8ed;
        justify-content: space-around;
    }
    
    .repost-action-btn {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.5rem 1rem;
        border: none;
        background: transparent;
        border-radius: 20px;
        transition: all 0.2s ease;
        cursor: pointer;
    }
    
    .repost-action-btn:hover {
        background: rgba(29, 161, 242, 0.1);
        color: #1da1f2;
    }
`;

// Add styles to document
const styleSheet = document.createElement('style');
styleSheet.textContent = repostStyles;
document.head.appendChild(styleSheet);
