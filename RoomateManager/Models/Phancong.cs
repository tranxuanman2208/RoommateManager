using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class Phancong
{
    public int Id { get; set; }

    public DateOnly? Ngayth { get; set; }

    public string? Tencv { get; set; }

    public string? Nguoithuchien { get; set; }

    public bool? Dalam { get; set; }

    public bool? Daxoa { get; set; }

    public string? Nguoiphancong { get; set; }

    public string? Minhchung { get; set; }

    public virtual Thanhvien? NguoiphancongNavigation { get; set; }

    public virtual Thanhvien? NguoithuchienNavigation { get; set; }
}
