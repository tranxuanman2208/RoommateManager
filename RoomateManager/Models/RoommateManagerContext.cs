using Microsoft.EntityFrameworkCore;
using RoommateManager.Models;
using System;
using System.Collections.Generic;

namespace RoomateManager.Models;

public partial class RoommateManagerContext : DbContext
{
    public RoommateManagerContext()
    {
    }

    public RoommateManagerContext(DbContextOptions<RoommateManagerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Baocao> Baocaos { get; set; }

    public virtual DbSet<Hoadontong> Hoadontongs { get; set; }

    public virtual DbSet<Hoadontv> Hoadontvs { get; set; }

    public virtual DbSet<Nha> Nhas { get; set; }

    public virtual DbSet<Phancong> Phancongs { get; set; }

    public virtual DbSet<Thanhvien> Thanhviens { get; set; }

    public virtual DbSet<Thongbao> Thongbaos { get; set; }

    public virtual DbSet<Vatdung> Vatdungs { get; set; }

    public virtual DbSet<Xulyvipham> Xulyviphams { get; set; }

    public virtual DbSet<ChitietXemTb> ChitietXemTbs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=RoommateManager;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Baocao>(entity =>
        {
            entity.HasKey(e => e.Mabc);
            entity.ToTable("BAOCAO");
            entity.HasIndex(e => e.Ngaybc, "ID_NGAYBC");
            entity.Property(e => e.Mabc).HasColumnName("MABC");
            entity.Property(e => e.Daxoa).HasColumnName("DAXOA");
            entity.Property(e => e.Daxuly).HasColumnName("DAXULY");
            entity.Property(e => e.Ngaybc).HasColumnName("NGAYBC");
            entity.Property(e => e.Nguoibc)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOIBC");
            entity.Property(e => e.Noidung)
                .HasMaxLength(200)
                .HasColumnName("NOIDUNG");
            entity.Property(e => e.Tieude)
                .HasMaxLength(50)
                .HasColumnName("TIEUDE");
            entity.HasOne(d => d.NguoibcNavigation).WithMany(p => p.Baocaos)
                .HasForeignKey(d => d.Nguoibc)
                .HasConstraintName("FK_NGUOIBC_IDTHANHVIEN");
        });

        modelBuilder.Entity<Hoadontong>(entity =>
        {
            entity.HasKey(e => e.Mahdt).HasName("PK__HOADONTO__78C57AA9FFDD5362");
            entity.ToTable("HOADONTONG");
            entity.HasIndex(e => e.Ngaygdt, "ID_NGAYGDT");
            entity.HasIndex(e => new { e.Nam, e.Thang }, "IX_HOADON_KITHANHTOAN");
            entity.Property(e => e.Mahdt).HasColumnName("MAHDT");
            entity.Property(e => e.Dadong).HasColumnName("DADONG");
            entity.Property(e => e.Daxoa).HasColumnName("DAXOA");
            entity.Property(e => e.Manha)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("MANHA");
            entity.Property(e => e.Nam)
                .HasDefaultValueSql("(datepart(year,getdate()))")
                .HasColumnName("NAM");
            entity.Property(e => e.Ngaygdt).HasColumnName("NGAYGDT");
            entity.Property(e => e.Ngaygui).HasColumnName("NGAYGUI");
            entity.Property(e => e.Nguoinhan)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NGUOINHAN");
            entity.Property(e => e.Noidung)
                .HasMaxLength(100)
                .HasColumnName("NOIDUNG");
            entity.Property(e => e.Sotien)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("SOTIEN");
            entity.Property(e => e.Ten)
                .HasMaxLength(50)
                .HasColumnName("TEN");
            entity.Property(e => e.Thang)
                .HasDefaultValueSql("(datepart(month,getdate()))")
                .HasColumnName("THANG");
            entity.HasOne(d => d.ManhaNavigation).WithMany(p => p.Hoadontongs)
                .HasForeignKey(d => d.Manha)
                .HasConstraintName("FK_MANHA_NHA");
        });

        modelBuilder.Entity<Hoadontv>(entity =>
        {
            entity.HasKey(e => e.Mahdtv).HasName("PK_MAHDTV");
            entity.ToTable("HOADONTV");
            entity.HasIndex(e => e.Ngaygdtv, "ID_NGAYGDTV");
            entity.HasIndex(e => new { e.Nam, e.Thang }, "IX_HOADON_KITHANHTOAN_TV");
            entity.HasIndex(e => new { e.Mahdt, e.Nguoichuyen }, "UNI_MAHDT_NGUOICHUYEN").IsUnique();
            entity.Property(e => e.Mahdtv).HasColumnName("MAHDTV");
            entity.Property(e => e.Dadong).HasColumnName("DADONG");
            entity.Property(e => e.Daxoa).HasColumnName("DAXOA");
            entity.Property(e => e.Mahdt).HasColumnName("MAHDT");
            entity.Property(e => e.Nam)
                .HasDefaultValueSql("(datepart(year,getdate()))")
                .HasColumnName("NAM");
            entity.Property(e => e.Ngaygdtv).HasColumnName("NGAYGDTV");
            entity.Property(e => e.Ngaygui).HasColumnName("NGAYGUI");
            entity.Property(e => e.Nguoichuyen)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOICHUYEN");
            entity.Property(e => e.Nguoinhan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOINHAN");
            entity.Property(e => e.Noidung)
                .HasMaxLength(100)
                .HasColumnName("NOIDUNG");
            entity.Property(e => e.Sotien)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("SOTIEN");
            entity.Property(e => e.Thang)
                .HasDefaultValueSql("(datepart(month,getdate()))")
                .HasColumnName("THANG");
            entity.HasOne(d => d.MahdtNavigation).WithMany(p => p.Hoadontvs)
                .HasForeignKey(d => d.Mahdt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MAHDT_MAHOADONTONG");
            entity.HasOne(d => d.NguoichuyenNavigation).WithMany(p => p.HoadontvNguoichuyenNavigations)
                .HasForeignKey(d => d.Nguoichuyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NGUOICHUYEN_IDTHANHVIEN");
            entity.HasOne(d => d.NguoinhanNavigation).WithMany(p => p.HoadontvNguoinhanNavigations)
                .HasForeignKey(d => d.Nguoinhan)
                .HasConstraintName("FK_NGUOINHAN_IDTHANHVIEN");
        });

        modelBuilder.Entity<Nha>(entity =>
        {
            entity.HasKey(e => e.Manha).HasName("PK__NHA__7ABD1CE9814F1F78");
            entity.ToTable("NHA");
            entity.Property(e => e.Manha)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("MANHA");
            entity.Property(e => e.Duong)
                .HasMaxLength(50)
                .HasColumnName("DUONG");
            entity.Property(e => e.Phuong)
                .HasMaxLength(50)
                .HasColumnName("PHUONG");
            entity.Property(e => e.Sonha)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("SONHA");
            entity.Property(e => e.Tp)
                .HasMaxLength(50)
                .HasColumnName("TP");
        });

        modelBuilder.Entity<Phancong>(entity =>
        {
            entity.ToTable("PHANCONG");
            entity.HasIndex(e => e.Ngayth, "ID_NGAY_THUC_HIEN");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Dalam).HasColumnName("DALAM");
            entity.Property(e => e.Daxoa).HasColumnName("DAXOA");
            entity.Property(e => e.Minhchung)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("MINHCHUNG");
            entity.Property(e => e.Ngayth).HasColumnName("NGAYTH");
            entity.Property(e => e.Nguoiphancong)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOIPHANCONG");
            entity.Property(e => e.Nguoithuchien)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOITHUCHIEN");
            entity.Property(e => e.Tencv)
                .HasMaxLength(50)
                .HasColumnName("TENCV");
            entity.HasOne(d => d.NguoiphancongNavigation).WithMany(p => p.PhancongNguoiphancongNavigations)
                .HasForeignKey(d => d.Nguoiphancong)
                .HasConstraintName("FK_NGUOIPHANCONG_IDTHANHVIEN");
            entity.HasOne(d => d.NguoithuchienNavigation).WithMany(p => p.PhancongNguoithuchienNavigations)
                .HasForeignKey(d => d.Nguoithuchien)
                .HasConstraintName("FK_NGUOITHUCHIEN_IDTHANHVIEN");
        });

        modelBuilder.Entity<Thanhvien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__THANHVIE__3214EC270786DCD1");
            entity.ToTable("THANHVIEN");
            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.Ad).HasColumnName("AD");
            entity.Property(e => e.Con).HasColumnName("CON");
            entity.Property(e => e.Mail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MAIL");
            entity.Property(e => e.Manha)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("MANHA");
            entity.Property(e => e.Ns).HasColumnName("NS");
            entity.Property(e => e.Pass)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("PASS");
            entity.Property(e => e.Sdt)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.Sex).HasColumnName("SEX");
            entity.Property(e => e.Ten)
                .HasMaxLength(50)
                .HasColumnName("TEN");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("USERNAME");
            entity.HasOne(d => d.ManhaNavigation).WithMany(p => p.Thanhviens)
                .HasForeignKey(d => d.Manha)
                .HasConstraintName("FK_MANHA_IDTHANHVIEN");
        });

