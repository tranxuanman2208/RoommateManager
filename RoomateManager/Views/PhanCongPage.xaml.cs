using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using RoomateManager.Helpers;
using RoomateManager.Models;
using RoommateManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RoomateManager
{
    public partial class PhanCongPage : Page
    {
        private string? temporaryFileName = null;
        ObservableCollection<KitchenTask> tasks = new ObservableCollection<KitchenTask>();

        public PhanCongPage()
        {
            InitializeComponent();
            LoadDanhSachXoayVong();
        }

        private void LoadDanhSachXoayVong()
        {
            if (!User.IsAdmin) { btnOpenAddTask.Visibility = Visibility.Collapsed; }
            else { btnOpenAddTask.Visibility = Visibility.Visible; }

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var members = db.Thanhviens
                                    .Where(tv => tv.Con == true)
                                    .OrderBy(tv => tv.Id)
                                    .Select(tv => new ThanhVienVM
                                    {
                                        ID = tv.Id,
                                        Ten = tv.Ten ?? ""
                                    })
                                    .ToList();

                    if (members.Count == 0) return;

                    int currentWeek = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        DateTime.Now, System.Globalization.DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, DayOfWeek.Monday);

                    List<ThanhVienVM> rotatedList = new List<ThanhVienVM>();
                    for (int i = 0; i < members.Count; i++)
                    {
                        int newIndex = (i + currentWeek) % members.Count;
                        rotatedList.Add(members[newIndex]);
                    }

                    lstThanhVien.ItemsSource = rotatedList;
                    lstThanhVien.DisplayMemberPath = "Ten";
                    lstThanhVien.SelectedValuePath = "ID";
                    lstThanhVien.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load thành viên: " + ex.Message); }
        }

        private void lstThanhVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstThanhVien.SelectedValue is string id) LoadDataFromDB(id);
        }

        private void LoadDataFromDB(string idThanhVien)
        {
            tasks.Clear();
            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var query = db.Phancongs
                                  .Include(pc => pc.NguoithuchienNavigation)
                                  .Where(pc => pc.Nguoithuchien == idThanhVien && (pc.Daxoa == false || pc.Daxoa == false))
                                  .OrderBy(pc => pc.Nguoithuchien)
                                  .ToList();

                    foreach (var pc in query)
                    {
                        tasks.Add(new KitchenTask
                        {
                            ID = pc.Id,
                            TaskName = pc.Tencv,
                            AssignedMemberName = pc.NguoithuchienNavigation?.Ten,
                            IsDone = pc.Dalam == true,
                            MinhChungFileName = pc.Minhchung,
                            NgayThucHienDisplay = pc.Ngayth.HasValue ? pc.Ngayth.Value.ToString("dd/MM/yyyy") : ""
                        });
                    }
                    lstNhiemVu.ItemsSource = tasks;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load nhiệm vụ: " + ex.Message); }
        }

        private async void btnOpenAddTask_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddTaskWindow { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true && lstThanhVien.SelectedValue is string idUser)
            {
                try
                {
                    using (var db = new RoommateManagerContext())
                    {
                        var newPC = new Phancong
                        {
                            Tencv = dlg.TaskName ?? "Nhiệm vụ mới",
                            Nguoithuchien = idUser,
                            Nguoiphancong = User.CurrentUserId,
                            Ngayth = DateOnly.FromDateTime(DateTime.Now), // SQL date -> DateOnly
                            Dalam = false,
                            Daxoa = false
                        };

                        db.Phancongs.Add(newPC);
                        Thongbao newtb = new Thongbao
                        {
                            Noidung = dlg.TaskName,
                            Nguoitb = User.CurrentUserId,
                            Ngaytb = DateOnly.FromDateTime(DateTime.Now),
                            Nguoinhan = idUser,
                            Dadoc = false,
                            Daxoa = false
                        };
                        db.Thongbaos.Add(newtb);
                        await db.SaveChangesAsync();
                        LoadDataFromDB(idUser);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            // 1. Phải chọn nhiệm vụ trước mới cho tải ảnh
            if (lstNhiemVu.SelectedItem is not KitchenTask selectedTask)
            {
                MessageBox.Show("Vui lòng chọn nhiệm vụ cần tải minh chứng!");
                return;
            }

            // 2. Nếu đã hoàn thành rồi thì không cho tải đè (tùy bạn quyết định)
            if (selectedTask.IsDone)
            {
                MessageBox.Show("Nhiệm vụ này đã hoàn thành!");
                return;
            }

            OpenFileDialog op = new OpenFileDialog { Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg" };
            if (op.ShowDialog() == true)
            {
                try
                {
                    // Tạo thư mục lưu trữ nếu chưa có
                    string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MinhChung");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    // Tạo tên file duy nhất để tránh trùng lặp
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(op.FileName);
                    string fullPath = Path.Combine(folder, fileName);

                    File.Copy(op.FileName, fullPath, true);

                    // Lưu Tên file vào biến tạm (đây chính là đường dẫn tương đối)
                    temporaryFileName = fileName;

                    // Hiển thị xem trước
                    imgPreview.Source = new BitmapImage(new Uri(fullPath));
                    MessageBox.Show("Đã tải ảnh lên bộ nhớ tạm. Nhấn 'Xác nhận' để lưu.");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi tải ảnh: " + ex.Message); }
            }
        }

        private async void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (lstNhiemVu.SelectedItem is not KitchenTask taskItem)
            {
                MessageBox.Show("Vui lòng chọn nhiệm vụ!"); return;
            }

            if (string.IsNullOrEmpty(temporaryFileName))
            {
                MessageBox.Show("Vui lòng tải ảnh minh chứng trước khi xác nhận!"); return;
            }

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var taskToUpdate = await db.Phancongs.FindAsync(taskItem.ID);
                    if (taskToUpdate != null)
                    {
                        // Cập nhật Database
                        taskToUpdate.Dalam = true;
                        taskToUpdate.Minhchung = temporaryFileName; // Lưu tên file (đường dẫn tương đối)
                        await db.SaveChangesAsync();

                        // Cập nhật giao diện (ObservableCollection tự báo cho ListView)
                        taskItem.IsDone = true;
                        taskItem.MinhChungFileName = temporaryFileName;

                        // Reset biến tạm
                        temporaryFileName = null;
                        imgPreview.Source = null;

                        MessageBox.Show("Xác nhận hoàn thành nhiệm vụ thành công!");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message); }
        }
    private void btnXemMinhChung_Click(object sender, RoutedEventArgs e)
    {
        // 1. Lấy nhiệm vụ đang được chọn từ ListView
        if (lstNhiemVu.SelectedItem is not KitchenTask task) return;

        // 2. Kiểm tra điều kiện: Phải hoàn thành và có tên file minh chứng mới cho xem
        if (!task.IsDone || string.IsNullOrEmpty(task.MinhChungFileName))
        {
            MessageBox.Show("Nhiệm vụ này chưa hoàn thành hoặc không có hình ảnh minh chứng!", "Thông báo");
            return;
        }

        try
        {
            // 3. Xác định đường dẫn đầy đủ tới file ảnh trong thư mục MinhChung
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MinhChung");
            string fullPath = Path.Combine(folderPath, task.MinhChungFileName);

            if (File.Exists(fullPath))
            {
                // 4. Tạo đối tượng BitmapImage từ đường dẫn file
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath);
                // Sử dụng OnLoad để tránh việc chiếm dụng file, giúp app linh hoạt hơn
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                // 5. Khởi tạo ImageViewerWindow và truyền ảnh vào constructor 
                ImageViewerWindow viewer = new ImageViewerWindow(bitmap);

                // Hiển thị cửa sổ xem ảnh dưới dạng hội thoại (Modal)
                viewer.Owner = Window.GetWindow(this); // Để cửa sổ hiện chính giữa trang hiện tại
                viewer.ShowDialog();
            }
            else
            {
                MessageBox.Show("Không tìm thấy tệp tin hình ảnh trên hệ thống!", "Lỗi");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể hiển thị hình ảnh: " + ex.Message, "Lỗi");
        }
    }

    public class KitchenTask : INotifyPropertyChanged
        {
            public int ID { get; set; }
            public string? TaskName { get; set; }
            public string? AssignedMemberName { get; set; }
            private bool _isDone;
            public bool IsDone { get => _isDone; set { _isDone = value; OnPropertyChanged(nameof(IsDone)); OnPropertyChanged(nameof(Status)); OnPropertyChanged(nameof(StatusColor)); } }
            public string Status => IsDone ? "Đã xong" : "Đang làm";
            public string StatusColor => IsDone ? "#27AE60" : "#E67E22";
            public string? MinhChungFileName { get; set; }
            public string? NgayThucHienDisplay { get; set; }
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class ThanhVienVM { public string ID { get; set; } = ""; public string Ten { get; set; } = ""; }
    }
}