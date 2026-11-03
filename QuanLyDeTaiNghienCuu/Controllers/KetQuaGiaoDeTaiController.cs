// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - Ghi nhận kết quả giao đề tài, kiểm soát số nhóm tối đa,
// đóng đề tài khi đủ số nhóm.
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
    public class KetQuaGiaoDeTaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KetQuaGiaoDeTaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KetQuaGiaoDeTai
        public async Task<IActionResult> Index()
        {
            var viewModel = new KetQuaIndexViewModel();

            // Hồ sơ đã bảo vệ đề cương, đang chờ nhập kết quả
            viewModel.HoSoChoNhapKetQua = await _context.HoSoDangKyDeTais
                .Include(h => h.SinhVien)
                .Include(h => h.DeTai)
                .Where(h => h.TrangThai == "Đã bảo vệ đề cương")
                .OrderBy(h => h.NgayNop)
                .ToListAsync();

            // Các kết quả đã ghi nhận
            viewModel.DanhSachKetQua = await _context.KetQuaGiaoDeTais
                .Include(k => k.HoSo).ThenInclude(h => h!.SinhVien)
                .Include(k => k.HoSo).ThenInclude(h => h!.DeTai)
                .OrderByDescending(k => k.NgayCapNhat)
                .ToListAsync();

            // Đề tài đã giao đủ số nhóm tối đa nhưng chưa đóng -> cảnh báo
            viewModel.DeTaiDayNhom = await _context.DeTais
                .Where(d => d.TrangThai != "Đã đóng"
                         && d.HoSoDangKyDeTais.Count(h => h.TrangThai == "Được giao đề tài") >= d.SoNhomToiDa)
                .Select(d => new DeTaiDayNhomItem
                {
                    MaDeTai = d.MaDeTai,
                    TenDeTai = d.TenDeTai,
                    SoNhomToiDa = d.SoNhomToiDa,
                    SoDaGiao = d.HoSoDangKyDeTais.Count(h => h.TrangThai == "Được giao đề tài")
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: KetQuaGiaoDeTai/Create?maHoSo=5
        public async Task<IActionResult> Create(int? maHoSo)
        {
            if (maHoSo == null) return NotFound();

            var hoSo = await LayHoSo(maHoSo.Value);
            if (hoSo == null) return NotFound();

            var loi = KiemTraHoSoDuocNhapKetQua(hoSo);
            if (loi != null)
            {
                TempData["ErrorMessage"] = loi;
                return RedirectToAction(nameof(Index));
            }

            var model = new KetQuaFormViewModel();
            await DienThongTinHoSo(model, hoSo);
            return View(model);
        }

        // POST: KetQuaGiaoDeTai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KetQuaFormViewModel model)
        {
            var hoSo = await LayHoSo(model.MaHoSo);
            if (hoSo == null) return NotFound();

            var loi = KiemTraHoSoDuocNhapKetQua(hoSo);
            if (loi != null)
            {
                TempData["ErrorMessage"] = loi;
                return RedirectToAction(nameof(Index));
            }

            if (model.KetQua != null && model.KetQua != "Được giao đề tài" && model.KetQua != "Không được giao đề tài")
            {
                ModelState.AddModelError(nameof(model.KetQua), "Kết quả không hợp lệ.");
            }

            // Đếm số hồ sơ đã được giao của đề tài, so với số nhóm tối đa
            int soDaGiao = await DemSoDaGiao(hoSo.MaDeTai);
            if (model.KetQua == "Được giao đề tài" && soDaGiao >= hoSo.DeTai!.SoNhomToiDa)
            {
                ModelState.AddModelError("", "Đề tài đã đạt số nhóm tối đa. Không thể giao thêm.");
            }

            if (ModelState.IsValid)
            {
                var ketQua = new KetQuaGiaoDeTai
                {
                    MaHoSo = hoSo.MaHoSo,
                    DiemDanhGia = model.DiemDanhGia,
                    NhanXet = model.NhanXet,
                    KetQua = model.KetQua!,
                    NgayCapNhat = DateTime.Now
                };
                _context.KetQuaGiaoDeTais.Add(ketQua);

                // Trạng thái hồ sơ = kết quả (Được giao đề tài / Không được giao đề tài)
                hoSo.TrangThai = model.KetQua!;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Không thể lưu kết quả giao đề tài do lỗi hệ thống.");
                    await DienThongTinHoSo(model, hoSo);
                    return View(model);
                }

                // Kiểm tra lại số nhóm đã giao sau khi lưu
                int soSauKhiLuu = await DemSoDaGiao(hoSo.MaDeTai);
                if (soSauKhiLuu >= hoSo.DeTai!.SoNhomToiDa)
                {
                    TempData["SuccessMessage"] = "Ghi nhận kết quả giao đề tài thành công. "
                        + $"Cảnh báo: đề tài đã giao đủ {soSauKhiLuu}/{hoSo.DeTai.SoNhomToiDa} nhóm, bạn có thể đóng đề tài.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Ghi nhận kết quả giao đề tài thành công.";
                }
                return RedirectToAction(nameof(Index));
            }

            await DienThongTinHoSo(model, hoSo);
            return View(model);
        }

        // POST: KetQuaGiaoDeTai/DongDeTai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DongDeTai(int maDeTai)
        {
            var deTai = await _context.DeTais.FindAsync(maDeTai);
            if (deTai == null) return NotFound();

            int soDaGiao = await DemSoDaGiao(maDeTai);
            if (soDaGiao < deTai.SoNhomToiDa)
            {
                TempData["ErrorMessage"] = "Đề tài chưa giao đủ số nhóm tối đa nên chưa cần đóng tại đây.";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ đổi trạng thái đề tài, giữ nguyên toàn bộ hồ sơ còn lại
            deTai.TrangThai = "Đã đóng";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đóng đề tài thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<HoSoDangKyDeTai?> LayHoSo(int maHoSo)
        {
            return await _context.HoSoDangKyDeTais
                .Include(h => h.SinhVien)
                .Include(h => h.DeTai)
                .Include(h => h.KetQuaGiaoDeTai)
                .FirstOrDefaultAsync(h => h.MaHoSo == maHoSo);
        }

        private static string? KiemTraHoSoDuocNhapKetQua(HoSoDangKyDeTai hoSo)
        {
            if (hoSo.KetQuaGiaoDeTai != null)
                return "Hồ sơ đã có kết quả cuối cùng, không thể thay đổi tùy ý.";

            if (hoSo.TrangThai != "Đã bảo vệ đề cương")
                return "Chỉ hồ sơ Đã bảo vệ đề cương mới được ghi nhận kết quả cuối cùng.";

            return null;
        }

        // Đếm số hồ sơ "Được giao đề tài" của một đề tài
        private async Task<int> DemSoDaGiao(int maDeTai)
        {
            return await _context.HoSoDangKyDeTais
                .CountAsync(h => h.MaDeTai == maDeTai && h.TrangThai == "Được giao đề tài");
        }

        private async Task DienThongTinHoSo(KetQuaFormViewModel model, HoSoDangKyDeTai hoSo)
        {
            model.MaHoSo = hoSo.MaHoSo;
            model.TenSinhVien = hoSo.SinhVien?.HoTen ?? "";
            model.TenDeTai = hoSo.DeTai?.TenDeTai ?? "";
            model.SoNhomToiDa = hoSo.DeTai?.SoNhomToiDa ?? 0;
            model.SoDaGiao = await DemSoDaGiao(hoSo.MaDeTai);
        }
    }
}
