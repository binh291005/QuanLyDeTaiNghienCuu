// Nguồn mã: Mã nền nhóm và/hoặc bản tham khảo Module 3 có hỗ trợ AI.
// Chức năng trong file: ViewModel biểu mẫu, validation và ánh xạ các trường được phép sửa.
// Thông tin người trực tiếp thực hiện: Chưa xác nhận; không gán tác giả cá nhân.
// Giữ nguyên thông tin tác giả mã nền khi hợp nhất với repository nhóm.
// Đối chiếu: DOI_CHIEU_DONG_GOP_HAI.md tại thư mục solution.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyDeTaiNghienCuu.Services;
namespace QuanLyDeTaiNghienCuu.Models.ViewModels;

public class SinhVienFormViewModel : IValidatableObject
{
    [Required(ErrorMessage="Vui lòng nhập họ tên."), StringLength(100), Display(Name="Họ và tên")]
    public string HoTen { get; set; } = "";
    [DataType(DataType.Date), Display(Name="Ngày sinh")] public DateTime? NgaySinh { get; set; }
    [StringLength(10), Display(Name="Giới tính")] public string? GioiTinh { get; set; }
    [Phone(ErrorMessage="Số điện thoại không hợp lệ."), StringLength(20), Display(Name="Số điện thoại")] public string? SoDienThoai { get; set; }
    [EmailAddress(ErrorMessage="Email không hợp lệ."), StringLength(100), Display(Name="Email")] public string? Email { get; set; }
    [StringLength(255), Display(Name="Địa chỉ")] public string? DiaChi { get; set; }
    [StringLength(200), Display(Name="Lĩnh vực quan tâm")] public string? LinhVucQuanTam { get; set; }
    [StringLength(100), Display(Name="Ngành học")] public string? Nganh { get; set; }
    [Range(0,4,ErrorMessage="Điểm trung bình phải từ 0 đến 4."), Display(Name="Điểm trung bình (0–4)")] public double? DiemTrungBinh { get; set; }
    [StringLength(5000), Display(Name="Kinh nghiệm nghiên cứu")] public string? KinhNghiemNghienCuu { get; set; }
    public Guid Version { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var today=Module3Rules.VietnamNow(TimeProvider.System).Date;
        if(NgaySinh.HasValue && (NgaySinh.Value.Date>=today || NgaySinh.Value.Year<1900))
            yield return new ValidationResult("Ngày sinh phải từ năm 1900 và trước hôm nay.", [nameof(NgaySinh)]);
        if(GioiTinh is not null && !new[]{"Nam","Nữ","Khác"}.Contains(GioiTinh))
            yield return new ValidationResult("Giới tính không hợp lệ.", [nameof(GioiTinh)]);
    }
    public static SinhVienFormViewModel From(SinhVien s) => new() { HoTen=s.HoTen,NgaySinh=s.NgaySinh,GioiTinh=s.GioiTinh,
        SoDienThoai=s.SoDienThoai,Email=s.Email,DiaChi=s.DiaChi,LinhVucQuanTam=s.LinhVucQuanTam,Nganh=s.Nganh,
        DiemTrungBinh=s.DiemTrungBinh,KinhNghiemNghienCuu=s.KinhNghiemNghienCuu,Version=s.Version };
    public void Apply(SinhVien s) { s.HoTen=HoTen.Trim();s.NgaySinh=NgaySinh?.Date;s.GioiTinh=GioiTinh;
        s.SoDienThoai=SoDienThoai?.Trim();s.Email=Email?.Trim();s.DiaChi=DiaChi?.Trim();s.LinhVucQuanTam=LinhVucQuanTam?.Trim();
        s.Nganh=Nganh?.Trim();s.DiemTrungBinh=DiemTrungBinh;s.KinhNghiemNghienCuu=KinhNghiemNghienCuu?.Trim(); }
}
public class SinhVienAdminFormViewModel : SinhVienFormViewModel
{
    [Display(Name="Tài khoản sinh viên (có thể liên kết sau)")] public int? MaTaiKhoan { get; set; }
    [Display(Name="Đang hoạt động")] public bool TrangThai { get; set; } = true;
    public List<SelectListItem> Accounts { get; set; } = [];
    public static SinhVienAdminFormViewModel FromAdmin(SinhVien s) { var m=new SinhVienAdminFormViewModel();
        foreach(var p in typeof(SinhVienFormViewModel).GetProperties()) p.SetValue(m,p.GetValue(From(s)));
        m.MaTaiKhoan=s.MaTaiKhoan;m.TrangThai=s.TrangThai;return m; }
}
public class NopHoSoViewModel
{
    [Range(1,int.MaxValue)] public int MaDeTai { get; set; }
    [Required(ErrorMessage="Vui lòng nhập đề cương sơ bộ."), StringLength(10000), Display(Name="Đề cương sơ bộ")] public string DeCuongSoBo { get; set; }="";
    [StringLength(2000), Display(Name="Ghi chú")] public string? GhiChu { get; set; }
    public DeTai? DeTai { get; set; }
}
public class SinhVienIndexViewModel { public List<SinhVien> Items { get; set; }=[]; public string? Q { get; set; } public int Page { get; set; } public int TotalPages { get; set; } }
public class HoSoIndexViewModel { public List<HoSoDangKyDeTai> Items { get; set; }=[]; public string? Status { get; set; } public int Page { get; set; } public int TotalPages { get; set; } }
