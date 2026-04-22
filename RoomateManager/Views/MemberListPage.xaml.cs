using RoomateManager.Helpers;
using RoomateManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RoommateManager.Views
{
    public partial class MemberListPage : Page
    {
        public class MemberVM
        {
            public string Id { get; set; } = "";
            public string? Name { get; set; }
            public bool IsAdmin { get; set; }
            public string? IsCurrentUser { get; set; }
            public string RoleDisplay => IsAdmin ? "👑 Quản lý" : "Thành viên";
            public string StatusDisplay { get; set; } = "Hoạt động";
            public string StatusColor { get; set; } = "#9E9E9E";
        }

        private List<MemberVM> _members = new();

        public MemberListPage()
        {
            InitializeComponent();

            LoadMembers();
            
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
                        IsAdmin = (tv.Ad == true) ? true : false,
                        IsCurrentUser = tv.Id,
                        StatusColor = tv.StatusColor,
                        StatusDisplay = tv.StatusDisplay
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
                    .Where(tb => tb.Nguoinhan == User.CurrentUserId
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

            btnTabAdd.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            btnTabAdd.Foreground = Brushes.White;
            btnTabStatus.Background = Brushes.White;
            btnTabStatus.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243));

            txtInputTB.Clear();

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var users = db.Thanhviens.ToList();
                    if (users.Count > 0)
                    {
                        cboRecipient.ItemsSource = users;
                        cboRecipient.SelectedIndex = 0;
                    }
                }
            }
            catch { }
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
                    Nguoitb = User.CurrentUserId,
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

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ThongTinCaNhanPage());
        }
    }
}