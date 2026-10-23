namespace QuanLyDeTaiNghienCuu.Helpers
{
    /// <summary>Hàm tiện ích đọc thông tin đăng nhập từ Session (dùng được cả trong Controller và View).</summary>
    public static class SessionExtensions
    {
        public static int? GetMaTaiKhoan(this ISession session) => session.GetInt32(SessionKeys.MaTaiKhoan);
        public static string? GetHoTen(this ISession session) => session.GetString(SessionKeys.HoTen);
        public static string? GetVaiTro(this ISession session) => session.GetString(SessionKeys.VaiTro);

        public static bool IsLoggedIn(this ISession session) => session.GetMaTaiKhoan().HasValue;
        public static bool IsInRole(this ISession session, string role) => session.GetVaiTro() == role;
        public static bool IsAdmin(this ISession session) => session.IsInRole(Roles.Admin);
        public static bool IsGiangVien(this ISession session) => session.IsInRole(Roles.GiangVien);
        public static bool IsSinhVien(this ISession session) => session.IsInRole(Roles.SinhVien);
    }
}
