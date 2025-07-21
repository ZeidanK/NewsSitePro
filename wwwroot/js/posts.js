// Post interaction functions

// Toggle like on a post
async function toggleLike(postId) {
    try {
        const response = await fetch(`/api/posts/${postId}/like`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include'
        });

        if (response.ok) {
            const result = await response.json();
            
            // Update like button and count
            const likeBtn = document.querySelector(`[data-post-id="${postId}"] .like-btn`);
            const likesCount = document.querySelector(`[data-post-id="${postId}"] .likes-count`);
            
            if (result.isLiked) {
                likeBtn.classList.add('liked');
                likeBtn.querySelector('span').textContent = 'Unlike';
            } else {
                likeBtn.classList.remove('liked');
                likeBtn.querySelector('span').textContent = 'Like';
            }
            
            likesCount.textContent = `${result.likeCount} ${result.likeCount === 1 ? 'like' : 'likes'}`;
        } else {
            console.error('Failed to toggle like');
        }
    } catch (error) {
        console.error('Error toggling like:', error);
    }
}

// Toggle comments section visibility
function toggleComments(postId) {
    const commentsSection = document.getElementById(`comments-section-${postId}`);
    if (commentsSection) {
        commentsSection.classList.toggle('hidden');
        
        // Load comments if not already loaded
        if (!commentsSection.classList.contains('hidden') && 
            !commentsSection.dataset.loaded) {
            loadComments(postId);
            commentsSection.dataset.loaded = 'true';
        }
    }
}

// Load comments for a post
async function loadComments(postId) {
    try {
        const response = await fetch(`/api/posts/${postId}/comments`);
        if (response.ok) {
            const comments = await response.json();
            renderComments(postId, comments);
        } else {
            console.error('Failed to load comments');
        }
    } catch (error) {
        console.error('Error loading comments:', error);
    }
}

// Render comments in the comments section
function renderComments(postId, comments) {
    const commentsList = document.getElementById(`comments-list-${postId}`);
    if (!commentsList) return;

    commentsList.innerHTML = '';
    
    comments.forEach(comment => {
        const commentElement = createCommentElement(comment);
        commentsList.appendChild(commentElement);
    });
}

// Create a comment element
function createCommentElement(comment) {
    const commentDiv = document.createElement('div');
    commentDiv.className = 'comment';
    commentDiv.innerHTML = `
        <img src="${comment.userProfileImageURL || '/images/default-avatar.png'}" 
             alt="${comment.userName}" 
             class="comment-avatar" />
        <div class="comment-content">
            <strong>${comment.userName}</strong>
            <p>${comment.content}</p>
            <span class="comment-time">${formatDate(comment.commentDate)}</span>
        </div>
    `;
    return commentDiv;
}

// Post a new comment
async function postComment(postId) {
    const commentInput = document.getElementById(`comment-input-${postId}`);
    const content = commentInput.value.trim();
    
    if (!content) {
        alert('Please enter a comment');
        return;
    }

    try {
        const response = await fetch(`/api/posts/${postId}/comments`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
            body: JSON.stringify({ content: content })
        });

        if (response.ok) {
            const newComment = await response.json();
            
            // Add new comment to the list
            const commentsList = document.getElementById(`comments-list-${postId}`);
            const commentElement = createCommentElement(newComment);
            commentsList.insertBefore(commentElement, commentsList.firstChild);
            
            // Clear input
            commentInput.value = '';
            
            // Update comment count
            const commentsCount = document.querySelector(`[data-post-id="${postId}"] .comments-count`);
            const currentCount = parseInt(commentsCount.textContent.match(/\d+/)[0]) + 1;
            commentsCount.textContent = `${currentCount} ${currentCount === 1 ? 'comment' : 'comments'}`;
            
        } else {
            console.error('Failed to post comment');
            alert('Failed to post comment. Please try again.');
        }
    } catch (error) {
        console.error('Error posting comment:', error);
        alert('Error posting comment. Please try again.');
    }
}

