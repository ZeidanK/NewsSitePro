// API Configuration
window.ApiConfig = {
    getBaseUrl: function() {
        const isLocalhost = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";
        const port = isLocalhost ? ":7128" : "";
        const address = isLocalhost ? "https://localhost" : "https://proj.ruppin.ac.il/cgroup4/test2/tar1";
        
        return `${address}${port}`;
    },
    
    getApiUrl: function(endpoint) {
        const baseUrl = this.getBaseUrl();
        //console.log(`DEBUG - Raw baseUrl: ${baseUrl}`);
        //console.log(`DEBUG - Raw endpoint: ${endpoint}`);
        // Ensure endpoint starts with slash for absolute path
        const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
        //console.log(`DEBUG - Clean endpoint: ${cleanEndpoint}`);
        // Construct and return final URL
        const finalUrl = `${baseUrl}${cleanEndpoint}`;
        //console.log(`DEBUG - Final constructed URL: ${finalUrl}`);
        return finalUrl;
    }
};

// Helper function for API URL generation (backward compatibility)
function getApiUrl(endpoint) {
    return window.ApiConfig.getApiUrl(endpoint);
}

// Authentication and notification management service
class AuthService {
    constructor() {
        this.currentUser = null;
        this.isAuthenticated = false;
        this.notificationCount = 0;
        this.init();
    }

    async init() {
        await this.checkAuthStatus();
        this.updateUI();
        this.startNotificationPolling();
        this.bindEvents();
    }

