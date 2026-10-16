using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDeTaiNghienCuu.Models
{
    [Table("LichBaoVeDeCuong")]
    public class LichBaoVeDeCuong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLichBaoVeDeCuong { get; set; }

        [Required]
        public int MaHoSo { get; set; }

        [ForeignKey("MaHoSo")]
        public virtual HoSoDangKyDeTai? HoSo { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime ThoiGianBatDau { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Thời gian kết thúc")]
        public DateTime ThoiGianKetThuc { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Hình thức bảo vệ")]
        public string HinhThucBaoVe { get; set; } = "Trực tiếp"; // Trực tiếp / Trực tuyến

        [StringLength(255)]
        [Display(Name = "Địa điểm hoặc liên kết")]
        public string? DiaDiemHoacLienKet { get; set; }

        [StringLength(100)]
        [Display(Name = "Giảng viên phản biện")]
        public string? GiangVienPhanBien { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Đã lên lịch"; // Đã lên lịch, Đã hoàn thành, Đã hủy
    }
}
