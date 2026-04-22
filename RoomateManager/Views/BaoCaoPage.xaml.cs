using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using RoomateManager.Models;
using RoomateManager.Helpers;

namespace RoomateManager
{
    public partial class BaoCaoPage : Page
    {
        public BaoCaoPage()
        {
            InitializeComponent();
        }


        private async void btnGui_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
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
                        Nguoibc = User.CurrentUserId,
                        Ngaybc = DateOnly.FromDateTime(DateTime.Now), // SQL date tương ứng DateOnly
                        Daxuly = false,
                        Daxoa = false,
                        Tieude = txtTieuDe.Text.Trim()
                    };
                    string? IDAD = db.Thanhviens.Where(tv => tv.Ad == true && tv.Manha == User.CurrentHome).Select(tv =>tv.Id).FirstOrDefault();
                    Thongbao newtb = new Thongbao
                    {
                        Noidung = bc.Noidung,
                        Nguoitb = User.CurrentUserId,
                        Ngaytb = DateOnly.FromDateTime(DateTime.Now),
                        Nguoinhan = IDAD,
                        Dadoc = false,
                        Daxoa = false
                    };

                    db.Baocaos.Add(bc);
                    db.Thongbaos.Add(newtb);
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
            if(!User.IsAdmin)
            {
                btnXoa.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnXoa.Visibility= Visibility.Visible;
            }
            try
            {
                using (var db = new RoommateManagerContext())
                {
                    // Dùng Include để lấy sẵn thông tin người báo cáo, tránh truy vấn nhiều lần
                    var listBC = await db.Xulyviphams
                        .Where(b => b.Daxoa == false || b.Daxoa == null)
                        .Include(b => b.NguoiviphamNavigation)
                        .OrderByDescending(b => b.Ngayxuly)
                        .Select(b => new {
                            b.Mavipham,
                            Noidung = b.Noidung,
                            Ngayxuly = b.Ngayxuly.HasValue ? b.Ngayxuly.Value.ToString("dd/MM/yyyy") : "",
                            Nguoibiphat = b.NguoiviphamNavigation != null ? b.NguoiviphamNavigation.Ten : "Không xác định",
                            StatusText = b.Done == true ? "Đã xử lý" : "Chờ xử lý",
                            StatusColor = b.Done == true ? "#27AE60" : "#E67E22"
                        }).ToListAsync();

                    lstBangTin.ItemsSource = listBC;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load bảng tin: " + ex.Message); }
        }

        private async void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstBangTin.SelectedItem as dynamic;
            if (selected == null) return;

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    int mavp = (int)selected.Mavipham;
                    var vp = await db.Xulyviphams.FindAsync(mavp);
                    if (vp != null)
                    {
                        vp.Daxoa = true; // Soft delete
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