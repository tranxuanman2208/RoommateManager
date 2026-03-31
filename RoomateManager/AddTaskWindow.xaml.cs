using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RoomateManager
{
    /// <summary>
    /// Interaction logic for AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : Window
    {
        public AddTaskWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => txtTaskName.Focus();
        }
        public string TaskName => txtTaskName.Text.Trim();
        public string Description => txtDescription.Text.Trim();
        public string Frequency { get; private set; } = "Hàng ngày";


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                MessageBox.Show("Vui lòng nhập tên nhiệm vụ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            var cbLocal = this.FindName("cmbFrequency") as ComboBox;
            Frequency = (cbLocal?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Hàng ngày";

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Segment_Click(object sender, RoutedEventArgs e)
        {

            var clicked = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (clicked == null) return;

            foreach (var name in new[] { "tbDaily", "tbWeekly", "tbMonthly" })
            {
                var tb = this.FindName(name) as System.Windows.Controls.Primitives.ToggleButton;
                if (tb == null) continue;
                if (!object.ReferenceEquals(tb, clicked)) tb.IsChecked = false;
            }
            clicked.IsChecked = true;
        }
    }
}

