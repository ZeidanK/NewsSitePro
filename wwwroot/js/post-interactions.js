// PostCard interactions module
window.PostCardInteractions = {
    async toggleLike(postId, button) {
        try {
            const response = await fetch(`/api/posts/Like/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const result = await response.json();
            if (result.action) {
                const postCard = button.closest('.post-card');
                const likesCountSpan = postCard.querySelector('.likes-count');
                const heartIcon = button.querySelector('i');
                const buttonText = button.querySelector('span');
                
                if (result.action === 'liked') {
                    button.classList.add('active');
                    heartIcon.classList.add('liked');
                    buttonText.textContent = 'Liked';
                    likesCountSpan.textContent = parseInt(likesCountSpan.textContent) + 1;
                } else {
                    button.classList.remove('active');
                    heartIcon.classList.remove('liked');
                    buttonText.textContent = 'Like';
                    likesCountSpan.textContent = Math.max(0, parseInt(likesCountSpan.textContent) - 1);
                }
            }
        } catch (error) {
            console.error('Error toggling like:', error);
            this.showMessage('Failed to update like status', 'error');
        }
    },

    async toggleSave(postId, button) {
        try {
            const response = await fetch(`/api/posts/Save/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const result = await response.json();
            if (result.action) {
                const buttonText = button.querySelector('span');
                
                if (result.action === 'saved') {
                    button.classList.add('active');
                    buttonText.textContent = 'Saved';
                } else {
                    button.classList.remove('active');
                    buttonText.textContent = 'Save';
                }
            }
        } catch (error) {
            console.error('Error toggling save:', error);
            this.showMessage('Failed to update save status', 'error');
        }
    },

    sharePost(postId) {
        const url = `${window.location.origin}/Post/${postId}`;
        if (navigator.share) {
            navigator.share({
                title: 'Check out this post',
                url: url
            });
        } else {
            navigator.clipboard.writeText(url).then(() => {
                this.showMessage('Link copied to clipboard!', 'success');
            });
        }
    },

    openPost(postId) {
        window.location.href = `/Post/${postId}`;
    },

    async reportPost(postId) {
        try {
            // Show confirmation dialog
            const reason = prompt('Please specify the reason for reporting this post:');
            if (!reason || reason.trim() === '') {
                return;
            }

            const response = await fetch(`/api/posts/Report/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ reason: reason.trim() })
            });

            const result = await response.json();
            if (response.ok) {
                this.showMessage('Post reported successfully. Thank you for helping keep our community safe.', 'success');
            } else {
                this.showMessage(result.message || 'Failed to report post', 'error');
            }
        } catch (error) {
            console.error('Error reporting post:', error);
            this.showMessage('Failed to report post', 'error');
        }
    },

    async blockUser(userId, username) {
        try {
            // Show confirmation dialog
            const confirmed = confirm(`Are you sure you want to block ${username}? You will no longer see their posts.`);
            if (!confirmed) {
                return;
            }

            const response = await fetch(`/api/User/Block/${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const result = await response.json();
            if (response.ok) {
                this.showMessage(`${username} has been blocked`, 'success');
                // Hide all posts from this user
                document.querySelectorAll(`[data-user-id="${userId}"]`).forEach(post => {
                    post.style.display = 'none';
                });
            } else {
                this.showMessage(result.message || 'Failed to block user', 'error');
            }
        } catch (error) {
            console.error('Error blocking user:', error);
            this.showMessage('Failed to block user', 'error');
        }
    },

    async followUser(userId, username, button) {
        // Delegate to the follow status manager for consistency
        if (window.followStatusManager) {
            if (!button.dataset.username) {
                button.dataset.username = username;
            }
            window.followStatusManager.handleFollowClick(button);
            return;
        }

        // Fallback implementation if follow manager isn't available
        try {
            const response = await fetch(`/api/User/Follow/${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const result = await response.json();
            if (response.ok) {
                const buttonText = button.querySelector('span') || button;
                const icon = button.querySelector('i');
                
                if (result.action === 'followed') {
                    button.classList.remove('btn-outline-primary');
                    button.classList.add('btn-primary');
                    buttonText.textContent = 'Following';
                    if (icon) icon.className = 'fas fa-user-check';
                    this.showMessage(`You are now following ${username}`, 'success');
                } else {
                    button.classList.remove('btn-primary');
                    button.classList.add('btn-outline-primary');
                    buttonText.textContent = 'Follow';
                    if (icon) icon.className = 'fas fa-user-plus';
                    this.showMessage(`You unfollowed ${username}`, 'info');
                }

                // Update all follow buttons for this user
                document.querySelectorAll(`[data-follow-user-id="${userId}"]`).forEach(btn => {
                    if (btn !== button) {
                        const btnText = btn.querySelector('span') || btn;
                        const btnIcon = btn.querySelector('i');
                        
                        if (result.action === 'followed') {
                            btn.classList.remove('btn-outline-primary');
                            btn.classList.add('btn-primary');
                            btnText.textContent = 'Following';
                            if (btnIcon) btnIcon.className = 'fas fa-user-check';
                        } else {
                            btn.classList.remove('btn-primary');
                            btn.classList.add('btn-outline-primary');
                            btnText.textContent = 'Follow';
                            if (btnIcon) btnIcon.className = 'fas fa-user-plus';
                        }
                    }
                });
            } else {
                this.showMessage(result.message || 'Failed to update follow status', 'error');
            }
        } catch (error) {
            console.error('Error following user:', error);
            this.showMessage('Failed to update follow status', 'error');
        }
    },

    showMessage(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} alert-dismissible fade show position-fixed`;
        toast.style.top = '20px';
        toast.style.right = '20px';
        toast.style.zIndex = '9999';
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button> 
        `;
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 3000);
    }
};

// Global functions for backward compatibility
window.toggleLike = (postId, button) => PostCardInteractions.toggleLike(postId, button);
window.toggleSave = (postId, button) => PostCardInteractions.toggleSave(postId, button);
window.sharePost = (postId) => PostCardInteractions.sharePost(postId);
window.openPost = (postId) => PostCardInteractions.openPost(postId);
window.reportPost = (postId) => PostCardInteractions.reportPost(postId);
window.blockUser = (userId, username) => PostCardInteractions.blockUser(userId, username);
// Follow functionality is now handled by follow-manager.js
// window.followUser = (userId, username, button) => PostCardInteractions.followUser(userId, username, button);
window.toggleFollow = (userId, button) => PostCardInteractions.followUser(userId, 'User', button);