    async checkAuthStatus() {
        try {
            const token = this.getToken();
            if (!token) {
                this.setUnauthenticated();
                return;
            }

            // Validate token with server - now correctly generates /api/Auth/validate
            const endpoint = 'api/Auth/validate';
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            //console.log(`Base URL: ${window.ApiConfig.getBaseUrl()}`);
            //console.log(`API URL: ${window.ApiConfig.getApiUrl(endpoint)}`);
            //console.log(`window.location.origin: ${window.location.origin}`);
            //console.log(`window.location.pathname: ${window.location.pathname}`);
            //console.log(`Endpoint parameter: ${endpoint}`);
            //console.log(`Final API URL: ${apiUrl}`);
            //console.log(`Checking auth status with token: ${token}`);
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const userData = await response.json();
                this.setAuthenticated({
                    id: userData.userId,
                    name: userData.username,
                    email: userData.email || '',
                    isAdmin: userData.isAdmin || false,
                    profilePicture: userData.profilePicture
                });
            } else {
                this.setUnauthenticated();
                this.removeToken();
            }
        } catch (error) {
            console.error('Auth check failed:', error);
            this.setUnauthenticated();
        }
    }

    getToken() {
        // Check localStorage first, then cookies
        let token = localStorage.getItem('jwtToken');
        if (!token) {
            token = this.getCookie('jwtToken');
        }
        return token;
    }

    getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    setAuthenticated(userData) {
        this.isAuthenticated = true;
        this.currentUser = userData;
        this.updateNotificationCount();
    }

    setUnauthenticated() {
        this.isAuthenticated = false;
        this.currentUser = null;
        this.notificationCount = 0;
    }

    removeToken() {
        localStorage.removeItem('jwtToken');
        document.cookie = 'jwtToken=; Max-Age=0; path=/;';
    }

    async updateNotificationCount() {
        if (!this.isAuthenticated) return;

        try {
            const apiUrl = window.ApiConfig.getApiUrl('Notifications?handler=UnreadCount');
            const response = await fetch(apiUrl, {
                headers: {
                    'Authorization': `Bearer ${this.getToken()}`
                }
            });
            if (response.ok) {
                const data = await response.json();
                this.notificationCount = data.count || 0;
                this.updateNotificationBadge();
            }
        } catch (error) {
            console.error('Failed to update notification count:', error);
        }
    }

    updateNotificationBadge() {
        const badges = document.querySelectorAll('.notification-badge, .badge');
        badges.forEach(badge => {
            if (this.notificationCount > 0) {
                badge.textContent = this.notificationCount;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        });
    }

    updateUI() {
        this.updateNavigation();
        this.updateHeader();
        this.updateSidebar();
    }

    updateNavigation() {
        const authElements = document.querySelectorAll('[data-auth-required]');
        const guestElements = document.querySelectorAll('[data-guest-only]');

        authElements.forEach(element => {
            element.style.display = this.isAuthenticated ? 'block' : 'none';
        });

        guestElements.forEach(element => {
            element.style.display = this.isAuthenticated ? 'none' : 'block';
        });

        // Update user name displays
        if (this.isAuthenticated && this.currentUser) {
            const userNameElements = document.querySelectorAll('[data-user-name]');
            userNameElements.forEach(element => {
                element.textContent = this.currentUser.name || this.currentUser.username;
            });
        }
    }

    updateHeader() {
        const header = document.querySelector('header nav');
        if (!header) return;

        // Clear existing content
        header.innerHTML = '';

        // Home link
        const homeLink = document.createElement('a');
        homeLink.className = 'nav-btn';
        homeLink.href = '/';
        homeLink.textContent = 'Home';
        header.appendChild(homeLink);

        if (this.isAuthenticated && this.currentUser) {
            // User profile link
            const profileLink = document.createElement('a');
            profileLink.className = 'nav-btn';
            profileLink.href = '/Profile';
            profileLink.textContent = this.currentUser.name || this.currentUser.username;
            header.appendChild(profileLink);

            // Post link
            const postLink = document.createElement('a');
            postLink.className = 'nav-btn';
            postLink.href = '/Post';
            postLink.textContent = 'Post';
            header.appendChild(postLink);

            // Notifications with dropdown
            const notificationContainer = document.createElement('div');
            notificationContainer.className = 'notification-container';
            
            const notificationBtn = document.createElement('button');
            notificationBtn.className = 'nav-btn notification-btn';
            notificationBtn.innerHTML = `
                <span class="icon-bell">🔔</span>
                Notifications
                <span class="badge notification-badge" style="display: ${this.notificationCount > 0 ? 'inline-block' : 'none'}">${this.notificationCount}</span>
            `;
            notificationBtn.onclick = () => this.toggleNotificationDropdown();
            
            notificationContainer.appendChild(notificationBtn);
            
            // Enhanced Notification dropdown
            const dropdown = document.createElement('div');
            dropdown.className = 'notification-dropdown enhanced-dropdown';
            dropdown.id = 'notificationDropdown';
            dropdown.innerHTML = `
                <div class="dropdown-header">
                    <div class="header-content">
                        <h6><i class="fas fa-bell"></i> Notifications</h6>
                        <div class="header-actions">
                            <button class="mark-all-read-btn" onclick="authService.markAllNotificationsRead()" title="Mark All Read">
                                <i class="fas fa-check-double"></i>
                            </button>
                            <a href="/Notifications" class="settings-btn" title="Notification Settings">
                                <i class="fas fa-cog"></i>
                            </a>
                        </div>
                    </div>
                </div>
                <div class="dropdown-content" id="notificationContent">
                    <div class="loading">
                        <i class="fas fa-spinner fa-spin"></i>
                        Loading notifications...
                    </div>
                </div>
                <div class="dropdown-footer">
                    <a href="/Notifications" class="view-all-btn">
                        <i class="fas fa-list"></i>
                        View All Notifications
                    </a>
                </div>
            `;
            
            notificationContainer.appendChild(dropdown);
            header.appendChild(notificationContainer);

            // Admin link
            if (this.currentUser.isAdmin) {
                const adminLink = document.createElement('a');
                adminLink.className = 'nav-btn';
                adminLink.href = '/Admin';
                adminLink.textContent = 'Admin';
                header.appendChild(adminLink);
            }

            // Logout button
            const logoutBtn = document.createElement('button');
            logoutBtn.className = 'nav-btn';
            logoutBtn.textContent = 'Logout';
            logoutBtn.onclick = () => this.logout();
            header.appendChild(logoutBtn);

        } else {
            // Login link
            const loginLink = document.createElement('a');
            loginLink.className = 'nav-btn';
            loginLink.href = '/Login';
            loginLink.textContent = 'Login';
            header.appendChild(loginLink);

            // Register link
            const registerLink = document.createElement('a');
            registerLink.className = 'nav-btn';
            registerLink.href = '/Register';
            registerLink.textContent = 'Register';
            header.appendChild(registerLink);
        }
    }

    updateSidebar() {
        const userProfileSection = document.getElementById('userProfileSection');
        const leftSidebar = document.getElementById('leftSidebar');
        const authNavItems = document.querySelectorAll('.nav-item[data-auth-required]');
        const guestNavItems = document.querySelectorAll('.nav-item[data-guest-only]');
        const adminNavItems = document.querySelectorAll('.nav-item[data-admin-only]');

        if (this.isAuthenticated && this.currentUser) {
            // Show authenticated sidebar
            if (leftSidebar) {
                leftSidebar.classList.add('authenticated');
            }
            
            // Show user profile section
            if (userProfileSection) {
                userProfileSection.style.display = 'block';
                
                // Update user avatar
                const userAvatar = document.getElementById('userAvatar');
                if (userAvatar) {
                    if (this.currentUser.profilePicture) {
                        userAvatar.innerHTML = `<img src="${this.currentUser.profilePicture}" style="width:100%;height:100%;border-radius:50%;object-fit:cover;" alt="alt" />`;
                    } else if (this.currentUser.name) {
                        userAvatar.textContent = "test";
                    }
                }
                
                // Update user name and email
                const userName = userProfileSection.querySelector('[data-user-name]');
                const userEmail = userProfileSection.querySelector('[data-user-email]');
                if (userName) userName.textContent = this.currentUser.name || this.currentUser.username;
                if (userEmail) userEmail.textContent = this.currentUser.email || '';
            }

            // Show auth-required items, hide guest items
            authNavItems.forEach(item => item.style.display = 'block');
            guestNavItems.forEach(item => item.style.display = 'none');
            
            // Show admin items if user is admin
            adminNavItems.forEach(item => {
                item.style.display = this.currentUser.isAdmin ? 'block' : 'none';
            });
            
        } else {
            // Show guest sidebar
            if (leftSidebar) {
                leftSidebar.classList.remove('authenticated');
            }
            
            // Hide user profile section
            if (userProfileSection) {
                userProfileSection.style.display = 'none';
            }

            // Hide auth-required items, show guest items
            authNavItems.forEach(item => item.style.display = 'none');
            guestNavItems.forEach(item => item.style.display = 'block');
            adminNavItems.forEach(item => item.style.display = 'none');
        }
    }

    async toggleNotificationDropdown() {
        const dropdown = document.getElementById('notificationDropdown');
        if (!dropdown) return;

        const isVisible = dropdown.classList.contains('show');
        
        if (isVisible) {
            dropdown.classList.remove('show');
        } else {
            dropdown.classList.add('show');
            await this.loadRecentNotifications();
        }
    }

    async loadRecentNotifications() {
        const content = document.getElementById('notificationContent');
        if (!content) return;

        try {
            content.innerHTML = `
                <div class="loading">
                    <i class="fas fa-spinner fa-spin"></i>
                    Loading notifications...
                </div>
            `;
            
            console.log('[AuthService] Loading recent notifications...');
            const apiUrl = window.ApiConfig.getApiUrl('Notifications?handler=RecentNotifications');
            console.log('[AuthService] API URL:', apiUrl);
            console.log('[AuthService] Token:', this.getToken());
            
            const response = await fetch(apiUrl, {
                headers: {
                    'Authorization': `Bearer ${this.getToken()}`,
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });
            
            console.log('[AuthService] Response status:', response.status);
            console.log('[AuthService] Response ok:', response.ok);
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error('[AuthService] API Error:', errorText);
                throw new Error(`Failed to load notifications: ${response.status} - ${errorText}`);
            }
            
            const data = await response.json();
            console.log('[AuthService] Response data:', data);
            
            if (data.success && data.notifications && data.notifications.length > 0) {
                console.log('[AuthService] Found', data.notifications.length, 'notifications');
                content.innerHTML = data.notifications.map(notification => `
                    <div class="notification-item ${notification.isRead ? 'read' : 'unread'}" data-id="${notification.id}">
                        <div class="notification-icon">
                            ${this.getNotificationIcon(notification.type)}
                        </div>
                        <div class="notification-content">
                            <div class="notification-title">${notification.title}</div>
                            <div class="notification-message">${notification.message}</div>
                            <div class="notification-meta">
                                <span class="notification-time">
                                    <i class="fas fa-clock"></i>
                                    ${this.formatTime(notification.createdAt)}
                                </span>
                                ${notification.fromUserName ? `<span class="notification-from">by ${notification.fromUserName}</span>` : ''}
                            </div>
                        </div>
                        <div class="notification-actions">
                            ${!notification.isRead ? `
                                <button class="mark-read-btn-small" onclick="authService.markNotificationRead(${notification.id})" title="Mark as Read">
                                    <i class="fas fa-check"></i>
                                </button>
                            ` : ''}
                            ${notification.actionUrl ? `
                                <a href="${notification.actionUrl}" class="view-btn-small" title="View">
                                    <i class="fas fa-external-link-alt"></i>
                                </a>
                            ` : ''}
                        </div>
                    </div>
                `).join('');
            } else {
                console.log('[AuthService] No notifications found or data.success is false');
                content.innerHTML = `
                    <div class="no-notifications">
                        <i class="fas fa-bell-slash"></i>
                        <p>No recent notifications</p>
                        <small>When you get notifications, they'll appear here.</small>
                    </div>
                `;
            }
        } catch (error) {
            console.error('[AuthService] Error loading notifications:', error);
            content.innerHTML = `
                <div class="error">
                    <i class="fas fa-exclamation-triangle"></i>
                    <p>Failed to load notifications</p>
                    <small>${error.message}</small>
                    <button onclick="authService.loadRecentNotifications()" class="retry-btn">
                        <i class="fas fa-redo"></i> Retry
                    </button>
                </div>
            `;
        }
    }

    async markNotificationRead(notificationId) {
        try {
            const apiUrl = window.ApiConfig.getApiUrl('Notifications?handler=MarkAsRead');
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify(notificationId)
            });

            if (response.ok) {
                const notificationItem = document.querySelector(`[data-id="${notificationId}"]`);
                if (notificationItem) {
                    notificationItem.classList.remove('unread');
                    notificationItem.classList.add('read');
                    const markBtn = notificationItem.querySelector('.mark-read-btn-small');
                    if (markBtn) markBtn.remove();
                }
                this.updateNotificationCount();
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    async markAllNotificationsRead() {
        try {
            const apiUrl = window.ApiConfig.getApiUrl('Notifications?handler=MarkAllAsRead');
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (response.ok) {
                this.notificationCount = 0;
                this.updateNotificationBadge();
                await this.loadRecentNotifications();
            }
        } catch (error) {
            console.error('Error marking all notifications as read:', error);
        }
    }

    async logout() {
        try {
            this.removeToken();
            this.setUnauthenticated();
            this.updateUI();
            window.location.href = '/Login';
        } catch (error) {
            console.error('Logout error:', error);
        }
    }

    getNotificationIcon(type) {
        const iconMap = {
            'Like': '<i class="fas fa-heart text-danger"></i>',
            'Comment': '<i class="fas fa-comment text-primary"></i>',
            'Follow': '<i class="fas fa-user-plus text-success"></i>',
            'NewPost': '<i class="fas fa-newspaper text-info"></i>',
            'PostShare': '<i class="fas fa-share text-warning"></i>',
            'Repost': '<i class="fas fa-retweet text-success"></i>',
            'RepostLike': '<i class="fas fa-heart text-danger"></i>',
            'RepostComment': '<i class="fas fa-comment text-primary"></i>',
            'CommentReply': '<i class="fas fa-reply text-info"></i>',
            'AdminMessage': '<i class="fas fa-shield-alt text-warning"></i>',
            'SystemUpdate': '<i class="fas fa-cog text-secondary"></i>',
            'SecurityAlert': '<i class="fas fa-exclamation-triangle text-danger"></i>'
        };
        return iconMap[type] || '<i class="fas fa-bell text-secondary"></i>';
    }

    bindEvents() {
        // Close notification dropdown when clicking outside
        document.addEventListener('click', (e) => {
            const dropdown = document.getElementById('notificationDropdown');
            const notificationBtn = document.querySelector('.notification-btn');
            
            if (dropdown && !dropdown.contains(e.target) && !notificationBtn?.contains(e.target)) {
                dropdown.classList.remove('show');
            }
        });

        // Handle logout button clicks
        document.addEventListener('click', (e) => {
            if (e.target.closest('#logoutBtn') || e.target.id === 'logoutBtn') {
                e.preventDefault();
                this.logout();
            }
        });

        // Handle responsive navigation
        const mobileMenuToggle = document.getElementById('mobileMenuToggle');
        const leftSidebar = document.querySelector('.left-sidebar');
        
        if (mobileMenuToggle && leftSidebar) {
            mobileMenuToggle.addEventListener('click', () => {
                leftSidebar.classList.toggle('open');
            });
        }
    }

    startNotificationPolling() {
        if (!this.isAuthenticated) return;
        
        // Poll for notifications every 30 seconds
        setInterval(() => {
            this.updateNotificationCount();
        }, 30000);
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

    getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }
}

// Initialize authentication service
let authService;
document.addEventListener('DOMContentLoaded', () => {
    authService = new AuthService();
});
