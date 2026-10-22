// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - ViewModel Dashboard và Thống kê.
using System.Collections.Generic;

namespace QuanLyDeTaiNghienCuu.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TongBoMon { get; set; }
        public int TongDeTai { get; set; }
        public int DeTaiDangMo { get; set; }
        public int TongSinhVien { get; set; }
        public int TongHoSo { get; set; }
        public int HoSoChoDuyet { get; set; }
        public int HoSoDatXetDuyet { get; set; }
        public int LichSapToi { get; set; }
        public int HoSoDuocGiao { get; set; }
    }

    public class ThongKeDong
    {
        public string Nhan { get; set; } = string.Empty;
        public double GiaTri { get; set; }
        public string DonVi { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class ThongKeViewModel
    {
        public string Loai { get; set; } = "bomon";
        public int Nam { get; set; }

        public string TieuDe { get; set; } = string.Empty;
        public string YNghia { get; set; } = string.Empty;
        public string CotNhan { get; set; } = string.Empty;
        public string CotGiaTri { get; set; } = string.Empty;
        public string? ThongBao { get; set; }
        public List<ThongKeDong> Dong { get; set; } = new List<ThongKeDong>();

        public static readonly List<(string Ma, string Ten)> DanhSachLoai = new List<(string, string)>
        {
            ("bomon", "Số đề tài theo bộ môn"),
            ("hosotheodetai", "Số hồ sơ theo từng đề tài"),
            ("hosotheotrangthai", "Số hồ sơ theo trạng thái"),
            ("datxetduyet", "Số hồ sơ đạt xét duyệt theo đề tài"),
            ("topdetai", "Đề tài nhận nhiều hồ sơ nhất"),
            ("giaotheobomon", "Số hồ sơ được giao đề tài theo bộ môn"),
            ("hosotheothang", "Số hồ sơ nộp theo tháng"),
            ("lichtheothang", "Số lịch bảo vệ đề cương theo tháng"),
            ("tylexetduyet", "Tỷ lệ hồ sơ qua xét duyệt theo từng đề tài"),
            ("tylegiao", "Tỷ lệ hồ sơ được giao đề tài trên số hồ sơ đã bảo vệ")
        };
    }
}
