// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - ViewModel kết quả giao đề tài.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiNghienCuu.Models.ViewModels
{
    // Đề tài đã giao đủ số nhóm tối đa nhưng chưa đóng
    public class DeTaiDayNhomItem
    {
        public int MaDeTai { get; set; }
        public string TenDeTai { get; set; } = string.Empty;
        public int SoNhomToiDa { get; set; }
        public int SoDaGiao { get; set; }
    }

    public class KetQuaIndexViewModel
    {
        public List<HoSoDangKyDeTai> HoSoChoNhapKetQua { get; set; } = new List<HoSoDangKyDeTai>();
        public List<KetQuaGiaoDeTai> DanhSachKetQua { get; set; } = new List<KetQuaGiaoDeTai>();
        public List<DeTaiDayNhomItem> DeTaiDayNhom { get; set; } = new List<DeTaiDayNhomItem>();
    }

    public class KetQuaFormViewModel
    {
        public int MaHoSo { get; set; }

        public string TenSinhVien { get; set; } = string.Empty;
        public string TenDeTai { get; set; } = string.Empty;
        public int SoNhomToiDa { get; set; }
        public int SoDaGiao { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập điểm đánh giá")]
        [Range(0.0, 100.0, ErrorMessage = "Điểm đánh giá phải từ 0 đến 100.")]
        [Display(Name = "Điểm đánh giá (0 - 100)")]
        public double? DiemDanhGia { get; set; }

        [Display(Name = "Nhận xét")]
        [DataType(DataType.MultilineText)]
        public string? NhanXet { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn kết quả")]
        [Display(Name = "Kết quả")]
        public string? KetQua { get; set; }
    }
}
