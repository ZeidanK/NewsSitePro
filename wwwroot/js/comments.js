// Enhanced Comments System with Guest User Restrictions
class CommentsManager {
    constructor() {
        this.currentPostId = null;
        this.isAuthenticated = false;
        this.init();
    }

    init() {
        this.checkAuthStatus();
        this.setupEventListeners();
        this.loadCommentsForCurrentPost();
    }

    async checkAuthStatus() {
        const token = localStorage.getItem('jwtToken');
        if (token) {
            try {
                const response = await fetch('/api/Auth/validate', {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });
                this.isAuthenticated = response.ok;
            } catch (error) {
                this.isAuthenticated = false;
            }
        } else {
            this.isAuthenticated = false;
        }
    }

    setupEventListeners() {
        // Comment form submission
        document.addEventListener('submit', (e) => {
            if (e.target.classList.contains('comment-form')) {
                e.preventDefault();
                this.handleCommentSubmit(e.target);
            }
        });

        // Like button clicks (with auth check)
        document.addEventListener('click', (e) => {
            if (e.target.closest('.like-btn')) {
                e.preventDefault();
                if (!this.isAuthenticated) {
                    this.showLoginPrompt('like this post');
                    return;
                }
                this.handleLike(e.target.closest('.like-btn'));
            }

            // Comment button clicks
            if (e.target.closest('.comment-btn')) {
                e.preventDefault();
                if (!this.isAuthenticated) {
                    this.showLoginPrompt('comment on this post');
                    return;
                }
                this.toggleCommentSection(e.target.closest('.comment-btn'));
            }

            // Reply button clicks
            if (e.target.closest('.reply-btn')) {
                e.preventDefault();
                if (!this.isAuthenticated) {
                    this.showLoginPrompt('reply to this comment');
                    return;
                }
                this.showReplyForm(e.target.closest('.reply-btn'));
            }

            // Edit comment
            if (e.target.closest('.edit-comment-btn')) {
                e.preventDefault();
                this.editComment(e.target.closest('.edit-comment-btn'));
            }

            // Delete comment
            if (e.target.closest('.delete-comment-btn')) {
                e.preventDefault();
                this.deleteComment(e.target.closest('.delete-comment-btn'));
            }

            // Close login prompt
            if (e.target.classList.contains('close-login-prompt') || e.target.closest('.close-login-prompt')) {
                this.hideLoginPrompt();
            }
        });

        // Focus events for comment inputs (auth check)
        document.addEventListener('focus', (e) => {
            if (e.target.classList.contains('comment-input')) {
                if (!this.isAuthenticated) {
                    e.target.blur();
                    this.showLoginPrompt('comment on this post');
                }
            }
        });
    }

    loadCommentsForCurrentPost() {
        // Get post ID from current page
        const postElement = document.querySelector('[data-post-id]');
        if (postElement) {
            this.currentPostId = parseInt(postElement.dataset.postId);
            this.loadComments(this.currentPostId);
        }
    }

