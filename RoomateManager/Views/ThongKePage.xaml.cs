using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using RoomateManager.Models;

namespace RoomateManager.Models
{
    public partial class ThongKePage : Page
    {
        // Khởi tạo DbContext (Thay bằng tên class context của bạn)
        RoommateManagerContext db = new RoommateManagerContext();

        public ThongKePage()
        {
            InitializeComponent();

            // Mặc định khi mở trang sẽ load dữ liệu tháng 4
            LoadChartData(4);
        }

        private void LoadChartData(int month)
        {
            try
            {
                using (var db = new RoommateManagerContext())
                {
                    // 1. Truy vấn và nhóm theo Mancc từ bảng HOADONTONG thông qua MahdtNavigation
                    var rawData = db.Hoadontvs
                        .Where(h => h.Thang == month && h.Dadong == true && (h.Daxoa == false || h.Daxoa == null))
                        .GroupBy(h => h.MahdtNavigation.Mancc)
                        .Select(g => new
                        {
                            MaNCC = g.Key,
                            TongTien = g.Sum(x => (decimal?)x.Sotien) ?? 0
                        })
                        .ToList();

                    // 2. Chuyển đổi Mã thành Tên hiển thị bằng switch expression
                    var data = rawData.Select(x => new
                    {
                        LoaiPhi = x.MaNCC switch
                        {
                            "NCC001" => "Tiền điện",
                            "NCC002" => "Tiền nước",
                            "NCC003" => "Tiền nhà",
                            _ => "Chi phí khác" // Các mã còn lại sẽ hiện là Chi phí khác
                        },
                        x.TongTien
                    }).ToList();

                    // 3. Kiểm tra dữ liệu (Giữ nguyên logic cũ của bạn)
                    if (data.Count == 0)
                    {
                        MyPieChart.Series.Clear();
                        TxtNoData.Visibility = Visibility.Visible;
                        return;
                    }
                    TxtNoData.Visibility = Visibility.Collapsed;

                    // 4. Đổ dữ liệu vào biểu đồ
                    SeriesCollection series = new SeriesCollection();
                    foreach (var item in data)
                    {
                        series.Add(new PieSeries
                        {
                            Title = item.LoaiPhi,
                            Values = new ChartValues<decimal> { item.TongTien },
                            DataLabels = true,
                            LabelPoint = chartPoint => string.Format("{0}: {1:N0} VNĐ ({2:P1})",
                                chartPoint.SeriesView.Title,
                                chartPoint.Y,
                                chartPoint.Participation)
                        });
                    }

                    MyPieChart.Series = series;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thống kê: " + ex.Message);
            }
        }

        private void CmbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tránh lỗi khi khởi tạo ComboBox chưa load xong
            if (MyPieChart == null) return;

            if (CmbMonth.SelectedItem is ComboBoxItem item)
            {
                // Lấy số tháng từ chuỗi "Tháng X"
                string content = item.Content.ToString();
                int month = int.Parse(content.Replace("Tháng ", ""));

                LoadChartData(month);
            }
        }
    }
}