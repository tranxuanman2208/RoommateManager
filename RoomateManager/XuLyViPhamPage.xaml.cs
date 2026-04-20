using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using RoomateManager.Models;

namespace RoomateManager
{
    public partial class XuLyViPhamPage : Page
    {
        public XuLyViPhamPage()
        {
            InitializeComponent();
            _ = LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                using (var db = new RoommateManagerContext())
                {
                    // 1. Load danh sách thành viên cho ComboBox
                    cbThanhVien.ItemsSource = await db.Thanhviens
                                                      .Where(tv => tv.Con == true)
                                                      .ToListAsync();

                    // 2. Load danh sách vi phạm bằng LINQ Join và Anonymous Object
                    var data = await (from vp in db.Xulyviphams
                                      join tv in db.Thanhviens on vp.Nguoivipham equals tv.Id
                                      where vp.Daxoa == false || vp.Daxoa == null
                                      orderby vp.Ngayxuly descending
                                      select new
                                      {
                                          vp.Mavipham,
                                          vp.Nguoivipham,
                                          vp.Noidung,
                                          vp.Ngayxuly,
                                          vp.Done,
                                          vp.Mabc,
                                          TenNguoiVP = tv.Ten,
                                          StatusText = (vp.Done == true) ? "Đã xong" : "Đang chờ",
                                          StatusColor = (vp.Done == true) ? "#27AE60" : "#E67E22"
                                      }).ToListAsync();

                    lstViPham.ItemsSource = data;
                }
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
                MessageBox.Show("Vui lòng nhập đủ thông tin!");
                return;
            }

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var newVP = new Xulyvipham // Sử dụng tên Model theo scaffold (PascalCase)
                    {
                        Nguoivipham = cbThanhVien.SelectedValue.ToString(),
                        Noidung = txtNoiDung.Text.Trim(),
                        Ngayxuly = DateOnly.FromDateTime(DateTime.Now), // SQL date tương ứng DateOnly trong EF Core
                        Done = false,
                        Daxoa = false
                    };

                    db.Xulyviphams.Add(newVP);
                    await db.SaveChangesAsync();

                    txtNoiDung.Clear();
                    await LoadData();
                    MessageBox.Show("Ghi nhận vi phạm thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
            }
        }

        private async void btnDone_Click(object sender, RoutedEventArgs e)
        {
            // Ép kiểu dynamic để lấy ID từ Anonymous Object trong ListView
            var selected = lstViPham.SelectedItem as dynamic;
            if (selected == null) return;

            try
            {
                int ma = selected.Mavipham;
                using (var db = new RoommateManagerContext())
                {
                    var vp = await db.Xulyviphams.FindAsync(ma);
                    if (vp != null)
                    {
                        vp.Done = true;
                        await db.SaveChangesAsync();
                        await LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstViPham.SelectedItem as dynamic;
            if (selected == null) return;

            if (MessageBox.Show("Xóa bản ghi này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    int ma = selected.Mavipham;
                    using (var db = new RoommateManagerContext())
                    {
                        var vp = await db.Xulyviphams.FindAsync(ma);
                        if (vp != null)
                        {
                            vp.Daxoa = true; // Đánh dấu xóa giả (Soft Delete)
                            await db.SaveChangesAsync();
                            await LoadData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa: " + ex.Message);
                }
            }
        }

        private async void btnXemBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstViPham.SelectedItem as dynamic;

            if (selected == null || selected.Mabc == null)
            {
                MessageBox.Show("Vi phạm này được ghi nhận trực tiếp, không có báo cáo gốc!", "Thông báo");
                return;
            }

            try
            {
                int maBC = selected.Mabc;
                using (var db = new RoommateManagerContext())
                {
                    // Truy vấn nội dung báo cáo gốc bằng LINQ
                    var bc = await db.Baocaos.FindAsync(maBC);
                    string noiDungGoc = bc?.Noidung ?? "Không tìm thấy nội dung...";

                    // Hiển thị Popup (Giả định bạn đã có MinhChungWindow)
                    MinhChungWindow popup = new MinhChungWindow(maBC.ToString(), noiDungGoc);
                    popup.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi truy xuất báo cáo: " + ex.Message);
            }
        }
    }
}