using RoomateManager.Models;
using RoomateManager.Services;
using RoommateManager;
using RoommateManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

namespace RoomateManager
{

    public partial class DangKyPage : Page
    {
        public DangKyPage()
        {
            InitializeComponent();
        }

        private async void btnDangKy_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra mật khẩu trùng khớp
            if (txtPass.Text != txtPassA.Text)
            {
                MessageBox.Show("Mật khẩu nhập lại không trùng khớp!");
                return;
            }

            // 2. Kiểm tra định dạng email @gmail.com
            if (!txtMail.Text.EndsWith("@gmail.com"))
            {
                MessageBox.Show("Email phải có đuôi @gmail.com!");
                return;
            }

            // 3. Kiểm tra các trường trống
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text) || string.IsNullOrWhiteSpace(txtMail.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            try
            {
                // 4. Tạo mã OTP ngẫu nhiên (6 số)
                Random rd = new Random();
                string generatedOTP = rd.Next(100000, 999999).ToString();

                // 5. Gửi Email OTP
                bool result = await EmailService.SendEmailAsync(txtMail.Text, "Mã OTP Đăng Ký", $"Mã của bạn là: {generatedOTP}");

                if (result)
                {
                    // 6. Yêu cầu nhập OTP để xác nhận
                    string inputOTP = Microsoft.VisualBasic.Interaction.InputBox(
                        "Mã xác thực đã được gửi đến email của bạn. Vui lòng nhập mã vào đây:",
                        "Xác thực OTP", "");

                    if (inputOTP == generatedOTP)
                    {
                        LuuThanhVienVaoDatabase();
                    }
                    else
                    {
                        MessageBox.Show("Mã OTP không chính xác!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        private void LuuThanhVienVaoDatabase()
        {
            try
            {
                using (var db = new RoommateManagerContext())
                {
                    // Kiểm tra username đã tồn tại chưa
                    if (db.Thanhviens.Any(t => t.Username == txtUser.Text))
                    {
                        MessageBox.Show("Tên đăng nhập đã tồn tại!");
                        return;
                    }
                    int count = db.Thanhviens.Count();
                    string newID = "TV" + count.ToString("D3");
                    // Tạo đối tượng thành viên mới
                    var newMember = new Thanhvien
                    {
                        Id = newID,
                        Username = txtUser.Text,
                        Pass = BcryptPass.HashPassword(txtPass.Text),
                        Mail = txtMail.Text,
                        Sdt = txtPhone.Text,
                        Con = true,
                        Ad = false,
                        Diemvipham = 12,
                        Manha = txtMaNha.Text,
                    };
                    if (!string.IsNullOrWhiteSpace(txtPhone.Text)) newMember.Sdt = txtPhone.Text;
                    db.Thanhviens.Add(newMember);
                    db.SaveChanges();

                    MessageBox.Show("Đăng ký thành công!");
                    // 1. Truy cập vào MainWindow hiện tại
                    var mainWindow = (MainWindow)Application.Current.MainWindow;

                    // 2. Làm trống Frame
                    mainWindow.MainFrame.Content = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu database: " + ex.Message);
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            // 1. Truy cập vào MainWindow hiện tại
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            // 2. Làm trống Frame
            mainWindow.MainFrame.Content = null;
        }
    }
}
