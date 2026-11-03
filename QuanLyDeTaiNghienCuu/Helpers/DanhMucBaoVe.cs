// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - Danh mục phòng bảo vệ và giảng viên phản biện (dùng cho ô chọn).
using System.Collections.Generic;
using System.Linq;

namespace QuanLyDeTaiNghienCuu.Helpers
{
    public static class DanhMucBaoVe
    {
        public static readonly List<string> PhongHoc = new List<string>
        {
            "Phòng A101", "Phòng A102", "Phòng A201", "Phòng B201", "Phòng B202", "Hội trường C"
        };
        public static readonly List<(string Gia, string Ten)> PhongOnline = new List<(string, string)>
        {
            ("https://meet.google.com/abc-defg-hij", "Phòng họp Meet 1 (meet.google.com/abc-defg-hij)"),
            ("https://meet.google.com/klm-nopq-rst", "Phòng họp Meet 2 (meet.google.com/klm-nopq-rst)"),
            ("https://meet.google.com/uvw-xyza-bcd", "Phòng họp Meet 3 (meet.google.com/uvw-xyza-bcd)")
        };

        public static readonly List<string> GiangViens = new List<string>
        {
            "TS. Lê Văn C", "ThS. Phạm Thị D", "TS. Hoàng Văn E", "PGS.TS. Nguyễn Văn F", "ThS. Trần Thị G"
        };

        public static bool LaPhongOnline(string giaTri)
        {
            return PhongOnline.Any(p => p.Gia == giaTri);
        }

        public static bool LaPhongHopLe(string giaTri)
        {
            return PhongHoc.Contains(giaTri) || LaPhongOnline(giaTri);
        }
    }
}
