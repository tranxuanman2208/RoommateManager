using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class ChitietXemTb
{
    public int Mact { get; set; }

    public int Matb { get; set; }

    public string Matv { get; set; } = null!;

    public bool? Dadoc { get; set; }

    public DateTime? Thoigianxem { get; set; }

    public virtual Thongbao MatbNavigation { get; set; } = null!;
}
