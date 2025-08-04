/**
 * SystemSettingsOptions.cs
 * Purpose: Configuration options for system-level settings
 * Used for dependency injection of system configuration values
 * 
 * Author: System
 * Date: 2025-01-27
 */

namespace NewsSitePro.Models
{
    /// <summary>
    /// Configuration options for system-level settings
    /// Maps to the "SystemSettings" section in appsettings.json
    /// </summary>
    public class SystemSettingsOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "SystemSettings";

        /// <summary>
        /// Default system user ID for system-generated content
        /// Used when no specific user context is available
        /// </summary>
        public int DefaultSystemUserId { get; set; } = 1;

        /// <summary>
        /// System user display name
        /// </summary>
        public string SystemUserName { get; set; } = "System";

        /// <summary>
        /// Fallback user ID for admin operations when no admin is logged in
        /// </summary>
        public int AdminFallbackUserId { get; set; } = 1;

        /// <summary>
        /// Validates that the configuration values are valid
        /// </summary>
        public bool IsValid()
        {
            return DefaultSystemUserId > 0 && 
                   AdminFallbackUserId > 0 && 
                   !string.IsNullOrWhiteSpace(SystemUserName);
        }
    }
}
