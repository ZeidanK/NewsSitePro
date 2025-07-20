// Notifications page JavaScript
class NotificationManager {
    constructor() {
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadUserPreferences();
    }

    bindEvents() {
        // Mark all as read
        document.getElementById('markAllReadBtn')?.addEventListener('click', () => {
            this.markAllAsRead();
        });

        // Individual mark as read buttons
        document.querySelectorAll('.mark-read-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const notificationId = e.target.getAttribute('data-id');
                this.markAsRead(notificationId);
            });
        });

        // Save settings button
        document.getElementById('saveSettingsBtn')?.addEventListener('click', () => {
            this.saveNotificationSettings();
        });

        // Auto-refresh notifications every 30 seconds
        setInterval(() => {
            this.refreshNotificationCount();
        }, 30000);
    }

    async markAsRead(notificationId) {
        try {
            const response = await fetch('/Notifications?handler=MarkAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify(parseInt(notificationId))
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    // Update UI
                    const notificationItem = document.querySelector(`[data-id="${notificationId}"]`);
                    if (notificationItem) {
                        notificationItem.classList.remove('unread');
                        notificationItem.classList.add('read');
                        
                        // Remove mark as read button
                        const markBtn = notificationItem.querySelector('.mark-read-btn');
                        if (markBtn) markBtn.remove();
                    }
                    
                    // Update summary counts
                    this.updateSummaryCounts();
                    this.showToast('Notification marked as read', 'success');
                } else {
                    this.showToast('Failed to mark notification as read', 'error');
                }
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
            this.showToast('Error occurred', 'error');
        }
    }

    async markAllAsRead() {
        try {
            const response = await fetch('/Notifications?handler=MarkAllAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    // Update all notification items
                    document.querySelectorAll('.notification-item.unread').forEach(item => {
                        item.classList.remove('unread');
                        item.classList.add('read');
                        
                        // Remove mark as read buttons
                        const markBtn = item.querySelector('.mark-read-btn');
                        if (markBtn) markBtn.remove();
                    });
                    
                    // Update summary
                    this.updateSummaryCounts();
                    this.showToast('All notifications marked as read', 'success');
                } else {
                    this.showToast('Failed to mark all notifications as read', 'error');
                }
            }
        } catch (error) {
            console.error('Error marking all notifications as read:', error);
            this.showToast('Error occurred', 'error');
        }
    }

    async saveNotificationSettings() {
        try {
            const preferences = {};
            const notificationTypes = ['Like', 'Comment', 'Follow', 'NewPost', 'PostShare', 'AdminMessage'];
            
            notificationTypes.forEach(type => {
                const enabled = document.getElementById(`${type}_enabled`)?.checked || false;
                const email = document.getElementById(`${type}_email`)?.checked || false;
                
                preferences[type] = {
                    IsEnabled: enabled,
                    EmailNotification: email,
                    PushNotification: enabled // Default to same as enabled
                };
            });

            const response = await fetch('/Notifications?handler=UpdatePreferences', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({
                    UserID: await this.getCurrentUserId(),
                    Preferences: preferences
                })
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    this.showToast('Notification settings saved successfully', 'success');
                    // Close modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('notificationSettingsModal'));
                    if (modal) modal.hide();
                } else {
                    this.showToast('Failed to save settings', 'error');
                }
            }
        } catch (error) {
            console.error('Error saving notification settings:', error);
            this.showToast('Error occurred while saving settings', 'error');
        }
    }

    updateSummaryCounts() {
        const unreadNotifications = document.querySelectorAll('.notification-item.unread').length;
        
        // Update total unread count
        const totalUnreadElement = document.querySelector('.summary-card h3');
        if (totalUnreadElement) {
            totalUnreadElement.textContent = unreadNotifications;
        }
        
        // Update type-specific counts
        const typeCounts = {};
        document.querySelectorAll('.notification-item.unread').forEach(item => {
            const icon = item.querySelector('.notification-icon span');
            let type = 'Other';
            
            if (icon?.classList.contains('icon-like')) type = 'Like';
            else if (icon?.classList.contains('icon-comment')) type = 'Comment';
            else if (icon?.classList.contains('icon-follow')) type = 'Follow';
            else if (icon?.classList.contains('icon-post')) type = 'NewPost';
            
            typeCounts[type] = (typeCounts[type] || 0) + 1;
        });
        
        // Update summary cards
        document.querySelectorAll('.summary-card').forEach((card, index) => {
            if (index > 0) { // Skip first card (total)
                const typeText = card.querySelector('p')?.textContent;
                const countElement = card.querySelector('h3');
                if (countElement && typeText) {
                    countElement.textContent = typeCounts[typeText] || 0;
                }
            }
        });
    }

    async refreshNotificationCount() {
        try {
            const response = await fetch('/Notifications?handler=UnreadCount');
            if (response.ok) {
                const data = await response.json();
                const count = data.count || 0;
                
                // Update any notification badges
                document.querySelectorAll('.notification-badge, .badge').forEach(badge => {
                    if (count > 0) {
                        badge.textContent = count;
                        badge.style.display = 'inline-block';
                    } else {
                        badge.style.display = 'none';
                    }
                });
            }
        } catch (error) {
            console.error('Error refreshing notification count:', error);
        }
    }

    async getCurrentUserId() {
        // Extract user ID from JWT token
        const token = this.getJwtToken();
        if (token) {
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                return parseInt(payload.userId || payload.sub || payload.nameid);
            } catch (error) {
                console.error('Error parsing JWT token:', error);
            }
        }
        return null;
    }

    getJwtToken() {
        // Check localStorage first, then cookies
        let token = localStorage.getItem('jwtToken');
        if (!token) {
            const cookies = document.cookie.split(';');
            for (let cookie of cookies) {
                const [name, value] = cookie.trim().split('=');
                if (name === 'jwtToken') {
                    token = value;
                    break;
                }
            }
        }
        return token;
    }

    getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    loadUserPreferences() {
        // This would typically load from the server or model data
        // For now, preferences are loaded from the Razor page
    }

    showToast(message, type = 'info') {
        // Create and show a toast notification
        const toast = document.createElement('div');
        toast.className = `alert alert-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info'} toast-notification`;
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            padding: 12px 16px;
            border-radius: 4px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            animation: slideInRight 0.3s ease-out;
        `;
        toast.textContent = message;

        document.body.appendChild(toast);

        // Auto-remove after 3 seconds
        setTimeout(() => {
            toast.style.animation = 'slideOutRight 0.3s ease-in';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, 3000);
    }
}

// Advanced notification features
class NotificationEnhancements {
    constructor() {
        this.setupKeyboardShortcuts();
        this.setupInfiniteScroll();
        this.setupNotificationSearch();
    }

    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl + Shift + A = Mark all as read
            if (e.ctrlKey && e.shiftKey && e.key === 'A') {
                e.preventDefault();
                document.getElementById('markAllReadBtn')?.click();
            }
            
            // Ctrl + Shift + S = Open settings
            if (e.ctrlKey && e.shiftKey && e.key === 'S') {
                e.preventDefault();
                document.getElementById('settingsBtn')?.click();
            }
        });
    }

    setupInfiniteScroll() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.loadMoreNotifications();
                }
            });
        }, { threshold: 0.1 });

        // Observe the last notification item
        const lastNotification = document.querySelector('.notification-item:last-child');
        if (lastNotification) {
            observer.observe(lastNotification);
        }
    }

    setupNotificationSearch() {
        // Add search functionality
        const searchInput = document.createElement('input');
        searchInput.type = 'text';
        searchInput.placeholder = 'Search notifications...';
        searchInput.className = 'form-control mb-3';
        searchInput.style.maxWidth = '300px';
        
        searchInput.addEventListener('input', (e) => {
            this.filterNotifications(e.target.value);
        });

        // Insert search box before notifications list
        const notificationsList = document.querySelector('.notifications-list');
        if (notificationsList) {
            notificationsList.parentNode.insertBefore(searchInput, notificationsList);
        }
    }

    filterNotifications(searchTerm) {
        const notifications = document.querySelectorAll('.notification-item');
        const term = searchTerm.toLowerCase();

        notifications.forEach(notification => {
            const title = notification.querySelector('.notification-title')?.textContent.toLowerCase() || '';
            const message = notification.querySelector('.notification-message')?.textContent.toLowerCase() || '';
            const fromUser = notification.querySelector('.notification-from')?.textContent.toLowerCase() || '';

            if (title.includes(term) || message.includes(term) || fromUser.includes(term)) {
                notification.style.display = 'flex';
            } else {
                notification.style.display = 'none';
            }
        });
    }

    async loadMoreNotifications() {
        // Implementation for infinite scroll loading
        console.log('Loading more notifications...');
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new NotificationManager();
    new NotificationEnhancements();
});

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    
    @keyframes slideOutRight {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
    
    .notification-item {
        transition: all 0.3s ease;
    }
    
    .notification-item:hover {
        background-color: #f8f9fa;
        transform: translateY(-1px);
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    
    .toast-notification {
        animation: slideInRight 0.3s ease-out;
    }
`;
document.head.appendChild(style);
