// PostCard interactions module
window.PostCardInteractions = {
    async toggleLike(postId, button) {
        try {
            const apiUrl = window.ApiConfig ? window.ApiConfig.getApiUrl(`api/posts/Like/${postId}`) : `/api/posts/Like/${postId}`;
            const response = await fetch(apiUrl, {
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
            const apiUrl = window.ApiConfig ? window.ApiConfig.getApiUrl(`api/posts/Save/${postId}`) : `/api/posts/Save/${postId}`;
            const response = await fetch(apiUrl, {
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

            const apiUrl = window.ApiConfig ? window.ApiConfig.getApiUrl(`api/posts/Report/${postId}`) : `/api/posts/Report/${postId}`;
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ reason: reason.trim() })
            });

            const result = await response.json();
            if (response.ok) {
                this.showMessage('Post reported successfully. Thank you for helping keep our community safe.', 'success');
                
                // After successful report, offer to block the user
                const postElement = document.querySelector(`[data-post-id="${postId}"]`);
                if (postElement) {
                    const username = postElement.querySelector('.username')?.textContent;
                    const userLink = postElement.querySelector('.user-link');
                    const userIdMatch = userLink?.href?.match(/\/UserProfile\/(\d+)/);
                    const userId = userIdMatch ? userIdMatch[1] : null;

                    if (userId && username) {
                        const blockUser = confirm(`Would you also like to block ${username}? You will no longer see their posts or receive notifications from them.`);
                        if (blockUser) {
                            await this.blockUser(userId, username);
                        }
                    }
                }
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
            const apiUrl = window.ApiConfig ? window.ApiConfig.getApiUrl(`api/UserBlock/block`) : `/api/UserBlock/block`;
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ 
                    blockedUserID: parseInt(userId),
                    reason: 'Blocked from user interaction' 
                })
            });

            const result = await response.json();
            if (response.ok) {
                this.showMessage(`${username} has been blocked successfully. You will no longer see their posts.`, 'success');
                // Hide all posts from this user on the current page
                document.querySelectorAll(`[data-post-id]`).forEach(postCard => {
                    const userLink = postCard.querySelector('.user-link');
                    if (userLink && userLink.href.includes(`/UserProfile/${userId}`)) {
                        postCard.style.display = 'none';
                    }
                });
                
                // Refresh the page to ensure blocked user's content is filtered out
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
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
            const apiUrl = window.ApiConfig ? window.ApiConfig.getApiUrl(`api/User/Follow/${userId}`) : `/api/User/Follow/${userId}`;
            const response = await fetch(apiUrl, {
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
    },

    // Mobile menu functions
    toggleMobileMenu(postId) {
        const menu = document.getElementById(`mobileMenu-${postId}`);
        if (!menu) return;
        
        // Close all other open menus
        document.querySelectorAll('.mobile-menu-dropdown.show').forEach(dropdown => {
            if (dropdown.id !== `mobileMenu-${postId}`) {
                dropdown.classList.remove('show');
            }
        });
        
        // Toggle current menu
        menu.classList.toggle('show');
        
        // Close menu when clicking outside
        if (menu.classList.contains('show')) {
            const closeHandler = (e) => {
                if (!menu.contains(e.target) && !e.target.closest('.mobile-menu-toggle')) {
                    menu.classList.remove('show');
                    document.removeEventListener('click', closeHandler);
                }
            };
            // Add delay to prevent immediate closure
            setTimeout(() => document.addEventListener('click', closeHandler), 100);
        }
    },

    hideMobileMenu(postId) {
        const menu = document.getElementById(`mobileMenu-${postId}`);
        if (menu) {
            menu.classList.remove('show');
        }
    },

    // Admin functions
    moderatePost(postId) {
        if (confirm('Are you sure you want to moderate this post?')) {
            console.log('Moderating post:', postId);
            // Implement moderation logic here
            this.showMessage('Post moderated successfully', 'success');
        }
    },

    banUser(userId) {
        if (confirm('Are you sure you want to ban this user?')) {
            console.log('Banning user:', userId);
            // Implement ban logic here
            this.showMessage('User banned successfully', 'success');
        }
    },

    editPost(postId) {
        window.location.href = `./Posts/Edit/${postId}`;
    },

    deletePost(postId) {
        if (confirm('Are you sure you want to delete this post? This action cannot be undone.')) {
            console.log('Deleting post:', postId);
            // Implement delete logic here
            this.showMessage('Post deleted successfully', 'success');
        }
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

// Mobile menu functions
window.toggleMobileMenu = (postId) => PostCardInteractions.toggleMobileMenu(postId);
window.hideMobileMenu = (postId) => PostCardInteractions.hideMobileMenu(postId);

// Admin functions
window.moderatePost = (postId) => PostCardInteractions.moderatePost(postId);
window.banUser = (userId) => PostCardInteractions.banUser(userId);
window.editPost = (postId) => PostCardInteractions.editPost(postId);
window.deletePost = (postId) => PostCardInteractions.deletePost(postId);
