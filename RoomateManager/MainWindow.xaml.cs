using RoomateManager;
using RoommateManager.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RoommateManager
{
    public partial class MainWindow : Window
    {
        private Button? _activeBtn; // Thêm dấu ? để cho phép null (hết lỗi CS8618)
        public bool HasUnsavedData { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            // Đăng ký sự kiện Loaded để đảm bảo UI đã sẵn sàng trước khi Navigate
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Chỉ gọi BtnHome khi file XAML đã hết lỗi đỏ
            if (BtnHome != null)
                Navigate(BtnHome, new MemberListPage(this));
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
                    Navigate(btn, new MemberListPage(this)); break;
                case "Invoice":
                    Navigate(btn, new CreateInvoicePage(this)); break;
                case "Task":
                    Navigate(btn, new PhanCongPage()); break;
                case "Violation":
                    Navigate(btn, new XuLyViPhamPage()); break;
                default:
                    Navigate(btn, new MemberListPage(this)); break;
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
    }
}