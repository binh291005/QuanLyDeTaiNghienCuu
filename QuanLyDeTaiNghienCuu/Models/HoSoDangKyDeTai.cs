using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDeTaiNghienCuu.Models
{
    [Table("HoSoDangKyDeTai")]
    public class HoSoDangKyDeTai
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHoSo { get; set; }

        [Required]
        public int MaSinhVien { get; set; }

        [ForeignKey("MaSinhVien")]
        public virtual SinhVien? SinhVien { get; set; }

        [Required]
        public int MaDeTai { get; set; }

        [ForeignKey("MaDeTai")]
        public virtual DeTai? DeTai { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày nộp")]
        public DateTime NgayNop { get; set; } = DateTime.Now;

        [Display(Name = "Đề cương sơ bộ")]
        [DataType(DataType.MultilineText)]
        public string? DeCuongSoBo { get; set; }

        [Display(Name = "Ghi chú")]
        [DataType(DataType.MultilineText)]
        public string? GhiChu { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái hồ sơ")]
        public string TrangThai { get; set; } = "Chờ duyệt";
        // Status values: "Chờ duyệt", "Đạt xét duyệt", "Không đạt xét duyệt", "Chờ bảo vệ đề cương", "Đã bảo vệ đề cương", "Được giao đề tài", "Không được giao đề tài", "Đã hủy"

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xử lý")]
        public DateTime? NgayXuLy { get; set; }

        [Display(Name = "Nhận xét xét duyệt")]
        [DataType(DataType.MultilineText)]
        public string? NhanXetXetDuyet { get; set; }

        public virtual ICollection<LichBaoVeDeCuong> LichBaoVeDeCuongs { get; set; } = new List<LichBaoVeDeCuong>();
        public virtual KetQuaGiaoDeTai? KetQuaGiaoDeTai { get; set; }
    }
}
