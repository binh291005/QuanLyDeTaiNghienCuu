// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - Dashboard và Thống kê.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDeTaiNghienCuu.Controllers
{
    [PhanQuyen("Admin", "GiangVien")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Các trạng thái hồ sơ được coi là "đã qua xét duyệt"
        private static readonly string[] DaQuaXetDuyet =
        {
            "Đạt xét duyệt", "Chờ bảo vệ đề cương", "Đã bảo vệ đề cương",
            "Được giao đề tài", "Không được giao đề tài"
        };

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ThongKe/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var now = DateTime.Now;

                var viewModel = new DashboardViewModel
                {
                    TongBoMon = await _context.BoMons.CountAsync(),
                    TongDeTai = await _context.DeTais.CountAsync(),
                    DeTaiDangMo = await _context.DeTais.CountAsync(d => d.TrangThai == "Đang mở đăng ký"),
                    TongSinhVien = await _context.SinhViens.CountAsync(),
                    TongHoSo = await _context.HoSoDangKyDeTais.CountAsync(),
                    HoSoChoDuyet = await _context.HoSoDangKyDeTais.CountAsync(h => h.TrangThai == "Chờ duyệt"),
                    HoSoDatXetDuyet = await _context.HoSoDangKyDeTais.CountAsync(h => h.TrangThai == "Đạt xét duyệt"),
                    LichSapToi = await _context.LichBaoVeDeCuongs
                        .CountAsync(l => l.TrangThai == "Đã lên lịch" && l.ThoiGianBatDau >= now),
                    HoSoDuocGiao = await _context.HoSoDangKyDeTais.CountAsync(h => h.TrangThai == "Được giao đề tài")
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể hiển thị Dashboard do lỗi hệ thống.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: ThongKe?loai=bomon&nam=2026
        public async Task<IActionResult> Index(string loai = "bomon", int? nam = null)
        {
            var viewModel = new ThongKeViewModel
            {
                Loai = loai,
                Nam = nam ?? DateTime.Now.Year
            };

            if (viewModel.Nam < 2000 || viewModel.Nam > 2100)
            {
                viewModel.ThongBao = "Năm thống kê không hợp lệ.";
                return View(viewModel);
            }

            try
            {
                switch (loai)
                {
                    case "bomon":
                        Dat(viewModel, "Số đề tài theo bộ môn",
                            "Cho biết mỗi bộ môn đang quản lý bao nhiêu đề tài.", "Bộ môn", "Số đề tài");
                        viewModel.Dong = await DeTaiTheoBoMon();
                        break;

                    case "hosotheodetai":
                        Dat(viewModel, "Số hồ sơ theo từng đề tài",
                            "Cho biết mức độ quan tâm của sinh viên đối với từng đề tài.", "Đề tài", "Số hồ sơ");
                        viewModel.Dong = await HoSoTheoDeTai();
                        break;

                    case "hosotheotrangthai":
                        Dat(viewModel, "Số hồ sơ theo trạng thái",
                            "Cho biết hồ sơ đang phân bố ở các bước nào của quy trình xử lý.", "Trạng thái", "Số hồ sơ");
                        viewModel.Dong = await HoSoTheoTrangThai();
                        break;

                    case "datxetduyet":
                        Dat(viewModel, "Số hồ sơ đạt xét duyệt theo đề tài",
                            "Đếm các hồ sơ đã qua xét duyệt (Đạt xét duyệt và các bước phía sau) của mỗi đề tài.", "Đề tài", "Số hồ sơ đạt");
                        viewModel.Dong = await DatXetDuyetTheoDeTai();
                        break;

                    case "topdetai":
                        Dat(viewModel, "Đề tài nhận nhiều hồ sơ nhất",
                            "Đề tài có số hồ sơ đăng ký cao nhất (nếu nhiều đề tài bằng nhau sẽ hiển thị tất cả).", "Đề tài", "Số hồ sơ");
                        viewModel.Dong = await TopDeTai();
                        break;

                    case "giaotheobomon":
                        Dat(viewModel, "Số hồ sơ được giao đề tài theo bộ môn",
                            "Cho biết mỗi bộ môn đã giao đề tài cho bao nhiêu sinh viên.", "Bộ môn", "Số hồ sơ được giao");
                        viewModel.Dong = await GiaoTheoBoMon();
                        break;

                    case "hosotheothang":
                        Dat(viewModel, $"Số hồ sơ nộp theo tháng - năm {viewModel.Nam}",
                            "Cho biết thời điểm sinh viên nộp hồ sơ nhiều nhất trong năm.", "Tháng", "Số hồ sơ");
                        viewModel.Dong = await HoSoTheoThang(viewModel.Nam);
                        break;

                    case "lichtheothang":
                        Dat(viewModel, $"Số lịch bảo vệ đề cương theo tháng - năm {viewModel.Nam}",
                            "Đếm các lịch bảo vệ chưa bị hủy, theo tháng bắt đầu bảo vệ.", "Tháng", "Số lịch");
                        viewModel.Dong = await LichTheoThang(viewModel.Nam);
                        break;

                    case "tylexetduyet":
                        Dat(viewModel, "Tỷ lệ hồ sơ qua xét duyệt theo từng đề tài",
                            "Tỷ lệ = số hồ sơ qua xét duyệt / số hồ sơ đã được xét duyệt (đạt + không đạt). Hồ sơ Chờ duyệt không tính.", "Đề tài", "Tỷ lệ");
                        viewModel.Dong = await TyLeQuaXetDuyet();
                        break;

                    case "tylegiao":
                        Dat(viewModel, "Tỷ lệ hồ sơ được giao đề tài trên số hồ sơ đã bảo vệ",
                            "Tỷ lệ = số hồ sơ Được giao đề tài / tổng số hồ sơ đã bảo vệ đề cương.", "Chỉ tiêu", "Giá trị");
                        viewModel.Dong = await TyLeGiao();
                        break;

                    default:
                        viewModel.ThongBao = "Loại thống kê không hợp lệ.";
                        return View(viewModel);
                }

                if (viewModel.Dong.Count == 0)
                {
                    viewModel.ThongBao = "Không có dữ liệu thống kê phù hợp.";
                }
            }
            catch (Exception)
            {
                viewModel.ThongBao = "Không thể hiển thị thống kê do lỗi hệ thống.";
            }

            return View(viewModel);
        }

        private static void Dat(ThongKeViewModel vm, string tieuDe, string yNghia, string cotNhan, string cotGiaTri)
        {
            vm.TieuDe = tieuDe;
            vm.YNghia = yNghia;
            vm.CotNhan = cotNhan;
            vm.CotGiaTri = cotGiaTri;
        }

        // 1. Số đề tài theo bộ môn
        private async Task<List<ThongKeDong>> DeTaiTheoBoMon()
        {
            var ds = await _context.BoMons
                .Select(b => new { b.TenBoMon, SoLuong = b.DeTais.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            return ds.Select(x => new ThongKeDong { Nhan = x.TenBoMon, GiaTri = x.SoLuong, DonVi = "đề tài" }).ToList();
        }

        // 2. Số hồ sơ theo từng đề tài
        private async Task<List<ThongKeDong>> HoSoTheoDeTai()
        {
            var ds = await _context.DeTais
                .Select(d => new { d.TenDeTai, SoLuong = d.HoSoDangKyDeTais.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            return ds.Select(x => new ThongKeDong { Nhan = x.TenDeTai, GiaTri = x.SoLuong, DonVi = "hồ sơ" }).ToList();
        }

        // 3. Số hồ sơ theo trạng thái
        private async Task<List<ThongKeDong>> HoSoTheoTrangThai()
        {
            var ds = await _context.HoSoDangKyDeTais
                .GroupBy(h => h.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            return ds.Select(x => new ThongKeDong { Nhan = x.TrangThai, GiaTri = x.SoLuong, DonVi = "hồ sơ" }).ToList();
        }

        // 4. Số hồ sơ đạt xét duyệt theo đề tài
        private async Task<List<ThongKeDong>> DatXetDuyetTheoDeTai()
        {
            var ds = await _context.DeTais
                .Select(d => new
                {
                    d.TenDeTai,
                    SoLuong = d.HoSoDangKyDeTais.Count(h => DaQuaXetDuyet.Contains(h.TrangThai))
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            return ds.Select(x => new ThongKeDong { Nhan = x.TenDeTai, GiaTri = x.SoLuong, DonVi = "hồ sơ" }).ToList();
        }

        // 5. Đề tài nhận nhiều hồ sơ nhất
        private async Task<List<ThongKeDong>> TopDeTai()
        {
            var ds = await HoSoTheoDeTai();
            if (ds.Count == 0) return ds;

            double max = ds.Max(x => x.GiaTri);
            if (max == 0) return new List<ThongKeDong>();

            return ds.Where(x => x.GiaTri == max).ToList();
        }

        // 6. Số hồ sơ được giao đề tài theo bộ môn
        private async Task<List<ThongKeDong>> GiaoTheoBoMon()
        {
            var ds = await _context.BoMons
                .Select(b => new
                {
                    b.TenBoMon,
                    SoLuong = b.DeTais
                        .SelectMany(d => d.HoSoDangKyDeTais)
                        .Count(h => h.TrangThai == "Được giao đề tài")
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            return ds.Select(x => new ThongKeDong { Nhan = x.TenBoMon, GiaTri = x.SoLuong, DonVi = "hồ sơ" }).ToList();
        }

        // 7. Số hồ sơ nộp theo tháng (của một năm)
        private async Task<List<ThongKeDong>> HoSoTheoThang(int nam)
        {
            var ds = await _context.HoSoDangKyDeTais
                .Where(h => h.NgayNop.Year == nam)
                .GroupBy(h => h.NgayNop.Month)
                .Select(g => new { Thang = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            if (ds.Count == 0) return new List<ThongKeDong>();

            // Đủ 12 tháng, tháng không có dữ liệu = 0
            return Enumerable.Range(1, 12).Select(t => new ThongKeDong
            {
                Nhan = $"Tháng {t}",
                GiaTri = ds.Where(x => x.Thang == t).Select(x => x.SoLuong).FirstOrDefault(),
                DonVi = "hồ sơ"
            }).ToList();
        }

        // 8. Số lịch bảo vệ đề cương theo tháng (không tính lịch đã hủy)
        private async Task<List<ThongKeDong>> LichTheoThang(int nam)
        {
            var ds = await _context.LichBaoVeDeCuongs
                .Where(l => l.TrangThai != "Đã hủy" && l.ThoiGianBatDau.Year == nam)
                .GroupBy(l => l.ThoiGianBatDau.Month)
                .Select(g => new { Thang = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            if (ds.Count == 0) return new List<ThongKeDong>();

            return Enumerable.Range(1, 12).Select(t => new ThongKeDong
            {
                Nhan = $"Tháng {t}",
                GiaTri = ds.Where(x => x.Thang == t).Select(x => x.SoLuong).FirstOrDefault(),
                DonVi = "lịch"
            }).ToList();
        }

        // 9. Tỷ lệ hồ sơ qua xét duyệt theo từng đề tài
        private async Task<List<ThongKeDong>> TyLeQuaXetDuyet()
        {
            var ds = await _context.DeTais
                .Select(d => new
                {
                    d.TenDeTai,
                    Qua = d.HoSoDangKyDeTais.Count(h => DaQuaXetDuyet.Contains(h.TrangThai)),
                    KhongDat = d.HoSoDangKyDeTais.Count(h => h.TrangThai == "Không đạt xét duyệt")
                })
                .ToListAsync();

            var ketQua = new List<ThongKeDong>();
            foreach (var x in ds)
            {
                int mau = x.Qua + x.KhongDat;
                ketQua.Add(new ThongKeDong
                {
                    Nhan = x.TenDeTai,
                    GiaTri = mau == 0 ? 0 : Math.Round(x.Qua * 100.0 / mau, 1),
                    DonVi = "%",
                    GhiChu = $"{x.Qua}/{mau} hồ sơ đã xét duyệt"
                });
            }
            return ketQua.OrderByDescending(x => x.GiaTri).ToList();
        }

        // 10. Tỷ lệ hồ sơ được giao đề tài trên số hồ sơ đã bảo vệ đề cương
        private async Task<List<ThongKeDong>> TyLeGiao()
        {
            int duocGiao = await _context.HoSoDangKyDeTais.CountAsync(h => h.TrangThai == "Được giao đề tài");
            int khongDuocGiao = await _context.HoSoDangKyDeTais.CountAsync(h => h.TrangThai == "Không được giao đề tài");
            int chuaCoKetQua = await _context.HoSoDangKyDeTais.CountAsync(h => h.TrangThai == "Đã bảo vệ đề cương");

            // Hồ sơ đã bảo vệ = đang chờ kết quả + đã có kết quả (được giao / không được giao)
            int daBaoVe = duocGiao + khongDuocGiao + chuaCoKetQua;

            var ketQua = new List<ThongKeDong>
            {
                new ThongKeDong { Nhan = "Số hồ sơ đã bảo vệ đề cương", GiaTri = daBaoVe, DonVi = "hồ sơ" },
                new ThongKeDong { Nhan = "Số hồ sơ được giao đề tài", GiaTri = duocGiao, DonVi = "hồ sơ" }
            };

            if (daBaoVe == 0)
            {
                ketQua.Add(new ThongKeDong { Nhan = "Tỷ lệ được giao đề tài", GiaTri = 0, DonVi = "%", GhiChu = "Chưa có hồ sơ nào đã bảo vệ đề cương" });
            }
            else
            {
                ketQua.Add(new ThongKeDong { Nhan = "Tỷ lệ được giao đề tài", GiaTri = Math.Round(duocGiao * 100.0 / daBaoVe, 1), DonVi = "%" });
            }
            return ketQua;
        }
    }
}
