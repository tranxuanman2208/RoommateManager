using Microsoft.EntityFrameworkCore;
using RoomateManager.Models;
using RoommateManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RoommateManager.Views
{
    public partial class MemberListPage : Page
    {
        private bool _isManager = true;
        private string _currentUserId = "1";

        public enum MemberRole { Manager, Member }
        public enum PaymentStatus { Paid, Unpaid, NotApplicable }

        public class MemberVM
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string AvatarInitial => Name?.Length > 0 ? Name[0].ToString().ToUpper() : "?";
            public MemberRole Role { get; set; }
            public PaymentStatus PaymentStatus { get; set; }
            public bool IsCurrentUser { get; set; }
            public string RoleDisplay => Role == MemberRole.Manager ? "👑 Quản lý" : "Thành viên";
            public string StatusDisplay => PaymentStatus switch
            {
                PaymentStatus.Paid => "Đã đóng",
                PaymentStatus.Unpaid => "Chưa đóng",
                _ => "N/A"
            };
            public string StatusColor => PaymentStatus switch
            {
                PaymentStatus.Paid => "#4CAF50",
                PaymentStatus.Unpaid => "#F44336",
                _ => "#9E9E9E"
            };
        }

        private List<MemberVM> _members = new List<MemberVM>
        {
            new MemberVM { Id=1, Name="Minh Nhật", Role=MemberRole.Manager, PaymentStatus=PaymentStatus.Paid, IsCurrentUser=true },
            new MemberVM { Id=2, Name="Lê Hùng", Role=MemberRole.Member, PaymentStatus=PaymentStatus.Unpaid },
            new MemberVM { Id=3, Name="Trần Minh", Role=MemberRole.Member, PaymentStatus=PaymentStatus.Paid },
            new MemberVM { Id=4, Name="Tuấn Bảo", Role=MemberRole.Member, PaymentStatus=PaymentStatus.NotApplicable },
        };

        public MemberListPage()
        {
            InitializeComponent();
            MemberList.ItemsSource = _members;
            RefreshNotificationBadge();
        }

        // --- HÀM NHUỘM MÀU XANH CHO [NỘI DUNG TRONG NGOẶC] ---
        private void txtNoidung_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb == null || string.IsNullOrEmpty(tb.Text)) return;

            string originalText = tb.Text;
            int closeBracketIndex = originalText.IndexOf("]");

            if (closeBracketIndex != -1)
            {
                string tagPart = originalText.Substring(0, closeBracketIndex + 1);
                string contentPart = originalText.Substring(closeBracketIndex + 1);

                tb.Inlines.Clear();

                Run runTag = new Run(tagPart)
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                    FontWeight = FontWeights.Bold
                };

                Run runContent = new Run(contentPart)
                {
                    Foreground = Brushes.Black
                };

                tb.Inlines.Add(runTag);
                tb.Inlines.Add(runContent);
            }
        }

        public void RefreshNotificationBadge()
        {
            try
            {
                using (var db = new RoommateManagerContext())
                {
                    string userId = _currentUserId.Trim();
                    var allDetails = db.ChitietXemTbs.AsNoTracking().ToList();

                    int count = allDetails.Count(ct =>
                        ct.Matv != null &&
                        ct.Matv.ToString().Trim() == userId &&
                        (ct.Dadoc == false || ct.Dadoc == null));

                    if (count > 0)
                    {
                        txtBadgeCount.Text = count > 9 ? "9+" : count.ToString();
                        BadgeBorder.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BadgeBorder.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi hiển thị Badge: " + ex.Message);
            }
        }

        private void BtnNotification_Click(object sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = true;
            btnModeStatus_Click(null, null);
        }

        private void btn_ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = false;
            RefreshNotificationBadge();
        }

        private void btnModeStatus_Click(object sender, RoutedEventArgs e)
        {
            scrViewStatus.Visibility = Visibility.Visible;
            stkAddNotification.Visibility = Visibility.Collapsed;

            btnTabStatus.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            btnTabStatus.Foreground = Brushes.White;
            btnTabAdd.Background = Brushes.White;
            btnTabAdd.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243));

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var ds = db.Thongbaos.Where(t => t.Daxoa == false)
                        .OrderByDescending(t => t.Matb)
                        .Select(t => new {
                            Matb = t.Matb, // Đã thêm Matb để xử lý xóa/xem chi tiết
                            Noidung = t.Noidung,
                            Ngaytb = t.Ngaytb,
                            TyleDoc = db.ChitietXemTbs.Count(ct => ct.Matb == t.Matb && ct.Dadoc == true) + "/" + db.Thanhviens.Count()
                        }).ToList();
                    lstNotifications.ItemsSource = ds;
                }
            }
            catch { }
        }

        private void btnModeAdd_Click(object sender, RoutedEventArgs e)
        {
            scrViewStatus.Visibility = Visibility.Collapsed;
            stkAddNotification.Visibility = Visibility.Visible;

            btnTabAdd.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            btnTabAdd.Foreground = Brushes.White;
            btnTabStatus.Background = Brushes.White;
            btnTabStatus.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243));

            txtInputTB.Clear();

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var users = db.Thanhviens.ToList();
                    if (users.Count > 0)
                    {
                        cboRecipient.ItemsSource = users;
                        cboRecipient.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private void btnConfirmSend_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputTB.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung thông báo!");
                return;
            }

            try
            {
                using (var db = new RoommateManagerContext())
                {
                    var tb = new Thongbao
                    {
                        Noidung = "[THÔNG BÁO] " + txtInputTB.Text.Trim(),
                        Ngaytb = DateOnly.FromDateTime(DateTime.Now),
                        Daxoa = false
                    };
                    db.Thongbaos.Add(tb);
                    db.SaveChanges();

                    var allMembers = db.Thanhviens.ToList();
                    foreach (var tv in allMembers)
                    {
                        db.ChitietXemTbs.Add(new ChitietXemTb
                        {
                            Matb = tb.Matb,
                            Matv = tv.Id.ToString(),
                            Dadoc = false
                        });
                    }

                    db.SaveChanges();
                    MessageBox.Show("Đã gửi thông báo hệ thống thành công!");

                    txtInputTB.Clear();
                    btnModeStatus_Click(null, null);
                    RefreshNotificationBadge();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi thông báo hệ thống: " + ex.Message);
            }
        }

        private void btnViewDetail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as TextBlock)?.DataContext;
            if (item == null) return;

            try
            {
                dynamic d = item;
                int maTb = d.Matb; // Sử dụng mã thông báo

                using (var db = new RoommateManagerContext())
                {
                    var thongBao = db.Thongbaos.AsNoTracking().FirstOrDefault(t => t.Matb == maTb);

                    if (thongBao != null)
                    {
                        var tatCaTV = db.Thanhviens.AsNoTracking().ToList();
                        var chiTietDoc = db.ChitietXemTbs
                                           .AsNoTracking()
                                           .Where(ct => ct.Matb == thongBao.Matb)
                                           .Select(ct => new { ct.Matv, ct.Dadoc })
                                           .ToList();

                        var daXem = (from ct in chiTietDoc
                                     join tv in tatCaTV on ct.Matv equals tv.Id.ToString()
                                     where ct.Dadoc == true
                                     select tv.Ten).ToList();

                        var daXemNames = daXem.ToList();
                        var chuaXem = tatCaTV.Where(tv => !daXemNames.Contains(tv.Ten)).Select(tv => tv.Ten).ToList();

                        string msg = $"--- TRẠNG THÁI XEM THÔNG BÁO ---\n\n";
                        msg += $"✅ ĐÃ XEM ({daXem.Count}):\n" + (daXem.Any() ? "- " + string.Join("\n- ", daXem) : "(Trống)");
                        msg += $"\n\n❌ CHƯA XEM ({chuaXem.Count}):\n" + (chuaXem.Any() ? "- " + string.Join("\n- ", chuaXem) : "(Trống)");

                        MessageBox.Show(msg, "Chi tiết hệ thống", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi hiển thị: " + ex.Message); }
        }

        private void btnDeleteNotification_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext;
            if (item == null) return;

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa thông báo này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    dynamic d = item;
                    int maTb = d.Matb; // Đã đổi sang xóa theo Mã (Matb) thay vì Nội dung

                    using (var db = new RoommateManagerContext())
                    {
                        var thongBao = db.Thongbaos.FirstOrDefault(t => t.Matb == maTb);
                        if (thongBao != null)
                        {
                            thongBao.Daxoa = true;
                            db.SaveChanges();
                            btnModeStatus_Click(null, null);
                            RefreshNotificationBadge();
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi khi xóa: " + ex.Message); }
            }
        }

        private void BtnAddMember_Click(object sender, RoutedEventArgs e) { }
        private void MemberCard_RightClick(object sender, MouseButtonEventArgs e) { }
    }
}