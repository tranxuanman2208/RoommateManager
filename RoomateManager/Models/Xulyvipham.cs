using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Xulyvipham
{
    public int Mavipham { get; set; }

    public string? Nguoivipham { get; set; }

    public string? Noidung { get; set; }

    public int? Mabc { get; set; }

    public DateOnly? Ngayxuly { get; set; }

    public bool? Done { get; set; }

    public bool? Daxoa { get; set; }

    public string? Nguoixuly { get; set; }

    public virtual Baocao? MabcNavigation { get; set; }

    public virtual Thanhvien? NguoiviphamNavigation { get; set; }

    public virtual Thanhvien? NguoixulyNavigation { get; set; }
}