    async loadComments(postId) {
        try {
            const response = await fetch(`/api/Comments/post/${postId}`);
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.renderComments(data.comments, postId);
                    this.updateCommentCount(postId, data.comments.length);
                }
            }
        } catch (error) {
            console.error('Failed to load comments:', error);
        }
    }

    renderComments(comments, postId) {
        const container = document.querySelector(`[data-post-id="${postId}"] .comments-container`);
        if (!container) return;

        container.innerHTML = `
            <div class="comments-header">
                <h4>Comments (${this.getTotalCommentsCount(comments)})</h4>
                ${this.isAuthenticated ? this.renderCommentForm(postId) : this.renderGuestCommentPrompt()}
            </div>
            <div class="comments-list">
                ${comments.map(comment => this.renderComment(comment)).join('')}
            </div>
        `;
    }

    renderCommentForm(postId, parentId = null) {
        return `
            <form class="comment-form" data-post-id="${postId}" ${parentId ? `data-parent-id="${parentId}"` : ''}>
                <div class="comment-input-container">
                    <textarea class="comment-input" placeholder="Write a comment..." required></textarea>
                    <div class="comment-actions">
                        <button type="submit" class="btn-comment-submit">Post Comment</button>
                        ${parentId ? '<button type="button" class="btn-cancel-reply">Cancel</button>' : ''}
                    </div>
                </div>
            </form>
        `;
    }

    renderGuestCommentPrompt() {
        return `
            <div class="guest-comment-prompt">
                <div class="comment-input-placeholder" onclick="commentsManager.showLoginPrompt('comment on this post')">
                    <span>Write a comment...</span>
                    <span class="login-hint">Click to login</span>
                </div>
            </div>
        `;
    }

    renderComment(comment) {
        return `
            <div class="comment" data-comment-id="${comment.id}">
                <div class="comment-header">
                    <div class="comment-avatar">${comment.userName ? comment.userName.charAt(0).toUpperCase() : 'U'}</div>
                    <div class="comment-meta">
                        <span class="comment-author">${comment.userName || 'Anonymous'}</span>
                        <span class="comment-date">${this.formatDate(comment.createdAt)}</span>
                    </div>
                    ${this.isAuthenticated ? this.renderCommentActions(comment) : ''}
                </div>
                <div class="comment-content">${this.escapeHtml(comment.content)}</div>
                <div class="comment-footer">
                    <button class="comment-like-btn ${comment.isLikedByCurrentUser ? 'liked' : ''}" 
                            ${!this.isAuthenticated ? 'onclick="commentsManager.showLoginPrompt(\'like this comment\')"' : ''}>
                        <i class="fas fa-heart"></i>
                        <span>${comment.likesCount || 0}</span>
                    </button>
                    <button class="reply-btn" ${!this.isAuthenticated ? 'onclick="commentsManager.showLoginPrompt(\'reply to this comment\')"' : ''}>
                        <i class="fas fa-reply"></i>
                        Reply
                    </button>
                </div>
                ${comment.replies && comment.replies.length > 0 ? `
                    <div class="comment-replies">
                        ${comment.replies.map(reply => this.renderComment(reply)).join('')}
                    </div>
                ` : ''}
                <div class="reply-form-container"></div>
            </div>
        `;
    }

    renderCommentActions(comment) {
        // Only show edit/delete for comment owner
        const currentUserId = this.getCurrentUserId();
        if (currentUserId && currentUserId === comment.userId) {
            return `
                <div class="comment-actions">
                    <button class="edit-comment-btn" data-comment-id="${comment.id}">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="delete-comment-btn" data-comment-id="${comment.id}">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            `;
        }
        return '';
    }

    async handleCommentSubmit(form) {
        if (!this.isAuthenticated) {
            this.showLoginPrompt('comment on this post');
            return;
        }

        const formData = new FormData(form);
        const postId = parseInt(form.dataset.postId);
        const parentId = form.dataset.parentId ? parseInt(form.dataset.parentId) : null;
        const content = form.querySelector('.comment-input').value.trim();

        if (!content) return;

        try {
            const response = await fetch('/api/Comments', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
                },
                body: JSON.stringify({
                    postID: postId,
                    content: content,
                    parentCommentID: parentId
                })
            });

            const result = await response.json();
            if (result.success) {
                this.showToast('Comment posted successfully!', 'success');
                form.reset();
                await this.loadComments(postId);
                
                // Hide reply form if it was a reply
                if (parentId) {
                    form.closest('.reply-form-container').innerHTML = '';
                }
            } else {
                this.showToast(result.message || 'Failed to post comment', 'error');
            }
        } catch (error) {
            console.error('Error posting comment:', error);
            this.showToast('Network error occurred', 'error');
        }
    }

    showReplyForm(button) {
        const comment = button.closest('.comment');
        const commentId = comment.dataset.commentId;
        const postId = this.currentPostId;
        const replyContainer = comment.querySelector('.reply-form-container');

        replyContainer.innerHTML = this.renderCommentForm(postId, commentId);
        replyContainer.querySelector('.comment-input').focus();
    }

    toggleCommentSection(button) {
        const post = button.closest('[data-post-id]');
        const commentsSection = post.querySelector('.comments-section');
        
        if (commentsSection.style.display === 'none' || !commentsSection.style.display) {
            commentsSection.style.display = 'block';
            button.querySelector('.nav-text').textContent = 'Hide Comments';
        } else {
            commentsSection.style.display = 'none';
            button.querySelector('.nav-text').textContent = 'Show Comments';
        }
    }

    showLoginPrompt(action) {
        // Remove existing prompt
        this.hideLoginPrompt();

        const prompt = document.createElement('div');
        prompt.className = 'login-prompt-overlay';
        prompt.innerHTML = `
            <div class="login-prompt-modal">
                <div class="login-prompt-header">
                    <h3>Login Required</h3>
                    <button class="close-login-prompt">&times;</button>
                </div>
                <div class="login-prompt-content">
                    <p>You need to be logged in to ${action}.</p>
                    <div class="login-prompt-actions">
                        <a href="/Login" class="btn btn-primary">Login</a>
                        <a href="/Register" class="btn btn-secondary">Register</a>
                        <button class="btn btn-cancel close-login-prompt">Cancel</button>
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(prompt);
        
        // Animate in
        setTimeout(() => prompt.classList.add('show'), 10);
    }

    hideLoginPrompt() {
        const prompt = document.querySelector('.login-prompt-overlay');
        if (prompt) {
            prompt.classList.remove('show');
            setTimeout(() => prompt.remove(), 300);
        }
    }

    async editComment(button) {
        const commentId = button.dataset.commentId;
        const comment = button.closest('.comment');
        const contentElement = comment.querySelector('.comment-content');
        const originalContent = contentElement.textContent;

        // Replace content with editable textarea
        contentElement.innerHTML = `
            <div class="edit-comment-form">
                <textarea class="edit-comment-input">${originalContent}</textarea>
                <div class="edit-comment-actions">
                    <button class="btn-save-edit" data-comment-id="${commentId}">Save</button>
                    <button class="btn-cancel-edit">Cancel</button>
                </div>
            </div>
        `;

        // Add event listeners for save/cancel
        const saveBtn = contentElement.querySelector('.btn-save-edit');
        const cancelBtn = contentElement.querySelector('.btn-cancel-edit');

        saveBtn.addEventListener('click', async () => {
            const newContent = contentElement.querySelector('.edit-comment-input').value.trim();
            if (newContent && newContent !== originalContent) {
                await this.saveCommentEdit(commentId, newContent, contentElement, originalContent);
            } else {
                contentElement.innerHTML = this.escapeHtml(originalContent);
            }
        });

        cancelBtn.addEventListener('click', () => {
            contentElement.innerHTML = this.escapeHtml(originalContent);
        });
    }

    async saveCommentEdit(commentId, newContent, contentElement, originalContent) {
        try {
            const response = await fetch(`/api/Comments/${commentId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
                },
                body: JSON.stringify({
                    commentID: parseInt(commentId),
                    content: newContent
                })
            });

            const result = await response.json();
            if (result.success) {
                contentElement.innerHTML = this.escapeHtml(newContent);
                this.showToast('Comment updated successfully!', 'success');
            } else {
                contentElement.innerHTML = this.escapeHtml(originalContent);
                this.showToast(result.message || 'Failed to update comment', 'error');
            }
        } catch (error) {
            contentElement.innerHTML = this.escapeHtml(originalContent);
            this.showToast('Network error occurred', 'error');
        }
    }

    async deleteComment(button) {
        if (!confirm('Are you sure you want to delete this comment?')) return;

        const commentId = button.dataset.commentId;
        try {
            const response = await fetch(`/api/Comments/${commentId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
                }
            });

            const result = await response.json();
            if (result.success) {
                button.closest('.comment').remove();
                this.showToast('Comment deleted successfully!', 'success');
                await this.loadComments(this.currentPostId);
            } else {
                this.showToast(result.message || 'Failed to delete comment', 'error');
            }
        } catch (error) {
            this.showToast('Network error occurred', 'error');
        }
    }

    async handleLike(button) {
        // This would connect to your existing like system
        // For now, just toggle the visual state
        button.classList.toggle('liked');
        const countSpan = button.querySelector('span');
        let count = parseInt(countSpan.textContent) || 0;
        countSpan.textContent = button.classList.contains('liked') ? count + 1 : count - 1;
    }

    updateCommentCount(postId, count) {
        const commentButtons = document.querySelectorAll(`[data-post-id="${postId}"] .comment-btn .nav-text`);
        commentButtons.forEach(btn => {
            btn.textContent = `Comments (${count})`;
        });
    }

    getTotalCommentsCount(comments) {
        let total = comments.length;
        comments.forEach(comment => {
            if (comment.replies) {
                total += comment.replies.length;
            }
        });
        return total;
    }

    getCurrentUserId() {
        const token = localStorage.getItem('jwtToken');
        if (!token) return null;

        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return parseInt(payload.userId || payload.id);
        } catch (error) {
            return null;
        }
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffInMinutes = Math.floor((now - date) / (1000 * 60));

        if (diffInMinutes < 1) return 'Just now';
        if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
        if (diffInMinutes < 1440) return `${Math.floor(diffInMinutes / 60)}h ago`;
        if (diffInMinutes < 10080) return `${Math.floor(diffInMinutes / 1440)}d ago`;
        
        return date.toLocaleDateString();
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        
        const toastContainer = document.querySelector('.toast-container') || this.createToastContainer();
        toastContainer.appendChild(toast);
        
        setTimeout(() => toast.classList.add('show'), 100);
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    createToastContainer() {
        const container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
        return container;
    }
}

// Initialize comments manager when DOM is loaded
let commentsManager;
document.addEventListener('DOMContentLoaded', () => {
    commentsManager = new CommentsManager();
});

// Export for global access
window.commentsManager = commentsManager;
