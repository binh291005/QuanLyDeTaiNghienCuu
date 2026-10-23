// Nguồn mã: Mã nền nhóm và/hoặc bản tham khảo Module 3 có hỗ trợ AI.
// Chức năng trong file: Entity sinh viên dùng chung; phần tích hợp bổ sung Version.
// Thông tin người trực tiếp thực hiện: Chưa xác nhận; không gán tác giả cá nhân.
// Giữ nguyên thông tin tác giả mã nền khi hợp nhất với repository nhóm.
// Đối chiếu: DOI_CHIEU_DONG_GOP_HAI.md tại thư mục solution.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDeTaiNghienCuu.Models
{
    [Table("SinhVien")]
    public class SinhVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaSinhVien { get; set; }

        [ConcurrencyCheck]
        public Guid Version { get; set; } = Guid.NewGuid();

        public int? MaTaiKhoan { get; set; }

        [ForeignKey("MaTaiKhoan")]
        public virtual TaiKhoan? TaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên sinh viên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; } // Nam, Nữ, Khác

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(200)]
        [Display(Name = "Lĩnh vực quan tâm")]
        public string? LinhVucQuanTam { get; set; }

        [StringLength(100)]
        [Display(Name = "Ngành học")]
        public string? Nganh { get; set; }

        [Range(0.0, 4.0, ErrorMessage = "Điểm trung bình phải từ 0 đến 4")]
        [Display(Name = "Điểm trung bình tích lũy (ĐTB)")]
        public double? DiemTrungBinh { get; set; }

        [Display(Name = "Kinh nghiệm nghiên cứu")]
        [DataType(DataType.MultilineText)]
        public string? KinhNghiemNghienCuu { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public virtual ICollection<HoSoDangKyDeTai> HoSoDangKyDeTais { get; set; } = new List<HoSoDangKyDeTai>();
    }
}
