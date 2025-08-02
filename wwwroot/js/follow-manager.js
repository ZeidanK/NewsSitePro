/**
 * Follow Status Manager
 * Handles follow/unfollow functionality and keeps status synchronized across all UI components
 */

// Helper function for API URL generation
function getApiUrl(endpoint) {
    const APP_BASE_URL = window.location.pathname.split('/').slice(0, -1).join('/') || '';
    return `${APP_BASE_URL}/${endpoint}`.replace(/\/+/g, '/').replace(/\/$/, '');
}

class FollowStatusManager {
    constructor() {
        this.followStatus = new Map(); // userId -> isFollowing
        this.init();
    }

    init() {
        // Initialize with current page follow statuses
        this.loadCurrentPageStatuses();
        
        // Set up event listeners for follow buttons
        document.addEventListener('click', (e) => {
            if (e.target.closest('.follow-btn, .btn-follow')) {
                e.preventDefault();
                const button = e.target.closest('.follow-btn, .btn-follow');
                this.handleFollowClick(button);
            }
        });
    }

    /**
     * Load follow statuses from all follow buttons on the current page
     */
    loadCurrentPageStatuses() {
        const followButtons = document.querySelectorAll('[data-following]');
        followButtons.forEach(button => {
            const userId = parseInt(button.dataset.userId);
            const isFollowing = button.dataset.following === 'true';
            if (userId) {
                this.followStatus.set(userId, isFollowing);
            }
        });
    }

    /**
     * Handle follow button click
     */
    async handleFollowClick(button) {
        const userId = parseInt(button.dataset.userId);
        const username = button.dataset.username || `User ${userId}`;
        
        if (!userId) {
            console.error('No user ID found on follow button');
            return;
        }

        // Disable button during request
        const originalContent = button.innerHTML;
        button.disabled = true;
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';

        try {
            const response = await fetch(`/api/User/Follow/${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const result = await response.json();

            if (response.ok) {
                // Update local status
                const newFollowStatus = result.isFollowing;
                this.followStatus.set(userId, newFollowStatus);
                
                // Update all UI elements for this user
                this.updateAllFollowButtons(userId, newFollowStatus);
                
                // Show success message
                this.showMessage(result.message || `Successfully ${result.action} ${username}`, 'success');
            } else {
                this.showMessage(result.message || 'Failed to update follow status', 'error');
                // Restore original button state
                button.innerHTML = originalContent;
                button.disabled = false;
            }
        } catch (error) {
            console.error('Error updating follow status:', error);
            this.showMessage('Failed to update follow status', 'error');
            // Restore original button state
            button.innerHTML = originalContent;
            button.disabled = false;
        }
    }

    /**
     * Update all follow buttons for a specific user ID across the page
     */
    updateAllFollowButtons(userId, isFollowing) {
        const allFollowButtons = document.querySelectorAll(`[data-user-id="${userId}"]`);
        
        allFollowButtons.forEach(button => {
            // Skip if this is not a follow button
            if (!button.classList.contains('follow-btn') && !button.classList.contains('btn-follow')) {
                return;
            }

            // Update button state
            button.dataset.following = isFollowing.toString();
            button.disabled = false;

            // Update button appearance and text
            if (isFollowing) {
                button.className = button.className.replace('btn-outline-primary', 'btn-secondary');
                button.innerHTML = '<i class="fas fa-user-check"></i> Following';
            } else {
                button.className = button.className.replace('btn-secondary', 'btn-outline-primary');
                button.innerHTML = '<i class="fas fa-user-plus"></i> Follow';
            }
        });
    }

    /**
     * Get follow status for a user
     */
    getFollowStatus(userId) {
        return this.followStatus.get(userId) || false;
    }

    /**
     * Check if user is being followed (async method that checks server)
     */
    async checkFollowStatus(userId) {
        try {
            const response = await fetch(`/api/User/Follow/Status/${userId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const result = await response.json();
                this.followStatus.set(userId, result.isFollowing);
                return result.isFollowing;
            }
        } catch (error) {
            console.error('Error checking follow status:', error);
        }
        
        return this.getFollowStatus(userId);
    }

    /**
     * Show message to user
     */
    showMessage(message, type = 'info') {
        // Check if we have a message container
        let messageContainer = document.getElementById('message-container');
        if (!messageContainer) {
            // Create one if it doesn't exist
            messageContainer = document.createElement('div');
            messageContainer.id = 'message-container';
            messageContainer.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 10000;
                max-width: 300px;
            `;
            document.body.appendChild(messageContainer);
        }

        // Create message element
        const messageElement = document.createElement('div');
        messageElement.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} alert-dismissible fade show`;
        messageElement.style.cssText = `
            margin-bottom: 10px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        `;
        messageElement.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
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
     * Refresh follow statuses for all users on the page
     */
    async refreshAllFollowStatuses() {
        const userIds = Array.from(this.followStatus.keys());
        
        for (const userId of userIds) {
            await this.checkFollowStatus(userId);
            this.updateAllFollowButtons(userId, this.getFollowStatus(userId));
        }
    }
}

// Global follow status manager instance
window.followStatusManager = new FollowStatusManager();

// Legacy function for backward compatibility
function followUser(userId, username, button) {
    // Update the button's dataset if it doesn't have username
    if (!button.dataset.username) {
        button.dataset.username = username;
    }
    window.followStatusManager.handleFollowClick(button);
}
