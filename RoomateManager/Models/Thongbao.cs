using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Thongbao
{
    public int Matb { get; set; }

    public string? Noidung { get; set; }

    public string? Nguoitb { get; set; }

    public DateOnly? Ngaytb { get; set; }

    public string? Nguoinhan { get; set; }

    public bool? Daxoa { get; set; }

    public bool? Dadoc { get; set; }

    public virtual ICollection<ChitietXemTb> ChitietXemTbs { get; set; } = new List<ChitietXemTb>();

    public virtual Thanhvien? NguoinhanNavigation { get; set; }

    public virtual Thanhvien? NguoitbNavigation { get; set; }
}
