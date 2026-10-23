// Họ và tên: [ĐIỀN HỌ TÊN]
// Mã sinh viên: [ĐIỀN MÃ SINH VIÊN]
// Nội dung thực hiện: Module 4 - Tiếp nhận, xét duyệt và quản lý trạng thái hồ sơ đăng ký đề tài.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;

namespace QuanLyDeTaiNghienCuu.Controllers
{
    public class HoSoDangKyDeTaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoSoDangKyDeTaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool CoQuyenQuanLyHoSo()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            // Tương thích tạm thời với branch hiện tại chưa có Module 1 đăng nhập/Session.
            // Khi Module 1 đã tích hợp, Session sẽ có VaiTro và SinhVien sẽ bị chặn tại Controller.
            if (string.IsNullOrWhiteSpace(vaiTro))
            {
                return true;
            }

            return vaiTro == "Admin" || vaiTro == "GiangVien" || vaiTro == "Giảng viên phụ trách";
        }

        private IActionResult? KiemTraQuyen()
        {
            if (!CoQuyenQuanLyHoSo())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng quản lý hồ sơ đăng ký.";
                return RedirectToAction("Index", "DeTai");
            }

            return null;
        }

        // MODULE 4 - Danh sách hồ sơ cần xử lý, tìm kiếm, lọc, sắp xếp, phân trang.
        public async Task<IActionResult> Index(
            string? tenSinhVien,
            string? tenDeTai,
            int? maBoMon,
            string? trangThai,
            DateTime? tuNgay,
            DateTime? denNgay,
            string sortBy = "ngaynop_desc",
            int page = 1,
            int pageSize = 10)
        {
            var forbidden = KiemTraQuyen();
            if (forbidden != null) return forbidden;

            if (page < 1) page = 1;
            if (pageSize is < 5 or > 50) pageSize = 10;

            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
            {
                ModelState.AddModelError(string.Empty, "Từ ngày không được lớn hơn Đến ngày.");
            }

            var query = _context.HoSoDangKyDeTais
                .Include(h => h.SinhVien)
                .Include(h => h.DeTai)
                    .ThenInclude(d => d!.BoMon)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tenSinhVien))
            {
                var term = tenSinhVien.Trim().ToLower();
                query = query.Where(h => h.SinhVien != null && h.SinhVien.HoTen.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(tenDeTai))
            {
                var term = tenDeTai.Trim().ToLower();
                query = query.Where(h => h.DeTai != null && h.DeTai.TenDeTai.ToLower().Contains(term));
            }

            if (maBoMon.HasValue && maBoMon.Value > 0)
            {
                query = query.Where(h => h.DeTai != null && h.DeTai.MaBoMon == maBoMon.Value);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }

            if (tuNgay.HasValue)
            {
                var from = tuNgay.Value.Date;
                query = query.Where(h => h.NgayNop >= from);
            }

            if (denNgay.HasValue)
            {
                var toExclusive = denNgay.Value.Date.AddDays(1);
                query = query.Where(h => h.NgayNop < toExclusive);
            }

            query = sortBy switch
            {
                "ngaynop_asc" => query.OrderBy(h => h.NgayNop),
                "sv_asc" => query.OrderBy(h => h.SinhVien!.HoTen).ThenByDescending(h => h.NgayNop),
                "sv_desc" => query.OrderByDescending(h => h.SinhVien!.HoTen).ThenByDescending(h => h.NgayNop),
                _ => query.OrderByDescending(h => h.NgayNop)
            };

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            if (page > totalPages) page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var boMons = await _context.BoMons
                .AsNoTracking()
                .OrderBy(b => b.TenBoMon)
                .ToListAsync();

            var model = new HoSoDangKyIndexViewModel
            {
                TenSinhVien = tenSinhVien,
                TenDeTai = tenDeTai,
                MaBoMon = maBoMon,
                TrangThai = trangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                SortBy = sortBy,
                PageIndex = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items,
                BoMonList = new SelectList(boMons, "MaBoMon", "TenBoMon", maBoMon),
                TrangThaiList = TrangThaiHoSo.TatCa
                    .Select(x => new SelectListItem { Text = x, Value = x, Selected = x == trangThai })
                    .ToList(),
                SortByList = new List<SelectListItem>
                {
                    new() { Text = "Ngày nộp mới nhất", Value = "ngaynop_desc", Selected = sortBy == "ngaynop_desc" },
                    new() { Text = "Ngày nộp cũ nhất", Value = "ngaynop_asc", Selected = sortBy == "ngaynop_asc" },
                    new() { Text = "Tên sinh viên A → Z", Value = "sv_asc", Selected = sortBy == "sv_asc" },
                    new() { Text = "Tên sinh viên Z → A", Value = "sv_desc", Selected = sortBy == "sv_desc" }
                }
            };

            return View(model);
        }

        // MODULE 4 - Xem chi tiết hồ sơ.
        public async Task<IActionResult> Details(int id)
        {
            var forbidden = KiemTraQuyen();
            if (forbidden != null) return forbidden;

            var hoSo = await _context.HoSoDangKyDeTais
                .Include(h => h.SinhVien)
                .Include(h => h.DeTai)
                    .ThenInclude(d => d!.BoMon)
                .Include(h => h.LichBaoVeDeCuongs)
                .Include(h => h.KetQuaGiaoDeTai)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.MaHoSo == id);

            if (hoSo == null)
            {
                return NotFound();
            }

            var soNhomDaDuocGiao = await _context.HoSoDangKyDeTais
                .CountAsync(h => h.MaDeTai == hoSo.MaDeTai && h.TrangThai == TrangThaiHoSo.DuocGiaoDeTai);

            ViewBag.SoNhomDaDuocGiao = soNhomDaDuocGiao;
            ViewBag.SoNhomToiDa = hoSo.DeTai?.SoNhomToiDa ?? 0;
            ViewBag.CoTheXetDuyet = hoSo.TrangThai == TrangThaiHoSo.ChoDuyet;

            return View(hoSo);
        }

        // MODULE 4 - Hiển thị màn hình xét duyệt.
        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var forbidden = KiemTraQuyen();
            if (forbidden != null) return forbidden;

            var hoSo = await _context.HoSoDangKyDeTais
                .Include(h => h.SinhVien)
                .Include(h => h.DeTai)
                    .ThenInclude(d => d!.BoMon)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.MaHoSo == id);

            if (hoSo == null)
            {
                return NotFound();
            }

            if (hoSo.TrangThai != TrangThaiHoSo.ChoDuyet)
            {
                TempData["ErrorMessage"] = "Chỉ hồ sơ ở trạng thái Chờ duyệt mới được xét duyệt.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var model = await TaoViewModelXetDuyet(hoSo);
            return View(model);
        }

        // MODULE 4 - Duyệt / từ chối hồ sơ.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(HoSoXetDuyetViewModel model)
        {
            var forbidden = KiemTraQuyen();
            if (forbidden != null) return forbidden;

            var hoSo = await _context.HoSoDangKyDeTais
                .Include(h => h.SinhVien)
                .Include(h => h.DeTai)
                    .ThenInclude(d => d!.BoMon)
                .FirstOrDefaultAsync(h => h.MaHoSo == model.MaHoSo);

            if (hoSo == null)
            {
                return NotFound();
            }

            if (model.KetQuaXetDuyet != TrangThaiHoSo.DatXetDuyet &&
                model.KetQuaXetDuyet != TrangThaiHoSo.KhongDatXetDuyet)
            {
                ModelState.AddModelError(nameof(model.KetQuaXetDuyet), "Kết quả xét duyệt không hợp lệ.");
            }

            // 8.3 - Kiểm tra điều kiện trước khi duyệt.
            if (hoSo.TrangThai == TrangThaiHoSo.DaHuy)
            {
                ModelState.AddModelError(string.Empty, "Hồ sơ đã hủy, không thể xét duyệt.");
            }
            else if (hoSo.TrangThai != TrangThaiHoSo.ChoDuyet || hoSo.NgayXuLy.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Hồ sơ này đã có kết quả xét duyệt hoặc không còn ở trạng thái Chờ duyệt.");
            }

            if (hoSo.DeTai == null)
            {
                ModelState.AddModelError(string.Empty, "Đề tài liên quan không còn tồn tại.");
            }

            if (hoSo.SinhVien == null || !hoSo.SinhVien.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Sinh viên không tồn tại hoặc hiện không hoạt động.");
            }

            if (!TrangThaiHoSo.ChuyenTrangThaiHopLe(hoSo.TrangThai, model.KetQuaXetDuyet))
            {
                ModelState.AddModelError(string.Empty, "Chuyển trạng thái hồ sơ không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                var rebuilt = await TaoViewModelXetDuyet(hoSo);
                rebuilt.KetQuaXetDuyet = model.KetQuaXetDuyet;
                rebuilt.NhanXetXetDuyet = model.NhanXetXetDuyet;
                return View(rebuilt);
            }

            hoSo.TrangThai = model.KetQuaXetDuyet;
            hoSo.NgayXuLy = DateTime.Now;
            hoSo.NhanXetXetDuyet = model.NhanXetXetDuyet.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = model.KetQuaXetDuyet == TrangThaiHoSo.DatXetDuyet
                ? "Hồ sơ đã được xét duyệt Đạt."
                : "Hồ sơ đã được cập nhật Không đạt xét duyệt.";

            return RedirectToAction(nameof(Details), new { id = hoSo.MaHoSo });
        }

        private async Task<HoSoXetDuyetViewModel> TaoViewModelXetDuyet(HoSoDangKyDeTai hoSo)
        {
            var soNhomDaDuocGiao = await _context.HoSoDangKyDeTais
                .CountAsync(h => h.MaDeTai == hoSo.MaDeTai && h.TrangThai == TrangThaiHoSo.DuocGiaoDeTai);

            return new HoSoXetDuyetViewModel
            {
                MaHoSo = hoSo.MaHoSo,
                HoTenSinhVien = hoSo.SinhVien?.HoTen ?? "Không xác định",
                TenDeTai = hoSo.DeTai?.TenDeTai ?? "Không xác định",
                TenBoMon = hoSo.DeTai?.BoMon?.TenBoMon ?? "Không xác định",
                LinhVucQuanTam = hoSo.SinhVien?.LinhVucQuanTam ?? "Chưa khai báo",
                LinhVucNghienCuu = hoSo.DeTai?.LinhVucNghienCuu ?? "Chưa khai báo",
                DiemTrungBinh = hoSo.SinhVien?.DiemTrungBinh,
                DiemTrungBinhToiThieu = hoSo.DeTai?.DiemTrungBinhToiThieu ?? 0,
                YeuCauSinhVien = hoSo.DeTai?.YeuCauSinhVien ?? "Không có yêu cầu bổ sung",
                DeCuongSoBo = hoSo.DeCuongSoBo ?? "Chưa có đề cương sơ bộ",
                GhiChu = hoSo.GhiChu ?? "Không có ghi chú",
                TrangThaiHienTai = hoSo.TrangThai,
                SinhVienDangHoatDong = hoSo.SinhVien?.TrangThai == true,
                SoNhomDaDuocGiao = soNhomDaDuocGiao,
                SoNhomToiDa = hoSo.DeTai?.SoNhomToiDa ?? 0
            };
        }
    }
}
