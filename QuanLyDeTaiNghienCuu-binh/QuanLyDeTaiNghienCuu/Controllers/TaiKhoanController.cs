using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Filters;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;

namespace QuanLyDeTaiNghienCuu.Controllers
{
    [AuthorizeRole(Roles.Admin)]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaiKhoan
        public async Task<IActionResult> Index(string? searchTerm, string? vaiTro, string? trangThai, int page = 1)
        {
            var vm = new TaiKhoanIndexViewModel
            {
                SearchTerm = searchTerm,
                VaiTro = vaiTro,
                TrangThai = trangThai
            };

            var query = _context.TaiKhoans.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(t => t.TenDangNhap.ToLower().Contains(term)
                                      || t.HoTen.ToLower().Contains(term)
                                      || t.Email.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(vaiTro))
                query = query.Where(t => t.VaiTro == vaiTro);

            if (trangThai == "active") query = query.Where(t => t.TrangThai);
            else if (trangThai == "locked") query = query.Where(t => !t.TrangThai);

            vm.TotalItems = await query.CountAsync();
            vm.Page = Math.Clamp(page, 1, Math.Max(1, vm.TotalPages));

            vm.Items = await query
                .OrderBy(t => t.VaiTro).ThenBy(t => t.TenDangNhap)
                .Skip((vm.Page - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .ToListAsync();

            ViewBag.RoleList = RoleSelectList(vaiTro);
            return View(vm);
        }

        // GET: TaiKhoan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.SinhVien)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        // GET: TaiKhoan/Create
        public IActionResult Create()
        {
            ViewBag.RoleList = RoleSelectList(Roles.GiangVien);
            return View(new TaiKhoanFormViewModel());
        }

        // POST: TaiKhoan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoanFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MatKhau))
                ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu không được để trống");

            if (!Roles.All.Contains(model.VaiTro))
                ModelState.AddModelError(nameof(model.VaiTro), "Vai trò không hợp lệ");

            var tenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại");

            if (!ModelState.IsValid)
            {
                ViewBag.RoleList = RoleSelectList(model.VaiTro);
                return View(model);
            }

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = model.MatKhau!,
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                VaiTro = model.VaiTro,
                TrangThai = model.TrangThai
            };

            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã tạo tài khoản '{taiKhoan.TenDangNhap}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: TaiKhoan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            var model = new TaiKhoanFormViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                VaiTro = taiKhoan.VaiTro,
                TrangThai = taiKhoan.TrangThai
            };

            ViewBag.RoleList = RoleSelectList(model.VaiTro);
            return View(model);
        }

        // POST: TaiKhoan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoanFormViewModel model)
        {
            if (id != model.MaTaiKhoan) return NotFound();

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            if (!Roles.All.Contains(model.VaiTro))
                ModelState.AddModelError(nameof(model.VaiTro), "Vai trò không hợp lệ");

            var tenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap && t.MaTaiKhoan != id))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại");

            // Không cho tự khóa / tự hạ quyền chính mình, và luôn phải còn ít nhất 1 Admin hoạt động
            var isSelf = HttpContext.Session.GetMaTaiKhoan() == id;
            var losingAdmin = taiKhoan.VaiTro == Roles.Admin && (model.VaiTro != Roles.Admin || !model.TrangThai);
            if (losingAdmin)
            {
                if (isSelf)
                    ModelState.AddModelError(string.Empty, "Bạn không thể tự khóa hoặc tự hạ quyền tài khoản Admin của chính mình.");
                else if (!await HasOtherActiveAdminAsync(id))
                    ModelState.AddModelError(string.Empty, "Hệ thống phải còn ít nhất một tài khoản Admin đang hoạt động.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RoleList = RoleSelectList(model.VaiTro);
                return View(model);
            }

            taiKhoan.TenDangNhap = tenDangNhap;
            taiKhoan.HoTen = model.HoTen.Trim();
            taiKhoan.Email = model.Email.Trim();
            taiKhoan.VaiTro = model.VaiTro;
            taiKhoan.TrangThai = model.TrangThai;
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
                taiKhoan.MatKhau = model.MatKhau;

            await _context.SaveChangesAsync();

            // Nếu đang sửa chính mình thì cập nhật lại Session
            if (isSelf)
            {
                HttpContext.Session.SetString(SessionKeys.HoTen, taiKhoan.HoTen);
                HttpContext.Session.SetString(SessionKeys.VaiTro, taiKhoan.VaiTro);
            }

            TempData["SuccessMessage"] = $"Đã cập nhật tài khoản '{taiKhoan.TenDangNhap}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: TaiKhoan/ToggleStatus/5  (khóa / mở khóa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            if (taiKhoan.TrangThai) // đang hoạt động -> sắp khóa
            {
                if (HttpContext.Session.GetMaTaiKhoan() == id)
                {
                    TempData["ErrorMessage"] = "Bạn không thể tự khóa tài khoản của chính mình.";
                    return RedirectToAction(nameof(Index));
                }
                if (taiKhoan.VaiTro == Roles.Admin && !await HasOtherActiveAdminAsync(id))
                {
                    TempData["ErrorMessage"] = "Không thể khóa Admin cuối cùng đang hoạt động của hệ thống.";
                    return RedirectToAction(nameof(Index));
                }
            }

            taiKhoan.TrangThai = !taiKhoan.TrangThai;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = taiKhoan.TrangThai
                ? $"Đã mở khóa tài khoản '{taiKhoan.TenDangNhap}'."
                : $"Đã khóa tài khoản '{taiKhoan.TenDangNhap}'.";
            return RedirectToAction(nameof(Index));
        }

        // GET: TaiKhoan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.SinhVien)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            ViewBag.DeleteBlockedReason = await GetDeleteBlockedReasonAsync(taiKhoan);
            return View(taiKhoan);
        }

        // POST: TaiKhoan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.SinhVien)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan == null) return NotFound();

            var reason = await GetDeleteBlockedReasonAsync(taiKhoan);
            if (reason != null)
            {
                TempData["ErrorMessage"] = reason;
                return RedirectToAction(nameof(Index));
            }

            _context.TaiKhoans.Remove(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa tài khoản '{taiKhoan.TenDangNhap}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------
        private Task<bool> HasOtherActiveAdminAsync(int excludeId) =>
            _context.TaiKhoans.AnyAsync(t => t.VaiTro == Roles.Admin && t.TrangThai && t.MaTaiKhoan != excludeId);

        private async Task<string?> GetDeleteBlockedReasonAsync(TaiKhoan taiKhoan)
        {
            if (HttpContext.Session.GetMaTaiKhoan() == taiKhoan.MaTaiKhoan)
                return "Bạn không thể xóa tài khoản đang đăng nhập.";

            if (taiKhoan.VaiTro == Roles.Admin && !await HasOtherActiveAdminAsync(taiKhoan.MaTaiKhoan))
                return "Không thể xóa Admin cuối cùng đang hoạt động của hệ thống.";

            if (taiKhoan.SinhVien != null)
                return $"Tài khoản đang gắn với hồ sơ sinh viên '{taiKhoan.SinhVien.HoTen}'. Hãy khóa tài khoản thay vì xóa để bảo toàn dữ liệu.";

            return null;
        }

        private static List<SelectListItem> RoleSelectList(string? selected) =>
            Roles.All.Select(r => new SelectListItem(Roles.ToDisplay(r), r, r == selected)).ToList();
    }
}
