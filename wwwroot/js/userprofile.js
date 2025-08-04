// UserProfile specific JavaScript functions

/**
 * Fetch and display blocked users for the current user profile.
 */
async function loadBlockedUsers() {
    const container = document.getElementById('blockedUsersContainer');
    const noMsg = document.getElementById('noBlockedMessage');
    container.innerHTML = '<div class="posts-loading">Loading blocked users...</div>';
    try {
        const response = await fetch(window.ApiConfig.getApiUrl('/api/UserBlock/blocked-users'), {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        });
        const data = await response.json();
        if (response.ok && data.success) {
            const blocks = data.blockedUsers;
            if (blocks.length > 0) {
                noMsg.style.display = 'none';
                container.innerHTML = blocks.map(b => `
    <div class="blocked-user-item d-flex justify-content-between align-items-center p-2 border mb-2">
        <div>
            <strong>${b.username}</strong><br/>
            <small>Blocked on ${new Date(b.blockDate).toLocaleDateString()}</small>
        </div>
        <button class="btn btn-sm btn-success" onclick="unblockUser(${b.blockedUserID})">Unblock</button>
    </div>
`).join('');
            } else {
                container.innerHTML = '';
                noMsg.style.display = 'block';
            }
        } else {
            throw new Error(data.message || 'Failed to load blocked users');
        }
    } catch (error) {
        console.error('Error loading blocked users:', error);
        container.innerHTML = `<div class="error-state"><p>${error.message}</p></div>`;
    }
}

/**
 * Unblock a user and refresh the blocked list.
 */
async function unblockUser(userId) {
    if (!confirm('Are you sure you want to unblock this user?')) return;
    try {
        const response = await fetch(window.ApiConfig.getApiUrl('/api/UserBlock/unblock'), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ blockedUserID: userId })
        });
        const data = await response.json();
        if (response.ok && data.success) {
            loadBlockedUsers();
        } else {
            alert(data.message || 'Failed to unblock user');
        }
    } catch (error) {
        console.error('Error unblocking user:', error);
        alert('Error unblocking user: ' + error.message);
    }
}

// Export functions for inline handlers
window.loadBlockedUsers = loadBlockedUsers;
window.unblockUser = unblockUser;
