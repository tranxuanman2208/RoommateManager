using RoomateManager;
using RoomateManager.Models;
using RoommateManager.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using RoomateManager.Helpers;

namespace RoommateManager
{
    public partial class MainWindow : Window
    {
        private Button? _activeBtn;
        public bool HasUnsavedData { get; set; } = false;
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigated += MainFrame_Navigated; //Thay đổi trang sẽ cập nhật
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(User.CurrentUserName) || string.IsNullOrWhiteSpace(User.CurrentUserId))
            {
                Menu.IsEnabled = false;
                btnDangKy.Visibility = Visibility.Visible;
                btnDangNhap.Visibility = Visibility.Visible;
            }
            else
            {
                Menu.Opacity = 1.0;
                Menu.IsEnabled = true;
                btnDangKy.Visibility = Visibility.Collapsed;
                btnDangNhap.Visibility = Visibility.Collapsed;
                if(User.IsAdmin == false)
                {
                    BtnChart.IsEnabled = false;
                    BtnViolation.IsEnabled = false;
                    BtnTask.IsEnabled = false;
                }
            }
        }
        public void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            if (HasUnsavedData)
            {
                var result = MessageBox.Show(
                    "Bạn có dữ liệu chưa lưu. Rời đi?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
                HasUnsavedData = false;
            }

            string tag = btn.Tag?.ToString() ?? "";
            switch (tag)
            {
                case "Home":
                    Navigate(btn, new MemberListPage()); break;
                case "Invoice":
                    Navigate(btn, new PaymentPage()); break;
                case "Chart":
                    MainFrame.Navigate(new ThongKePage()); break;
                case "Task":
                    Navigate(btn, new PhanCongPage()); break;
                case "Violation":
                    Navigate(btn, new XuLyViPhamPage()); break;
                case "BaoCao": 
                    MainFrame.Navigate(new BaoCaoPage()); break;
                case "VatDung":
                    Navigate(btn, new VatDungPage()); break;
                case "Logout": DangXuat_Click(btn, e); break;
                default:
                    Navigate(btn, new MemberListPage()); break;
            }
        }

        private void Navigate(Button activeBtn, Page page)
        {
            if (activeBtn == null) return;

            // Reset độ mờ của các nút
            var navButtons = GetNavigationButtons();
            if (navButtons != null)
            {
                foreach (UIElement child in navButtons)
                {
                    if (child is Button b) b.Opacity = 0.6;
                }
            }

            activeBtn.Opacity = 1.0;
            _activeBtn = activeBtn;
            MainFrame.Navigate(page);
        }

        private UIElementCollection? GetNavigationButtons()
        {
            try
            {
                if (VisualTreeHelper.GetChildrenCount(this) > 0)
                {
                    var mainGrid = VisualTreeHelper.GetChild(this, 0) as Grid;
                    var border = mainGrid?.Children[1] as Border;
                    var navGrid = border?.Child as Grid;
                    return navGrid?.Children;
                }
            }
            catch { }
            return null;
        }

        private void DangKy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            Navigate(btn, new DangKyPage());
        }

        private void DangNhap_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            Navigate(btn, new DangNhapPage());
        }
        
        private void DangXuat_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có muốn đăng xuất không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                User.Clear();
                MainFrame.Content = null;
            }
            else return;
        }
    }
}