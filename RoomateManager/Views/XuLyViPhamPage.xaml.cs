using RoomateManager.Models;
using RoomateManager.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace RoommateManager.Views
{
    public partial class XuLyViPhamPage : Page
    {
        public XuLyViPhamPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new RoommateManagerContext())
            {
                // 1. Load ComboBox: Lấy ID và Ten (đúng hoa thường)
                cbThanhVien.ItemsSource = db.Thanhviens.Where(tv => tv.Con == true).ToList();

                // 2. Load danh sách vi phạm
                var query = db.Xulyviphams
                    .Include(vp => vp.NguoiviphamNavigation)
                    .Where(vp => vp.Daxoa == false || vp.Daxoa == null)
                    .OrderByDescending(vp => vp.Ngayxuly)
                    .Select(vp => new
                    {
                        Mavipham = vp.Mavipham,
                        TenNguoiVP = vp.NguoiviphamNavigation != null ? vp.NguoiviphamNavigation.Ten : "N/A",
                        Noidung = vp.Noidung,
                        Ngayxuly = vp.Ngayxuly,
                        Mabc = vp.Mabc,
                        StatusText = vp.Done == true ? "Đã xong" : "Đang chờ",
                        StatusColor = vp.Done == true ? "#27AE60" : "#E67E22"
                    }).ToList();

                lstViPham.ItemsSource = query;
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cbThanhVien.SelectedValue == null || string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Vui lòng chọn người vi phạm và nhập nội dung!");
                return;
            }

            string idVP = cbThanhVien.SelectedValue.ToString();
            string noiDung = txtNoiDung.Text.Trim();

            using (var db = new RoommateManagerContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Lưu bản ghi Vi phạm
                        Xulyvipham vpMoi = new Xulyvipham
                        {
                            Nguoivipham = idVP,
                            Noidung = noiDung,
                            Ngayxuly = DateOnly.FromDateTime(DateTime.Now),
                            Done = false,
                            Daxoa = false,
                            Nguoixuly = User.CurrentUserId
                        };
                        db.Xulyviphams.Add(vpMoi);

                        // 2. Cộng điểm vi phạm cho thành viên (càng cao càng bị phạt)
                        var tv = db.Thanhviens.FirstOrDefault(x => x.Id == idVP);
                        if (tv != null)
                        {
                            tv.Diemvipham = (tv.Diemvipham ?? 12) - 1;
                        }

                        // 3. Gửi thông báo đích danh
                        Thongbao tb = new Thongbao
                        {
                            Noidung = "[CẢNH BÁO VI PHẠM] " + noiDung,
                            Nguoitb = User.CurrentUserId,
                            Nguoinhan = idVP,
                            Ngaytb = DateOnly.FromDateTime(DateTime.Now),
                            Dadoc = false,
                            Daxoa = false
                        };
                        db.Thongbaos.Add(tb);

                        db.SaveChanges();
                        transaction.Commit();

                        MessageBox.Show("Đã ghi nhận và gửi thông báo vi phạm!");
                        txtNoiDung.Clear();
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi: " + ex.Message);
                    }
                }
            }
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = lstViPham.SelectedItem;
            if (selected == null) return;

            int ma = (int)selected.Mavipham;
            using (var db = new RoommateManagerContext())
            {
                var vp = db.Xulyviphams.Find(ma);
                if (vp != null)
                {
                    vp.Done = true;
                    db.SaveChanges();
                    LoadData();
                    MessageBox.Show("Đã đánh dấu xử lý xong.");
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = lstViPham.SelectedItem;
            if (selected == null) return;

            if (MessageBox.Show("Xóa vi phạm này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int ma = (int)selected.Mavipham;
                using (var db = new RoommateManagerContext())
                {
                    var vp = db.Xulyviphams.Find(ma);
                    if (vp != null)
                    {
                        vp.Daxoa = true;
                        db.SaveChanges();
                        LoadData();
                    }
                }
            }
        }
    }
}