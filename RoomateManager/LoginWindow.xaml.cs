
using BCrypt.Net;
using Microsoft.Data.SqlClient;
using RoomateManager.Helpers;
using RoommateManager;
using System;
using System.IO;
using System.Windows;

namespace RoomateManager
{
    public partial class LoginWindow : Window
    {
        private readonly string _connStr =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=RoommateManager;Integrated Security=True;TrustServerCertificate=True;";


        private const int MaxAttempts = 5;
        private const int LockMinutes = 15;


        private readonly string _rememberFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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
                    TxtEmail.Text = File.ReadAllText(_rememberFile);
                    ChkRemember.IsChecked = true;
                }
            }
            catch { }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            
            BannerError.Visibility = Visibility.Collapsed;
            ErrEmail.Visibility = Visibility.Collapsed;
            ErrPassword.Visibility = Visibility.Collapsed;

            string email = TxtEmail.Text.Trim();
            string password = TxtPassword.Password;

            
            bool valid = true;
            if (string.IsNullOrWhiteSpace(email))
            { ErrEmail.Visibility = Visibility.Visible; valid = false; }
            if (string.IsNullOrWhiteSpace(password))
            { ErrPassword.Visibility = Visibility.Visible; valid = false; }
            if (!valid) return;

            try
            {
                using var conn = new SqlConnection(_connStr);
                conn.Open();

                
                var cmd = new SqlCommand(
                    @"SELECT ID, TEN, AD, MATKHAU, SOLANSAT, THOIGIANKHOA
                      FROM THANHVIEN
                      WHERE USERNAME = @email",
                    conn);
               
                cmd.Parameters.AddWithValue("@email", email);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                  
                    ShowLoginError();
                    return;
                }

                string userId = reader["ID"].ToString()!;
                string userName = reader["TEN"]?.ToString() ?? "";
                string userRole = reader["AD"]?.ToString() ?? "0";
                string? storedHash = reader["MATKHAU"]?.ToString();
                int failCount = reader["SOLANSAT"] == DBNull.Value ? 0 : (int)reader["SOLANSAT"];
                DateTime? lockedUntil = reader["THOIGIANKHOA"] == DBNull.Value
                    ? null : (DateTime?)reader["THOIGIANKHOA"];
                reader.Close();

               
                if (lockedUntil.HasValue && lockedUntil.Value > DateTime.Now)
                {
                    var remaining = (lockedUntil.Value - DateTime.Now).Minutes + 1;
                    BannerLock.Visibility = Visibility.Visible;
                    TxtLockMsg.Text = $"Tài khoản tạm khóa. Vui lòng thử lại sau {remaining} phút.";
                    return;
                }

                
                bool passwordOk = storedHash == password;
             

                if (!passwordOk)
                {
                    failCount++;
                    
                    if (failCount >= MaxAttempts)
                    {
                        var lockCmd = new SqlCommand(
                            "UPDATE THANHVIEN SET SOLANSAT = @sc, THOIGIANKHOA = @lock WHERE ID = @id", conn);
                        lockCmd.Parameters.AddWithValue("@sc", failCount);
                        lockCmd.Parameters.AddWithValue("@lock", DateTime.Now.AddMinutes(LockMinutes));
                        lockCmd.Parameters.AddWithValue("@id", userId);
                        lockCmd.ExecuteNonQuery();

                        BannerLock.Visibility = Visibility.Visible;
                        TxtLockMsg.Text = $"Tài khoản đã bị khóa {LockMinutes} phút do đăng nhập sai quá nhiều lần.";
                    }
                    else
                    {
                        
                        var updateCmd = new SqlCommand(
                            "UPDATE THANHVIEN SET SOLANSAT = @sc WHERE ID = @id", conn);
                        updateCmd.Parameters.AddWithValue("@sc", failCount);
                        updateCmd.Parameters.AddWithValue("@id", userId);
                        updateCmd.ExecuteNonQuery();

                        int left = MaxAttempts - failCount;
                        TxtAttemptsLeft.Text = $"Còn {left} lần thử trước khi tài khoản bị khóa.";
                        TxtAttemptsLeft.Visibility = Visibility.Visible;
                        ShowLoginError(); 
                    }
                    return;
                }

                
                var resetCmd = new SqlCommand(
                    "UPDATE THANHVIEN SET SOLANSAT = 0, THOIGIANKHOA = NULL WHERE ID = @id", conn);
                resetCmd.Parameters.AddWithValue("@id", userId);
                resetCmd.ExecuteNonQuery();

          
                var dir = Path.GetDirectoryName(_rememberFile);
                if (dir != null) Directory.CreateDirectory(dir);
                if (ChkRemember.IsChecked == true)
                    File.WriteAllText(_rememberFile, email);
                else if (File.Exists(_rememberFile))
                    File.Delete(_rememberFile);

                
                SessionManager.CurrentUserId = userId;
                SessionManager.CurrentUserName = userName;
                SessionManager.CurrentUserRole = userRole;

             
                var main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối database: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

 
        private void ShowLoginError()
        {
            BannerError.Visibility = Visibility.Visible;
        }
    }
}