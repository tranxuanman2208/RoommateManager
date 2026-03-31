// MainWindow.xaml.cs
using RoomateManager;
using RoommateManager.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RoommateManager
{
    public partial class MainWindow : Window
    {
        private Button _activeBtn;
        public bool HasUnsavedData { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            // Mặc định vào trang thành viên
            Navigate(BtnHome, new MemberListPage(this));
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            // AC4: Cảnh báo nếu có dữ liệu chưa lưu
            if (HasUnsavedData)
            {
                var result = MessageBox.Show(
                    "Bạn có dữ liệu chưa lưu. Rời đi?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
                HasUnsavedData = false;
            }

            switch (btn.Tag.ToString())
            {
                case "Home":
                    Navigate(btn, new MemberListPage(this)); break;
                case "Invoice":
                    Navigate(btn, new CreateInvoicePage(this)); break;
                case "Task": // "Task" là cái Tag bạn đặt cho nút Phân công
                    MainFrame.Navigate(new PhanCongPage()); // ĐÂY LÀ TRANG CỦA BẠN!
                    break;
                default:
                    Navigate(btn, new MemberListPage(this)); break;
            }
        }

        private void Navigate(Button activeBtn, System.Windows.Controls.Page page)
        {
            // Reset màu tất cả nút
            foreach (var child in ((Grid)((Border)((Grid)Content).Children[1]).Child).Children)
                if (child is Button b) b.Opacity = 0.6;

            // Highlight nút active
            activeBtn.Opacity = 1.0;
            _activeBtn = activeBtn;

            MainFrame.Navigate(page);
        }
    }
}