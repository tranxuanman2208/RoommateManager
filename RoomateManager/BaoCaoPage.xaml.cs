using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using RoomateManager.Models;

namespace RoomateManager
{
    public partial class BaoCaoPage : Page
    {
        public BaoCaoPage()
        {
            InitializeComponent();
            LoadDuLieuBanDau();
        }

        private void LoadDuLieuBanDau()
        {
            try
            {
                using (var db = new RoommateManagerContext())
                {
                    // Lấy danh sách thành viên còn ở (Con == true)
                    var listTV = db.Thanhviens.Where(tv => tv.Con == true).ToList();
                    cbNguoiBao.ItemsSource = listTV;
                    cbChonNguoiVP.ItemsSource = listTV;

                    // Thiết lập hiển thị cho ComboBox
                    cbNguoiBao.DisplayMemberPath = "Ten";
                    cbNguoiBao.SelectedValuePath = "Id";
                    cbChonNguoiVP.DisplayMemberPath = "Ten";
                    cbChonNguoiVP.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu ban đầu: " + ex.Message);
            }
        }

        private async void btnGui_Click(object sender, RoutedEventArgs e)
        {
            if (cbNguoiBao.SelectedValue == null || string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // Xử lý nội dung ẩn danh
            string noiDungLuu = $"[{txtTieuDe.Text.Trim()}] {txtNoiDung.Text.Trim()}" + (chkAnDanh.IsChecked == true ? " [AN_DANH]" : "");

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var bc = new Baocao
                    {
                        Noidung = noiDungLuu,
                        Nguoibc = cbNguoiBao.SelectedValue.ToString(),
                        Ngaybc = DateOnly.FromDateTime(DateTime.Now), // SQL date tương ứng DateOnly
                        Daxuly = false,
                        Daxoa = false,
                        Tieude = txtTieuDe.Text.Trim()
                    };

                    db.Baocaos.Add(bc);
                    await db.SaveChangesAsync();

                    MessageBox.Show("Đã gửi báo cáo vi phạm!");
                    txtTieuDe.Clear();
                    txtNoiDung.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi báo cáo: " + ex.Message);
            }
        }

        private async Task LoadBangTin()
        {
            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var listBC = await db.Baocaos
                        .Where(b => b.Daxoa == false || b.Daxoa == null)
                        .OrderByDescending(b => b.Ngaybc)
                        .Select(b => new {
                            b.Mabc,
                            b.Noidung,
                            b.Ngaybc,
                            b.Daxuly,
                            // Xử lý hiển thị tên người báo cáo hoặc ẩn danh bằng LINQ
                            DisplayName = b.Noidung.Contains("[AN_DANH]")
                                          ? "👤 Ẩn danh"
                                          : db.Thanhviens.Where(t => t.Id == b.Nguoibc).Select(t => t.Ten).FirstOrDefault(),
                            CleanContent = b.Noidung.Replace("[AN_DANH]", ""),
                            StatusText = b.Daxuly == true ? "Đã xong" : "Chờ duyệt",
                            StatusColor = b.Daxuly == true ? "#27AE60" : "#E67E22"
                        }).ToListAsync();

                    lstBangTin.ItemsSource = listBC;
                }
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây nếu cần
            }
        }

        private async void btnXacNhanPhat_Click(object sender, RoutedEventArgs e)
        {
            var selectedBC = lstBangTin.SelectedItem as dynamic;
            if (selectedBC == null || cbChonNguoiVP.SelectedValue == null)
            {
                MessageBox.Show("Hãy chọn báo cáo và người vi phạm!");
                return;
            }

            try
            {
                int maBC = selectedBC.Mabc;

                using (var db = new RoommateManagerContext())
                {
                    // 1. Cập nhật trạng thái báo cáo đã được xử lý
                    var bc = await db.Baocaos.FindAsync(maBC);
                    if (bc != null) bc.Daxuly = true;

                    // 2. Thêm bản ghi mới vào bảng xử lý vi phạm
                    var xl = new Xulyvipham
                    {
                        Nguoivipham = cbChonNguoiVP.SelectedValue.ToString(),
                        Noidung = selectedBC.CleanContent,
                        Mabc = maBC,
                        Ngayxuly = DateOnly.FromDateTime(DateTime.Now),
                        Done = false,
                        Daxoa = false
                    };
                    db.Xulyviphams.Add(xl);

                    // 3. Thực hiện lưu thay đổi
                    await db.SaveChangesAsync();

                    await LoadBangTin();
                    MessageBox.Show("Đã xác nhận phạt thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xử lý phạt: " + ex.Message);
            }
        }

        private async void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var selectedBC = lstBangTin.SelectedItem as dynamic;
            if (selectedBC == null) return;

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    int maBC = (int)selectedBC.Mabc;
                    var bc = await db.Baocaos.FindAsync(maBC);
                    if (bc != null)
                    {
                        bc.Daxoa = true; // Soft delete
                        await db.SaveChangesAsync();
                        await LoadBangTin();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa báo cáo: " + ex.Message);
            }
        }

        private void TabItem_Selected(object sender, RoutedEventArgs e)
        {
            _ = LoadBangTin();
        }
    }
}