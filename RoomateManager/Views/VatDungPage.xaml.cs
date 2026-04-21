using Microsoft.Win32;
using RoomateManager.Models;
using RoommateManager.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RoommateManager.Views
{
    public partial class VatDungPage : Page
    {
        RoommateManagerContext db = new RoommateManagerContext();

        private void ImagePreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Kiểm tra xem imgPreview hiện tại có ảnh hay không
            if (imgPreview.Source != null)
            {
                // Tạo một cửa sổ xem ảnh mới và truyền ảnh hiện tại sang
                ImageViewerWindow viewer = new ImageViewerWindow(imgPreview.Source);

                // Thiết lập cửa sổ xem ảnh là cửa sổ con của cửa sổ chính (để không bị che)
                viewer.Owner = Window.GetWindow(this);

                // Hiển thị cửa sổ xem ảnh (dùng ShowDialog để ngăn người dùng tương tác với app chính đến khi đóng ảnh)
                viewer.ShowDialog();
            }
        }
        // lưu đường dẫn ảnh tạm
        private string selectedImagePath = "";

        public VatDungPage()
        {
            InitializeComponent();
            LoadData();
        }

        // ================= LOAD DATA =================
        void LoadData()
        {
            gridVatDung.ItemsSource = db.Vatdungs
                .Where(x => x.Dabo != true)
                .ToList();
        }

        // ================= CHỌN ẢNH =================
        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg";

            if (open.ShowDialog() == true)
            {
                selectedImagePath = open.FileName;

                imgPreview.Source = new BitmapImage(
                    new Uri(selectedImagePath, UriKind.Absolute));
            }
        }

        // ================= THÊM =================
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTen.Text))
            {
                MessageBox.Show("Tên vật dụng không được để trống!");
                return;
            }

            Vatdung v = new Vatdung()
            {
                Tenvd = txtTen.Text,
                Ghichu = txtGhiChu.Text,
                Baoduong = chkBaoDuong.IsChecked,
                Ngaytao = DateTime.Now,
                Hinhanh = selectedImagePath // lưu đường dẫn ảnh
            };

            db.Vatdungs.Add(v);
            db.SaveChanges();

            LoadData();
            ResetForm(); // ⭐ reset sau khi thêm
        }

        // ================= SỬA =================
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            var selected = gridVatDung.SelectedItem as Vatdung;
            if (selected == null) return;

            var v = db.Vatdungs.Find(selected.Mavatdung);
            if (v != null)
            {
                v.Tenvd = txtTen.Text;
                v.Ghichu = txtGhiChu.Text;
                v.Baoduong = chkBaoDuong.IsChecked;

                if (!string.IsNullOrEmpty(selectedImagePath))
                    v.Hinhanh = selectedImagePath;

                db.SaveChanges();
                LoadData();
                ResetForm(); // ⭐ reset sau khi sửa
            }
        }

        // ================= XÓA =================
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = gridVatDung.SelectedItem as Vatdung;
            if (selected == null) return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa vật dụng này?",
                "Xác nhận",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No) return;

            var v = db.Vatdungs.Find(selected.Mavatdung);
            if (v != null)
            {
                v.Dabo = true;
                db.SaveChanges();
                LoadData();
                ResetForm();
            }
        }

        // ================= CLICK GRID =================
        private void gridVatDung_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = gridVatDung.SelectedItem as Vatdung;
            if (selected == null) return;

            txtTen.Text = selected.Tenvd;
            txtGhiChu.Text = selected.Ghichu;
            chkBaoDuong.IsChecked = selected.Baoduong;

            if (!string.IsNullOrEmpty(selected.Hinhanh) && File.Exists(selected.Hinhanh))
            {
                imgPreview.Source = new BitmapImage(
                    new Uri(selected.Hinhanh, UriKind.Absolute));

                selectedImagePath = selected.Hinhanh;
            }
        }

        // ================= RESET FORM =================
        void ResetForm()
        {
            txtTen.Text = "";
            txtGhiChu.Text = "";
            chkBaoDuong.IsChecked = false;

            selectedImagePath = "";
            imgPreview.Source = null;

            gridVatDung.SelectedItem = null;
        }
    }
}