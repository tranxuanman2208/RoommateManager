// Views/CreateInvoicePage.xaml.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RoommateManager.Views
{
    public partial class CreateInvoicePage : Page
    {
        private MainWindow _mainWindow;
        private static readonly string DraftPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RoommateManager", "draft.json");

        private List<string> _memberNames = new() { "Minh Nhật", "Lê Hùng", "Trần Minh", "Tuấn Bảo" };
        private Dictionary<string, TextBox> _manualInputs = new();

        public CreateInvoicePage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            BuildManualInputs();
            CheckDraft();
        }

        // AC5: Kiểm tra bản nháp khi mở màn hình
        private void CheckDraft()
        {
            if (File.Exists(DraftPath))
                DraftBanner.Visibility = Visibility.Visible;
        }

        private void BtnRestoreDraft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var json = File.ReadAllText(DraftPath);
                var draft = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (draft != null)
                {
                    TxtName.Text = draft.GetValueOrDefault("name", "");
                    TxtAmount.Text = draft.GetValueOrDefault("amount", "");
                    TxtNote.Text = draft.GetValueOrDefault("note", "");
                }
            }
            catch { }
            DraftBanner.Visibility = Visibility.Collapsed;
        }

        private void BtnDiscardDraft_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DraftPath)) File.Delete(DraftPath);
            DraftBanner.Visibility = Visibility.Collapsed;
        }

        // Build input chia thủ công
        private void BuildManualInputs()
        {
            ManualInputs.Children.Clear();
            _manualInputs.Clear();
            foreach (var name in _memberNames)
            {
                var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                var lbl = new TextBlock { Text = name, VerticalAlignment = VerticalAlignment.Center };
                var tb = new TextBox { Padding = new Thickness(8, 4, 8, 4) };
                tb.TextChanged += ManualInput_Changed;
                Grid.SetColumn(lbl, 0); Grid.SetColumn(tb, 1);
                row.Children.Add(lbl); row.Children.Add(tb);
                ManualInputs.Children.Add(row);
                _manualInputs[name] = tb;
            }
        }

        private void SplitMethod_Changed(object sender, RoutedEventArgs e)
        {
            if (ManualPanel == null) return;
            ManualPanel.Visibility = RbManual.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            UpdateManualSummary();
        }

        // AC4: Cập nhật bộ đếm chia thủ công
        private void ManualInput_Changed(object sender, TextChangedEventArgs e) => UpdateManualSummary();

        private void UpdateManualSummary()
        {
            if (TxtSplitSummary == null) return;
            decimal total = decimal.TryParse(TxtAmount.Text, out var t) ? t : 0;
            decimal allocated = 0;
            foreach (var tb in _manualInputs.Values)
                if (decimal.TryParse(tb.Text, out var v)) allocated += v;

            TxtSplitSummary.Text = $"Đã phân bổ: {allocated:N0}đ / {total:N0}đ";
            TxtSplitSummary.Foreground = allocated == total && total > 0
                ? Brushes.Green : Brushes.OrangeRed;
        }

        // AC3: Validate realtime số tiền
        private void TxtAmount_Changed(object sender, TextChangedEventArgs e)
        {
            if (ErrAmount == null) return;
            var text = TxtAmount.Text;
            bool valid = decimal.TryParse(text, out _) || string.IsNullOrEmpty(text);
            ErrAmount.Visibility = (!valid || string.IsNullOrEmpty(text))
                ? Visibility.Visible : Visibility.Collapsed;
            ErrAmount.Text = string.IsNullOrEmpty(text) ? "Số tiền không được để trống."
                : "Số tiền chỉ được nhập số.";
            SaveDraft();
            UpdateManualSummary();
            _mainWindow.HasUnsavedData = true;
        }

        private void Form_Changed(object sender, RoutedEventArgs e)
        {
            SaveDraft();
            _mainWindow.HasUnsavedData = true;
        }

        // AC5: Lưu nháp tự động
        private void SaveDraft()
        {
            try
            {
                var dir = Path.GetDirectoryName(DraftPath);
                if (dir != null) Directory.CreateDirectory(dir);
                var draft = new Dictionary<string, string>
                {
                    ["name"] = TxtName?.Text ?? "",
                    ["amount"] = TxtAmount?.Text ?? "",
                    ["note"] = TxtNote?.Text ?? ""
                };
                File.WriteAllText(DraftPath, JsonSerializer.Serialize(draft));
            }
            catch { }
        }

        // AC1 + AC2: Validate và Xem trước
        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            { ErrName.Visibility = Visibility.Visible; valid = false; }
            else ErrName.Visibility = Visibility.Collapsed;

            if (!decimal.TryParse(TxtAmount.Text, out decimal total) || total <= 0)
            { ErrAmount.Visibility = Visibility.Visible; valid = false; }
            else ErrAmount.Visibility = Visibility.Collapsed;

            if (!valid) return;

            // Tạo preview
            decimal perPerson = total / _memberNames.Count;
            string preview = $"📋 XEM TRƯỚC HÓA ĐƠN\n{'─'.ToString().PadRight(30, '─')}\n";
            preview += $"Tên: {TxtName.Text}\nTổng: {total:N0}đ\n\n";
            foreach (var name in _memberNames)
            {
                decimal amt = RbManual.IsChecked == true && _manualInputs.ContainsKey(name)
                    && decimal.TryParse(_manualInputs[name].Text, out var mv) ? mv : perPerson;
                preview += $"• {name}: {amt:N0}đ\n";
            }

            var result = MessageBox.Show(preview + "\nXác nhận gửi hóa đơn?",
                "Xem trước hóa đơn", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (File.Exists(DraftPath)) File.Delete(DraftPath);
                _mainWindow.HasUnsavedData = false;
                MessageBox.Show("✅ Hóa đơn đã được gửi thành công!", "Hoàn tất");
            }
        }
    }
}