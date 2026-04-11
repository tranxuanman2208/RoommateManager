using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using RoomateManager.Models; // Đã sửa thành 1 chữ 'm'

namespace RoomateManager // Đảm bảo namespace này khớp với project của bạn
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
            try
            {
                using (var db = new RoommateManagerContext()) // Đổi tên Context theo project
                {
                    var members = db.Thanhviens
                                    .Where(tv => tv.Con == true) // bit trong SQL là bool
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
                                  .Where(pc => pc.Nguoithuchien == idThanhVien && (pc.Daxoa == false || pc.Daxoa == null))
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
                            Ngayth = DateOnly.FromDateTime(DateTime.Now), // SQL date -> DateOnly
                            Dalam = false,
                            Daxoa = false
                        };

                        db.Phancongs.Add(newPC);
                        await db.SaveChangesAsync();
                        LoadDataFromDB(idUser);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg" };
            if (op.ShowDialog() == true)
            {
                try
                {
                    string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MinhChung");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(op.FileName);
                    File.Copy(op.FileName, Path.Combine(folder, fileName), true);
                    temporaryFileName = fileName;
                    imgPreview.Source = new BitmapImage(new Uri(Path.Combine(folder, fileName)));
                }
                catch { }
            }
        }

        private async void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (lstNhiemVu.SelectedItem is not KitchenTask taskItem || string.IsNullOrEmpty(temporaryFileName))
            {
                MessageBox.Show("Chọn nhiệm vụ và tải ảnh minh chứng!"); return;
            }

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var taskToUpdate = await db.Phancongs.FindAsync(taskItem.ID);
                    if (taskToUpdate != null)
                    {
                        taskToUpdate.Dalam = true;
                        taskToUpdate.Minhchung = temporaryFileName;
                        await db.SaveChangesAsync();

                        taskItem.IsDone = true;
                        taskItem.MinhChungFileName = temporaryFileName;
                        temporaryFileName = null; imgPreview.Source = null;
                        MessageBox.Show("Thành công!");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnXemMinhChung_Click(object sender, RoutedEventArgs e)
        {
            if (lstNhiemVu.SelectedItem is KitchenTask task && !string.IsNullOrEmpty(task.MinhChungFileName))
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MinhChung", task.MinhChungFileName);
                if (File.Exists(path)) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            }
        }

        public class KitchenTask : INotifyPropertyChanged
        {
            public int ID { get; set; }
            public string? TaskName { get; set; }
            public string? AssignedMemberName { get; set; }
            private bool _isDone;
            public bool IsDone { get => _isDone; set { _isDone = value; OnPropertyChanged(nameof(IsDone)); OnPropertyChanged(nameof(Status)); OnPropertyChanged(nameof(StatusColor)); } }
            public string Status => IsDone ? "Đã xong" : "Đang trực";
            public string StatusColor => IsDone ? "#27AE60" : "#E67E22";
            public string? MinhChungFileName { get; set; }
            public string? NgayThucHienDisplay { get; set; }
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class ThanhVienVM { public string ID { get; set; } = ""; public string Ten { get; set; } = ""; }
    }
}