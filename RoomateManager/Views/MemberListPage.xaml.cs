// Views/MemberListPage.xaml.cs
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RoommateManager.Models;

namespace RoommateManager.Views
{
    public partial class MemberListPage : Page
    {
        private MainWindow _mainWindow;
        private bool _isManager = true; // Giả lập: user hiện tại là quản lý

        // ViewModel đơn giản để bind
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

            // Ẩn nút thêm nếu không phải quản lý (AC2)
            BtnAddMember.Visibility = _isManager ? Visibility.Visible : Visibility.Collapsed;
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
            var sp = new StackPanel { Margin = new Thickness(20) };
            var tb = new TextBox { Margin = new Thickness(0, 8, 0, 8) };
            var btn = new Button
            {
                Content = "Gửi lời mời",
                Background = System.Windows.Media.Brushes.DodgerBlue,
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new Thickness(12, 8, 12, 8)
            };
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
            var border = sender as System.Windows.Controls.Border;
            var member = border?.DataContext as MemberVM;
            if (member == null) return;

            // AC5: Không cho quản lý tự xóa mình
            if (member.IsCurrentUser)
            {
                MessageBox.Show("Bạn cần chuyển quyền Quản lý cho thành viên khác trước khi rời phòng.",
                    "Không thể thực hiện", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Xóa thành viên '{member.Name}' khỏi phòng?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _members.Remove(member);
                MemberList.ItemsSource = null;
                MemberList.ItemsSource = _members;
            }
        }
    }
}