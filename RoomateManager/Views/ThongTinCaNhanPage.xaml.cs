using RoomateManager.Helpers;
using RoomateManager.Models;
using RoomateManager.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RoommateManager.Views
{
    public partial class ThongTinCaNhanPage : Page
    {
        private string? originalMail = "";

        public ThongTinCaNhanPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new RoommateManagerContext())
            {
                var user = db.Thanhviens.FirstOrDefault(u => u.Id == User.CurrentUserId);
                if (user != null)
                {
                    txtId.Text = user.Id;
                    txtTen.Text = user.Ten;
                    txtSdt.Text = user.Sdt;
                    txtMaNha.Text = user.Manha;
                    txtMail.Text = user.Mail;
                    originalMail = user.Mail;
                    if (user.Ns.HasValue)
                        dpNs.SelectedDate = user.Ns.Value.ToDateTime(TimeOnly.MinValue);
                }
            }
        }

        // Mở khóa các TextBox thông thường
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            string targetName = (sender as Button).Tag.ToString();
            TextBox target = this.FindName(targetName) as TextBox;
            if (target != null)
            {
                target.IsReadOnly = false;
                target.Focus();
                btnLuu.Visibility = Visibility.Visible;
            }
        }

        private void EditDate_Click(object sender, RoutedEventArgs e)
        {
            dpNs.IsEnabled = true;
            btnLuu.Visibility = Visibility.Visible;
        }

        // Riêng Gmail xử lý xác thực
        private void EditGmail_Click(object sender, RoutedEventArgs e)
        {
            txtMail.IsReadOnly = false;
            txtMail.Focus();
            btnLuu.Visibility = Visibility.Visible;
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // 1. Nếu thay đổi Gmail thì phải qua bước OTP
            if (txtMail.Text != originalMail)
            {
                string otp = new Random().Next(1000, 9999).ToString();
                bool result = await EmailService.SendEmailAsync(txtMail.Text, "Mã OTP Đăng Ký", $"Mã của bạn là: {otp}");
                if (result)
                {
                    string input = Microsoft.VisualBasic.Interaction.InputBox("Nhập mã OTP được gửi đến Gmail mới để xác nhận đổi Gmail:", "Xác thực", "");
                    if (input != otp)
                    {
                        MessageBox.Show("Mã OTP sai! Không thể đổi Gmail.");
                        txtMail.Text = originalMail;
                        return;
                    }
                }
            }

            // 2. Cập nhật vào DB
            using (var db = new RoommateManagerContext())
            {
                try
                {
                    var user = db.Thanhviens.FirstOrDefault(u => u.Id == User.CurrentUserId);
                    if (user != null)
                    {
                        user.Ten = txtTen.Text;
                        user.Sdt = txtSdt.Text;
                        user.Manha = txtMaNha.Text;
                        user.Mail = txtMail.Text;
                        if (dpNs.SelectedDate.HasValue)
                            user.Ns = DateOnly.FromDateTime(dpNs.SelectedDate.Value);

                        db.SaveChanges();
                        MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo");

                        // Khóa lại các ô
                        LockFields();
                        btnLuu.Visibility = Visibility.Collapsed;
                        originalMail = txtMail.Text;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        private void LockFields()
        {
            txtTen.IsReadOnly = txtSdt.IsReadOnly = txtMaNha.IsReadOnly = txtMail.IsReadOnly = true;
            dpNs.IsEnabled = false;
        }
    }
}