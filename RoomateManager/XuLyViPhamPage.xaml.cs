using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace RoomateManager
{
    public partial class XuLyViPhamPage : Page
    {
        // Khởi tạo context dùng chung
        private readonly AppDbContext _db = new AppDbContext();

        public XuLyViPhamPage()
        {
            InitializeComponent();
            _ = LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                // Load danh sách thành viên cho ComboBox bằng LINQ
                cbThanhVien.ItemsSource = await _db.ThanhViens.ToListAsync();

                // Load danh sách vi phạm bằng LINQ Join
                var data = await (from vp in _db.XuLyViPhams
                                  join tv in _db.ThanhViens on vp.NGUOIVIPHAM equals tv.ID
                                  where vp.DAXOA == false
                                  orderby vp.NGAYXULY descending
                                  select new
                                  {
                                      vp.MAVIPHAM,
                                      vp.NGUOIVIPHAM,
                                      vp.NOIDUNG,
                                      vp.NGAYXULY,
                                      vp.DONE,
                                      vp.MABC,
                                      TenNguoiVP = tv.TEN,
                                      StatusText = (vp.DONE == true) ? "Đã xong" : "Đang chờ",
                                      StatusColor = (vp.DONE == true) ? "#27AE60" : "#E67E22"
                                  }).ToListAsync();

                lstViPham.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cbThanhVien.SelectedValue == null || string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin!"); return;
            }

            try
            {
                var newVP = new XuLyViPham
                {
                    NGUOIVIPHAM = cbThanhVien.SelectedValue.ToString(),
                    NOIDUNG = txtNoiDung.Text.Trim(),
                    NGAYXULY = DateTime.Now,
                    DONE = false,
                    DAXOA = false
                };

                _db.XuLyViPhams.Add(newVP);
                await _db.SaveChangesAsync();

                txtNoiDung.Clear();
                await LoadData();
                MessageBox.Show("Ghi nhận vi phạm thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message); }
        }

        private async void btnDone_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstViPham.SelectedItem as dynamic;
            if (selected == null) return;

            try
            {
                int ma = selected.MAVIPHAM;
                var vp = await _db.XuLyViPhams.FindAsync(ma);
                if (vp != null)
                {
                    vp.DONE = true;
                    await _db.SaveChangesAsync();
                    await LoadData();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstViPham.SelectedItem as dynamic;
            if (selected == null) return;

            if (MessageBox.Show("Xóa bản ghi này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    int ma = selected.MAVIPHAM;
                    var vp = await _db.XuLyViPhams.FindAsync(ma);
                    if (vp != null)
                    {
                        vp.DAXOA = true;
                        await _db.SaveChangesAsync();
                        await LoadData();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private async void btnXemBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstViPham.SelectedItem as dynamic;

            if (selected == null || selected.MABC == null)
            {
                MessageBox.Show("Vi phạm này được ghi nhận trực tiếp, không có báo cáo gốc liên kết!", "Thông báo");
                return;
            }

            try
            {
                int maBC = selected.MABC;
                // Dùng LINQ tìm nội dung báo cáo
                var bc = await _db.BaoCaos.FindAsync(maBC);
                string noiDungGoc = bc?.NOIDUNG ?? "Không tìm thấy nội dung...";

                // Hiển thị Popup 
                MinhChungWindow popup = new MinhChungWindow(maBC.ToString(), noiDungGoc);
                popup.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi truy xuất: " + ex.Message); }
        }
    }
}