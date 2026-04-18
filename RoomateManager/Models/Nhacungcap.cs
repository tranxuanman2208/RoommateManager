using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Nhacungcap
{
    public string Mancc { get; set; } = null!;

    public string? Tenncc { get; set; }

    public string? Diachi { get; set; }

    public virtual ICollection<Hoadontong> Hoadontongs { get; set; } = new List<Hoadontong>();
}
