using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Helpers;

namespace QuanLyDeTaiNghienCuu.Filters
{
    /// <summary>
    /// Kiểm tra đăng nhập + vai trò ngay tại Controller (không chỉ ẩn menu trên View).
    /// Cách dùng:
    ///   [AuthorizeRole]                                  -> chỉ cần đã đăng nhập
    ///   [AuthorizeRole(Roles.Admin)]                     -> chỉ Admin
    ///   [AuthorizeRole(Roles.Admin, Roles.GiangVien)]    -> Admin hoặc Giảng viên
    /// Đặt trên class (áp cho mọi action) hoặc trên từng action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var session = http.Session;
            var tempData = (context.Controller as Controller)?.TempData;

            // 1. Chưa đăng nhập
            var maTaiKhoan = session.GetMaTaiKhoan();
            if (maTaiKhoan == null)
            {
                if (tempData != null)
                    tempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục.";
                context.Result = RedirectToLogin(context);
                return;
            }

            // 2. Đọc lại trạng thái/vai trò mới nhất trong DB để khóa tài khoản có hiệu lực ngay
            var db = http.RequestServices.GetRequiredService<ApplicationDbContext>();
            var tk = await db.TaiKhoans.AsNoTracking()
                .Where(t => t.MaTaiKhoan == maTaiKhoan.Value)
                .Select(t => new { t.TrangThai, t.VaiTro })
                .FirstOrDefaultAsync();

            if (tk == null || !tk.TrangThai)
            {
                session.Clear();
                if (tempData != null)
                    tempData["ErrorMessage"] = "Tài khoản không còn hợp lệ hoặc đã bị khóa. Vui lòng đăng nhập lại.";
                context.Result = RedirectToLogin(context);
                return;
            }

            // Đồng bộ vai trò nếu Admin vừa đổi quyền
            if (session.GetVaiTro() != tk.VaiTro)
                session.SetString(SessionKeys.VaiTro, tk.VaiTro);

            // 3. Sai vai trò
            if (_roles.Length > 0 && !_roles.Contains(tk.VaiTro))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            await next();
        }

        private static IActionResult RedirectToLogin(ActionExecutingContext context)
        {
            // Chỉ giữ returnUrl với request GET (POST không quay lại được)
            var req = context.HttpContext.Request;
            string? returnUrl = HttpMethods.IsGet(req.Method) ? req.Path + req.QueryString : null;
            return new RedirectToActionResult("Login", "Account", new { returnUrl });
        }
    }
}
