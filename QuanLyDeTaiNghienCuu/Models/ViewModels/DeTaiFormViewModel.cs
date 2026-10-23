using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiNghienCuu.Models.ViewModels
{
    public class DeTaiFormViewModel : IValidatableObject
    {
        public int MaDeTai { get; set; }

        [Required(ErrorMessage = "Tên đề tài bắt buộc nhập")]
        [StringLength(250, ErrorMessage = "Tên đề tài không vượt quá 250 ký tự")]
        [Display(Name = "Tên đề tài")]
        public string TenDeTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn bộ môn")]
        [Display(Name = "Bộ môn")]
        public int MaBoMon { get; set; }

        [Required(ErrorMessage = "Số nhóm tối đa bắt buộc nhập")]
        [Range(1, 100, ErrorMessage = "Số nhóm tối đa phải lớn hơn 0")]
        [Display(Name = "Số nhóm tối đa")]
        public int SoNhomToiDa { get; set; } = 1;

        [Required(ErrorMessage = "Lĩnh vực nghiên cứu không được để trống")]
        [StringLength(150, ErrorMessage = "Lĩnh vực nghiên cứu không quá 150 ký tự")]
        [Display(Name = "Lĩnh vực nghiên cứu")]
        public string LinhVucNghienCuu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Điểm trung bình tối thiểu bắt buộc nhập")]
        [Range(0.0, 4.0, ErrorMessage = "Điểm trung bình tối thiểu phải từ 0.0 đến 4.0")]
        [Display(Name = "ĐTB tối thiểu (thang điểm 4)")]
        public double DiemTrungBinhToiThieu { get; set; } = 2.0;

        [Required(ErrorMessage = "Ngày bắt đầu đăng ký không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu nhận đăng ký")]
        public DateTime NgayBatDauDangKy { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Hạn đăng ký không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn đăng ký")]
        public DateTime HanDangKy { get; set; } = DateTime.Today.AddDays(30);

        [Display(Name = "Mô tả chi tiết đề tài")]
        [DataType(DataType.MultilineText)]
        public string? MoTaDeTai { get; set; }

        [Display(Name = "Yêu cầu đối với sinh viên")]
        [DataType(DataType.MultilineText)]
        public string? YeuCauSinhVien { get; set; }

        [Required(ErrorMessage = "Trạng thái đề tài không được để trống")]
        [Display(Name = "Trạng thái đề tài")]
        public string TrangThai { get; set; } = "Chưa mở";

        // Dropdown data
        public SelectList? BoMonList { get; set; }
        public List<SelectListItem>? TrangThaiList { get; set; }

        // Validation for date range and business rules
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HanDangKy.Date < NgayBatDauDangKy.Date)
            {
                yield return new ValidationResult(
                    "Hạn đăng ký phải sau hoặc bằng ngày bắt đầu nhận đăng ký.",
                    new[] { nameof(HanDangKy) }
                );
            }

            if (TrangThai == "Đang mở đăng ký")
            {
                if (string.IsNullOrWhiteSpace(TenDeTai))
                {
                    yield return new ValidationResult(
                        "Không thể chuyển sang trạng thái 'Đang mở đăng ký' khi chưa nhập Tên đề tài hợp lệ.",
                        new[] { nameof(TrangThai) }
                    );
                }

                if (string.IsNullOrWhiteSpace(LinhVucNghienCuu))
                {
                    yield return new ValidationResult(
                        "Không thể chuyển sang trạng thái 'Đang mở đăng ký' khi chưa nhập Lĩnh vực nghiên cứu.",
                        new[] { nameof(TrangThai) }
                    );
                }
            }
        }
    }
}
