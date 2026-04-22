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
                    string? tenHienTai = db.Thanhviens
                          .Where(tv => tv.Id == User.CurrentUserId)
                          .Select(tv => tv.Ten).FirstOrDefault();
                    // 1. Lấy danh sách Hóa đơn tổng (Master)
                    var qMaster = db.Hoadontongs
                        .Where(hd => hd.Manha == User.CurrentHome && (hd.Daxoa == false || hd.Dadong == false))
                        .Select(hd => new
                        {
                            ID = hd.Mahdt,
                            DisplayName = "[TỔNG] " + tenHienTai, // Thêm tiền tố để dễ phân biệt
                            Noidung = hd.Noidung,
                            Sotien = hd.Sotien,
                            Thang = hd.Thang,
                            Dadong = hd.Dadong,
                            Type = "Master"
                        }).ToList();

                    // 2. Lấy danh sách Hóa đơn thành viên (Member) - Admin xem hết để quản lý
                    var qMember = db.Hoadontvs
                        .Where(hd => hd.MahdtNavigation.Manha == User.CurrentHome && (hd.Daxoa == false || hd.Dadong == false))
                        .OrderBy(hd => hd.Nguoichuyen)
                        .Select(hd => new
                        {
                            ID = hd.Mahdtv,
                            DisplayName = hd.NguoichuyenNavigation.Ten, // Tên thành viên cần nộp
                            Noidung = hd.Noidung,
                            Sotien = hd.Sotien,
                            Thang = hd.Thang,
                            Dadong = hd.Dadong,
                            Type = "Member"
                        }).ToList();

                    // 3. Nối hai danh sách lại và hiển thị
                    DgInvoices.ItemsSource = qMaster.Concat(qMember).ToList();
                    return;
                }
                else
                {
                    var query = from hd in db.Hoadontvs
                                join tv in db.Thanhviens on hd.Nguoichuyen equals tv.Id
                                where (hd.Daxoa == false || hd.Dadong == false) && hd.Nguoichuyen == User.CurrentUserId
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
            if (!User.IsAdmin)
            {
                string type = (CmbBillType.SelectedItem as ComboBoxItem).Content.ToString();
                using (var db = new RoommateManagerContext())
                {
                    if (type == "Tất cả")
                    {
                        var query = db.Hoadontvs
                                    .Where(hd => hd.Nguoichuyen == User.CurrentUserId && (hd.Daxoa == false || hd.Dadong == false))
                                    .OrderBy(hd => hd.Ngaygui)
                                    .Select(hd => new
                                    {
                                        ID = hd.Mahdtv,
                                        DisplayName = hd.NguoichuyenNavigation.Ten,
                                        Noidung = hd.Noidung,
                                        Sotien = hd.Sotien,
                                        Thang = hd.Thang,
                                        Dadong = hd.Dadong,
                                        Type = "Member"
                                    });
                        DgInvoices.ItemsSource = query.ToList();
                        return;
                    }
                    else
                    {
                        if (type == "Tiền điện")
                        {
                            var query = db.Hoadontvs.Where(tv => tv.Nguoichuyen == User.CurrentUserId && (tv.Daxoa == false || tv.Dadong == false))
                                .Where(tv => tv.MahdtNavigation.Mancc == "NCC001")
                                .OrderBy(hd => hd.Ngaygui)
                                .Select(tv => new
                                {
                                    ID = tv.Mahdtv,
                                    DisplayName = tv.NguoichuyenNavigation.Ten,
                                    Noidung = tv.Noidung,
                                    Sotien = tv.Sotien,
                                    Thang = tv.Thang,
                                    Dadong = tv.Dadong,
                                    Type = "Member"
                                });
                            DgInvoices.ItemsSource = query.ToList();
                        }
                        else if (type == "Tiền nước")
                        {
                            var query = db.Hoadontvs.Where(tv => tv.Nguoichuyen == User.CurrentUserId && (tv.Daxoa == false || tv.Dadong == false))
                                .Where(tv => tv.MahdtNavigation.Mancc == "NCC002")
                                .OrderBy(hd => hd.Ngaygui)
                                .Select(tv => new
                                {
                                    ID = tv.Mahdtv,
                                    DisplayName = tv.NguoichuyenNavigation.Ten,
                                    Noidung = tv.Noidung,
                                    Sotien = tv.Sotien,
                                    Thang = tv.Thang,
                                    Dadong = tv.Dadong,
                                    Type = "Member"
                                });
                            DgInvoices.ItemsSource = query.ToList();
                        }
                        else if (type == "Tiền nhà")
                        {
                            var query = db.Hoadontvs.Where(tv => tv.Nguoichuyen == User.CurrentUserId && (tv.Daxoa == false || tv.Dadong == false))
                                .Where(tv => tv.MahdtNavigation.Mancc == "NCC003")
                                .Select(tv => new
                                {
                                    ID = tv.Mahdtv,
                                    DisplayName = tv.NguoichuyenNavigation.Ten,
                                    Noidung = tv.Noidung,
                                    Sotien = tv.Sotien,
                                    Thang = tv.Thang,
                                    Dadong = tv.Dadong,
                                    Type = "Member"
                                });
                            DgInvoices.ItemsSource = query.ToList();
                        }
                    }
                }
            }
            else
            {
                string type = (CmbBillType.SelectedItem as ComboBoxItem).Content.ToString();
                using (var db = new RoommateManagerContext())
                {
                    string? tenHienTai = db.Thanhviens
                          .Where(tv => tv.Id == User.CurrentUserId)
                          .Select(tv => tv.Ten).FirstOrDefault();
                    if (type == "Tất cả")
                    {
                        // 1. Lấy danh sách Hóa đơn tổng (Master)
                        var qMaster = db.Hoadontongs
                            .Where(hd => hd.Manha == User.CurrentHome && (hd.Daxoa == false || hd.Dadong == false))
                            .Select(hd => new
                            {
                                ID = hd.Mahdt,
                                DisplayName = "[TỔNG] " + tenHienTai, // Thêm tiền tố để dễ phân biệt
                                Noidung = hd.Noidung,
                                Sotien = hd.Sotien,
                                Thang = hd.Thang,
                                Dadong = hd.Dadong,
                                Type = "Master"
                            }).ToList();

                        // 2. Lấy danh sách Hóa đơn thành viên (Member) - Admin xem hết để quản lý
                        var qMember = db.Hoadontvs
                            .Where(hd => hd.MahdtNavigation.Manha == User.CurrentHome && (hd.Daxoa == false || hd.Dadong == false))
                            .Select(hd => new
                            {
                                ID = hd.Mahdtv,
                                DisplayName = hd.NguoichuyenNavigation.Ten, // Tên thành viên cần nộp
                                Noidung = hd.Noidung,
                                Sotien = hd.Sotien,
                                Thang = hd.Thang,
                                Dadong = hd.Dadong,
                                Type = "Member"
                            }).ToList();

                        // 3. Nối hai danh sách lại và hiển thị
                        DgInvoices.ItemsSource = qMaster.Concat(qMember).ToList();
                        return;
                    }
                    else if (type != "Tất cả")
                    {
                        if (type == "Tiền điện")
                        {
                            var query = db.Hoadontongs.Where(hd => hd.Manha == User.CurrentHome && (hd.Daxoa == false || hd.Dadong == false))
                                .Where(hd => hd.Mancc == "NCC001")
                                .Select(hd => new
                                {
                                    ID = hd.Mahdt,
                                    DisplayName = "[TỔNG] "+ tenHienTai,
                                    Noidung = hd.Noidung,
                                    Sotien = hd.Sotien,
                                    Thang = hd.Thang,
                                    Dadong = hd.Dadong,
                                    Type = "Master"
                                });
                            DgInvoices.ItemsSource = query.ToList();
                            return;
                        }
                        else if (type == "Tiền nước")
                        {
                            var query = db.Hoadontongs.Where(hd => hd.Manha == User.CurrentHome && (hd.Daxoa == false || hd.Dadong == false))
                                .Where(hd => hd.Mancc == "NCC002")
                                .Select(hd => new
                                {
                                    ID = hd.Mahdt,
                                    DisplayName = "[TỔNG] " + tenHienTai,
                                    Noidung = hd.Noidung,
                                    Sotien = hd.Sotien,
                                    Thang = hd.Thang,
                                    Dadong = hd.Dadong,
                                    Type = "Master"
                                });
                            DgInvoices.ItemsSource = query.ToList();
                            return;
                        }
                        else if (type == "Tiền nhà")
                        {
                            var query = db.Hoadontongs.Where(hd => hd.Manha == User.CurrentHome && (hd.Daxoa == false || hd.Dadong == false))
                                .Where(hd => hd.Mancc == "NCC003")
                                .Select(hd => new
                                {
                                    ID = hd.Mahdt,
                                    DisplayName = "[TỔNG] " + tenHienTai,
                                    Noidung = hd.Noidung,
                                    Sotien = hd.Sotien,
                                    Thang = hd.Thang,
                                    Dadong = hd.Dadong,
                                    Type = "Master"
                                });
                            DgInvoices.ItemsSource = query.ToList();
                            return;
                        }
                    }
                }
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

                        Thongbao tbMoi = new Thongbao();
                        tbMoi.Nguoitb = User.CurrentUserId;
                        tbMoi.Nguoinhan = tv.Id.ToString();
                        tbMoi.Noidung = hdMoi.Noidung;
                        tbMoi.Ngaytb = hdMoi.Ngaygui;
                        tbMoi.Dadoc = false;
                        tbMoi.Daxoa = false;
                        db.Thongbaos.Add(tbMoi);
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