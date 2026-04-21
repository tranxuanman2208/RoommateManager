using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using RoommateManager.Models;

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
                // 1. Truy vấn dữ liệu: Nhóm theo tên loại phí và tổng tiền các hóa đơn ĐÃ ĐÓNG
                var data = db.Hoadontvs
                    .Where(h => h.Thang == month && h.Dadong == true && h.Daxoa == false)
                    .GroupBy(h => h.Noidung)
                    .Select(g => new
                    {
                        LoaiPhi = g.Key,
                        TongTien = g.Sum(x => (decimal?)x.Sotien) ?? 0
                    })
                    .ToList();

                // 2. Kiểm tra nếu không có dữ liệu thì hiện thông báo
                if (data.Count == 0)
                {
                    MyPieChart.Series.Clear();
                    TxtNoData.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    TxtNoData.Visibility = Visibility.Collapsed;
                }

                // 3. Tạo SeriesCollection cho PieChart
                SeriesCollection series = new SeriesCollection();

                foreach (var item in data)
                {
                    series.Add(new PieSeries
                    {
                        Title = item.LoaiPhi,
                        Values = new ChartValues<decimal> { item.TongTien },
                        DataLabels = true,
                        // Định dạng hiển thị: Tên: 1,000,000 VNĐ (25%)
                        LabelPoint = chartPoint => string.Format("{0}: {1:N0} VNĐ ({2:P1})",
                                                    chartPoint.SeriesView.Title,
                                                    chartPoint.Y,
                                                    chartPoint.Participation)
                    });
                }

                // 4. Đổ dữ liệu vào biểu đồ
                MyPieChart.Series = series;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu biểu đồ: " + ex.Message);
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