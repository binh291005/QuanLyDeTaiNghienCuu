using System.ComponentModel.DataAnnotations;

namespace QuanLyDeTaiNghienCuu.Models.ViewModels
{
    public class TaiKhoanFormViewModel
    {
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Tên đăng nhập không được chứa khoảng trắng")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        // Bắt buộc khi tạo mới (kiểm tra trong Controller); khi sửa để trống = giữ nguyên mật khẩu cũ
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? MatKhau { get; set; }

        [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string? XacNhanMatKhau { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "GiangVien";

        [Display(Name = "Hoạt động")]
        public bool TrangThai { get; set; } = true;

        public bool IsEdit => MaTaiKhoan > 0;
    }

    public class TaiKhoanIndexViewModel
    {
        public List<TaiKhoan> Items { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? VaiTro { get; set; }
        public string? TrangThai { get; set; } // "active" | "locked" | null
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }

    public class BoMonRowViewModel
    {
        public int MaBoMon { get; set; }
        public string TenBoMon { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? EmailLienHe { get; set; }
        public bool TrangThai { get; set; }
        public int SoDeTai { get; set; }
    }

    public class BoMonIndexViewModel
    {
        public List<BoMonRowViewModel> Items { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? TrangThai { get; set; } // "active" | "inactive" | null
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }

    public class PagerModel
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}
