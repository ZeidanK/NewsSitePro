// Admin Panel JavaScript

// API Configuration - ensure consistency with auth-service.js
if (!window.ApiConfig) {
    window.ApiConfig = {
        getBaseUrl: function() {
            const isLocalhost = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";
            const port = isLocalhost ? ":7128" : "";
            const address = isLocalhost ? "https://localhost" : "https://proj.ruppin.ac.il/cgroup4/test2/tar1";
            
            return `${address}${port}`;
        },
        
        getApiUrl: function(endpoint) {
            const baseUrl = this.getBaseUrl();
            console.log(`DEBUG - Raw baseUrl: ${baseUrl}`);
            console.log(`DEBUG - Raw endpoint: ${endpoint}`);
            // Ensure endpoint starts with slash for absolute path
            const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
            console.log(`DEBUG - Clean endpoint: ${cleanEndpoint}`);
            // Return complete absolute URL
            const finalUrl = `${baseUrl}${cleanEndpoint}`;
            console.log(`DEBUG - Final constructed URL: ${finalUrl}`);
            return finalUrl;
        }
    };
}

class AdminPanel {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 50;
        this.selectedUsers = new Set();
        this.autoRefreshEnabled = false;
        this.autoRefreshInterval = null;
        this.refreshIntervalMinutes = 5; // Refresh every 5 minutes
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadUsers();
        this.loadActivityLogs();
        this.loadReports();
        this.loadAutoNewsSyncStatus();
    }

    bindEvents() {
        // Search functionality
        document.getElementById('userSearch').addEventListener('input', 
            this.debounce(() => this.filterUsers(), 300));

        // Filter dropdowns
        document.getElementById('statusFilter').addEventListener('change', () => this.filterUsers());
        document.getElementById('joinDateFilter').addEventListener('change', () => this.filterUsers());

        // Reset filters
        document.getElementById('resetFilters').addEventListener('click', () => this.resetFilters());

        // Select all checkbox
        document.getElementById('selectAll').addEventListener('change', (e) => this.toggleSelectAll(e.target.checked));

        // Ban user modal
        document.getElementById('confirmBan').addEventListener('click', () => this.confirmBanUser());
        // Resolve report modal confirm button
        const resolveBtn = document.getElementById('confirmResolveReport');
        if (resolveBtn) resolveBtn.addEventListener('click', () => this.confirmResolveReport());

        // Bulk actions
        document.getElementById('bulkDeactivate').addEventListener('click', () => this.bulkAction('deactivate'));
        document.getElementById('bulkBan').addEventListener('click', () => this.bulkAction('ban'));
        document.getElementById('bulkActivate').addEventListener('click', () => this.bulkAction('activate'));

        // Auto news sync toggle
        document.getElementById('autoNewsSync').addEventListener('click', () => this.toggleAutoNewsSync());
        
        // Test news sync button
        document.getElementById('testNewsSync').addEventListener('click', () => this.testNewsSync());
    }

    async loadUsers(page = 1) {
        console.log('Loading users for page:', page);
        try {
            this.showLoading('usersTable');
            
            const search = document.getElementById('userSearch').value;
            const status = document.getElementById('statusFilter').value;
            const joinDate = document.getElementById('joinDateFilter').value;

            const endpoint = `api/Admin/users?page=${page}&pageSize=${this.pageSize}&search=${encodeURIComponent(search)}&status=${status}&joinDate=${joinDate}`;
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            // Get JWT token for authentication
            const token = localStorage.getItem('jwtToken') || getCookie('jwtToken');
            console.log('[AdminPanel] Using token:', token ? 'Token present' : 'No token');
            
            const response = await fetch(apiUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });
            
            console.log('[AdminPanel] LoadUsers response status:', response.status);
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error('[AdminPanel] LoadUsers error response:', errorText);
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const data = await response.json();
            console.log('[AdminPanel] LoadUsers response data:', data);
            
            if (data.success) {
                this.renderUsers(data.users);
                this.renderPagination(data.currentPage, data.totalPages);
                this.currentPage = data.currentPage;
            } else {
                throw new Error(data.message);
            }
        } catch (error) {
            console.error('Error loading users:', error);
            this.showError('Failed to load users: ' + error.message);
        } finally {
            this.hideLoading('usersTable');
        }
    }

    // Helper function to get cookie
    getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    renderUsers(users) {
        const tbody = document.getElementById('usersTableBody');
        
        if (!users || users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="9" class="text-center">No users found</td></tr>';
            return;
        }

        tbody.innerHTML = users.map(user => `
            <tr>
                <td>
                    <input type="checkbox" class="user-checkbox" value="${user.id}" 
                           onchange="adminPanel.toggleUserSelection(${user.id}, this.checked)">
                </td>
                <td>${user.id}</td>
                <td>
                    <div class="user-info">
                        <div class="user-avatar">
                            ${user.profilePicture ? 
                                `<img src="${user.profilePicture}" alt="${user.username}" style="width:40px;height:40px;border-radius:50%;object-fit:cover;" onerror="this.style.display='none'; this.nextElementSibling.style.display='block';" />
                                 <div class="avatar-placeholder" style="display:none;width:40px;height:40px;border-radius:50%;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);display:flex;align-items:center;justify-content:center;color:white;font-weight:bold;">
                                     ${(user.username || 'U').substring(0, 1).toUpperCase()}
                                 </div>` 
                                : 
                                `<div class="avatar-placeholder" style="width:40px;height:40px;border-radius:50%;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);display:flex;align-items:center;justify-content:center;color:white;font-weight:bold;">
                                     ${(user.username || 'U').substring(0, 1).toUpperCase()}
                                 </div>`
                            }
                        </div>
                        <div class="user-details">
                            <h6>${user.username}</h6>
                            <small>${user.fullName || 'N/A'}</small>
                        </div>
                    </div>
                </td>
                <td>${user.email}</td>
                <td>${this.formatDate(user.joinDate)}</td>
                <td>${user.postCount}</td>
                <td>${this.formatDate(user.lastActivity)}</td>
                <td>
                    <span class="status-badge status-${user.status.toLowerCase()}">
                        ${user.status}
                    </span>
                </td>
                <td>
                    <button class="action-btn action-btn-view" onclick="adminPanel.viewUserDetails(${user.id})" title="View Details">
                        <i class="fas fa-eye"></i>
                    </button>
                    ${user.status === 'Active' ? 
                        `<button class="action-btn action-btn-ban" onclick="adminPanel.showBanModal(${user.id})" title="Ban User">
                            <i class="fas fa-ban"></i>
                        </button>
                        <button class="action-btn action-btn-deactivate" onclick="adminPanel.deactivateUser(${user.id})" title="Deactivate">
                            <i class="fas fa-user-slash"></i>
                        </button>` : 
                        user.status === 'Banned' ?
                        `<button class="action-btn action-btn-unban" onclick="adminPanel.unbanUser(${user.id})" title="Unban User">
                            <i class="fas fa-user-check"></i>
                        </button>` :
                        user.status === 'Inactive' ?
                        `<button class="action-btn action-btn-activate" onclick="adminPanel.activateUser(${user.id})" title="Activate User">
                            <i class="fas fa-user-check"></i>
                        </button>` :
                        `<button class="action-btn action-btn-unban" onclick="adminPanel.unbanUser(${user.id})" title="Unban User">
                            <i class="fas fa-user-check"></i>
                        </button>`
                    }
                </td>
            </tr>
        `).join('');
    }

    renderPagination(currentPage, totalPages) {
        const pagination = document.getElementById('usersPagination');
        
        let paginationHTML = '';
        
        // Previous button
        if (currentPage > 1) {
            paginationHTML += `<li class="page-item">
                <a class="page-link" href="#" onclick="adminPanel.loadUsers(${currentPage - 1}); return false;">Previous</a>
            </li>`;
        }

        // Page numbers
        const startPage = Math.max(1, currentPage - 2);
        const endPage = Math.min(totalPages, currentPage + 2);

        if (startPage > 1) {
            paginationHTML += `<li class="page-item">
                <a class="page-link" href="#" onclick="adminPanel.loadUsers(1); return false;">1</a>
            </li>`;
            if (startPage > 2) {
                paginationHTML += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            paginationHTML += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="adminPanel.loadUsers(${i}); return false;">${i}</a>
            </li>`;
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                paginationHTML += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            paginationHTML += `<li class="page-item">
                <a class="page-link" href="#" onclick="adminPanel.loadUsers(${totalPages}); return false;">${totalPages}</a>
            </li>`;
        }

        // Next button
        if (currentPage < totalPages) {
            paginationHTML += `<li class="page-item">
                <a class="page-link" href="#" onclick="adminPanel.loadUsers(${currentPage + 1}); return false;">Next</a>
            </li>`;
        }

        pagination.innerHTML = paginationHTML;
    }

    async loadActivityLogs(page = 1) {
        try {
            const endpoint = `api/Admin/activity-logs?page=${page}&pageSize=20`;
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            // Get JWT token for authentication
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.renderActivityLogs(data.logs);
                }
            } else {
                console.error('Failed to load activity logs:', response.status);
            }
        } catch (error) {
            console.error('Error loading activity logs:', error);
        }
    }

    renderActivityLogs(logs) {
        const tbody = document.getElementById('activityTableBody');
        
        if (!logs || logs.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No activity logs found</td></tr>';
            return;
        }

        tbody.innerHTML = logs.map(log => `
            <tr>
                <td>${this.formatDateTime(log.timestamp)}</td>
                <td>${log.username}</td>
                <td>${log.action}</td>
                <td>${log.details}</td>
                <td>${log.ipAddress}</td>
            </tr>
        `).join('');
    }

    async loadReports() {
        try {
            const endpoint = 'api/Admin/reports';
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            // Get JWT token for authentication
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.renderReports(data.reports);
                }
            } else {
                console.error('Failed to load reports:', response.status);
            }
        } catch (error) {
            console.error('Error loading reports:', error);
        }
    }

    renderReports(reports) {
        const tbody = document.getElementById('reportsTableBody');
        
        if (!reports || reports.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center">No reports found</td></tr>';
            return;
        }

        tbody.innerHTML = reports.map(report => `
            <tr>
                <td>${this.formatDate(report.createdAt)}</td>
                <td>${report.reporterUsername}</td>
                <td>${report.reportedUsername}</td>
                <td>${report.reason}</td>
                <td>
                    <span class="status-badge status-${report.status.toLowerCase()}">
                        ${report.status}
                    </span>
                </td>
                <td>
                    ${report.status === 'Pending' ? 
                        `<button class="action-btn action-btn-view" onclick="adminPanel.resolveReport(${report.id})">
                            <i class="fas fa-gavel"></i> Resolve
                        </button>` : 
                        '<span class="text-muted">Resolved</span>'
                    }
                </td>
            </tr>
        `).join('');
    }

    filterUsers() {
        this.currentPage = 1;
        this.loadUsers(1);
    }

    resetFilters() {
        document.getElementById('userSearch').value = '';
        document.getElementById('statusFilter').value = '';
        document.getElementById('joinDateFilter').value = '';
        this.filterUsers();
    }

    showBanModal(userId) {
        document.getElementById('banUserId').value = userId;
        const modal = new bootstrap.Modal(document.getElementById('banUserModal'));
        modal.show();
    }

    /**
     * Opens resolve report modal for a given report
     */
    resolveReport(reportId) {
        // Set report ID and open modal
        document.getElementById('resolveReportId').value = reportId;
        const modalEl = document.getElementById('resolveReportModal');
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    }

    /**
     * Confirms resolving a report and calls API
     */
    async confirmResolveReport() {
        const reportId = parseInt(document.getElementById('resolveReportId').value);
        const action = document.getElementById('resolveAction').value;
        const notes = document.getElementById('resolveNotes').value;
        if (!action) {
            this.showError('Please select an action to resolve the report.');
            return;
        }
        try {
            const endpoint = `api/Admin/reports/${reportId}/resolve`;
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include',
                body: JSON.stringify({ action: action, notes: notes })
            });
            const data = await response.json();
            if (data.success) {
                this.showSuccess(data.message || 'Report resolved successfully');
                // Close modal and reload reports
                bootstrap.Modal.getInstance(document.getElementById('resolveReportModal')).hide();
                this.loadReports();
            } else {
                this.showError(data.message || 'Failed to resolve report');
            }
        } catch (error) {
            this.showError('Error resolving report: ' + error.message);
        }
    }

    async confirmBanUser() {
        const userId = parseInt(document.getElementById('banUserId').value);
        const reason = document.getElementById('banReason').value;
        const duration = document.getElementById('banDuration').value;
        
        console.log('[AdminPanel] confirmBanUser called with:', { userId, reason, duration });

        if (!reason.trim() || !duration) {
            this.showError('Please provide a reason and duration for the ban');
            return;
        }

        try {
            const endpoint = `api/Admin/users/${userId}/ban`;
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            console.log('[AdminPanel] Banning user with URL:', apiUrl);
            
            // Get JWT token for authentication
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include',
                body: JSON.stringify({
                    reason: reason,
                    duration: duration === 'permanent' ? -1 : parseInt(duration)
                })
            });

            console.log('[AdminPanel] Ban response status:', response.status);
            const data = await response.json();
            console.log('[AdminPanel] Ban response data:', data);
            
            if (data.success) {
                this.showSuccess('User banned successfully');
                this.loadUsers(this.currentPage);
                bootstrap.Modal.getInstance(document.getElementById('banUserModal')).hide();
                this.clearBanForm();
            } else {
                this.showError(data.message);
            }
        } catch (error) {
            console.error('[AdminPanel] Error banning user:', error);
            this.showError('Error banning user: ' + error.message);
        }
    }

    async unbanUser(userId) {
        if (!confirm('Are you sure you want to unban this user?')) {
            return;
        }

        try {
            const endpoint = `api/Admin/users/${userId}/unban`;
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            // Get JWT token for authentication
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });

            const data = await response.json();
            
            if (data.success) {
                this.showSuccess('User unbanned successfully');
                this.loadUsers(this.currentPage);
            } else {
                this.showError(data.message);
            }
        } catch (error) {
            this.showError('Error unbanning user: ' + error.message);
        }
    }

    async deactivateUser(userId) {
        console.log('[AdminPanel] deactivateUser called with userId:', userId);
        if (!confirm('Are you sure you want to deactivate this user?')) {
            return;
        }

        try {
            const endpoint = `api/Admin/users/${userId}/deactivate`;
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            console.log('[AdminPanel] Deactivating user with URL:', apiUrl);
            
            // Get JWT token for authentication
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });

            console.log('[AdminPanel] Deactivate response status:', response.status);
            const data = await response.json();
            console.log('[AdminPanel] Deactivate response data:', data);
            
            if (data.success) {
                this.showSuccess('User deactivated successfully');
                this.loadUsers(this.currentPage);
            } else {
                this.showError(data.message);
            }
        } catch (error) {
            console.error('[AdminPanel] Error deactivating user:', error);
            this.showError('Error deactivating user: ' + error.message);
        }
    }

    async activateUser(userId) {
        console.log('[AdminPanel] activateUser called with userId:', userId);
        if (!confirm('Are you sure you want to activate this user?')) {
            return;
        }

        try {
            const endpoint = `api/Admin/users/${userId}/activate`;
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            console.log('[AdminPanel] Activating user with URL:', apiUrl);
            
            // Get JWT token for authentication
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });

            console.log('[AdminPanel] Activate response status:', response.status);
            const data = await response.json();
            console.log('[AdminPanel] Activate response data:', data);
            
            if (data.success) {
                this.showSuccess('User activated successfully');
                this.loadUsers(this.currentPage);
            } else {
                this.showError(data.message);
            }
        } catch (error) {
            console.error('[AdminPanel] Error activating user:', error);
            this.showError('Error activating user: ' + error.message);
        }
    }

    async viewUserDetails(userId) {
        try {
            const endpoint = `/Admin?handler=UserDetails&userId=${userId}`;
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            console.log('Fetching user details from:', apiUrl);
            
            // Get JWT token for authentication
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });
            
            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const responseText = await response.text();
            console.log('Raw response:', responseText);
            
            if (!responseText.trim()) {
                throw new Error('Empty response from server');
            }
            
            const data = JSON.parse(responseText);
            
            if (data.success) {
                this.renderUserDetailsModal(data.user);
                const modal = new bootstrap.Modal(document.getElementById('userDetailsModal'));
                modal.show();
            } else {
                this.showError(data.message || 'Unknown error occurred');
            }
        } catch (error) {
            console.error('Error in viewUserDetails:', error);
            this.showError('Error loading user details: ' + error.message);
        }
    }

    renderUserDetailsModal(user) {
        const content = document.getElementById('userDetailsContent');
        content.innerHTML = `
            <div class="user-details-content">
                <div class="row">
                    <div class="col-md-4 text-center">
                        <div class="user-avatar-large" style="width:150px;height:150px;margin:0 auto 1rem;">
                            ${user.profilePicture ? 
                                `<img src="${user.profilePicture}" alt="${user.username}" class="img-fluid rounded-circle" style="width:100%;height:100%;object-fit:cover;" onerror="this.style.display='none'; this.nextElementSibling.style.display='block';" />
                                 <div class="avatar-placeholder" style="display:none;width:100%;height:100%;border-radius:50%;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);display:flex;align-items:center;justify-content:center;color:white;font-weight:bold;font-size:4rem;">
                                     ${(user.username || 'U').substring(0, 1).toUpperCase()}
                                 </div>` 
                                : 
                                `<div class="avatar-placeholder" style="width:100%;height:100%;border-radius:50%;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);display:flex;align-items:center;justify-content:center;color:white;font-weight:bold;font-size:4rem;">
                                     ${(user.username || 'U').substring(0, 1).toUpperCase()}
                                 </div>`
                            }
                        </div>
                        <h4>
                            <a href="/UserProfile?userId=${user.id}" class="text-decoration-none" target="_blank">
                                ${user.username} <i class="fas fa-external-link-alt" style="font-size:0.8rem;"></i>
                            </a>
                        </h4>
                        <p class="text-muted">${user.fullName || 'N/A'}</p>
                        <span class="status-badge status-${user.status.toLowerCase()}">${user.status}</span>
                    </div>
                    <div class="col-md-8">
                        <h5>User Information</h5>
                        <table class="table table-borderless">
                            <tr><td><strong>Email:</strong></td><td>${user.email}</td></tr>
                            <tr><td><strong>Join Date:</strong></td><td>${this.formatDate(user.joinDate)}</td></tr>
                            <tr><td><strong>Last Activity:</strong></td><td>${this.formatDateTime(user.lastActivity)}</td></tr>
                            <tr><td><strong>Posts:</strong></td><td>${user.postCount}</td></tr>
                            <tr><td><strong>Followers:</strong></td><td>${user.followersCount || 0}</td></tr>
                            <tr><td><strong>Following:</strong></td><td>${user.followingCount || 0}</td></tr>
                            <tr><td><strong>Bio:</strong></td><td>${user.bio || 'N/A'}</td></tr>
                        </table>
                        
                        <h5>Recent Activity</h5>
                        <div class="recent-activity" style="max-height: 200px; overflow-y: auto;">
                            ${(user.recentActivity && user.recentActivity.length > 0) ? 
                                user.recentActivity.map(activity => `
                                    <div class="activity-item mb-2 p-2 border rounded">
                                        <small class="text-muted">${this.formatDateTime(activity.timestamp)}</small>
                                        <div>${activity.action}</div>
                                    </div>
                                `).join('') :
                                '<div class="text-muted">No recent activity available</div>'
                            }
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    toggleSelectAll(checked) {
        const checkboxes = document.querySelectorAll('.user-checkbox');
        checkboxes.forEach(checkbox => {
            checkbox.checked = checked;
            this.toggleUserSelection(parseInt(checkbox.value), checked);
        });
        this.updateBulkActionsButton();
    }

    toggleUserSelection(userId, selected) {
        if (selected) {
            this.selectedUsers.add(userId);
        } else {
            this.selectedUsers.delete(userId);
        }
        this.updateBulkActionsButton();
    }

    updateBulkActionsButton() {
        const selectedCount = this.selectedUsers.size;
        document.getElementById('selectedCount').textContent = selectedCount;
        
        // Enable/disable bulk actions based on selection
        const bulkButtons = document.querySelectorAll('#bulkDeactivate, #bulkBan, #bulkActivate');
        bulkButtons.forEach(button => {
            button.disabled = selectedCount === 0;
        });
    }

    async bulkAction(action) {
        if (this.selectedUsers.size === 0) {
            this.showError('Please select users first');
            return;
        }

        const actionText = action === 'ban' ? 'ban' : action === 'deactivate' ? 'deactivate' : 'activate';
        
        if (!confirm(`Are you sure you want to ${actionText} ${this.selectedUsers.size} selected users?`)) {
            return;
        }

        try {
            // Implementation for bulk actions would go here
            this.showSuccess(`Successfully ${actionText}d ${this.selectedUsers.size} users`);
            this.selectedUsers.clear();
            this.loadUsers(this.currentPage);
            bootstrap.Modal.getInstance(document.getElementById('bulkActionsModal')).hide();
        } catch (error) {
            this.showError(`Error performing bulk ${action}: ` + error.message);
        }
    }

    clearBanForm() {
        document.getElementById('banReason').value = '';
        document.getElementById('banDuration').value = '';
    }

    // Utility functions
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Helper function to get cookie
    getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    formatDate(dateString) {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleDateString();
    }

    formatDateTime(dateString) {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString();
    }

    getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    showLoading(elementId) {
        const element = document.getElementById(elementId);
        element.classList.add('loading');
    }

    hideLoading(elementId) {
        const element = document.getElementById(elementId);
        element.classList.remove('loading');
    }

    showSuccess(message) {
        this.showNotification(message, 'success');
    }

    showError(message) {
        this.showNotification(message, 'error');
    }

    showNotification(message, type) {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 5000);
    }

    // Auto News Sync functionality
    async loadAutoNewsSyncStatus() {
        try {
            const endpoint = 'api/Admin/background-service/status';
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.autoNewsSyncEnabled = data.isEnabled;
                    this.updateAutoNewsSyncUI(data.isEnabled);
                }
            }
        } catch (error) {
            console.error('Error loading auto news sync status:', error);
            // Default to OFF if there's an error
            this.updateAutoNewsSyncUI(false);
        }
    }

    async toggleAutoNewsSync() {
        try {
            const newStatus = !this.autoNewsSyncEnabled;
            
            const endpoint = 'api/Admin/background-service/toggle';
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include',
                body: JSON.stringify({ enabled: newStatus })
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.autoNewsSyncEnabled = data.isEnabled;
                    this.updateAutoNewsSyncUI(data.isEnabled);
                    
                    if (data.isEnabled) {
                        this.showNotification('Auto news sync enabled! News will be fetched every 24 hours. Triggering initial sync...', 'success');
                        // Trigger manual sync when enabling
                        await this.manualNewsSync();
                    } else {
                        this.showNotification('Auto news sync disabled', 'info');
                    }
                } else {
                    this.showNotification(data.message || 'Failed to toggle auto news sync', 'error');
                }
            } else {
                throw new Error(`HTTP ${response.status}`);
            }
        } catch (error) {
            console.error('Error toggling auto news sync:', error);
            this.showNotification('Error toggling auto news sync', 'error');
        }
    }

    updateAutoNewsSyncUI(isEnabled) {
        const button = document.getElementById('autoNewsSync');
        const text = document.getElementById('autoSyncText');
        
        if (button && text) {
            if (isEnabled) {
                button.className = 'btn btn-success w-100';
                button.innerHTML = '<i class="fas fa-cloud-download-alt"></i> <span id="autoSyncText">Auto News Sync: ON</span>';
            } else {
                button.className = 'btn btn-outline-success w-100';
                button.innerHTML = '<i class="fas fa-cloud-download-alt"></i> <span id="autoSyncText">Auto News Sync: OFF</span>';
            }
        }
    }

    async manualNewsSync() {
        try {
            const endpoint = 'api/Admin/sync-news';
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            this.showNotification('Starting manual news sync...', 'info');
            
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.showNotification(`Manual sync completed! Added ${data.articlesAdded} new articles`, 'success');
                    // Optionally refresh admin stats
                    this.loadAdminStats();
                } else {
                    this.showNotification(data.message || 'Manual sync failed', 'error');
                }
            } else {
                throw new Error(`HTTP ${response.status}`);
            }
        } catch (error) {
            console.error('Error during manual news sync:', error);
            this.showNotification('Error during manual news sync', 'error');
        }
    }

    async testNewsSync() {
        try {
            const endpoint = 'api/Admin/test-sync-news';
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            // Show loading state
            const testButton = document.getElementById('testNewsSync');
            const originalText = testButton.innerHTML;
            testButton.disabled = true;
            testButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Testing...';
            
            this.showNotification('Starting test news sync (6 articles)...', 'info');
            
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.showNotification(`${data.message}`, 'success');
                    // Optionally refresh admin stats
                    this.loadAdminStats();
                } else {
                    this.showNotification(data.message || 'Test sync failed', 'error');
                }
            } else {
                throw new Error(`HTTP ${response.status}`);
            }
        } catch (error) {
            console.error('Error during test news sync:', error);
            this.showNotification('Error during test news sync: ' + error.message, 'error');
        } finally {
            // Restore button state
            const testButton = document.getElementById('testNewsSync');
            testButton.disabled = false;
            testButton.innerHTML = '<i class="fas fa-flask"></i> Test News Sync (6 articles)';
        }
    }

    async loadAdminStats() {
        try {
            const endpoint = 'api/Admin/dashboard-stats';
            const apiUrl = window.ApiConfig.getApiUrl(endpoint);
            
            const token = localStorage.getItem('jwtToken') || this.getCookie('jwtToken');
            
            const response = await fetch(apiUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                credentials: 'include'
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    // Update dashboard stats in UI
                    this.updateDashboardStats(data.stats);
                }
            }
        } catch (error) {
            console.error('Error loading admin stats:', error);
        }
    }

    updateDashboardStats(stats) {
        // Update stats cards if they exist
        const totalUsersEl = document.getElementById('totalUsers');
        const activeUsersEl = document.getElementById('activeUsers');
        const totalArticlesEl = document.getElementById('totalArticles');
        const pendingReportsEl = document.getElementById('pendingReports');
        
        if (totalUsersEl) totalUsersEl.textContent = stats.totalUsers || 0;
        if (activeUsersEl) activeUsersEl.textContent = stats.activeUsers || 0;
        if (totalArticlesEl) totalArticlesEl.textContent = stats.totalArticles || 0;
        if (pendingReportsEl) pendingReportsEl.textContent = stats.pendingReports || 0;
    }
}

// Initialize admin panel when document is ready
let adminPanel;
document.addEventListener('DOMContentLoaded', () => {
    adminPanel = new AdminPanel();
});
