using RoomateManager.Helpers;
using RoomateManager.Models;
using RoomateManager.Services;
using RoommateManager;
using RoommateManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace RoomateManager
{
    public partial class DangNhapPage : Page
    {
        public DangNhapPage()
        {
            InitializeComponent();
        }

        private void btnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }
            string inputUser = txtUser.Text;
            string inputPass = txtPass.Password;
            try
            {
                using(var db = new RoommateManagerContext())
                {
                    var user = db.Thanhviens.FirstOrDefault(u => u.Username == inputUser);
                    if (user != null)
                    {
                        // So sánh mật khẩu bằng BCrypt (Hàm Verify đã tạo ở Services)
                        bool isPasswordCorrect = BcryptPass.VerifyPassword(inputPass, user.Pass);

                        if (isPasswordCorrect)
                        {
                            // Đăng nhập thành công
                            MessageBox.Show($"Chào mừng {user.Username}!", "Đăng nhập thành công");
                            User.CurrentUserName = user.Username;
                            User.CurrentUserId = user.Id;
                            User.IsAdmin = (user.Ad == true) ? true : false;
                            var main = (MainWindow)Application.Current.MainWindow;
                            main.NavBtn_Click(main.BtnHome, new RoutedEventArgs());
                        }
                        else
                        {
                            MessageBox.Show("Mật khẩu không chính xác!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Tên đăng nhập không tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi:" + ex.Message);
                return;
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Content = null;
        }
    }
}
