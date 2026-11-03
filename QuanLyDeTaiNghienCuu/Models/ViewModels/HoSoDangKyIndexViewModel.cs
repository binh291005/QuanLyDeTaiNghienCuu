// Họ và tên: [ĐIỀN HỌ TÊN]
// Mã sinh viên: [ĐIỀN MÃ SINH VIÊN]
// Nội dung thực hiện: Module 4 - Tiếp nhận, xét duyệt và quản lý trạng thái hồ sơ đăng ký đề tài.

using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyDeTaiNghienCuu.Models.ViewModels
{
    public class HoSoDangKyIndexViewModel
    {
        public string? TenSinhVien { get; set; }
        public string? TenDeTai { get; set; }
        public int? MaBoMon { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string SortBy { get; set; } = "ngaynop_desc";

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public List<HoSoDangKyDeTai> Items { get; set; } = new();
        public SelectList? BoMonList { get; set; }
        public List<SelectListItem> TrangThaiList { get; set; } = new();
        public List<SelectListItem> SortByList { get; set; } = new();
    }
}
