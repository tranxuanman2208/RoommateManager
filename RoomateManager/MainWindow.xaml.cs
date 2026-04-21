using RoomateManager;
using RoomateManager.Models;
using RoommateManager.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace RoommateManager
{
    public partial class MainWindow : Window
    {
        private Button? _activeBtn;
        public bool HasUnsavedData { get; set; } = false;
        private bool Ad = false;
        private string? User;
        public MainWindow()
        {
            InitializeComponent();
            // Đăng ký sự kiện Loaded để đảm bảo UI đã sẵn sàng trước khi Navigate
            this.Loaded += MainWindow_Loaded;
            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Nếu trang hiện tại KHÔNG PHẢI là DangKyPage, hiện lại menu
            if (!(e.Content is DangKyPage))
            {
                Menu.Visibility = Visibility.Visible;
                MenuRow.Height = new GridLength(65);
                btnDangKy.Visibility = Visibility.Visible;
            }
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
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
                // Tìm Grid chứa các nút điều hướng (nằm trong Row 1)
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
            Menu.Visibility = Visibility.Collapsed;
            MenuRow.Height = new GridLength(0);
            btnDangKy.Visibility = Visibility.Collapsed;
        }
    }
}