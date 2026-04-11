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

    public virtual Nha? ManhaNavigation { get; set; }
}
