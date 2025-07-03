using NewsSite.BL;

namespace NewsSitePro.Models
{
    public class HeaderViewModel
    {
        public string UserName { get; set; }
        public int NotificationCount { get; set; }
        public string CurrentPage { get; set; }

        // Add a property to hold the user object
        public User User { get; set; }
    }
}
