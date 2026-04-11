using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Hoadontong
{
    public int Mahdt { get; set; }

    public string? Ten { get; set; }

    public string? Noidung { get; set; }

    public string? Nguoinhan { get; set; }

    public DateOnly? Ngaygdt { get; set; }

    public decimal? Sotien { get; set; }

    public bool? Dadong { get; set; }

    public DateOnly? Ngaygui { get; set; }

    public bool? Daxoa { get; set; }

    public string? Manha { get; set; }

    public byte Thang { get; set; }

    public short Nam { get; set; }

    public virtual ICollection<Hoadontv> Hoadontvs { get; set; } = new List<Hoadontv>();

    public virtual Nha? ManhaNavigation { get; set; }
}
