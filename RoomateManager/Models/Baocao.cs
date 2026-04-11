using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Baocao
{
    public int Mabc { get; set; }

    public string? Noidung { get; set; }

    public string? Nguoibc { get; set; }

    public DateOnly? Ngaybc { get; set; }

    public bool? Daxuly { get; set; }

    public bool? Daxoa { get; set; }

    public string? Tieude { get; set; }

    public virtual Thanhvien? NguoibcNavigation { get; set; }

    public virtual ICollection<Xulyvipham> Xulyviphams { get; set; } = new List<Xulyvipham>();
}
