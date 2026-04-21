using RoomateManager.Models;
using RoomateManager.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoommateManager.Views
{
    public partial class PaymentPage : Page
    {
        private string currentOTP = "";

        public PaymentPage()
        {
            InitializeComponent();
            LoadData();
            BtnSplitMoney.Visibility = User.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        }
        private void LoadData()
        {
            using (var db = new RoommateManagerContext())
            {
                if (User.IsAdmin)
                {
                    var query = from hd in db.Hoadontongs
                                where hd.Daxoa == false && hd.Dadong == false
                                select new
                                {
                                    ID = hd.Mahdt,
                                    DisplayName = hd.Ten,
                                    Noidung = hd.Noidung,
                                    Sotien = hd.Sotien,
                                    Thang = hd.Thang,
                                    Dadong = hd.Dadong,
                                    Type = "Master"
                                };
                    DgInvoices.ItemsSource = query.ToList();
                }
                else
                {
                    var query = from hd in db.Hoadontvs
                                join tv in db.Thanhviens on hd.Nguoichuyen equals tv.Id
                                where hd.Daxoa == false && hd.Dadong == false && hd.Nguoichuyen == User.CurrentUserId
                                select new
                                {
                                    ID = hd.Mahdtv,
                                    DisplayName = tv.Ten,
                                    Noidung = hd.Noidung,
                                    Sotien = hd.Sotien,
                                    Thang = hd.Thang,
                                    Dadong = hd.Dadong,
                                    Type = "Member"
                                };
                    DgInvoices.ItemsSource = query.ToList();
                }
            }
        }

        // Lọc theo loại hóa đơn (Nội dung)
        private void CmbBillType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgInvoices == null) return;

            string type = (CmbBillType.SelectedItem as ComboBoxItem).Content.ToString();
            using (var db = new RoommateManagerContext())
            {
                var query = from hd in db.Hoadontvs
                            join tv in db.Thanhviens on hd.Nguoichuyen equals tv.Id
                            where hd.Daxoa == false && hd.Dadong == false
                            select new
                            {
                                Mahdtv = hd.Mahdtv,
                                TenNguoiThanhToan = tv.Ten,
                                Sotien = hd.Sotien,
                                Thang = hd.Thang,
                                Dadong = hd.Dadong,
                                Noidung = hd.Noidung
                            };

                if (type != "Tất cả")
                    query = query.Where(x => x.Noidung.Contains(type));

                DgInvoices.ItemsSource = query.ToList();
            }
        }


        private void CmbMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra xem phần chọn ngân hàng đã được khởi tạo chưa (tránh lỗi null khi load trang)
            if (PnlBankSelection == null) return;

            ComboBoxItem selectedItem = CmbMethod.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string method = selectedItem.Content.ToString();

                if (method == "Chuyển khoản Ngân hàng")
                {
                    // Hiện phần chọn ngân hàng
                    PnlBankSelection.Visibility = Visibility.Visible;
                }
                else
                {
                    // Ẩn phần chọn ngân hàng khi chọn Momo hoặc ZaloPay
                    PnlBankSelection.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Khi chọn 1 dòng trên Grid
        private void DgInvoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic selected = DgInvoices.SelectedItem;
            if (selected != null)
            {
                TxtBillCode.Text = selected.ID.ToString();
                if (selected.Sotien != null)
                    TxtPayAmount.Text = ((decimal)selected.Sotien).ToString("N0");
            }
        }

        private string GetSelectedBank()
        {
            if (RbVcb.IsChecked == true) return "Vietcombank";
            // Thêm các RadioButton khác của bạn vào đây tương tự
            return "Khác";
        }

        private void BtnPayNow_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DgInvoices.SelectedItem;
            if (selected == null) return;
            currentOTP = new Random().Next(1000, 9999).ToString();
            string userOTP = Microsoft.VisualBasic.Interaction.InputBox($"Mã xác thực: {currentOTP}", "Nhập OTP");

            if (userOTP == currentOTP)
            {
                using (var db = new RoommateManagerContext())
                {
                    int id = (int)selected.ID;
                    if (selected.Type == "Member")
                    {
                        var hd = db.Hoadontvs.FirstOrDefault(x => x.Mahdtv == id);
                        if (hd != null)
                        {
                            hd.Dadong = true;
                            hd.Ngaygdtv = DateOnly.FromDateTime(DateTime.Now);
                        }
                    }
                    else
                    {
                        var hd = db.Hoadontongs.FirstOrDefault(x => x.Mahdt == id);
                        if (hd != null)
                        {
                            hd.Dadong = true;
                            hd.Ngaygdt = DateOnly.FromDateTime(DateTime.Now);
                        }
                    }
                    db.SaveChanges();
                }
                MessageBox.Show("Thanh toán thành công!");
                LoadData();
            }
        }

        private void BtnSplitMoney_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DgInvoices.SelectedItem;
            if (selected == null || selected.Type != "Master")
            {
                MessageBox.Show("Vui lòng chọn một Hóa đơn tổng từ danh sách!");
                return;
            }

            int idTong = (int)selected.ID;

            using (var db = new RoommateManagerContext())
            {
                try
                {
                    var hdTong = db.Hoadontongs.FirstOrDefault(x => x.Mahdt == idTong);
                    if (hdTong == null) return;

                    var dsThanhVien = db.Thanhviens.Where(tv => tv.Manha == hdTong.Manha).ToList();
                    if (dsThanhVien.Count == 0)
                    {
                        MessageBox.Show("Phòng này chưa có thành viên!");
                        return;
                    }

                    var confirm = MessageBox.Show($"Chia {hdTong.Sotien:N0} VNĐ cho {dsThanhVien.Count} người?",
                        "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm == MessageBoxResult.No) return;

                    decimal moiNguoi = Math.Round((hdTong.Sotien ?? 0) / dsThanhVien.Count, 2);

                    foreach (var tv in dsThanhVien)
                    {
                        if (db.Hoadontvs.Any(x => x.Mahdt == idTong && x.Nguoichuyen == tv.Id)) continue;

                        Hoadontv hdMoi = new Hoadontv();
                        hdMoi.Mahdt = idTong;
                        hdMoi.Noidung = hdTong.Noidung;
                        hdMoi.Nguoichuyen = tv.Id;
                        hdMoi.Nguoinhan = User.CurrentUserId;
                        hdMoi.Sotien = moiNguoi;
                        hdMoi.Thang = hdTong.Thang;
                        hdMoi.Nam = hdTong.Nam;
                        hdMoi.Ngaygui = DateOnly.FromDateTime(DateTime.Now);
                        hdMoi.Dadong = false;
                        hdMoi.Daxoa = false;

                        db.Hoadontvs.Add(hdMoi);
                    }

                    db.SaveChanges();
                    MessageBox.Show("Đã chia tiền xong!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
    }
}