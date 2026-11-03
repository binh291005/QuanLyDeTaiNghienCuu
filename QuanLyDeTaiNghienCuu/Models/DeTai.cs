using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDeTaiNghienCuu.Models
{
    [Table("DeTai")]
    public class DeTai
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã đề tài")]
        public int MaDeTai { get; set; }

        [Required(ErrorMessage = "Tên đề tài bắt buộc nhập")]
        [StringLength(250, ErrorMessage = "Tên đề tài tối đa 250 ký tự")]
        [Display(Name = "Tên đề tài")]
        public string TenDeTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn bộ môn")]
        [Display(Name = "Bộ môn")]
        public int MaBoMon { get; set; }

        [ForeignKey("MaBoMon")]
        [Display(Name = "Bộ môn")]
        public virtual BoMon? BoMon { get; set; }

        [Required(ErrorMessage = "Số nhóm tối đa bắt buộc nhập")]
        [Range(1, 100, ErrorMessage = "Số nhóm tối đa phải lớn hơn 0")]
        [Display(Name = "Số nhóm tối đa")]
        public int SoNhomToiDa { get; set; } = 1;

        [Required(ErrorMessage = "Lĩnh vực nghiên cứu không được để trống")]
        [StringLength(150, ErrorMessage = "Lĩnh vực nghiên cứu tối đa 150 ký tự")]
        [Display(Name = "Lĩnh vực nghiên cứu")]
        public string LinhVucNghienCuu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Điểm trung bình tối thiểu bắt buộc nhập")]
        [Range(0.0, 4.0, ErrorMessage = "Điểm trung bình tối thiểu phải nằm trong khoảng từ 0.0 đến 4.0")]
        [Display(Name = "ĐTB tối thiểu")]
        public double DiemTrungBinhToiThieu { get; set; } = 2.0;

        [Required(ErrorMessage = "Ngày bắt đầu đăng ký không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu đăng ký")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime NgayBatDauDangKy { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Hạn đăng ký không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn đăng ký")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime HanDangKy { get; set; } = DateTime.Today.AddDays(30);

        [Display(Name = "Mô tả đề tài")]
        [DataType(DataType.MultilineText)]
        public string? MoTaDeTai { get; set; }

        [Display(Name = "Yêu cầu đối với sinh viên")]
        [DataType(DataType.MultilineText)]
        public string? YeuCauSinhVien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái đề tài")]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chưa mở"; 
        // Trạng thái hợp lệ: "Chưa mở", "Đang mở đăng ký", "Tạm dừng", "Đã đóng"

        public virtual ICollection<HoSoDangKyDeTai> HoSoDangKyDeTais { get; set; } = new List<HoSoDangKyDeTai>();

        // Helper computed properties
        [NotMapped]
        public bool IsActiveRegistration => 
            TrangThai == "Đang mở đăng ký" && 
            DateTime.Now.Date >= NgayBatDauDangKy.Date && 
            DateTime.Now.Date <= HanDangKy.Date;

        [NotMapped]
        public bool IsExpired => DateTime.Now.Date > HanDangKy.Date;

        [NotMapped]
        public bool IsNotStarted => DateTime.Now.Date < NgayBatDauDangKy.Date;
    }
}
