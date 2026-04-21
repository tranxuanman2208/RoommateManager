using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomateManager.Helpers
{
    public static class User
    {
        public static string? CurrentUserId { get; set; }
        public static string? CurrentUserName { get; set; }
        public static bool IsAdmin { get; set; }

        public static void Clear()
        {
            CurrentUserId = null;
            CurrentUserName = null;
            IsAdmin = false;
        }
    }
}
