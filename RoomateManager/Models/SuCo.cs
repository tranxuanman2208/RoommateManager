using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class SuCo
{
    public int Masuco { get; set; }

    public string TenThietbi { get; set; } = null!;

    public string? Mota { get; set; }

    public string? Hinhanh { get; set; }

    public string Trangthai { get; set; } = null!;

    public string? Nguoitao { get; set; }

    public DateTime? Ngaytao { get; set; }

    public bool? Daxoa { get; set; }
}
