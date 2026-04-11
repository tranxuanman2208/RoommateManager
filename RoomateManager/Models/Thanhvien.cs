using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Thanhvien
{
    public string Id { get; set; } = null!;

    public string? Pass { get; set; }

    public string? Manha { get; set; }

    public string? Ten { get; set; }

    public DateOnly? Ns { get; set; }

    public string? Sdt { get; set; }

    public string? Mail { get; set; }

    public bool? Sex { get; set; }

    public bool? Ad { get; set; }

    public bool? Con { get; set; }

    public string? Username { get; set; }

    public virtual ICollection<Baocao> Baocaos { get; set; } = new List<Baocao>();

    public virtual ICollection<Hoadontv> HoadontvNguoichuyenNavigations { get; set; } = new List<Hoadontv>();

    public virtual ICollection<Hoadontv> HoadontvNguoinhanNavigations { get; set; } = new List<Hoadontv>();

    public virtual Nha? ManhaNavigation { get; set; }

    public virtual ICollection<Phancong> PhancongNguoiphancongNavigations { get; set; } = new List<Phancong>();

    public virtual ICollection<Phancong> PhancongNguoithuchienNavigations { get; set; } = new List<Phancong>();

    public virtual ICollection<Thongbao> ThongbaoNguoinhanNavigations { get; set; } = new List<Thongbao>();

    public virtual ICollection<Thongbao> ThongbaoNguoitbNavigations { get; set; } = new List<Thongbao>();

    public virtual ICollection<Xulyvipham> XulyviphamNguoiviphamNavigations { get; set; } = new List<Xulyvipham>();

    public virtual ICollection<Xulyvipham> XulyviphamNguoixulyNavigations { get; set; } = new List<Xulyvipham>();
}
