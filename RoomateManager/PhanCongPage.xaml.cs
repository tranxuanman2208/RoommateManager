using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient; 
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
        // Tài kiểm tra lại Server Name của ông chỗ Data Source nhé
        private string strCon = @"Data Source=.\SQLEXPRESS;Initial Catalog=RoommateManager;Integrated Security=True;TrustServerCertificate=True";

        ObservableCollection<KitchenTask> tasks = new ObservableCollection<KitchenTask>();
        Dictionary<KitchenTask, DateTime> nextReminders = new Dictionary<KitchenTask, DateTime>();

        public PhanCongPage()
        {
            InitializeComponent();
            LoadDanhSachXoayVong();
        }

        private void btnOpenAddTask_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddTaskWindow();
            dlg.Owner = Window.GetWindow(this);
            if (dlg.ShowDialog() == true)
            {
                var newTask = new KitchenTask
                {
                    TaskName = dlg.TaskName ?? "Nhiệm vụ mới",
                    Description = dlg.Description ?? "",
                    Frequency = dlg.Frequency ?? "Hàng ngày",
                    AssignedMember = lstThanhVien.SelectedItem?.ToString()?.Split('.')[1].Trim() ?? "",
                    IsDone = false,
                    Status = "Đang trực",
                    NextReminderDisplay = ""
                };

                tasks.Add(newTask);
                DateTime next = ComputeNextOccurrence(DateTime.Now, newTask.Frequency);
                nextReminders[newTask] = next;
                newTask.NextReminderDisplay = next.ToString("g");
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MinhChung");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(openFileDialog.FileName);
                    string destPath = Path.Combine(folderPath, fileName);

                    File.Copy(openFileDialog.FileName, destPath, true);

                    temporaryFileName = fileName;
                    imgPreview.Source = new BitmapImage(new Uri(destPath));

                    MessageBox.Show("Đã tải ảnh lên bộ nhớ tạm!", "Thông báo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải ảnh: " + ex.Message);
                }
            }
        }

        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (lstNhiemVu.SelectedItem is not KitchenTask selectedTask)
            {
                MessageBox.Show("Vui lòng chọn một nhiệm vụ!");
                return;
            }

            if (string.IsNullOrEmpty(temporaryFileName))
            {
                MessageBox.Show("Vui lòng tải ảnh minh chứng trước!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(strCon))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE PHANCONG SET DALAM = 1, MINHCHUNG = @minhchung WHERE ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@minhchung", temporaryFileName);
                    cmd.Parameters.AddWithValue("@id", selectedTask.ID);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        selectedTask.IsDone = true;
                        selectedTask.Status = "Đã xong";
                        selectedTask.MinhChungFileName = temporaryFileName;

                        DateTime next = ComputeNextOccurrence(DateTime.Now, selectedTask.Frequency ?? "Hàng ngày");
                        selectedTask.NextReminderDisplay = next.ToString("g");

                        if (pgbTienDo.Value < 100)
                        {
                            pgbTienDo.Value = Math.Min(100, pgbTienDo.Value + 14);
                            txtPhanTram.Text = (int)pgbTienDo.Value + "% Hoàn thành";
                        }

                        temporaryFileName = null;
                        imgPreview.Source = null;
                        MessageBox.Show("Xác nhận hoàn thành thành công!", "Thành công");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi Database: " + ex.Message);
                }
            }
        }

        private void btnXemMinhChung_Click(object sender, RoutedEventArgs e)
        {
            if (lstNhiemVu.SelectedItem is not KitchenTask selectedTask || string.IsNullOrEmpty(selectedTask.MinhChungFileName))
            {
                MessageBox.Show("Chưa có minh chứng!");
                return;
            }

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MinhChung", selectedTask.MinhChungFileName);
            if (File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullPath) { UseShellExecute = true });
            }
            else { MessageBox.Show("File không tồn tại!"); }
        }

        private DateTime ComputeNextOccurrence(DateTime from, string frequency)
        {
            var f = frequency.Trim();
            if (f.Contains("Hàng ngày")) return from.Date.AddDays(1).AddHours(9);
            if (f.Contains("Hàng tuần")) return from.Date.AddDays(7).AddHours(9);
            if (f.Contains("Hàng tháng")) return from.Date.AddMonths(1).AddHours(9);
            return from.Date.AddDays(1).AddHours(9);
        }

        private void LoadDanhSachXoayVong()
        {
            List<string> members = new List<string> { "Tấn Tài", "Xuân Mẫn", "Minh Nhật", "Tấn Thiện", "Hoàng Huy", "Gia Bảo", "Trần Minh Nhật" };
            int currentWeek = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, System.Globalization.DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, DayOfWeek.Monday);

            List<string> rotatedList = new List<string>();
            for (int i = 0; i < members.Count; i++)
            {
                int newIndex = (i + currentWeek + 3) % members.Count;
                rotatedList.Add($"{i + 1}. {members[newIndex]}");
            }

            lstThanhVien.ItemsSource = rotatedList;
            lstThanhVien.SelectedIndex = 0;
            string firstMember = rotatedList[0].Split('.')[1].Trim();
            LoadDataFromDB(firstMember);
        }

        private void LoadDataFromDB(string nguoiTruc)
        {
            tasks.Clear();
            using (SqlConnection conn = new SqlConnection(strCon))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ID, TENCV, NGUOITHUCHIEN, DALAM, MINHCHUNG FROM PHANCONG WHERE NGUOITHUCHIEN = @name AND (DAXOA = 0 OR DAXOA IS NULL)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", nguoiTruc);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            tasks.Add(new KitchenTask
                            {
                                ID = r["ID"] != DBNull.Value ? (int)r["ID"] : 0,
                                TaskName = r["TENCV"]?.ToString() ?? "",
                                AssignedMember = r["NGUOITHUCHIEN"]?.ToString() ?? "",
                                IsDone = r["DALAM"] != DBNull.Value && (bool)r["DALAM"],
                                MinhChungFileName = r["MINHCHUNG"]?.ToString() ?? "",
                                Status = (r["DALAM"] != DBNull.Value && (bool)r["DALAM"]) ? "Đã xong" : "Đang trực",
                                Frequency = "Hàng ngày",
                                Description = "",
                                NextReminderDisplay = ""
                            });
                        }
                    }
                }
                catch (Exception) { }
            }
            lstNhiemVu.ItemsSource = tasks;
        }

        public class KitchenTask : System.ComponentModel.INotifyPropertyChanged
        {
            public int ID { get; set; }
            public string? TaskName { get; set; }
            public string? Description { get; set; }
            public string? Frequency { get; set; }
            public string? AssignedMember { get; set; }
            public bool IsDone { get; set; }
            public string? NextReminderDisplay { get; set; }

            private string _status = "Đang trực";
            public string? Status { get => _status; set { _status = value ?? ""; OnPropertyChanged(nameof(Status)); } }

            private string _minhChung = "";
            public string? MinhChungFileName { get => _minhChung; set { _minhChung = value ?? ""; OnPropertyChanged(nameof(MinhChungFileName)); } }

            public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
            void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
}