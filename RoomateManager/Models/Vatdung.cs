using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Vatdung
{
    public int Mavatdung { get; set; }

    public string? Tenvd { get; set; }

    public string? Manha { get; set; }

    public bool? Dabo { get; set; }

    public bool? Baoduong { get; set; }

    public string? Ghichu { get; set; }

    public string? Nguoitao { get; set; }

    public string? Nguoicapnhat { get; set; }

    public DateTime? Ngaytao { get; set; }

    public DateTime? Ngaycapnhat { get; set; }

    public string? Hinhanh { get; set; }

    public virtual Nha? ManhaNavigation { get; set; }
}
