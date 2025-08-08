using System.ComponentModel.DataAnnotations;

// ----------------------------------------------------------------------------------
// Requests.cs
//
// This file contains request models for user actions and API endpoints in the NewsSitePro application.
// These models are used to validate and transfer data for login, registration, profile updates, post creation,
// preferences, password changes, and reporting. Comments are added to key classes for clarity.
// ----------------------------------------------------------------------------------

namespace NewsSite.Models
{
    public class LoginRequest
    // Request model for user login
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    // Request model for user registration
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    // Request model for updating user information
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreatePostRequest
    // Request model for creating a new post
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title must be at most 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(500, ErrorMessage = "Content must be at most 500 characters.")]
        public string Content { get; set; } = string.Empty;

        public string? ImageURL { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; } = string.Empty;

        public string? SourceURL { get; set; }
        public string? SourceName { get; set; }
    }

    public class UpdatePreferencesRequest
    // Request model for updating user notification and content preferences
    {
        public string Categories { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool WeeklyDigest { get; set; }
    }

    public class UpdateProfileRequest
    // Request model for updating user profile information
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Username must be at most 100 characters.")]
        public string Username { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio must be at most 500 characters.")]
        public string? Bio { get; set; }
    }

    public class ChangePasswordRequest
    // Request model for changing a user's password
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("NewPassword", ErrorMessage = "Password confirmation does not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ReportRequest
    // Request model for reporting content or users
    {
        [StringLength(255, ErrorMessage = "Reason must be at most 255 characters.")]
        public string? Reason { get; set; }
    }
}