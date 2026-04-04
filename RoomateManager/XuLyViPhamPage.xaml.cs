using System;
using System.Data;
using Microsoft.Data.SqlClient; // Đảm bảo đã cài NuGet Microsoft.Data.SqlClient
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RoomateManager
{
    public partial class XuLyViPhamPage : Page
    {
        // Thêm TrustServerCertificate=True để tránh lỗi treo kết nối ở bản mới
        private readonly string connStr = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=RoommateManager;Integrated Security=True;TrustServerCertificate=True;";

        public XuLyViPhamPage()
        {
            InitializeComponent();
            _ = LoadData(); // Gọi hàm async
        }

        private async Task LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    // 1. Load ComboBox thành viên (ID là varchar(10))
                    SqlCommand cmdTV = new SqlCommand("SELECT ID, TEN FROM THANHVIEN WHERE CON = 1", conn);
                    SqlDataAdapter daTV = new SqlDataAdapter(cmdTV);
                    DataTable dtTV = new DataTable();
                    daTV.Fill(dtTV);
                    cbThanhVien.ItemsSource = dtTV.DefaultView;

                    // 2. Load danh sách vi phạm
                    string query = @"SELECT vp.*, tv.TEN as TenNguoiVP 
                                     FROM XULYVIPHAM vp 
                                     LEFT JOIN THANHVIEN tv ON vp.NGUOIVIPHAM = tv.ID 
                                     WHERE vp.DAXOA = 0 ORDER BY vp.NGAYXULY DESC";

                    SqlDataAdapter daVP = new SqlDataAdapter(query, conn);
                    DataTable dtVP = new DataTable();
                    await Task.Run(() => daVP.Fill(dtVP)); // Chạy ngầm để không đơ UI

                    // Xử lý cột hiển thị
                    dtVP.Columns.Add("StatusText", typeof(string));
                    dtVP.Columns.Add("StatusColor", typeof(string));
                    foreach (DataRow r in dtVP.Rows)
                    {
                        bool isDone = r["DONE"] != DBNull.Value && (bool)r["DONE"];
                        r["StatusText"] = isDone ? "Đã xong" : "Đang chờ";
                        r["StatusColor"] = isDone ? "#27AE60" : "#E67E22";
                    }
                    lstViPham.ItemsSource = dtVP.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Thông báo");
            }
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cbThanhVien.SelectedValue == null || string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    string sql = "INSERT INTO XULYVIPHAM (NGUOIVIPHAM, NOIDUNG, NGAYXULY, DONE, DAXOA) VALUES (@id, @nd, GETDATE(), 0, 0)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 10).Value = cbThanhVien.SelectedValue.ToString();
                    cmd.Parameters.Add("@nd", SqlDbType.NVarChar, 200).Value = txtNoiDung.Text.Trim();

                    await cmd.ExecuteNonQueryAsync();
                }
                txtNoiDung.Clear();
                await LoadData();
                MessageBox.Show("Ghi nhận vi phạm thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
            }
        }

        private async void btnDone_Click(object sender, RoutedEventArgs e)
        {
            var row = lstViPham.SelectedItem as DataRowView;
            if (row == null) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE XULYVIPHAM SET DONE = 1 WHERE MAVIPHAM = @ma";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ma", row["MAVIPHAM"]);
                    await cmd.ExecuteNonQueryAsync();
                }
                await LoadData();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var row = lstViPham.SelectedItem as DataRowView;
            if (row == null) return;

            if (MessageBox.Show("Xóa bản ghi này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        await conn.OpenAsync();
                        string sql = "UPDATE XULYVIPHAM SET DAXOA = 1 WHERE MAVIPHAM = @ma";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@ma", row["MAVIPHAM"]);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    await LoadData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void btnXemBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var row = lstViPham.SelectedItem as DataRowView;
            if (row == null || row["MABC"] == DBNull.Value)
            {
                MessageBox.Show("Không có báo cáo liên kết.");
                return;
            }
            MessageBox.Show($"Bằng chứng từ báo cáo mã: {row["MABC"]}");
        }
    }
}