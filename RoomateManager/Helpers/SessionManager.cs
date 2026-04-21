
namespace RoomateManager.Helpers
{
    public static class SessionManager
    {
        public static string? CurrentUserId { get; set; }
        public static string? CurrentUserName { get; set; }
        public static bool IsAdmin { get; set; }
        public static bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUserId);

        public static void Clear()
        {
            CurrentUserId = null;
            CurrentUserName = null;
            IsAdmin = false;
        }
    }
}