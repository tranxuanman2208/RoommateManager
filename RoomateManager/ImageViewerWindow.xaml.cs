using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RoommateManager.Views // Đảm bảo đúng namespace của bạn
{
    public partial class ImageViewerWindow : Window
    {
        // Constructor nhận ImageSource để hiển thị
        public ImageViewerWindow(ImageSource imageSource)
        {
            InitializeComponent();
            imgFull.Source = imageSource; // Gán ảnh
        }

        // Nhấn nút đóng (X)
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Nhấn phím bất kỳ (dùng ESC để đóng)
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        // Nhấn chuột trái ra vùng nền ngoài ảnh để đóng cửa sổ
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}