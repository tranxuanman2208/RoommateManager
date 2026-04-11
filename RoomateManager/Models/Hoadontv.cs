using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Hoadontv
{
    public int Mahdtv { get; set; }

    public int Mahdt { get; set; }

    public string? Noidung { get; set; }

    public string Nguoichuyen { get; set; } = null!;

    public DateOnly? Ngaygdtv { get; set; }

    public decimal? Sotien { get; set; }

    public bool? Dadong { get; set; }

    public DateOnly? Ngaygui { get; set; }

    public bool? Daxoa { get; set; }

    public string? Nguoinhan { get; set; }

    public byte Thang { get; set; }

    public short Nam { get; set; }

    public virtual Hoadontong MahdtNavigation { get; set; } = null!;

    public virtual Thanhvien NguoichuyenNavigation { get; set; } = null!;

    public virtual Thanhvien? NguoinhanNavigation { get; set; }
}
