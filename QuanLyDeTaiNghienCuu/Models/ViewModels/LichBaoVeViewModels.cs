// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - ViewModel lịch bảo vệ đề cương.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiNghienCuu.Models.ViewModels
{
    // Dữ liệu cho trang danh sách lịch bảo vệ
    public class LichIndexViewModel
    {
        public string? TrangThai { get; set; }
        public List<LichBaoVeDeCuong> DanhSachLich { get; set; } = new List<LichBaoVeDeCuong>();

        // Hồ sơ "Đạt xét duyệt" và chưa có lịch hiệu lực -> hiện nút "Lập lịch"
        public List<HoSoDangKyDeTai> HoSoChoLapLich { get; set; } = new List<HoSoDangKyDeTai>();
    }

    // Dữ liệu cho form lập lịch / điều chỉnh lịch
    public class LichFormViewModel
    {
        public int MaLichBaoVeDeCuong { get; set; }
        public int MaHoSo { get; set; }

        public string TenSinhVien { get; set; } = string.Empty;
        public string TenDeTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        [Display(Name = "Thời gian bắt đầu")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ThoiGianBatDau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        [Display(Name = "Thời gian kết thúc")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ThoiGianKetThuc { get; set; }

        public string HinhThucBaoVe { get; set; } = "Trực tiếp";

        [StringLength(255)]
        [Display(Name = "Phòng bảo vệ")]
        public string? DiaDiemHoacLienKet { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giảng viên phản biện")]
        [StringLength(100)]
        [Display(Name = "Giảng viên phản biện")]
        public string? GiangVienPhanBien { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}
