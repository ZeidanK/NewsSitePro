using NewsSite.BL;

namespace NewsSitePro.Models
{
    public class HeaderViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int NotificationCount { get; set; }
        public string CurrentPage { get; set; } = string.Empty;

        // Add a property to hold the user object
        public User? user { get; set; }
    }
}