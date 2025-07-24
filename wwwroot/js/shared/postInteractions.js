/**
 * Post Interactions Module
 * Handles all post-related interactions (like, share, save, etc.)
 */

class PostInteractions {
    constructor() {
        this.currentUserId = null;
        this.isLoggedIn = false;
        this.init();
    }

    init() {
        // Get user info from session or DOM
        this.getUserInfo();
    }

    getUserInfo() {
        // Try to get user info from various sources
        if (window.currentUser) {
            this.currentUserId = window.currentUser.id;
            this.isLoggedIn = true;
        } else if (sessionStorage.getItem('userId')) {
            this.currentUserId = sessionStorage.getItem('userId');
            this.isLoggedIn = true;
        } else {
            // Check if user is logged in via DOM element or session
            const userElement = document.querySelector('[data-user-id]');
            if (userElement) {
                this.currentUserId = userElement.getAttribute('data-user-id');
                this.isLoggedIn = true;
            }
        }
    }

    /**
     * Like/Unlike a post
     */
    async likePost(postId) {
        if (!this.isLoggedIn) {
            this.showLoginPrompt();
            return;
        }

        try {
            const response = await fetch('/api/posts/like', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    postId: postId,
                    userId: this.currentUserId
                })
            });

            if (response.ok) {
                const result = await response.json();
                
                // Update UI
                if (window.postTemplates && window.postTemplates.feed) {
                    window.postTemplates.feed.toggleLikeButton(postId, result.isLiked, result.likeCount);
                }
                
                this.showToast(result.isLiked ? 'Post liked!' : 'Post unliked!', 'success');
            } else {
                throw new Error('Failed to like post');
            }
        } catch (error) {
            console.error('Error liking post:', error);
            this.showToast('Failed to like post. Please try again.', 'error');
        }
    }

    /**
     * Share a post
     */
    async sharePost(postId, title, sourceUrl = '') {
        try {
            // Try native Web Share API first
            if (navigator.share) {
                await navigator.share({
                    title: title,
                    text: `Check out this post: ${title}`,
                    url: window.location.origin + `/Post/${postId}`
                });
                
                // Log share event
                this.logShareEvent(postId, 'native');
                return;
            }
            
            // Fallback to share modal
            this.showShareModal(postId, title, sourceUrl);
        } catch (error) {
            console.error('Error sharing post:', error);
            // Fallback to share modal on error
            this.showShareModal(postId, title, sourceUrl);
        }
    }

    /**
     * Show share modal with various sharing options
     */
    showShareModal(postId, title, sourceUrl) {
        const postUrl = window.location.origin + `/Post/${postId}`;
        const encodedTitle = encodeURIComponent(title);
        const encodedUrl = encodeURIComponent(postUrl);
        
        const shareOptions = [
            {
                name: 'Copy Link',
                icon: '🔗',
                action: () => this.copyToClipboard(postUrl, 'Link copied to clipboard!')
            },
            {
                name: 'Twitter',
                icon: '🐦',
                action: () => window.open(`https://twitter.com/intent/tweet?text=${encodedTitle}&url=${encodedUrl}`, '_blank')
            },
            {
                name: 'Facebook',
                icon: '📘',
                action: () => window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodedUrl}`, '_blank')
            },
            {
                name: 'WhatsApp',
                icon: '💬',
                action: () => window.open(`https://wa.me/?text=${encodedTitle}%20${encodedUrl}`, '_blank')
            },
            {
                name: 'Telegram',
                icon: '✈️',
                action: () => window.open(`https://t.me/share/url?url=${encodedUrl}&text=${encodedTitle}`, '_blank')
            },
            {
                name: 'Email',
                icon: '📧',
                action: () => window.location.href = `mailto:?subject=${encodedTitle}&body=Check out this post: ${postUrl}`
            }
        ];
        
        this.createShareModal(shareOptions, postId);
    }

    /**
     * Create and show share modal
     */
    createShareModal(shareOptions, postId) {
        // Remove existing modal if any
        const existingModal = document.getElementById('shareModal');
        if (existingModal) {
            existingModal.remove();
        }
        
        const modal = document.createElement('div');
        modal.id = 'shareModal';
        modal.className = 'share-modal';
        modal.innerHTML = `
            <div class="share-modal-content">
                <div class="share-modal-header">
                    <h3>Share Post</h3>
                    <button class="close-modal">&times;</button>
                </div>
                <div class="share-modal-body">
                    ${shareOptions.map(option => `
                        <button class="share-option" data-action="${option.name.toLowerCase()}">
                            <span class="share-icon">${option.icon}</span>
                            <span class="share-text">${option.name}</span>
                        </button>
                    `).join('')}
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        
        // Add event listeners
        modal.addEventListener('click', (e) => {
            if (e.target === modal || e.target.classList.contains('close-modal')) {
                modal.remove();
            }
        });
        
        shareOptions.forEach(option => {
            const btn = modal.querySelector(`[data-action="${option.name.toLowerCase()}"]`);
            if (btn) {
                btn.addEventListener('click', () => {
                    option.action();
                    this.logShareEvent(postId, option.name.toLowerCase());
                    modal.remove();
                });
            }
        });
        
        // Show modal
        setTimeout(() => modal.classList.add('show'), 10);
    }

    /**
     * Save a post
     */
    async savePost(postId) {
        if (!this.isLoggedIn) {
            this.showLoginPrompt();
            return;
        }

        try {
            const response = await fetch('/api/posts/save', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    postId: postId,
                    userId: this.currentUserId
                })
            });

            if (response.ok) {
                const result = await response.json();
                this.showToast('Post saved to your collection!', 'success');
            } else {
                throw new Error('Failed to save post');
            }
        } catch (error) {
            console.error('Error saving post:', error);
            this.showToast('Failed to save post. Please try again.', 'error');
        }
    }

    /**
     * Save a live article to user feed
     */
    async saveLiveArticle(article) {
        if (!this.isLoggedIn) {
            this.showLoginPrompt();
            return;
        }

        try {
            const response = await fetch('/api/posts/save-live-article', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    article: article,
                    userId: this.currentUserId
                })
            });

            if (response.ok) {
                const result = await response.json();
                this.showToast('Article saved to your feed!', 'success');
            } else {
                throw new Error('Failed to save article');
            }
        } catch (error) {
            console.error('Error saving article:', error);
            this.showToast('Failed to save article. Please try again.', 'error');
        }
    }

    /**
     * Report a post
     */
    async reportPost(postId) {
        if (!this.isLoggedIn) {
            this.showLoginPrompt();
            return;
        }

        const reason = await this.showReportModal();
        if (!reason) return;

        try {
            const response = await fetch('/api/posts/report', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    postId: postId,
                    userId: this.currentUserId,
                    reason: reason
                })
            });

            if (response.ok) {
                this.showToast('Post reported. Thank you for helping keep our community safe.', 'success');
            } else {
                throw new Error('Failed to report post');
            }
        } catch (error) {
            console.error('Error reporting post:', error);
            this.showToast('Failed to report post. Please try again.', 'error');
        }
    }

    /**
     * Show report modal
     */
    showReportModal() {
        return new Promise((resolve) => {
            const modal = document.createElement('div');
            modal.className = 'report-modal share-modal';
            modal.innerHTML = `
                <div class="share-modal-content">
                    <div class="share-modal-header">
                        <h3>Report Post</h3>
                        <button class="close-modal">&times;</button>
                    </div>
                    <div class="share-modal-body">
                        <p>Why are you reporting this post?</p>
                        <div class="report-reasons">
                            <button class="report-reason" data-reason="spam">Spam</button>
                            <button class="report-reason" data-reason="harassment">Harassment</button>
                            <button class="report-reason" data-reason="inappropriate">Inappropriate Content</button>
                            <button class="report-reason" data-reason="misinformation">Misinformation</button>
                            <button class="report-reason" data-reason="copyright">Copyright Violation</button>
                            <button class="report-reason" data-reason="other">Other</button>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
            
            modal.addEventListener('click', (e) => {
                if (e.target === modal || e.target.classList.contains('close-modal')) {
                    modal.remove();
                    resolve(null);
                }
                
                if (e.target.classList.contains('report-reason')) {
                    const reason = e.target.getAttribute('data-reason');
                    modal.remove();
                    resolve(reason);
                }
            });
            
            setTimeout(() => modal.classList.add('show'), 10);
        });
    }

    /**
     * Log share event for analytics
     */
    async logShareEvent(postId, platform) {
        try {
            await fetch('/api/analytics/share', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    postId: postId,
                    platform: platform,
                    userId: this.currentUserId
                })
            });
        } catch (error) {
            console.error('Error logging share event:', error);
        }
    }

    /**
     * Copy text to clipboard
     */
    async copyToClipboard(text, successMessage = 'Copied to clipboard!') {
        try {
            await navigator.clipboard.writeText(text);
            this.showToast(successMessage, 'success');
        } catch (error) {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
            this.showToast(successMessage, 'success');
        }
    }

    /**
     * Show login prompt
     */
    showLoginPrompt() {
        const loginUrl = '/Login';
        this.showToast('Please log in to interact with posts.', 'info', {
            action: 'Login',
            actionCallback: () => window.location.href = loginUrl
        });
    }

    /**
     * Show toast notification
     */
    showToast(message, type = 'info', options = {}) {
        // Remove existing toasts
        const existingToasts = document.querySelectorAll('.toast');
        existingToasts.forEach(toast => toast.remove());
        
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <span class="toast-message">${message}</span>
                ${options.action ? `<button class="toast-action">${options.action}</button>` : ''}
                <button class="toast-close">&times;</button>
            </div>
        `;
        
        document.body.appendChild(toast);
        
        // Add event listeners
        toast.addEventListener('click', (e) => {
            if (e.target.classList.contains('toast-close')) {
                toast.remove();
            } else if (e.target.classList.contains('toast-action') && options.actionCallback) {
                options.actionCallback();
                toast.remove();
            }
        });
        
        // Show toast
        setTimeout(() => toast.classList.add('show'), 10);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 5000);
    }
}

// Initialize global post interactions
window.postInteractions = new PostInteractions();

// CSS for modals and toasts
if (!document.getElementById('postInteractionsCSS')) {
    const style = document.createElement('style');
    style.id = 'postInteractionsCSS';
    style.textContent = `
        /* Share Modal Styles */
        .share-modal {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10000;
            opacity: 0;
            visibility: hidden;
            transition: all 0.3s ease;
        }
        
        .share-modal.show {
            opacity: 1;
            visibility: visible;
        }
        
        .share-modal-content {
            background: white;
            border-radius: 12px;
            padding: 0;
            max-width: 400px;
            width: 90%;
            max-height: 80vh;
            overflow: hidden;
            transform: scale(0.7);
            transition: transform 0.3s ease;
        }
        
        .share-modal.show .share-modal-content {
            transform: scale(1);
        }
        
        .share-modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 20px;
            border-bottom: 1px solid #e0e0e0;
            background: #f8f9fa;
        }
        
        .share-modal-header h3 {
            margin: 0;
            font-size: 18px;
            font-weight: 600;
        }
        
        .close-modal {
            background: none;
            border: none;
            font-size: 24px;
            cursor: pointer;
            color: #666;
            padding: 0;
            width: 30px;
            height: 30px;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        .share-modal-body {
            padding: 20px;
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
            gap: 12px;
        }
        
        .share-option {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 8px;
            padding: 16px 12px;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            background: white;
            cursor: pointer;
            transition: all 0.2s ease;
            text-decoration: none;
            color: inherit;
        }
        
        .share-option:hover {
            border-color: #007bff;
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }
        
        .share-icon {
            font-size: 24px;
        }
        
        .share-text {
            font-size: 12px;
            font-weight: 500;
            text-align: center;
        }
        
        .report-reasons {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }
        
        .report-reason {
            padding: 12px 16px;
            border: 1px solid #e0e0e0;
            border-radius: 6px;
            background: white;
            cursor: pointer;
            transition: all 0.2s ease;
            text-align: left;
        }
        
        .report-reason:hover {
            border-color: #dc3545;
            background: #fff5f5;
        }
        
        /* Toast Styles */
        .toast {
            position: fixed;
            top: 20px;
            right: 20px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
            z-index: 10001;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            min-width: 300px;
            max-width: 500px;
        }
        
        .toast.show {
            transform: translateX(0);
        }
        
        .toast-content {
            display: flex;
            align-items: center;
            padding: 16px;
            gap: 12px;
        }
        
        .toast-message {
            flex: 1;
            font-size: 14px;
        }
        
        .toast-action {
            background: #007bff;
            color: white;
            border: none;
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 12px;
            cursor: pointer;
        }
        
        .toast-close {
            background: none;
            border: none;
            font-size: 18px;
            cursor: pointer;
            color: #666;
            padding: 0;
            width: 20px;
            height: 20px;
        }
        
        .toast-success {
            border-left: 4px solid #28a745;
        }
        
        .toast-error {
            border-left: 4px solid #dc3545;
        }
        
        .toast-info {
            border-left: 4px solid #17a2b8;
        }
        
        @media (max-width: 768px) {
            .share-modal-body {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .toast {
                right: 10px;
                left: 10px;
                min-width: auto;
                transform: translateY(-100%);
            }
            
            .toast.show {
                transform: translateY(0);
            }
        }
    `;
    document.head.appendChild(style);
}
