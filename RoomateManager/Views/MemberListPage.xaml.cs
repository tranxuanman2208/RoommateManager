using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RoomateManager.Helpers;
using RoomateManager.Models;

namespace RoommateManager.Views
{
    public partial class MemberListPage : Page
    {
        private MainWindow _mainWindow;
        private bool _isManager => SessionManager.IsAdmin;

        public class MemberVM
        {
            public string Id { get; set; } = "";
            public string? Name { get; set; }
            public string AvatarInitial =>
                Name?.Length > 0 ? Name[0].ToString().ToUpper() : "?";
            public bool IsAdmin { get; set; }
            public bool IsCurrentUser { get; set; }
            public string RoleDisplay => IsAdmin ? "👑 Quản lý" : "Thành viên";
            public string StatusDisplay { get; set; } = "N/A";
            public string StatusColor { get; set; } = "#9E9E9E";
        }

        private List<MemberVM> _members = new();

        public MemberListPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadMembers();
            BtnAddMember.Visibility = _isManager
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadMembers()
        {
            try
            {
                using var db = new RoommateManagerContext();
                _members = db.Thanhviens
                    .Where(tv => tv.Con == true)
                    .Select(tv => new MemberVM
                    {
                        Id = tv.Id,
                        Name = tv.Ten,
                        IsAdmin = tv.Ad == true,
                        IsCurrentUser = tv.Id == SessionManager.CurrentUserId
                    })
                    .ToList();
                MemberList.ItemsSource = _members;
                LoadNotifications();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách thành viên: " + ex.Message);
            }
        }

       
        private void LoadNotifications()
        {
            try
            {
                using var db = new RoommateManagerContext();
                var notifications = db.Thongbaos
                    .Where(tb => tb.Nguoinhan == SessionManager.CurrentUserId
                              && (tb.Daxoa == false || tb.Daxoa == null))
                    .OrderByDescending(tb => tb.Ngaytb)
                    .ToList();

                lstNotifications.ItemsSource = notifications;

                int unread = notifications.Count(tb => tb.Dadoc == false);
                if (unread > 0)
                {
                    BadgeBorder.Visibility = Visibility.Visible;
                    txtBadgeCount.Text = unread.ToString();
                }
                else
                {
                    BadgeBorder.Visibility = Visibility.Collapsed;
                }

                
                var members = db.Thanhviens
                    .Where(tv => tv.Con == true)
                    .ToList();
                cboRecipient.ItemsSource = members;
            }
            catch { }
        }

        

        private void BtnNotification_Click(object sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = true;
        }

        private void btn_ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = false;
        }

        private void btnModeStatus_Click(object sender, RoutedEventArgs e)
        {
            scrViewStatus.Visibility = Visibility.Visible;
            stkAddNotification.Visibility = Visibility.Collapsed;
        }

        private void btnModeAdd_Click(object sender, RoutedEventArgs e)
        {
            scrViewStatus.Visibility = Visibility.Collapsed;
            stkAddNotification.Visibility = Visibility.Visible;
        }

        private void btnConfirmSend_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputTB.Text) || cboRecipient.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập nội dung và chọn người nhận.");
                return;
            }

            try
            {
                using var db = new RoommateManagerContext();
                var tb = new Thongbao
                {
                    Noidung = txtInputTB.Text.Trim(),
                    Nguoinhan = cboRecipient.SelectedValue.ToString(),
                    Nguoitb = SessionManager.CurrentUserId,
                    Ngaytb = DateOnly.FromDateTime(System.DateTime.Now),
                    Dadoc = false,
                    Daxoa = false
                };
                db.Thongbaos.Add(tb);
                db.SaveChanges();

                txtInputTB.Clear();
                MessageBox.Show("Đã gửi thông báo thành công!");
                LoadNotifications();
                btnModeStatus_Click(sender, e); 
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi gửi thông báo: " + ex.Message);
            }
        }

        private void btnDeleteNotification_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tb = btn?.DataContext as Thongbao;
            if (tb == null) return;

            try
            {
                using var db = new RoommateManagerContext();
                var item = db.Thongbaos.FirstOrDefault(t => t.Matb == tb.Matb);
                if (item != null)
                {
                    item.Daxoa = true;
                    db.SaveChanges();
                    LoadNotifications();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi xóa thông báo: " + ex.Message);
            }
        }

        private void btnViewDetail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tb = (sender as System.Windows.Controls.TextBlock)?.DataContext as Thongbao;
            if (tb == null) return;

            try
            {
                using var db = new RoommateManagerContext();
                var item = db.Thongbaos.FirstOrDefault(t => t.Matb == tb.Matb);
                if (item != null)
                {
                    item.Dadoc = true;
                    db.SaveChanges();
                }
            }
            catch { }

            MessageBox.Show(tb.Noidung ?? "", "Chi tiết thông báo");
            LoadNotifications();
        }

        private void txtNoidung_Loaded(object sender, RoutedEventArgs e)
        {
        }

        

        private void BtnAddMember_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "Thêm thành viên",
                Width = 320,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = _mainWindow
            };
            var sp = new StackPanel { Margin = new System.Windows.Thickness(20) };
            var tb = new TextBox { Margin = new System.Windows.Thickness(0, 8, 0, 8) };
            var btn = new Button
            {
                Content = "Gửi lời mời",
                Background = System.Windows.Media.Brushes.DodgerBlue,
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new System.Windows.Thickness(12, 8, 12, 8)
            };
            btn.Click += (s, ev) =>
            {
                if (!string.IsNullOrWhiteSpace(tb.Text))
                {
                    MessageBox.Show($"Đã gửi lời mời tới: {tb.Text}", "Thành công");
                    dialog.Close();
                }
            };
            sp.Children.Add(new TextBlock { Text = "Nhập thông tin thành viên:" });
            sp.Children.Add(tb);
            sp.Children.Add(btn);
            dialog.Content = sp;
            dialog.ShowDialog();
        }

        private void MemberCard_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (!_isManager) return;
            var border = sender as System.Windows.Controls.Border;
            var member = border?.DataContext as MemberVM;
            if (member == null) return;

            if (member.IsCurrentUser)
            {
                MessageBox.Show(
                    "Bạn cần chuyển quyền Quản lý cho thành viên khác trước khi rời phòng.",
                    "Không thể thực hiện",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Xóa thành viên '{member.Name}' khỏi phòng?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var db = new RoommateManagerContext();
                    var tv = db.Thanhviens.FirstOrDefault(t => t.Id == member.Id);
                    if (tv != null)
                    {
                        tv.Con = false;
                        db.SaveChanges();
                        LoadMembers();
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Lỗi xóa thành viên: " + ex.Message);
                }
            }
        }
    }
}