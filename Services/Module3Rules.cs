// Nguồn mã: Mã nền nhóm và/hoặc bản tham khảo Module 3 có hỗ trợ AI.
// Chức năng trong file: Quy tắc trạng thái, điều kiện hủy/công bố và thời gian Việt Nam.
// Thông tin người trực tiếp thực hiện: Chưa xác nhận; không gán tác giả cá nhân.
// Giữ nguyên thông tin tác giả mã nền khi hợp nhất với repository nhóm.
// Đối chiếu: DOI_CHIEU_DONG_GOP_HAI.md tại thư mục solution.
using QuanLyDeTaiNghienCuu.Models;
namespace QuanLyDeTaiNghienCuu.Services;

public static class Module3Rules
{
    public const string Pending = "Chờ duyệt", Accepted = "Đạt xét duyệt", Rejected = "Không đạt xét duyệt",
        Scheduled = "Chờ bảo vệ đề cương", Defended = "Đã bảo vệ đề cương", Assigned = "Được giao đề tài",
        NotAssigned = "Không được giao đề tài", Cancelled = "Đã hủy";
    public static readonly string[] Processing = [Pending, Accepted, Scheduled, Defended];
    public static readonly string[] AllStatuses = [Pending, Accepted, Rejected, Scheduled, Defended, Assigned, NotAssigned, Cancelled];
    public static DateTime VietnamNow(TimeProvider time) => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time.GetUtcNow(), "SE Asia Standard Time").DateTime;
    // Chốt chính sách theo ví dụ mục 7.4: không có BẤT KỲ lịch nào, kể cả lịch đã hủy.
    public static bool CanCancel(HoSoDangKyDeTai h) => (h.TrangThai == Pending || h.TrangThai == Accepted)
        && !h.LichBaoVeDeCuongs.Any() && h.KetQuaGiaoDeTai == null;
    public static bool ResultPublished(HoSoDangKyDeTai h) => h.KetQuaGiaoDeTai != null
        && (h.TrangThai == Assigned || h.TrangThai == NotAssigned)
        && h.KetQuaGiaoDeTai.KetQua == h.TrangThai;
    // Required ở Entity hiện tại chỉ bắt buộc HoTen. Không tự tăng điều kiện từ các trường tùy chọn.
    public static bool ProfileComplete(SinhVien s) => !string.IsNullOrWhiteSpace(s.HoTen);
}