// Share a post
async function sharePost(postId) {
    try {
        const response = await fetch(`/api/posts/${postId}/share`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include'
        });

        if (response.ok) {
            const result = await response.json();
            
            // Update share count
            const sharesCount = document.querySelector(`[data-post-id="${postId}"] .shares-count`);
            sharesCount.textContent = `${result.shareCount} ${result.shareCount === 1 ? 'share' : 'shares'}`;
            
            // Show success message
            showNotification('Post shared successfully!');
        } else {
            console.error('Failed to share post');
            alert('Failed to share post. Please try again.');
        }
    } catch (error) {
        console.error('Error sharing post:', error);
        alert('Error sharing post. Please try again.');
    }
}

// Report a post
async function reportPost(postId) {
    const reason = prompt('Please provide a reason for reporting this post:');
    if (!reason) return;

    try {
        const response = await fetch(`/api/posts/${postId}/report`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
            body: JSON.stringify({ reason: reason })
        });

        if (response.ok) {
            showNotification('Post reported successfully. Thank you for helping keep our community safe.');
            hidePostMenu(postId);
        } else {
            console.error('Failed to report post');
            alert('Failed to report post. Please try again.');
        }
    } catch (error) {
        console.error('Error reporting post:', error);
        alert('Error reporting post. Please try again.');
    }
}

// Toggle post menu
function togglePostMenu(postId) {
    const menu = document.getElementById(`post-menu-${postId}`);
    if (menu) {
        menu.classList.toggle('hidden');
    }
}

// Hide post menu
function hidePostMenu(postId) {
    const menu = document.getElementById(`post-menu-${postId}`);
    if (menu) {
        menu.classList.add('hidden');
    }
}

// Load more comments
async function loadMoreComments(postId) {
    try {
        const commentsSection = document.getElementById(`comments-section-${postId}`);
        const currentComments = commentsSection.querySelectorAll('.comment').length;
        
        const response = await fetch(`/api/posts/${postId}/comments?skip=${currentComments}`);
        if (response.ok) {
            const moreComments = await response.json();
            const commentsList = document.getElementById(`comments-list-${postId}`);
            
            moreComments.forEach(comment => {
                const commentElement = createCommentElement(comment);
                commentsList.appendChild(commentElement);
            });
            
            // Hide load more button if no more comments
            if (moreComments.length === 0) {
                const loadMoreBtn = commentsList.querySelector('.btn-load-more');
                if (loadMoreBtn) {
                    loadMoreBtn.style.display = 'none';
                }
            }
        }
    } catch (error) {
        console.error('Error loading more comments:', error);
    }
}

// Utility function to format dates
function formatDate(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffInSeconds = Math.floor((now - date) / 1000);
    
    if (diffInSeconds < 60) {
        return 'Just now';
    } else if (diffInSeconds < 3600) {
        const minutes = Math.floor(diffInSeconds / 60);
        return `${minutes}m ago`;
    } else if (diffInSeconds < 86400) {
        const hours = Math.floor(diffInSeconds / 3600);
        return `${hours}h ago`;
    } else {
        return date.toLocaleDateString('en-US', { 
            month: 'short', 
            day: 'numeric',
            hour: 'numeric',
            minute: '2-digit'
        });
    }
}

// Show notification
function showNotification(message) {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = 'notification';
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #4caf50;
        color: white;
        padding: 12px 24px;
        border-radius: 6px;
        z-index: 1000;
        box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        animation: slideIn 0.3s ease;
    `;
    
    document.body.appendChild(notification);
    
    // Remove after 3 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 300);
    }, 3000);
}

// Close menus when clicking outside
document.addEventListener('click', function(e) {
    if (!e.target.closest('.post-actions-menu')) {
        document.querySelectorAll('.post-menu').forEach(menu => {
            menu.classList.add('hidden');
        });
    }
});

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    
    @keyframes slideOut {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
`;
document.head.appendChild(style);
