using RoomateManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoommateManager.Models;

public partial class ChitietXemTb
{
    public int Matb { get; set; }
    public string Matv { get; set; } = null!;
    public bool? Dadoc { get; set; }
    public virtual Thongbao? MatbNavigation { get; set; }
    public virtual Thanhvien? MatvNavigation { get; set; }
}
