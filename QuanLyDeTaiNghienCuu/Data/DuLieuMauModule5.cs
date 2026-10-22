// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - Dữ liệu mẫu hồ sơ, lịch bảo vệ đề cương, kết quả giao đề tài.
using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyDeTaiNghienCuu.Models;

namespace QuanLyDeTaiNghienCuu.Data
{
    public static class DuLieuMauModule5
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Đã có hồ sơ thì không thêm nữa
            if (context.HoSoDangKyDeTais.Any()) return;

            var deTais = context.DeTais.OrderBy(d => d.MaDeTai).ToList();
            if (deTais.Count == 0) return;

            // 1. Bổ sung sinh viên mẫu cho đủ 30 sinh viên
            int soSinhVien = context.SinhViens.Count();
            for (int i = soSinhVien + 1; i <= 30; i++)
            {
                context.SinhViens.Add(new SinhVien
                {
                    HoTen = $"Sinh viên mẫu {i:00}",
                    Email = $"sv{i:00}@student.uneti.edu.vn",
                    SoDienThoai = "0900000" + i.ToString("000"),
                    GioiTinh = i % 2 == 0 ? "Nữ" : "Nam",
                    Nganh = "Công nghệ thông tin",
                    DiemTrungBinh = 2.5 + (i % 15) / 10.0,
                    LinhVucQuanTam = "Lập trình Web",
                    TrangThai = true
                });
            }
            context.SaveChanges();
            var sinhViens = context.SinhViens.OrderBy(s => s.MaSinhVien).ToList();

            // 2. Tạo 40 hồ sơ với nhiều trạng thái khác nhau
            string[] trangThais =
            {
                "Chờ duyệt", "Không đạt xét duyệt", "Đạt xét duyệt", "Chờ bảo vệ đề cương",
                "Đã bảo vệ đề cương", "Được giao đề tài", "Không được giao đề tài", "Đã hủy"
            };

            var hoSos = new List<HoSoDangKyDeTai>();
            var daCo = new HashSet<(int, int)>(); // tránh 1 sinh viên đăng ký trùng 1 đề tài

            for (int i = 0; i < 40; i++)
            {
                var sv = sinhViens[i % sinhViens.Count];
                var dt = deTais[i % deTais.Count];
                if (!daCo.Add((sv.MaSinhVien, dt.MaDeTai))) continue;

                string trangThai = trangThais[(i + i / 8) % 8];
                var ngayNop = DateTime.Now.AddDays(-(i * 4 + 1));

                hoSos.Add(new HoSoDangKyDeTai
                {
                    MaSinhVien = sv.MaSinhVien,
                    MaDeTai = dt.MaDeTai,
                    NgayNop = ngayNop,
                    DeCuongSoBo = "Đề cương sơ bộ mẫu của sinh viên.",
                    TrangThai = trangThai,
                    NgayXuLy = trangThai == "Chờ duyệt" ? (DateTime?)null : ngayNop.AddDays(1),
                    NhanXetXetDuyet = trangThai == "Chờ duyệt" ? null : "Nhận xét xét duyệt mẫu."
                });
            }
            context.HoSoDangKyDeTais.AddRange(hoSos);
            context.SaveChanges();

            // 3. Tạo lịch bảo vệ + kết quả cho các hồ sơ ở trạng thái tương ứng
            // Mỗi lịch nằm ở một ngày khác nhau nên không bao giờ trùng nhau.
            string[] giangVienPhanBiens = { "TS. Lê Văn C", "ThS. Phạm Thị D", "TS. Hoàng Văn E" };
            string[] phongs = { "Phòng A101", "Phòng A102", "Phòng B201" };

            int soLichSapToi = 0;
            int soLichDaQua = 0;
            int stt = 0;

            foreach (var h in hoSos)
            {
                DateTime batDau;
                string trangThaiLich;

                if (h.TrangThai == "Chờ bảo vệ đề cương")
                {
                    soLichSapToi++;
                    batDau = DateTime.Today.AddDays(soLichSapToi).AddHours(8);
                    trangThaiLich = "Đã lên lịch";
                }
                else if (h.TrangThai == "Đã bảo vệ đề cương"
                      || h.TrangThai == "Được giao đề tài"
                      || h.TrangThai == "Không được giao đề tài")
                {
                    soLichDaQua++;
                    batDau = DateTime.Today.AddDays(-soLichDaQua).AddHours(8);
                    trangThaiLich = "Đã hoàn thành";
                }
                else
                {
                    continue;
                }

                stt++;
                bool trucTuyen = stt % 4 == 0;

                context.LichBaoVeDeCuongs.Add(new LichBaoVeDeCuong
                {
                    MaHoSo = h.MaHoSo,
                    ThoiGianBatDau = batDau,
                    ThoiGianKetThuc = batDau.AddHours(1),
                    HinhThucBaoVe = trucTuyen ? "Trực tuyến" : "Trực tiếp",
                    DiaDiemHoacLienKet = trucTuyen ? "https://meet.google.com/abc-defg-hij" : phongs[stt % phongs.Length],
                    GiangVienPhanBien = giangVienPhanBiens[stt % giangVienPhanBiens.Length],
                    GhiChu = "Lịch mẫu",
                    TrangThai = trangThaiLich
                });

                if (h.TrangThai == "Được giao đề tài" || h.TrangThai == "Không được giao đề tài")
                {
                    bool duocGiao = h.TrangThai == "Được giao đề tài";
                    context.KetQuaGiaoDeTais.Add(new KetQuaGiaoDeTai
                    {
                        MaHoSo = h.MaHoSo,
                        DiemDanhGia = duocGiao ? 80 + (h.MaHoSo % 15) : 40 + (h.MaHoSo % 20),
                        NhanXet = duocGiao ? "Đề cương tốt, đủ điều kiện thực hiện." : "Đề cương chưa đạt yêu cầu.",
                        KetQua = h.TrangThai,
                        NgayCapNhat = DateTime.Now.AddDays(-1)
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
