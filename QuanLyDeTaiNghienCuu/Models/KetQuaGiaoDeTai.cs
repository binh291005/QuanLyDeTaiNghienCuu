using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDeTaiNghienCuu.Models
{
    [Table("KetQuaGiaoDeTai")]
    public class KetQuaGiaoDeTai
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKetQuaGiaoDeTai { get; set; }

        [Required]
        public int MaHoSo { get; set; }

        [ForeignKey("MaHoSo")]
        public virtual HoSoDangKyDeTai? HoSo { get; set; }

        [Range(0.0, 100.0, ErrorMessage = "Điểm đánh giá phải từ 0 đến 100")]
        [Display(Name = "Điểm đánh giá")]
        public double? DiemDanhGia { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Nhận xét")]
        public string? NhanXet { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Kết quả")]
        public string KetQua { get; set; } = "Được giao đề tài"; // Được giao đề tài / Không được giao đề tài

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày cập nhật")]
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;
    }
}
