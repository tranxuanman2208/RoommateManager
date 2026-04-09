using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RoomateManager
{
    public partial class BaoCaoPage : Page
    {
        private readonly string connStr = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=RoommateManager;Integrated Security=True;TrustServerCertificate=True;";

        public BaoCaoPage()
        {
            InitializeComponent();
            LoadDuLieuBanDau();
        }

        private void LoadDuLieuBanDau()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT ID, TEN FROM THANHVIEN", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cbNguoiBao.ItemsSource = dt.DefaultView;
                    cbChonNguoiVP.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load dữ liệu: " + ex.Message); }
        }

        private async void btnGui_Click(object sender, RoutedEventArgs e)
        {
            if (cbNguoiBao.SelectedValue == null || string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!"); return;
            }

            string noiDungLuu = $"[{txtTieuDe.Text.Trim()}] {txtNoiDung.Text.Trim()}" + (chkAnDanh.IsChecked == true ? " [AN_DANH]" : "");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    string sql = "INSERT INTO BAOCAO (NOIDUNG, NGUOIBC, NGAYBC, DAXULY, DAXOA) VALUES (@c, @u, GETDATE(), 0, 0)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@c", noiDungLuu);
                    cmd.Parameters.AddWithValue("@u", cbNguoiBao.SelectedValue.ToString());
                    await cmd.ExecuteNonQueryAsync();
                    MessageBox.Show("Đã gửi báo cáo vi phạm!");
                    txtTieuDe.Clear(); txtNoiDung.Clear();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private async Task LoadBangTin()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    string query = "SELECT b.*, t.TEN FROM BAOCAO b JOIN THANHVIEN t ON b.NGUOIBC = t.ID WHERE b.DAXOA = 0 ORDER BY b.NGAYBC DESC";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dt.Columns.Add("DisplayName", typeof(string));
                    dt.Columns.Add("StatusText", typeof(string));
                    dt.Columns.Add("StatusColor", typeof(string));

                    foreach (DataRow r in dt.Rows)
                    {
                        string nd = r["NOIDUNG"].ToString();
                        r["DisplayName"] = nd.Contains("[AN_DANH]") ? "👤 Ẩn danh" : r["TEN"].ToString();
                        r["NOIDUNG"] = nd.Replace("[AN_DANH]", "");

                        bool daXuLy = (bool)r["DAXULY"];
                        r["StatusText"] = daXuLy ? "Đã xong" : "Chờ duyệt";
                        r["StatusColor"] = daXuLy ? "#27AE60" : "#E67E22";
                    }
                    lstBangTin.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        // --- CẬP NHẬT MỚI TẠI ĐÂY ---
        private async void btnXacNhanPhat_Click(object sender, RoutedEventArgs e)
        {
            var row = lstBangTin.SelectedItem as DataRowView;
            if (row == null || cbChonNguoiVP.SelectedValue == null)
            {
                MessageBox.Show("Hãy chọn báo cáo và người vi phạm!"); return;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    SqlTransaction trans = conn.BeginTransaction();
                    try
                    {
                        int maBaoCaoGoc = Convert.ToInt32(row["MABC"]);

                        // 1. Update trạng thái báo cáo
                        new SqlCommand($"UPDATE BAOCAO SET DAXULY = 1 WHERE MABC = {maBaoCaoGoc}", conn, trans).ExecuteNonQuery();

                        // 2. Chèn vào XULYVIPHAM kèm mã báo cáo liên kết (MABC)
                        string sqlPhat = @"INSERT INTO XULYVIPHAM (NGUOIVIPHAM, NOIDUNG, MABC, NGAYXULY, DONE, DAXOA) 
                                           VALUES (@idVP, @nd, @maBC, GETDATE(), 0, 0)";

                        SqlCommand cmdPhat = new SqlCommand(sqlPhat, conn, trans);
                        cmdPhat.Parameters.AddWithValue("@idVP", cbChonNguoiVP.SelectedValue.ToString());
                        cmdPhat.Parameters.AddWithValue("@nd", row["NOIDUNG"].ToString());
                        cmdPhat.Parameters.AddWithValue("@maBC", maBaoCaoGoc);
                        cmdPhat.ExecuteNonQuery();

                        // 3. Cộng điểm vi phạm (Giữ nguyên logic cũ của Tài)
                        new SqlCommand($"UPDATE THANHVIEN SET DIEMVIPHAM = ISNULL(DIEMVIPHAM,0) + 1 WHERE ID = '{cbChonNguoiVP.SelectedValue}'", conn, trans).ExecuteNonQuery();

                        trans.Commit();
                        await LoadBangTin();
                        MessageBox.Show("Đã xác nhận phạt và lưu liên kết minh chứng!");
                    }
                    catch { trans.Rollback(); throw; }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private async void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var row = lstBangTin.SelectedItem as DataRowView;
            if (row == null) return;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                new SqlCommand($"UPDATE BAOCAO SET DAXOA = 1 WHERE MABC = {row["MABC"]}", conn).ExecuteNonQuery();
            }
            await LoadBangTin();
        }

        private void TabItem_Selected(object sender, RoutedEventArgs e)
        {
            var tab = sender as TabItem;
            if (tab != null && tab.Header.ToString().Contains("Bảng tin"))
            {
                _ = LoadBangTin();
            }
        }
    }
}