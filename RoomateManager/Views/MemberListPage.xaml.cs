using RoomateManager.Models;
using RoommateManager.Models;
using System;
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
        private MainWindow _mainWindow;
        private bool _isManager = true;

        // --- GIỮ NGUYÊN CLASS MemberVM ---
        public class MemberVM
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string AvatarInitial => Name?.Length > 0 ? Name[0].ToString().ToUpper() : "?";
            public MemberRole Role { get; set; }
            public PaymentStatus PaymentStatus { get; set; }
            public bool IsCurrentUser { get; set; }
            public string RoleDisplay => Role == MemberRole.Manager ? "👑 Quản lý" : "Thành viên";
            public string StatusDisplay => PaymentStatus switch
            {
                PaymentStatus.Paid => "Đã đóng",
                PaymentStatus.Unpaid => "Chưa đóng",
                _ => "N/A"
            };
            public string StatusColor => PaymentStatus switch
            {
                PaymentStatus.Paid => "#4CAF50",
                PaymentStatus.Unpaid => "#F44336",
                _ => "#9E9E9E"
            };
        }

        // --- GIỮ NGUYÊN DANH SÁCH TẠM THỜI _members ---
        private List<MemberVM> _members = new List<MemberVM>
        {
            new MemberVM { Id=1, Name="Minh Nhật", Role=MemberRole.Manager, PaymentStatus=PaymentStatus.Paid, IsCurrentUser=true },
            new MemberVM { Id=2, Name="Lê Hùng", Role=MemberRole.Member, PaymentStatus=PaymentStatus.Unpaid },
            new MemberVM { Id=3, Name="Trần Minh", Role=MemberRole.Member, PaymentStatus=PaymentStatus.Paid },
            new MemberVM { Id=4, Name="Tuấn Bảo", Role=MemberRole.Member, PaymentStatus=PaymentStatus.NotApplicable },
        };

        public MemberListPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            MemberList.ItemsSource = _members;
            BtnAddMember.Visibility = _isManager ? Visibility.Visible : Visibility.Collapsed;
        }

        // ==========================================
        // LOGIC POPUP THÔNG BÁO (ĐÃ CẬP NHẬT THEO GIAO DIỆN MỚI)
        // ==========================================

        private void BtnNotification_Click(object sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = true;
            btnModeStatus_Click(null, null);
        }

        private void btn_ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = false;
        }

        private void btnModeStatus_Click(object sender, RoutedEventArgs e)
        {
            scrViewStatus.Visibility = Visibility.Visible;
            stkAddNotification.Visibility = Visibility.Collapsed;

            btnTabStatus.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            btnTabStatus.Foreground = Brushes.White;
            btnTabAdd.Background = Brushes.White;
            btnTabAdd.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243));

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var ds = db.Thongbaos.Where(t => t.Daxoa == false)
                        .OrderByDescending(t => t.Matb)
                        .Select(t => new {
                            Noidung = t.Noidung,
                            Ngaytb = t.Ngaytb,
                            TyleDoc = db.ChitietXemTbs.Count(ct => ct.Matb == t.Matb && ct.Dadoc == true) + "/" + db.Thanhviens.Count()
                        }).ToList();
                    lstNotifications.ItemsSource = ds;
                }
            }
            catch { }
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
                        // Chỉ load cho combobox người nhận (người gửi đã là Admin cố định trên XAML)
                        cboRecipient.ItemsSource = users;
                        cboRecipient.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private void btnConfirmSend_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputTB.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung thông báo!");
                return;
            }

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var tb = new Thongbao
                    {
                        Noidung = txtInputTB.Text,
                        Ngaytb = DateOnly.FromDateTime(DateTime.Now),
                        Daxoa = false
                    };
                    db.Thongbaos.Add(tb);
                    db.SaveChanges();

                    foreach (var tv in db.Thanhviens.ToList())
                    {
                        db.ChitietXemTbs.Add(new ChitietXemTb
                        {
                            Matb = tb.Matb,
                            Matv = tv.Id,
                            Dadoc = false
                        });
                    }

                    db.SaveChanges();
                    MessageBox.Show("Đã gửi thông báo thành công!");

                    txtInputTB.Clear();
                    btnModeStatus_Click(null, null);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi gửi thông báo: " + ex.Message); }
        }

        // ==========================================
        // LOGIC QUẢN LÝ THÀNH VIÊN (GIỮ NGUYÊN GỐC)
        // ==========================================

        private void BtnAddMember_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window { Title = "Thêm thành viên", Width = 320, Height = 200, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = _mainWindow };
            var sp = new StackPanel { Margin = new Thickness(20) };
            var tb = new TextBox { Margin = new Thickness(0, 8, 0, 8) };
            var btn = new Button { Content = "Gửi lời mời", Background = Brushes.DodgerBlue, Foreground = Brushes.White, Padding = new Thickness(12, 8, 12, 8) };

            btn.Click += (s, ev) => {
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
            var border = sender as Border;
            var member = border?.DataContext as MemberVM;
            if (member == null || member.IsCurrentUser)
            {
                if (member?.IsCurrentUser == true)
                    MessageBox.Show("Bạn cần chuyển quyền Quản lý cho thành viên khác trước khi rời phòng.", "Không thể thực hiện", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Xóa thành viên '{member.Name}' khỏi phòng?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _members.Remove(member);
                MemberList.ItemsSource = null;
                MemberList.ItemsSource = _members;
            }
        }
    }
}