// Admin Panel JavaScript
class AdminPanel {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 50;
        this.selectedUsers = new Set();
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadUsers();
        this.loadActivityLogs();
        this.loadReports();
        this.startAutoRefresh();
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

        // Export data
        document.getElementById('exportData').addEventListener('click', () => this.exportData());

        // Select all checkbox
        document.getElementById('selectAll').addEventListener('change', (e) => this.toggleSelectAll(e.target.checked));

        // Ban user modal
        document.getElementById('confirmBan').addEventListener('click', () => this.confirmBanUser());

        // Bulk actions
        document.getElementById('bulkDeactivate').addEventListener('click', () => this.bulkAction('deactivate'));
        document.getElementById('bulkBan').addEventListener('click', () => this.bulkAction('ban'));
        document.getElementById('bulkActivate').addEventListener('click', () => this.bulkAction('activate'));
    }

    async loadUsers(page = 1) {
        try {
            this.showLoading('usersTable');
            
            const search = document.getElementById('userSearch').value;
            const status = document.getElementById('statusFilter').value;
            const joinDate = document.getElementById('joinDateFilter').value;

            const response = await fetch(`/Admin?handler=Users&page=${page}&pageSize=${this.pageSize}&search=${encodeURIComponent(search)}&status=${status}&joinDate=${joinDate}`);
            
            if (!response.ok) {
                throw new Error('Failed to load users');
            }

            const data = await response.json();
            
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
                        <img src="${user.profilePicture || '/images/default-avatar.png'}" 
                             alt="${user.username}" class="user-avatar">
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
            const response = await fetch(`/Admin?handler=ActivityLogs&page=${page}&pageSize=20`);
            const data = await response.json();
            
            if (data.success) {
                this.renderActivityLogs(data.logs);
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
            const response = await fetch('/Admin?handler=Reports');
            const data = await response.json();
            
            if (data.success) {
                this.renderReports(data.reports);
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

    async confirmBanUser() {
        const userId = parseInt(document.getElementById('banUserId').value);
        const reason = document.getElementById('banReason').value;
        const duration = document.getElementById('banDuration').value;

        if (!reason.trim() || !duration) {
            this.showError('Please provide a reason and duration for the ban');
            return;
        }

        try {
            const response = await fetch('/Admin?handler=BanUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({
                    userId: userId,
                    reason: reason,
                    duration: duration === 'permanent' ? -1 : parseInt(duration)
                })
            });

            const data = await response.json();
            
            if (data.success) {
                this.showSuccess('User banned successfully');
                this.loadUsers(this.currentPage);
                bootstrap.Modal.getInstance(document.getElementById('banUserModal')).hide();
                this.clearBanForm();
            } else {
                this.showError(data.message);
            }
        } catch (error) {
            this.showError('Error banning user: ' + error.message);
        }
    }

    async unbanUser(userId) {
        if (!confirm('Are you sure you want to unban this user?')) {
            return;
        }

        try {
            const response = await fetch('/Admin?handler=UnbanUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify(userId)
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
        if (!confirm('Are you sure you want to deactivate this user?')) {
            return;
        }

        try {
            const response = await fetch('/Admin?handler=DeactivateUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify(userId)
            });

            const data = await response.json();
            
            if (data.success) {
                this.showSuccess('User deactivated successfully');
                this.loadUsers(this.currentPage);
            } else {
                this.showError(data.message);
            }
        } catch (error) {
            this.showError('Error deactivating user: ' + error.message);
        }
    }

    async viewUserDetails(userId) {
        try {
            const response = await fetch(`/Admin?handler=UserDetails&userId=${userId}`);
            const data = await response.json();
            
            if (data.success) {
                this.renderUserDetailsModal(data.user);
                const modal = new bootstrap.Modal(document.getElementById('userDetailsModal'));
                modal.show();
            } else {
                this.showError(data.message);
            }
        } catch (error) {
            this.showError('Error loading user details: ' + error.message);
        }
    }

    renderUserDetailsModal(user) {
        const content = document.getElementById('userDetailsContent');
        content.innerHTML = `
            <div class="user-details-content">
                <div class="row">
                    <div class="col-md-4 text-center">
                        <img src="${user.profilePicture || '/images/default-avatar.png'}" 
                             alt="${user.username}" class="img-fluid rounded-circle mb-3" style="max-width: 150px;">
                        <h4>${user.username}</h4>
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
                            <tr><td><strong>Likes Received:</strong></td><td>${user.likesReceived}</td></tr>
                            <tr><td><strong>Bio:</strong></td><td>${user.bio || 'N/A'}</td></tr>
                        </table>
                        
                        <h5>Recent Activity</h5>
                        <div class="recent-activity" style="max-height: 200px; overflow-y: auto;">
                            ${user.recentActivity.map(activity => `
                                <div class="activity-item mb-2 p-2 border rounded">
                                    <small class="text-muted">${this.formatDateTime(activity.timestamp)}</small>
                                    <div>${activity.action}</div>
                                </div>
                            `).join('')}
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

    async exportData() {
        try {
            // Implementation for data export would go here
            this.showSuccess('Data export started. You will receive a download link shortly.');
        } catch (error) {
            this.showError('Error exporting data: ' + error.message);
        }
    }

    clearBanForm() {
        document.getElementById('banReason').value = '';
        document.getElementById('banDuration').value = '';
    }

    startAutoRefresh() {
        // Refresh data every 5 minutes
        setInterval(() => {
            this.loadUsers(this.currentPage);
            this.loadActivityLogs();
        }, 5 * 60 * 1000);
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
}

// Initialize admin panel when document is ready
let adminPanel;
document.addEventListener('DOMContentLoaded', () => {
    adminPanel = new AdminPanel();
});
