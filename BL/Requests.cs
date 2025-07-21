using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
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
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreatePostRequest
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
    {
        public string Categories { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool WeeklyDigest { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Username must be at most 100 characters.")]
        public string Username { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio must be at most 500 characters.")]
        public string? Bio { get; set; }
    }

    public class ChangePasswordRequest
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
    {
        [StringLength(255, ErrorMessage = "Reason must be at most 255 characters.")]
        public string? Reason { get; set; }
    }
}