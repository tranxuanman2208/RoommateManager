using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RoomateManager
{
    public partial class BaoCaoPage : Page
    {
        // Khởi tạo Context của EF Core
        private readonly AppDbContext _db = new AppDbContext();

        public BaoCaoPage()
        {
            InitializeComponent();
            LoadDuLieuBanDau();
        }

        private void LoadDuLieuBanDau()
        {
            try
            {
                var listTV = _db.ThanhViens.ToList();
                cbNguoiBao.ItemsSource = listTV;
                cbChonNguoiVP.ItemsSource = listTV;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi EF Core: " + ex.Message); }
        }

        private async void btnGui_Click(object sender, RoutedEventArgs e)
        {
            if (cbNguoiBao.SelectedValue == null || string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!"); return;
            }

            string noiDungLuu = $"[{txtTieuDe.Text.Trim()}] {txtNoiDung.Text.Trim()}" + (chkAnDanh.IsChecked == true ? " [AN_DANH]" : "");

            try
            {
                var bc = new BaoCao
                {
                    NOIDUNG = noiDungLuu,
                    NGUOIBC = cbNguoiBao.SelectedValue.ToString(),
                    NGAYBC = DateTime.Now,
                    DAXULY = false,
                    DAXOA = false,
                    TIEUDE = txtTieuDe.Text.Trim()
                };

                _db.BaoCaos.Add(bc);
                await _db.SaveChangesAsync();

                MessageBox.Show("Đã gửi báo cáo vi phạm!");
                txtTieuDe.Clear(); txtNoiDung.Clear();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi lưu EF: " + ex.Message); }
        }

        private async Task LoadBangTin()
        {
            try
            {
                // Dùng LINQ thay cho chuỗi SQL dài ngoằng
                var listBC = await _db.BaoCaos
                    .Where(b => b.DAXOA == false)
                    .OrderByDescending(b => b.NGAYBC)
                    .Select(b => new {
                        b.MABC,
                        b.NOIDUNG,
                        b.NGAYBC,
                        b.DAXULY,
                        // Xử lý ẩn danh trực tiếp trong lúc load
                        DisplayName = b.NOIDUNG.Contains("[AN_DANH]") ? "👤 Ẩn danh" : _db.ThanhViens.Where(t => t.ID == b.NGUOIBC).Select(t => t.TEN).FirstOrDefault(),
                        CleanContent = b.NOIDUNG.Replace("[AN_DANH]", ""),
                        StatusText = b.DAXULY == true ? "Đã xong" : "Chờ duyệt",
                        StatusColor = b.DAXULY == true ? "#27AE60" : "#E67E22"
                    }).ToListAsync();

                lstBangTin.ItemsSource = listBC;
            }
            catch { }
        }

        private async void btnXacNhanPhat_Click(object sender, RoutedEventArgs e)
        {
            var selectedBC = lstBangTin.SelectedItem as dynamic;
            if (selectedBC == null || cbChonNguoiVP.SelectedValue == null)
            {
                MessageBox.Show("Hãy chọn báo cáo và người vi phạm!"); return;
            }

            try
            {
                int maBC = selectedBC.MABC;

                // 1. Cập nhật trạng thái báo cáo
                var bc = _db.BaoCaos.Find(maBC);
                if (bc != null) bc.DAXULY = true;

                // 2. Thêm vào bảng xử lý vi phạm
                var xl = new XuLyViPham
                {
                    NGUOIVIPHAM = cbChonNguoiVP.SelectedValue.ToString(),
                    NOIDUNG = selectedBC.CleanContent,
                    MABC = maBC,
                    NGAYXULY = DateTime.Now,
                    DONE = false,
                    DAXOA = false
                };
                _db.XuLyViPhams.Add(xl);

                // 3. Cộng điểm vi phạm (Logic của Tài)
                var tv = _db.ThanhViens.Find(cbChonNguoiVP.SelectedValue.ToString());
                if (tv != null) tv.DIEMVIPHAM = (tv.DIEMVIPHAM ?? 0) + 1;

                await _db.SaveChangesAsync();
                await LoadBangTin();
                MessageBox.Show("Đã xác nhận phạt thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi xử lý: " + ex.Message); }
        }

        private async void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var selectedBC = lstBangTin.SelectedItem as dynamic;
            if (selectedBC == null) return;

            var bc = _db.BaoCaos.Find((int)selectedBC.MABC);
            if (bc != null)
            {
                bc.DAXOA = true;
                await _db.SaveChangesAsync();
                await LoadBangTin();
            }
        }

        private void TabItem_Selected(object sender, RoutedEventArgs e)
        {
            _ = LoadBangTin();
        }
    }
}