using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RoomateManager
{
    public partial class XuLyViPhamPage : Page
    {
        private readonly string connStr = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=RoommateManager;Integrated Security=True;TrustServerCertificate=True;";

        public XuLyViPhamPage()
        {
            InitializeComponent();
            _ = LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    SqlCommand cmdTV = new SqlCommand("SELECT ID, TEN FROM THANHVIEN WHERE CON = 1", conn);
                    SqlDataAdapter daTV = new SqlDataAdapter(cmdTV);
                    DataTable dtTV = new DataTable();
                    daTV.Fill(dtTV);
                    cbThanhVien.ItemsSource = dtTV.DefaultView;

                    // Đảm bảo query lấy vp.* để có cột MABC
                    string query = @"SELECT vp.*, tv.TEN as TenNguoiVP 
                                     FROM XULYVIPHAM vp 
                                     LEFT JOIN THANHVIEN tv ON vp.NGUOIVIPHAM = tv.ID 
                                     WHERE vp.DAXOA = 0 ORDER BY vp.NGAYXULY DESC";

                    SqlDataAdapter daVP = new SqlDataAdapter(query, conn);
                    DataTable dtVP = new DataTable();
                    await Task.Run(() => daVP.Fill(dtVP));

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
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cbThanhVien.SelectedValue == null || string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin!"); return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    // Lưu trực tiếp không qua báo cáo nên MABC sẽ để NULL (mặc định)
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
            catch (Exception ex) { MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message); }
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

        // --- CẬP NHẬT MỚI TẠI ĐÂY ---
        private async void btnXemBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var row = lstViPham.SelectedItem as DataRowView;

            if (row == null || row["MABC"] == DBNull.Value || string.IsNullOrEmpty(row["MABC"].ToString()))
            {
                MessageBox.Show("Vi phạm này được ghi nhận trực tiếp, không có báo cáo gốc liên kết!", "Thông báo");
                return;
            }

            string maBC = row["MABC"].ToString();
            string noiDungGoc = "Không tìm thấy nội dung...";

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    string sql = "SELECT NOIDUNG FROM BAOCAO WHERE MABC = @ma";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ma", maBC);

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null) noiDungGoc = result.ToString();
                }

                MinhChungWindow popup = new MinhChungWindow(maBC, noiDungGoc);
                popup.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi truy xuất: " + ex.Message); }
        }
    }
}