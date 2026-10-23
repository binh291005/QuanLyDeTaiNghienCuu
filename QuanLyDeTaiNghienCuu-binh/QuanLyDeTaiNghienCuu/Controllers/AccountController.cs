using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;

namespace QuanLyDeTaiNghienCuu.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.IsLoggedIn())
                return RedirectToAction("Index", "DeTai");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!Roles.All.Contains(model.VaiTro))
                ModelState.AddModelError(nameof(model.VaiTro), "Vai trò đăng nhập không hợp lệ");

            if (!ModelState.IsValid)
                return View(model);

            var tenDangNhap = model.TenDangNhap.Trim();

            // Kiểm tra tài khoản bằng EF Core + LINQ
            var taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại.");
                return View(model);
            }

            if (taiKhoan.MatKhau != model.MatKhau)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
                return View(model);
            }

            // Vai trò đã chọn trên form phải trùng với vai trò thật của tài khoản
            if (taiKhoan.VaiTro != model.VaiTro)
            {
                ModelState.AddModelError(string.Empty,
                    $"Tài khoản này không phải tài khoản {Roles.ToDisplay(model.VaiTro)}. Vui lòng chọn đúng vai trò đăng nhập.");
                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            // Đăng nhập thành công: làm sạch Session cũ rồi lưu thông tin mới
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32(SessionKeys.MaTaiKhoan, taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString(SessionKeys.HoTen, taiKhoan.HoTen);
            HttpContext.Session.SetString(SessionKeys.VaiTro, taiKhoan.VaiTro);

            TempData["SuccessMessage"] = $"Xin chào {taiKhoan.HoTen}, bạn đã đăng nhập với vai trò {Roles.ToDisplay(taiKhoan.VaiTro)}.";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);

            return RedirectToAction("Index", "DeTai");
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.IsLoggedIn())
                return RedirectToAction("Index", "DeTai");

            return View(new RegisterViewModel());
        }

        // POST: Account/Register  (chỉ cho phép tự đăng ký tài khoản Sinh viên)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var tenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;

            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại");

            if (!ModelState.IsValid)
                return View(model);

            // Vai trò luôn là Sinh viên, không nhận từ form để tránh tự nâng quyền
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = model.MatKhau,
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                VaiTro = Roles.SinhVien,
                TrangThai = true
            };

            // Tạo luôn hồ sơ sinh viên gắn với tài khoản (Module 3 sẽ bổ sung thông tin chi tiết)
            var sinhVien = new SinhVien
            {
                TaiKhoan = taiKhoan,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                TrangThai = true
            };

            _context.SinhViens.Add(sinhVien);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập bằng tài khoản vừa tạo.";
            return RedirectToAction(nameof(Login));
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            TempData["SuccessMessage"] = "Bạn đã đăng xuất khỏi hệ thống.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}
