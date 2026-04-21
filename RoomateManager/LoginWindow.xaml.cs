using BCrypt.Net;
using RoomateManager.Helpers;
using RoomateManager.Models;
using RoommateManager;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace RoomateManager
{
    public partial class LoginWindow : Window
    {
       
        private const int MaxAttempts = 99;
        private const int LockMinutes = 3;

      
        private readonly string _rememberFile =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RoommateManager", "remember.txt");

        public LoginWindow()
        {
            InitializeComponent();
            LoadRememberMe();
        }

      
        private void LoadRememberMe()
        {
            try
            {
                if (File.Exists(_rememberFile))
                {
                    TxtUsername.Text = File.ReadAllText(_rememberFile);
                    ChkRemember.IsChecked = true;
                }
            }
            catch { }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
           
            BannerError.Visibility = Visibility.Collapsed;
            BannerLock.Visibility = Visibility.Collapsed;
            ErrUsername.Visibility = Visibility.Collapsed;
            ErrPassword.Visibility = Visibility.Collapsed;
            TxtAttemptsLeft.Visibility = Visibility.Collapsed;

            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

          
            bool valid = true;
            if (string.IsNullOrWhiteSpace(username))
            { ErrUsername.Visibility = Visibility.Visible; valid = false; }
            if (string.IsNullOrWhiteSpace(password))
            { ErrPassword.Visibility = Visibility.Visible; valid = false; }
            if (!valid) return;

            try
            {
                
                using var db = new RoommateManagerContext();

                
                var user = db.Thanhviens
                             .FirstOrDefault(tv => tv.Username == username);

        
                if (user == null)
                {
                    BannerError.Visibility = Visibility.Visible;
                    return;
                }


                if (user.Thoigiankhoa.HasValue && user.Thoigiankhoa.Value > DateTime.Now)
                {
                    int remaining = (int)(user.Thoigiankhoa.Value - DateTime.Now).TotalMinutes + 1;
                    BannerLock.Visibility = Visibility.Visible;
                    TxtLockMsg.Text = $"Tài khoản tạm khóa. Vui lòng thử lại sau {remaining} phút.";
                    return;
                }

          
                bool passwordOk = false;
                try
                {
                    passwordOk = BCrypt.Net.BCrypt.Verify(password, user.Matkhau);
                }
                catch
                {
                   
                    passwordOk = user.Matkhau == password;
                }

                if (!passwordOk)
                {
                 
                    user.Solansat = (user.Solansat ?? 0) + 1;

                    
                    if (user.Solansat >= MaxAttempts)
                    {
                        user.Thoigiankhoa = DateTime.Now.AddMinutes(LockMinutes);
                        db.SaveChanges();

                        BannerLock.Visibility = Visibility.Visible;
                        TxtLockMsg.Text = $"Tài khoản đã bị khóa {LockMinutes} phút do đăng nhập sai quá nhiều lần.";
                    }
                    else
                    {
                        db.SaveChanges();
                        int left = MaxAttempts - user.Solansat.Value;
                        TxtAttemptsLeft.Text = $"Còn {left} lần thử trước khi bị khóa.";
                        TxtAttemptsLeft.Visibility = Visibility.Visible;
                        BannerError.Visibility = Visibility.Visible; // AC3
                    }
                    return;
                }

                
                user.Solansat = 0;
                user.Thoigiankhoa = null;
                db.SaveChanges();

                
                var dir = Path.GetDirectoryName(_rememberFile);
                if (dir != null) Directory.CreateDirectory(dir);
                if (ChkRemember.IsChecked == true)
                    File.WriteAllText(_rememberFile, username);
                else if (File.Exists(_rememberFile))
                    File.Delete(_rememberFile);

               
                SessionManager.CurrentUserId = user.Id;
                SessionManager.CurrentUserName = user.Ten;
                SessionManager.IsAdmin = user.Ad == true;

              
                var main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}