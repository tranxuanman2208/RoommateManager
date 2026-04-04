using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoomateManager
{
    /// <summary>
    /// Interaction logic for PhanCongPage.xaml
    /// </summary>
    public partial class PhanCongPage : Page
    {
        // Lưu các ảnh đã chọn chung (khi chưa chọn task cụ thể)
        List<string> currentSelectedImagePaths = new List<string>();

        System.Collections.ObjectModel.ObservableCollection<KitchenTask> tasks = new System.Collections.ObjectModel.ObservableCollection<KitchenTask>();
        // Lưu reminder (lần nhắc tiếp theo) cho mỗi task
        Dictionary<KitchenTask, DateTime> nextReminders = new Dictionary<KitchenTask, DateTime>();
        
        public PhanCongPage()
        {
            InitializeComponent();
            LoadDanhSachXoayVong();
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            // Khởi tạo hộp thoại chọn file
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Chỉ cho phép chọn các định dạng ảnh
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MinhChung");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var selectedTask = lstNhiemVu.SelectedItem as KitchenTask;
                // Clear current list when user selects new files
                currentSelectedImagePaths.Clear();

                foreach (var src in openFileDialog.FileNames)
                {
                    string ext = System.IO.Path.GetExtension(src);
                    string destFile = System.Guid.NewGuid().ToString() + ext;
                    string destPath = System.IO.Path.Combine(folderPath, destFile);
                    System.IO.File.Copy(src, destPath, true);

                    // thêm vào danh sách ảnh tạm chung
                    currentSelectedImagePaths.Add(destPath);

                    // nếu đã chọn task thì thêm vào Pending list của task
                    if (selectedTask != null)
                    {
                        if (selectedTask.PendingImagePaths == null) selectedTask.PendingImagePaths = new List<string>();
                        selectedTask.PendingImagePaths.Add(destPath);
                    }
                }

                // Hiển thị preview file đầu tiên nếu có
                if (currentSelectedImagePaths.Count > 0)
                {
                    imgPreview.Source = new BitmapImage(new Uri(currentSelectedImagePaths[0]));
                }

                MessageBox.Show($"Đã tải {openFileDialog.FileNames.Length} ảnh minh chứng và lưu bản sao trong app!", "Thông báo");
            }
        }

        private void btnOpenAddTask_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddTaskWindow();
            dlg.Owner = Window.GetWindow(this);
            if (dlg.ShowDialog() == true)
            {
                var newTask = new KitchenTask
                {
                    TaskName = dlg.TaskName,
                    Description = dlg.Description,
                    Frequency = dlg.Frequency,
                    AssignedMember = lstThanhVien.SelectedItem != null ? lstThanhVien.SelectedItem.ToString().Split('.')[1].Trim() : string.Empty,
                    IsDone = false
                };

                // If user selected images while no task selected, move those pending images to this new task
                if (currentSelectedImagePaths.Count > 0)
                {
                    newTask.PendingImagePaths = new List<string>(currentSelectedImagePaths);
                    currentSelectedImagePaths.Clear();
                }

                tasks.Add(newTask);
                // Tạo lịch nhắc tự động dựa trên tần suất
                DateTime next = ComputeNextOccurrence(DateTime.Now, newTask.Frequency);
                nextReminders[newTask] = next;
                // Cập nhật thuộc tính hiển thị nếu Binding cần
                newTask.NextReminderDisplay = next.ToString("g");
            }
        }
        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            // Xác nhận hoàn thành cho nhiệm vụ được chọn (phải có ảnh minh chứng)
            var selectedTask = lstNhiemVu.SelectedItem as KitchenTask;
            if (selectedTask == null)
            {
                MessageBox.Show("Vui lòng chọn một nhiệm vụ để xác nhận!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Lấy đường dẫn ảnh để lưu: ưu tiên ảnh gán cho task (PendingImagePaths), nếu không có dùng ảnh vừa tải chung
            string imageToSave = null;
            if (selectedTask.PendingImagePaths != null && selectedTask.PendingImagePaths.Count > 0)
                imageToSave = selectedTask.PendingImagePaths[0];
            else if (currentSelectedImagePaths.Count > 0)
                imageToSave = currentSelectedImagePaths[0];

            if (string.IsNullOrEmpty(imageToSave))
            {
                MessageBox.Show("Bạn chưa tải ảnh minh chứng! Vui lòng tải ảnh trước khi xác nhận.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Vì khi tải ảnh app đã tạo 1 bản sao vĩnh viễn (currentSelectedImagePath or PendingImagePath)
                // Chỉ cần dùng bản sao đó làm minh chứng. Không copy lại nữa.
                string destPath = imageToSave;

                // Cập nhật trạng thái nhiệm vụ
                selectedTask.IsDone = true;
                selectedTask.Status = "Đã xong";
                // lưu đường dẫn ảnh minh chứng vào task (bản sao trong app)
                if (selectedTask.ImageProofPaths == null) selectedTask.ImageProofPaths = new List<string>();
                selectedTask.ImageProofPaths.Add(destPath);
                // xóa khỏi Pending list (nếu có)
                if (selectedTask.PendingImagePaths != null && selectedTask.PendingImagePaths.Count > 0)
                    selectedTask.PendingImagePaths.Remove(imageToSave);
                // Sau khi xác nhận, tính lần nhắc tiếp theo theo tần suất và hiển thị
                DateTime next = ComputeNextOccurrence(DateTime.Now, selectedTask.Frequency);
                nextReminders[selectedTask] = next;
                selectedTask.NextReminderDisplay = next.ToString("g");

                // Cập nhật progress bar
                if (pgbTienDo.Value < 100)
                {
                    pgbTienDo.Value += 14;
                    if (pgbTienDo.Value > 100) pgbTienDo.Value = 100;
                    txtPhanTram.Text = (int)pgbTienDo.Value + "% Hoàn thành";
                }

                // Reset ảnh preview (nếu ảnh đang dùng chung)
                if (currentSelectedImagePaths.Count > 0 && imageToSave == currentSelectedImagePaths[0])
                {
                    currentSelectedImagePaths.Clear();
                    imgPreview.Source = null;
                }

                MessageBox.Show($"Đã xác nhận hoàn thành: {selectedTask.TaskName}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu ảnh minh chứng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnXemMinhChung_Click(object sender, RoutedEventArgs e)
        {
            var selectedTask = lstNhiemVu.SelectedItem as KitchenTask;
            if (selectedTask == null)
            {
                MessageBox.Show("Vui lòng chọn một nhiệm vụ để xem minh chứng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Kiểm tra xem có minh chứng đã lưu hay tạm cho task
            string? path = null;
            if (selectedTask.ImageProofPaths != null && selectedTask.ImageProofPaths.Count > 0)
                path = selectedTask.ImageProofPaths[0];
            else if (selectedTask.PendingImagePaths != null && selectedTask.PendingImagePaths.Count > 0)
                path = selectedTask.PendingImagePaths[0];

            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                MessageBox.Show("Chưa có minh chứng cho nhiệm vụ này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Mở file ảnh bằng ứng dụng mặc định của hệ thống
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(path)
                {
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở file minh chứng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadDanhSachXoayVong()
        {
            // 1. Danh sách 7 người cố định
            List<string> members = new List<string> {
        "Tấn Tài", "Xuân Mẫn", "Minh Nhật", "Tấn Thiện", "Hoàng Huy", "Gia Bảo", "Trần Minh Nhật"
    };

            // 2. Lấy số tuần hiện tại trong năm (ví dụ: tuần 12)
            int currentWeek = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                DateTime.Now, System.Globalization.DateTimeFormatInfo.CurrentInfo.CalendarWeekRule,
                DayOfWeek.Monday);

            // 3. Thuật toán xoay vòng: dùng phép chia lấy dư (%)
            List<string> rotatedList = new List<string>();
            for (int i = 0; i < members.Count; i++)
            {
                // Vị trí mới = (Vị trí cũ + số tuần) chia dư cho 7
                int newIndex = (i + currentWeek + 3) % members.Count;
                rotatedList.Add($"{i + 1}. {members[newIndex]}");
            }

            // 4. Đổ danh sách đã xoay vào giao diện
            lstThanhVien.ItemsSource = rotatedList;

            // Đánh dấu người đứng đầu là người đang trực tuần này
            lstThanhVien.SelectedIndex = 0;
            string nguoiDauTien = rotatedList[0].Split('.')[1].Trim(); // Lấy tên bỏ số thứ tự
            LoadDanhSachNhiemVu(nguoiDauTien);
        }
        public class KitchenTask : System.ComponentModel.INotifyPropertyChanged
        {
            public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
            void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

            public string TaskName { get; set; }
            public string Description { get; set; }
            public string Frequency { get; set; }
            public string AssignedMember { get; set; } // Tên người sẽ được gán
            public bool IsDone { get; set; }
            // Trạng thái hiển thị: Đang trực / Đã xong
            string _status = "Đang trực";
            public string Status
            {
                get => _status;
                set { _status = value; OnPropertyChanged(nameof(Status)); }
            }

            // Danh sách đường dẫn ảnh minh chứng tạm cho task (bản sao trong app)
            List<string>? _pendingImagePaths;
            public List<string>? PendingImagePaths
            {
                get => _pendingImagePaths;
                set { _pendingImagePaths = value; OnPropertyChanged(nameof(PendingImagePaths)); }
            }

            // Danh sách đường dẫn ảnh minh chứng đã lưu (sau khi xác nhận)
            List<string>? _imageProofPaths;
            public List<string>? ImageProofPaths
            {
                get => _imageProofPaths;
                set { _imageProofPaths = value; OnPropertyChanged(nameof(ImageProofPaths)); }
            }

            string? _nextReminderDisplay;
            public string? NextReminderDisplay
            {
                get => _nextReminderDisplay;
                set { _nextReminderDisplay = value; OnPropertyChanged(nameof(NextReminderDisplay)); }
            }
        }

        // Tính thời điểm nhắc tiếp theo theo tần suất
        private DateTime ComputeNextOccurrence(DateTime from, string frequency)
        {
            // Chuẩn hoá input
            var f = (frequency ?? string.Empty).Trim();
            if (string.Equals(f, "Hàng ngày", StringComparison.OrdinalIgnoreCase))
            {
                return from.Date.AddDays(1).AddHours(9);
            }
            if (string.Equals(f, "Hàng tuần", StringComparison.OrdinalIgnoreCase))
            {
                return from.Date.AddDays(7).AddHours(9);
            }
            if (string.Equals(f, "Hàng tháng", StringComparison.OrdinalIgnoreCase))
            {
                return new DateTime(from.Year, from.Month, 1).AddMonths(1).AddHours(9);
            }
            // mặc định: ngày mai 9:00
            return from.Date.AddDays(1).AddHours(9);
        }
        private void LoadDanhSachNhiemVu(string nguoiTrucTuanNay)
        {
            // Kiểm tra xem ListView đã tồn tại chưa
            if (lstNhiemVu == null) return;

            // Sử dụng field 'tasks' để giữ trạng thái chung của danh sách nhiệm vụ
            tasks.Clear();

            // Gán tên người đang trực tuần này vào mọi nhiệm vụ bếp (mặc định)
            tasks.Add(new KitchenTask
            {
                TaskName = "Lau bếp từ và khu vực nấu",
                Description = "Dùng dung dịch chuyên dụng, lau khô mặt kính.",
                Frequency = "Hàng ngày",
                AssignedMember = nguoiTrucTuanNay,
                IsDone = false,
                Status = "Đang trực"
            });

            tasks.Add(new KitchenTask
            {
                TaskName = "Cọ bồn rửa và vòi nước",
                Description = "Đảm bảo không còn cặn thức ăn ở lưới lọc.",
                Frequency = "Hàng ngày",
                AssignedMember = nguoiTrucTuanNay,
                IsDone = false,
                Status = "Đang trực"
            });

            // Gán ItemsSource một lần; tasks là ObservableCollection nên sẽ tự cập nhật khi thêm/xóa
            lstNhiemVu.ItemsSource = tasks;
        }
    }
}

    

