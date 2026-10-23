// Họ và tên: Nguyễn Thanh Hải
// Mã sinh viên: 23103100249
// Nội dung thực hiện: Áp dụng bản sửa có hỗ trợ AI cho POST Edit để tải tài khoản
// liên kết, đồng bộ HoTen và cập nhật Session sau khi lưu database thành công.
// Phạm vi đóng góp: Chỉnh sửa nêu trên; không nhận tác giả toàn bộ mã nền.
// Nguồn: Mã nền nhóm và bản tham khảo Module 3 có hỗ trợ AI.
// Kiểm thử đồng bộ tên sau sửa: chưa có xác nhận kết quả từ người thực hiện.
// Đối chiếu: DOI_CHIEU_DONG_GOP_HAI.md tại thư mục solution.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Filters;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models.ViewModels;

namespace QuanLyDeTaiNghienCuu.Controllers;

[AuthorizeRole(Roles.SinhVien)]
public class HoSoCaNhanController(ApplicationDbContext db) : Controller
{
    private int AccountId =>
        HttpContext.Session.GetMaTaiKhoan()!.Value;

    // Hiển thị hồ sơ của sinh viên đang đăng nhập.
    public async Task<IActionResult> Index()
    {
        var sinhVien = await db.SinhViens
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.MaTaiKhoan == AccountId);

        if (sinhVien == null)
        {
            TempData["ErrorMessage"] =
                "Tài khoản chưa được liên kết hồ sơ sinh viên. "
                + "Vui lòng liên hệ quản trị.";

            return RedirectToAction("Index", "DeTai");
        }

        return View(sinhVien);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var sinhVien = await db.SinhViens
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.MaTaiKhoan == AccountId);

        if (sinhVien == null)
            return NotFound();

        return View(SinhVienFormViewModel.From(sinhVien));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SinhVienFormViewModel model)
    {
        var sinhVien = await db.SinhViens
            .Include(s => s.TaiKhoan)
            .SingleOrDefaultAsync(s => s.MaTaiKhoan == AccountId);

        if (sinhVien == null || sinhVien.TaiKhoan == null)
            return NotFound();

        if (sinhVien.Version != model.Version)
        {
            ModelState.AddModelError(
                "",
                "Thông tin đã thay đổi. Tải lại trang để tránh ghi đè.");
        }

        if (!ModelState.IsValid)
            return View(model);

        model.Apply(sinhVien);

        sinhVien.TaiKhoan.HoTen = sinhVien.HoTen;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(
                "",
                "Dữ liệu vừa thay đổi. Hãy tải lại trang.");

            return View(model);
        }

        HttpContext.Session.SetString(
            SessionKeys.HoTen,
            sinhVien.HoTen);

        TempData["SuccessMessage"] = "Đã cập nhật hồ sơ cá nhân.";

        return RedirectToAction(nameof(Index));
    }
}