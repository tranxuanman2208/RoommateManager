using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomateManager
{
    // Đây là lớp quản lý kết nối Database
    public class AppDbContext : DbContext
    {
        public DbSet<ThanhVien> ThanhViens { get; set; }
        public DbSet<BaoCao> BaoCaos { get; set; }
        public DbSet<XuLyViPham> XuLyViPhams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            
            options.UseSqlServer(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=RoommateManager;Integrated Security=True;TrustServerCertificate=True;");
        }
    }

    // Định nghĩa bảng THANHVIEN
    [Table("THANHVIEN")]
    public class ThanhVien
    {
        [Key]
        public string ID { get; set; }
        public string TEN { get; set; }
        public int? DIEMVIPHAM { get; set; } // Cột này Tài thêm vào DB để lưu điểm phạt nhé
    }

    // Định nghĩa bảng BAOCAO
    [Table("BAOCAO")]
    public class BaoCao
    {
        [Key]
        public int MABC { get; set; }
        public string NOIDUNG { get; set; }
        public string NGUOIBC { get; set; }
        public DateTime? NGAYBC { get; set; }
        public bool? DAXULY { get; set; }
        public bool? DAXOA { get; set; }
        public string TIEUDE { get; set; }
    }

    // Định nghĩa bảng XULYVIPHAM
    [Table("XULYVIPHAM")]
    public class XuLyViPham
    {
        [Key]
        public int MAVIPHAM { get; set; }
        public string NGUOIVIPHAM { get; set; }
        public string NOIDUNG { get; set; }
        public int? MABC { get; set; }
        public DateTime? NGAYXULY { get; set; }
        public bool? DONE { get; set; }
        public bool? DAXOA { get; set; }
    }
}