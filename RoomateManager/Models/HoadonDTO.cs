using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoommateManager.Models
{
    public class HoadonDTO
    {
        public int Mahdtv { get; set; }
        public string TenNguoiThanhToan { get; set; }
        public string Noidung { get; set; } // Đảm bảo dòng này có tồn tại
        public decimal? Sotien { get; set; }
        public int? Thang { get; set; }
        public bool? Dadong { get; set; }
    }
}
