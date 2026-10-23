// Họ và tên: [ĐIỀN HỌ TÊN]
// Mã sinh viên: [ĐIỀN MÃ SINH VIÊN]
// Nội dung thực hiện: Module 4 - Tiếp nhận, xét duyệt và quản lý trạng thái hồ sơ đăng ký đề tài.

namespace QuanLyDeTaiNghienCuu.Models
{
    public static class TrangThaiHoSo
    {
        public const string ChoDuyet = "Chờ duyệt";
        public const string DatXetDuyet = "Đạt xét duyệt";
        public const string KhongDatXetDuyet = "Không đạt xét duyệt";
        public const string ChoBaoVeDeCuong = "Chờ bảo vệ đề cương";
        public const string DaBaoVeDeCuong = "Đã bảo vệ đề cương";
        public const string DuocGiaoDeTai = "Được giao đề tài";
        public const string KhongDuocGiaoDeTai = "Không được giao đề tài";
        public const string DaHuy = "Đã hủy";

        public static readonly string[] TatCa =
        {
            ChoDuyet, DatXetDuyet, KhongDatXetDuyet, ChoBaoVeDeCuong,
            DaBaoVeDeCuong, DuocGiaoDeTai, KhongDuocGiaoDeTai, DaHuy
        };

        public static bool ChuyenTrangThaiHopLe(string trangThaiHienTai, string trangThaiMoi)
        {
            return trangThaiHienTai switch
            {
                ChoDuyet => trangThaiMoi is DatXetDuyet or KhongDatXetDuyet or DaHuy,
                DatXetDuyet => trangThaiMoi is ChoBaoVeDeCuong or DaHuy,
                ChoBaoVeDeCuong => trangThaiMoi is DaBaoVeDeCuong or DaHuy,
                DaBaoVeDeCuong => trangThaiMoi is DuocGiaoDeTai or KhongDuocGiaoDeTai,
                _ => false
            };
        }
    }
}