        modelBuilder.Entity<Thongbao>(entity =>
        {
            entity.HasKey(e => e.Matb).HasName("PK__THONGBAO__6023721D2EED3F43");
            entity.ToTable("THONGBAO");
            entity.HasIndex(e => e.Ngaytb, "ID_NGAYTB");
            entity.Property(e => e.Matb).HasColumnName("MATB");
            entity.Property(e => e.Daxoa).HasColumnName("DAXOA");
            entity.Property(e => e.Ngaytb).HasColumnName("NGAYTB");
            entity.Property(e => e.Nguoinhan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOINHAN");
            entity.Property(e => e.Nguoitb)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOITB");
            entity.Property(e => e.Noidung)
                .HasMaxLength(150)
                .HasColumnName("NOIDUNG");
            entity.HasOne(d => d.NguoinhanNavigation).WithMany(p => p.ThongbaoNguoinhanNavigations)
                .HasForeignKey(d => d.Nguoinhan)
                .HasConstraintName("FK_NGUOINHANTB_IDTHANHVIEN");
            entity.HasOne(d => d.NguoitbNavigation).WithMany(p => p.ThongbaoNguoitbNavigations)
                .HasForeignKey(d => d.Nguoitb)
                .HasConstraintName("FK_NGUOITB_IDTHANHVIEN");
        });

        modelBuilder.Entity<Vatdung>(entity =>
        {
            entity.HasKey(e => e.Mavatdung).HasName("PK__VATDUNG__6D9077EFF9E82422");
            entity.ToTable("VATDUNG");
            entity.Property(e => e.Mavatdung).HasColumnName("MAVATDUNG");
            entity.Property(e => e.Baoduong).HasColumnName("BAODUONG");
            entity.Property(e => e.Dabo).HasColumnName("DABO");
            entity.Property(e => e.Manha)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("MANHA");
            entity.Property(e => e.Tenvd)
                .HasMaxLength(50)
                .HasColumnName("TENVD");
            entity.HasOne(d => d.ManhaNavigation).WithMany(p => p.Vatdungs)
                .HasForeignKey(d => d.Manha)
                .HasConstraintName("FK_MANHA_VATDUNG");
        });

        modelBuilder.Entity<Xulyvipham>(entity =>
        {
            entity.HasKey(e => e.Mavipham);
            entity.ToTable("XULYVIPHAM");
            entity.HasIndex(e => e.Ngayxuly, "ID_NGAYXULY");
            entity.Property(e => e.Mavipham).HasColumnName("MAVIPHAM");
            entity.Property(e => e.Daxoa).HasColumnName("DAXOA");
            entity.Property(e => e.Done).HasColumnName("DONE");
            entity.Property(e => e.Mabc).HasColumnName("MABC");
            entity.Property(e => e.Ngayxuly).HasColumnName("NGAYXULY");
            entity.Property(e => e.Nguoivipham)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOIVIPHAM");
            entity.Property(e => e.Nguoixuly)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NGUOIXULY");
            entity.Property(e => e.Noidung)
                .HasMaxLength(200)
                .HasColumnName("NOIDUNG");
            entity.HasOne(d => d.MabcNavigation).WithMany(p => p.Xulyviphams)
                .HasForeignKey(d => d.Mabc)
                .HasConstraintName("FK_MABC_BAOCAO");
            entity.HasOne(d => d.NguoiviphamNavigation).WithMany(p => p.XulyviphamNguoiviphamNavigations)
                .HasForeignKey(d => d.Nguoivipham)
                .HasConstraintName("FK_NGUOIVIPHAM_IDTHANHVIEN");
            entity.HasOne(d => d.NguoixulyNavigation).WithMany(p => p.XulyviphamNguoixulyNavigations)
                .HasForeignKey(d => d.Nguoixuly)
                .HasConstraintName("FK_NGUOIXULY_IDTHANHVIEN");
        });

        // --- ĐOẠN THÊM MỚI Ở ĐÂY ---
        modelBuilder.Entity<ChitietXemTb>(entity =>
        {
            // Định nghĩa khóa chính gồm 2 cột: Mã thông báo và Mã thành viên
            entity.HasKey(e => new { e.Matb, e.Matv });

            // Map chính xác tên bảng trong SQL (Nếu trong SQL bạn đặt tên khác thì sửa lại đây)
            entity.ToTable("CHITIET_XEM_TB");

            entity.Property(e => e.Matb).HasColumnName("MATB");
            entity.Property(e => e.Matv).HasColumnName("MATV").HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Dadoc).HasColumnName("DADOC");
        });
        // ---------------------------

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}