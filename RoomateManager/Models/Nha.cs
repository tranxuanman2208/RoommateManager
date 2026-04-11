using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Nha
{
    public string Manha { get; set; } = null!;

    public string? Sonha { get; set; }

    public string? Duong { get; set; }

    public string? Phuong { get; set; }

    public string? Tp { get; set; }

    public virtual ICollection<Hoadontong> Hoadontongs { get; set; } = new List<Hoadontong>();

    public virtual ICollection<Thanhvien> Thanhviens { get; set; } = new List<Thanhvien>();

    public virtual ICollection<Vatdung> Vatdungs { get; set; } = new List<Vatdung>();
}
