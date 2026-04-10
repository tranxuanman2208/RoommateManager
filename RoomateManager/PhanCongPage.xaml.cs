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
using System.ComponentModel;

namespace RoomateManager
{
    public partial class PhanCongPage : Page
    {
        private string? temporaryFileName = null;
        private string strCon = @"Data Source=.\SQLEXPRESS;Initial Catalog=RoommateManager;Integrated Security=True;TrustServerCertificate=True";
        ObservableCollection<KitchenTask> tasks = new ObservableCollection<KitchenTask>();

        public PhanCongPage()
        {
            InitializeComponent();
            LoadDanhSachXoayVong();
        }

        private void LoadDanhSachXoayVong()
        {
            List<ThanhVienVM> members = new List<ThanhVienVM>();
            try
            {
                using (SqlConnection conn = new SqlConnection(strCon))
                {
                    conn.Open();
                    string sql = "SELECT ID, TEN FROM THANHVIEN WHERE CON = 1";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            members.Add(new ThanhVienVM { ID = r["ID"].ToString()!, Ten = r["TEN"].ToString()! });
                    }
                }

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
            catch (Exception ex) { MessageBox.Show("Lỗi load thành viên: " + ex.Message); }
        }

        private void lstThanhVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstThanhVien.SelectedValue is string id) LoadDataFromDB(id);
        }

        private void LoadDataFromDB(string idThanhVien)
        {
            tasks.Clear();
            using (SqlConnection conn = new SqlConnection(strCon))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT pc.*, tv.TEN FROM PHANCONG pc 
                                   JOIN THANHVIEN tv ON pc.NGUOITHUCHIEN = tv.ID 
                                   WHERE pc.NGUOITHUCHIEN = @id AND (pc.DAXOA = 0 OR pc.DAXOA IS NULL)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idThanhVien);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            tasks.Add(new KitchenTask
                            {
                                ID = (int)r["ID"],
                                TaskName = r["TENCV"]?.ToString(),
                                AssignedMemberName = r["TEN"]?.ToString(),
                                IsDone = r["DALAM"] != DBNull.Value && (bool)r["DALAM"],
                                MinhChungFileName = r["MINHCHUNG"]?.ToString(),
                                NgayThucHienDisplay = r["NGAYTH"] != DBNull.Value ? Convert.ToDateTime(r["NGAYTH"]).ToString("dd/MM/yyyy") : ""
                            });
                        }
                    }
                    lstNhiemVu.ItemsSource = tasks;
                }
                catch { }
            }
        }

        private async void btnOpenAddTask_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddTaskWindow { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true && lstThanhVien.SelectedValue is string idUser)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(strCon))
                    {
                        await conn.OpenAsync();
                        string sql = "INSERT INTO PHANCONG (TENCV, NGUOITHUCHIEN, NGAYTH, DALAM, DAXOA) VALUES (@ten, @id, GETDATE(), 0, 0)";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@ten", dlg.TaskName ?? "Nhiệm vụ mới");
                        cmd.Parameters.AddWithValue("@id", idUser);
                        await cmd.ExecuteNonQueryAsync();
                        LoadDataFromDB(idUser);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
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
            if (lstNhiemVu.SelectedItem is not KitchenTask task || string.IsNullOrEmpty(temporaryFileName))
            {
                MessageBox.Show("Chọn nhiệm vụ và tải ảnh minh chứng!"); return;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(strCon))
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE PHANCONG SET DALAM = 1, MINHCHUNG = @mc WHERE ID = @id";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@mc", temporaryFileName);
                    cmd.Parameters.AddWithValue("@id", task.ID);
                    if (await cmd.ExecuteNonQueryAsync() > 0)
                    {
                        task.IsDone = true;
                        task.MinhChungFileName = temporaryFileName;
                        temporaryFileName = null; imgPreview.Source = null;
                        MessageBox.Show("Thành công!");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
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
            public string? Description { get; set; }
            public string? AssignedMemberName { get; set; }
            private bool _isDone;
            public bool IsDone { get { return _isDone; } set { _isDone = value; OnPropertyChanged("IsDone"); OnPropertyChanged("Status"); OnPropertyChanged("StatusColor"); } }
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