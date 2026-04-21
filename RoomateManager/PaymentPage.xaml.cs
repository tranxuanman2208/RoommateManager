using RoomateManager.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RoommateManager.Models; // Thêm dòng này để nhận diện HOADONTV và RoommateManagerContext

namespace RoommateManager.Views
{
    public partial class PaymentPage : Page
    {
        RoommateManagerContext db = new RoommateManagerContext();
        private string currentOTP = "";

        public PaymentPage()
        {
            InitializeComponent();
            LoadData();
        }

        // Tải dữ liệu LINQ to SQL
        private void LoadData()
        {
            // Đảm bảo db là Context của bạn
            var query = from hd in db.Hoadontvs
                        join tv in db.Thanhviens on hd.Nguoichuyen equals tv.Id
                        where hd.Daxoa == false && hd.Dadong == false
                        select new
                        {
                            Mahdtv = hd.Mahdtv,
                            TenNguoiThanhToan = tv.Ten,
                            Noidung = hd.Noidung,
                            Sotien = hd.Sotien,
                            Thang = hd.Thang,
                            Dadong = hd.Dadong
                        };

            DgInvoices.ItemsSource = query.ToList();
        }

        // Lọc theo loại hóa đơn (Nội dung)
        private void CmbBillType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgInvoices == null) return;

            string type = (CmbBillType.SelectedItem as ComboBoxItem).Content.ToString();

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
                TxtBillCode.Text = selected.Mahdtv.ToString();

                // SỬA TẠI ĐÂY: Thêm định dạng "N0" để bỏ phần thập phân và thêm dấu chấm phân cách
                if (selected.Sotien != null)
                {
                    TxtPayAmount.Text = ((decimal)selected.Sotien).ToString("N0");
                }
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
            // 1. Kiểm tra xem đã chọn hóa đơn chưa
            if (string.IsNullOrEmpty(TxtBillCode.Text))
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn từ danh sách trước!", "Thông báo");
                return;
            }

            // 2. Lấy dữ liệu dòng đang chọn
            dynamic selected = DgInvoices.SelectedItem;
            if (selected == null) return;

            decimal soTienNo = (decimal)selected.Sotien;
            decimal soTienDong = 0;

            // --- CẬP NHẬT: Xử lý chuỗi số tiền nhập vào (Lọc bỏ tất cả ký tự không phải số) ---
            string rawInput = TxtPayAmount.Text;
            string cleanNumber = new string(rawInput.Where(c => char.IsDigit(c)).ToArray());

            if (!decimal.TryParse(cleanNumber, out soTienDong))
            {
                MessageBox.Show("Số tiền nhập vào không hợp lệ!", "Lỗi nhập liệu");
                return;
            }

            // --- LOGIC QUAN TRỌNG: PHẢI NHẬP CHÍNH XÁC ---
            if (soTienDong != soTienNo)
            {
                MessageBox.Show($"Số tiền đóng không khớp! Bạn phải nhập chính xác: {soTienNo:N0} VNĐ",
                                "Thanh toán thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Xác nhận giao dịch
            MessageBoxResult confirm = MessageBox.Show($"Xác nhận thanh toán {soTienNo:N0} VNĐ cho hóa đơn này?",
                                                      "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.No) return;

            // 4. Tạo và gửi mã OTP
            Random rd = new Random();
            currentOTP = rd.Next(1000, 9999).ToString();
            MessageBox.Show($"Mã OTP đã được gửi!\n(Mã xác thực: {currentOTP})", "Xác thực OTP");

            // 5. Nhập OTP
            string userOTP = Microsoft.VisualBasic.Interaction.InputBox("Nhập mã xác thực OTP:", "Xác thực", "");

            if (userOTP == currentOTP)
            {
                // 6. Cập nhật Database
                int maHD = int.Parse(TxtBillCode.Text);
                var hdUpdate = db.Hoadontvs.SingleOrDefault(x => x.Mahdtv == maHD);

                if (hdUpdate != null)
                {
                    hdUpdate.Dadong = true;
                    hdUpdate.Ngaygdtv = DateOnly.FromDateTime(DateTime.Now);
                    db.SaveChanges();

                    // 7. Hiện biên lai
                    string phuongThuc = (CmbMethod.SelectedItem as ComboBoxItem).Content.ToString();
                    string nganHang = phuongThuc == "Chuyển khoản Ngân hàng" ? $"\n- Ngân hàng: {GetSelectedBank()}" : "";

                    string msg = "THANH TOÁN THÀNH CÔNG!\n" +
                                 "--------------------------\n" +
                                 $"- Mã giao dịch: {hdUpdate.Mahdtv}\n" +
                                 $"- Người trả: {selected.TenNguoiThanhToan}\n" +
                                 $"- Loại phí: {selected.Noidung}\n" +
                                 $"- Số tiền: {soTienNo:N0} VNĐ\n" +
                                 $"- Hình thức: {phuongThuc}{nganHang}\n" +
                                 $"- Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

                    MessageBox.Show(msg, "Biên lai giao dịch điện tử", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 8. Dọn dẹp
                    TxtBillCode.Text = "";
                    TxtPayAmount.Text = "";
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Mã OTP không chính xác!", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}