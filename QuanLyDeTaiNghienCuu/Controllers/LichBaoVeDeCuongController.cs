// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - Lập lịch bảo vệ đề cương, kiểm tra trùng lịch,
// điều chỉnh / hoàn thành / hủy lịch bảo vệ.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDeTaiNghienCuu.Controllers
{
    [PhanQuyen("Admin", "GiangVien")]
    public class LichBaoVeDeCuongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichBaoVeDeCuongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LichBaoVeDeCuong
        public async Task<IActionResult> Index(string? trangThai)
        {
            var query = _context.LichBaoVeDeCuongs
                .Include(l => l.HoSo).ThenInclude(h => h!.SinhVien)
                .Include(l => l.HoSo).ThenInclude(h => h!.DeTai)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(l => l.TrangThai == trangThai);
            }

            var viewModel = new LichIndexViewModel
            {
                TrangThai = trangThai,
                DanhSachLich = await query.OrderByDescending(l => l.ThoiGianBatDau).ToListAsync(),

                // Hồ sơ Đạt xét duyệt và chưa có lịch nào còn hiệu lực
                HoSoChoLapLich = await _context.HoSoDangKyDeTais
                    .Include(h => h.SinhVien)
                    .Include(h => h.DeTai)
                    .Where(h => h.TrangThai == "Đạt xét duyệt"
                             && !h.LichBaoVeDeCuongs.Any(l => l.TrangThai != "Đã hủy"))
                    .OrderBy(h => h.NgayNop)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: LichBaoVeDeCuong/Create?maHoSo=5
        public async Task<IActionResult> Create(int? maHoSo)
        {
            if (maHoSo == null) return NotFound();

            var hoSo = await LayHoSo(maHoSo.Value);
            if (hoSo == null) return NotFound();

            var loi = KiemTraHoSoDuocLapLich(hoSo);
            if (loi != null)
            {
                TempData["ErrorMessage"] = loi;
                return RedirectToAction(nameof(Index));
            }

            var model = new LichFormViewModel
            {
                ThoiGianBatDau = DateTime.Today.AddDays(1).AddHours(8),
                ThoiGianKetThuc = DateTime.Today.AddDays(1).AddHours(9)
            };
            DienThongTinHoSo(model, hoSo);
            return View(model);
        }

        // POST: LichBaoVeDeCuong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichFormViewModel model)
        {
            var hoSo = await LayHoSo(model.MaHoSo);
            if (hoSo == null) return NotFound();

            var loi = KiemTraHoSoDuocLapLich(hoSo);
            if (loi != null)
            {
                TempData["ErrorMessage"] = loi;
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra thời gian + trùng lịch (sinh viên, giảng viên phản biện, phòng)
            await KiemTraThoiGianVaTrungLich(model, hoSo.MaSinhVien, 0);

            if (ModelState.IsValid)
            {
                var lich = new LichBaoVeDeCuong
                {
                    MaHoSo = hoSo.MaHoSo,
                    ThoiGianBatDau = model.ThoiGianBatDau,
                    ThoiGianKetThuc = model.ThoiGianKetThuc,
                    HinhThucBaoVe = model.HinhThucBaoVe,
                    DiaDiemHoacLienKet = model.DiaDiemHoacLienKet,
                    GiangVienPhanBien = model.GiangVienPhanBien,
                    GhiChu = model.GhiChu,
                    TrangThai = "Đã lên lịch"
                };
                _context.LichBaoVeDeCuongs.Add(lich);

                // Hồ sơ chuyển sang Chờ bảo vệ đề cương
                hoSo.TrangThai = "Chờ bảo vệ đề cương";

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lập lịch bảo vệ đề cương thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Không thể lưu lịch bảo vệ đề cương do lỗi hệ thống.");
                }
            }

            DienThongTinHoSo(model, hoSo);
            return View(model);
        }

        // GET: LichBaoVeDeCuong/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lich = await LayLich(id.Value);
            var loi = KiemTraLichCoTheThaoTac(lich);
            if (loi != null)
            {
                TempData["ErrorMessage"] = loi;
                return RedirectToAction(nameof(Index));
            }

            var model = new LichFormViewModel
            {
                MaLichBaoVeDeCuong = lich!.MaLichBaoVeDeCuong,
                ThoiGianBatDau = lich.ThoiGianBatDau,
                ThoiGianKetThuc = lich.ThoiGianKetThuc,
                HinhThucBaoVe = lich.HinhThucBaoVe,
                DiaDiemHoacLienKet = lich.DiaDiemHoacLienKet,
                GiangVienPhanBien = lich.GiangVienPhanBien,
                GhiChu = lich.GhiChu
            };
            DienThongTinHoSo(model, lich.HoSo);
            return View(model);
        }

        // POST: LichBaoVeDeCuong/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LichFormViewModel model)
        {
            if (id != model.MaLichBaoVeDeCuong) return NotFound();

            var lich = await LayLich(id);
            var loi = KiemTraLichCoTheThaoTac(lich);
            if (loi != null)
            {
                TempData["ErrorMessage"] = loi;
                return RedirectToAction(nameof(Index));
            }

            // Truyền id để khi kiểm tra trùng KHÔNG tính chính lịch đang sửa
            await KiemTraThoiGianVaTrungLich(model, lich!.HoSo!.MaSinhVien, id);

            if (ModelState.IsValid)
            {
                lich.ThoiGianBatDau = model.ThoiGianBatDau;
                lich.ThoiGianKetThuc = model.ThoiGianKetThuc;
                lich.HinhThucBaoVe = model.HinhThucBaoVe;
                lich.DiaDiemHoacLienKet = model.DiaDiemHoacLienKet;
                lich.GiangVienPhanBien = model.GiangVienPhanBien;
                lich.GhiChu = model.GhiChu;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật lịch bảo vệ thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Không thể cập nhật lịch bảo vệ do lỗi hệ thống.");
                }
            }

            DienThongTinHoSo(model, lich.HoSo);
            return View(model);
        }

        // POST: LichBaoVeDeCuong/HoanThanh/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var lich = await LayLich(id);
            var loi = KiemTraLichCoTheThaoTac(lich);

            if (loi == null && lich!.HoSo!.TrangThai != "Chờ bảo vệ đề cương")
            {
                loi = "Hồ sơ không ở trạng thái Chờ bảo vệ đề cương.";
            }

            if (loi != null)
            {
                TempData["ErrorMessage"] = loi;
                return RedirectToAction(nameof(Index));
            }

            lich!.TrangThai = "Đã hoàn thành";
            lich.HoSo!.TrangThai = "Đã bảo vệ đề cương";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật lịch bảo vệ thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: LichBaoVeDeCuong/Huy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var lich = await LayLich(id);
            var loi = KiemTraLichCoTheThaoTac(lich);
            if (loi != null)
            {
                TempData["ErrorMessage"] = loi;
                return RedirectToAction(nameof(Index));
            }

            lich!.TrangThai = "Đã hủy";

            // Đưa hồ sơ về Đạt xét duyệt để có thể lập lịch lại
            if (lich.HoSo != null && lich.HoSo.TrangThai == "Chờ bảo vệ đề cương")
            {
                lich.HoSo.TrangThai = "Đạt xét duyệt";
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Hủy lịch bảo vệ thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<HoSoDangKyDeTai?> LayHoSo(int maHoSo)
        {
            return await _context.HoSoDangKyDeTais
                .Include(h => h.SinhVien)
                .Include(h => h.DeTai)
                .Include(h => h.LichBaoVeDeCuongs)
                .FirstOrDefaultAsync(h => h.MaHoSo == maHoSo);
        }

        private async Task<LichBaoVeDeCuong?> LayLich(int id)
        {
            return await _context.LichBaoVeDeCuongs
                .Include(l => l.HoSo).ThenInclude(h => h!.SinhVien)
                .Include(l => l.HoSo).ThenInclude(h => h!.DeTai)
                .FirstOrDefaultAsync(l => l.MaLichBaoVeDeCuong == id);
        }

        private static void DienThongTinHoSo(LichFormViewModel model, HoSoDangKyDeTai? hoSo)
        {
            if (hoSo == null) return;
            model.MaHoSo = hoSo.MaHoSo;
            model.TenSinhVien = hoSo.SinhVien?.HoTen ?? "";
            model.TenDeTai = hoSo.DeTai?.TenDeTai ?? "";
        }

        // Chỉ hồ sơ Đạt xét duyệt và chưa có lịch hiệu lực mới được lập lịch
        private static string? KiemTraHoSoDuocLapLich(HoSoDangKyDeTai hoSo)
        {
            if (hoSo.TrangThai != "Đạt xét duyệt")
                return "Chỉ hồ sơ Đạt xét duyệt mới được lập lịch bảo vệ đề cương.";

            if (hoSo.LichBaoVeDeCuongs.Any(l => l.TrangThai != "Đã hủy"))
                return "Hồ sơ đã có lịch bảo vệ đề cương đang hiệu lực.";

            return null;
        }

        // Chỉ lịch "Đã lên lịch" mới được sửa / hoàn thành / hủy
        private static string? KiemTraLichCoTheThaoTac(LichBaoVeDeCuong? lich)
        {
            if (lich == null || lich.TrangThai == "Đã hủy")
                return "Lịch bảo vệ không hợp lệ để thực hiện thao tác này.";

            if (lich.TrangThai == "Đã hoàn thành")
                return "Lịch bảo vệ đã hoàn thành và không thể thay đổi.";

            return null;
        }

        private async Task KiemTraThoiGianVaTrungLich(LichFormViewModel model, int maSinhVien, int maLichBoQua)
        {
            model.GiangVienPhanBien = model.GiangVienPhanBien?.Trim();
            model.DiaDiemHoacLienKet = model.DiaDiemHoacLienKet?.Trim();

            if (string.IsNullOrEmpty(model.DiaDiemHoacLienKet))
            {
                ModelState.AddModelError(nameof(model.DiaDiemHoacLienKet), "Vui lòng chọn phòng bảo vệ.");
            }
            else if (!DanhMucBaoVe.LaPhongHopLe(model.DiaDiemHoacLienKet))
            {
                ModelState.AddModelError(nameof(model.DiaDiemHoacLienKet), "Phòng bảo vệ không hợp lệ.");
            }
            else
            {
                model.HinhThucBaoVe = DanhMucBaoVe.LaPhongOnline(model.DiaDiemHoacLienKet) ? "Trực tuyến" : "Trực tiếp";
            }

            if (!string.IsNullOrEmpty(model.GiangVienPhanBien) && !DanhMucBaoVe.GiangViens.Contains(model.GiangVienPhanBien))
            {
                ModelState.AddModelError(nameof(model.GiangVienPhanBien), "Giảng viên phản biện không hợp lệ.");
            }

            if (model.ThoiGianKetThuc <= model.ThoiGianBatDau)
            {
                ModelState.AddModelError(nameof(model.ThoiGianKetThuc), "Thời gian kết thúc phải sau thời gian bắt đầu.");
                return;
            }

            if (model.ThoiGianBatDau < DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.ThoiGianBatDau), "Thời gian bảo vệ không được nằm trong quá khứ.");
                return;
            }

            var batDau = model.ThoiGianBatDau;
            var ketThuc = model.ThoiGianKetThuc;
            var giangVien = model.GiangVienPhanBien ?? "";
            var phong = model.DiaDiemHoacLienKet ?? "";

            var lichConHieuLuc = _context.LichBaoVeDeCuongs
                .Where(l => l.TrangThai != "Đã hủy" && l.MaLichBaoVeDeCuong != maLichBoQua);

            var lichTrungKhoang = lichConHieuLuc
                .Where(l => l.ThoiGianBatDau < ketThuc && l.ThoiGianKetThuc > batDau);

            if (await lichTrungKhoang.AnyAsync(l => l.HoSo!.MaSinhVien == maSinhVien))
            {
                ModelState.AddModelError("", "Sinh viên đã có lịch bảo vệ trùng thời gian.");
            }
            if (giangVien != "" && await lichTrungKhoang.AnyAsync(l => l.GiangVienPhanBien == giangVien))
            {
                ModelState.AddModelError("", "Giảng viên phản biện đã có lịch trùng thời gian.");
            }
            if (phong != "" && await lichTrungKhoang.AnyAsync(l => l.DiaDiemHoacLienKet == phong))
            {
                ModelState.AddModelError("", "Phòng bảo vệ đã có lịch trùng thời gian.");
            }
        }
    }
}
