namespace QuanLyDeTaiNghienCuu.Helpers
{
    /// <summary>Các vai trò trong hệ thống (khớp với giá trị lưu ở TaiKhoan.VaiTro).</summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string GiangVien = "GiangVien";
        public const string SinhVien = "SinhVien";

        public static readonly string[] All = { Admin, GiangVien, SinhVien };

        public static string ToDisplay(string? role) => role switch
        {
            Admin => "Quản trị viên",
            GiangVien => "Giảng viên phụ trách",
            SinhVien => "Sinh viên",
            _ => role ?? string.Empty
        };
    }

    /// <summary>Tên các khóa lưu trong Session.</summary>
    public static class SessionKeys
    {
        public const string MaTaiKhoan = "MaTaiKhoan";
        public const string HoTen = "HoTen";
        public const string VaiTro = "VaiTro";
    }
}
