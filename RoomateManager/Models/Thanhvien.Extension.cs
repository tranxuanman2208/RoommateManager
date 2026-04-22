using System.ComponentModel.DataAnnotations.Schema;

namespace RoomateManager.Models
{
    public partial class Thanhvien
    {
        public string AvatarInitial => Ten?.Length > 0 ? Ten[0].ToString().ToUpper() : "?";

        public string RoleDisplay => (Ad == true) ? "👑 Quản lý" : "Thành viên";

        public string StatusDisplay => (Con == true) ? "Hoạt động" : "OFF";

        public string StatusColor => (Con == true) ? "#4CAF50" : "#F44336";
        [NotMapped]
        public bool IsCurrentUser { get; set; }
    }
}