using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Filters;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;

namespace QuanLyDeTaiNghienCuu.Controllers
{
    [AuthorizeRole(Roles.Admin)]
    public class BoMonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoMonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BoMon
        public async Task<IActionResult> Index(string? searchTerm, string? trangThai, int page = 1)
        {
            var vm = new BoMonIndexViewModel { SearchTerm = searchTerm, TrangThai = trangThai };

            var query = _context.BoMons.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(b => b.TenBoMon.ToLower().Contains(term)
                                      || (b.EmailLienHe != null && b.EmailLienHe.ToLower().Contains(term)));
            }

            if (trangThai == "active") query = query.Where(b => b.TrangThai);
            else if (trangThai == "inactive") query = query.Where(b => !b.TrangThai);

            vm.TotalItems = await query.CountAsync();
            vm.Page = Math.Clamp(page, 1, Math.Max(1, vm.TotalPages));

            vm.Items = await query
                .OrderBy(b => b.TenBoMon)
                .Skip((vm.Page - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .Select(b => new BoMonRowViewModel
                {
                    MaBoMon = b.MaBoMon,
                    TenBoMon = b.TenBoMon,
                    MoTa = b.MoTa,
                    EmailLienHe = b.EmailLienHe,
                    TrangThai = b.TrangThai,
                    SoDeTai = b.DeTais.Count
                })
                .ToListAsync();

            return View(vm);
        }

        // GET: BoMon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var boMon = await _context.BoMons
                .Include(b => b.DeTais)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.MaBoMon == id);

            if (boMon == null) return NotFound();
            return View(boMon);
        }

        // GET: BoMon/Create
        public IActionResult Create() => View(new BoMon());

        // POST: BoMon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenBoMon,MoTa,EmailLienHe,TrangThai")] BoMon boMon)
        {
            Normalize(boMon);

            if (await TenBoMonExistsAsync(boMon.TenBoMon, null))
                ModelState.AddModelError(nameof(boMon.TenBoMon), "Tên bộ môn đã tồn tại");

            if (!ModelState.IsValid) return View(boMon);

            _context.BoMons.Add(boMon);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thêm bộ môn '{boMon.TenBoMon}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: BoMon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var boMon = await _context.BoMons.FindAsync(id);
            if (boMon == null) return NotFound();
            return View(boMon);
        }

        // POST: BoMon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBoMon,TenBoMon,MoTa,EmailLienHe,TrangThai")] BoMon input)
        {
            if (id != input.MaBoMon) return NotFound();

            var boMon = await _context.BoMons.FindAsync(id);
            if (boMon == null) return NotFound();

            Normalize(input);

            if (await TenBoMonExistsAsync(input.TenBoMon, id))
                ModelState.AddModelError(nameof(input.TenBoMon), "Tên bộ môn đã tồn tại");

            if (!ModelState.IsValid) return View(input);

            boMon.TenBoMon = input.TenBoMon;
            boMon.MoTa = input.MoTa;
            boMon.EmailLienHe = input.EmailLienHe;
            boMon.TrangThai = input.TrangThai;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật bộ môn '{boMon.TenBoMon}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: BoMon/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var boMon = await _context.BoMons.FindAsync(id);
            if (boMon == null) return NotFound();

            boMon.TrangThai = !boMon.TrangThai;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = boMon.TrangThai
                ? $"Bộ môn '{boMon.TenBoMon}' đã chuyển sang trạng thái Hoạt động."
                : $"Bộ môn '{boMon.TenBoMon}' đã chuyển sang trạng thái Tạm dừng.";
            return RedirectToAction(nameof(Index));
        }

        // GET: BoMon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var boMon = await _context.BoMons
                .Include(b => b.DeTais)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.MaBoMon == id);
            if (boMon == null) return NotFound();

            return View(boMon);
        }

        // POST: BoMon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var boMon = await _context.BoMons.Include(b => b.DeTais).FirstOrDefaultAsync(b => b.MaBoMon == id);
            if (boMon == null) return NotFound();

            // Không xóa bộ môn nếu còn đề tài liên quan (tránh làm đề tài mất hợp lệ)
            if (boMon.DeTais.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa bộ môn '{boMon.TenBoMon}' vì đang có {boMon.DeTais.Count} đề tài liên quan. Hãy chuyển trạng thái sang 'Tạm dừng' thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }

            _context.BoMons.Remove(boMon);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa bộ môn '{boMon.TenBoMon}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------
        private static void Normalize(BoMon b)
        {
            b.TenBoMon = b.TenBoMon?.Trim() ?? string.Empty;
            b.MoTa = string.IsNullOrWhiteSpace(b.MoTa) ? null : b.MoTa.Trim();
            b.EmailLienHe = string.IsNullOrWhiteSpace(b.EmailLienHe) ? null : b.EmailLienHe.Trim();
        }

        private Task<bool> TenBoMonExistsAsync(string ten, int? excludeId)
        {
            var lower = ten.ToLower();
            return _context.BoMons.AnyAsync(b => b.TenBoMon.ToLower() == lower && (excludeId == null || b.MaBoMon != excludeId));
        }
    }
}
