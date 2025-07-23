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
