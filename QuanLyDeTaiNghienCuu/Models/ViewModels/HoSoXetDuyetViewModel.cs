// Họ và tên: [ĐIỀN HỌ TÊN]
// Mã sinh viên: [ĐIỀN MÃ SINH VIÊN]
// Nội dung thực hiện: Module 4 - Tiếp nhận, xét duyệt và quản lý trạng thái hồ sơ đăng ký đề tài.

using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiNghienCuu.Models.ViewModels
{
    public class HoSoXetDuyetViewModel
    {
        public int MaHoSo { get; set; }

        public string HoTenSinhVien { get; set; } = string.Empty;
        public string TenDeTai { get; set; } = string.Empty;
        public string TenBoMon { get; set; } = string.Empty;
        public string LinhVucQuanTam { get; set; } = string.Empty;
        public string LinhVucNghienCuu { get; set; } = string.Empty;
        public double? DiemTrungBinh { get; set; }
        public double DiemTrungBinhToiThieu { get; set; }
        public string YeuCauSinhVien { get; set; } = string.Empty;
        public string DeCuongSoBo { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public string TrangThaiHienTai { get; set; } = string.Empty;
        public bool SinhVienDangHoatDong { get; set; }

        public int SoNhomDaDuocGiao { get; set; }
        public int SoNhomToiDa { get; set; }
        public bool DaDatSoNhomToiDa => SoNhomDaDuocGiao >= SoNhomToiDa;

        [Required(ErrorMessage = "Vui lòng chọn kết quả xét duyệt")]
        [Display(Name = "Kết quả xét duyệt")]
        public string KetQuaXetDuyet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhận xét xét duyệt không được để trống")]
        [StringLength(1000, ErrorMessage = "Nhận xét tối đa 1000 ký tự")]
        [Display(Name = "Nhận xét xét duyệt")]
        public string NhanXetXetDuyet { get; set; } = string.Empty;
    }
}
