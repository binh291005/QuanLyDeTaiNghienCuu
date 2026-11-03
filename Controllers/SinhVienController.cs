// Nguồn mã: Mã nền nhóm và/hoặc bản tham khảo Module 3 có hỗ trợ AI.
// Chức năng trong file: Danh sách/chi tiết sinh viên; Admin thêm, sửa, xóa có kiểm soát.
// Thông tin người trực tiếp thực hiện: Chưa xác nhận; không gán tác giả cá nhân.
// Giữ nguyên thông tin tác giả mã nền khi hợp nhất với repository nhóm.
// Đối chiếu: DOI_CHIEU_DONG_GOP_HAI.md tại thư mục solution.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Filters;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;
namespace QuanLyDeTaiNghienCuu.Controllers;

[AuthorizeRole(Roles.Admin,Roles.GiangVien)]
public class SinhVienController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? q,int page=1) {
        q=q?.Trim();var query=db.SinhViens.AsNoTracking();if(!string.IsNullOrEmpty(q))query=query.Where(s=>s.HoTen.Contains(q)||s.Email!=null&&s.Email.Contains(q));
        var pages=Math.Max(1,(int)Math.Ceiling(await query.CountAsync()/10d));page=Math.Clamp(page,1,pages);
        return View(new SinhVienIndexViewModel{Items=await query.OrderBy(s=>s.HoTen).ThenBy(s=>s.MaSinhVien).Skip((page-1)*10).Take(10).ToListAsync(),Q=q,Page=page,TotalPages=pages});
    }
    public async Task<IActionResult> Details(int id) {var s=await db.SinhViens.AsNoTracking().Include(s=>s.TaiKhoan).SingleOrDefaultAsync(s=>s.MaSinhVien==id);return s==null?NotFound():View(s);}
    [AuthorizeRole(Roles.Admin)]
    public async Task<IActionResult> Create(){var m=new SinhVienAdminFormViewModel();await Accounts(m);return View(m);}
    [HttpPost,ValidateAntiForgeryToken,AuthorizeRole(Roles.Admin)]
    public async Task<IActionResult> Create(SinhVienAdminFormViewModel model){await CheckAccount(model,0);if(ModelState.IsValid){var s=new SinhVien{MaTaiKhoan=model.MaTaiKhoan,TrangThai=model.TrangThai};model.Apply(s);db.Add(s);
        try{await db.SaveChangesAsync();TempData["SuccessMessage"]="Đã thêm sinh viên.";return RedirectToAction(nameof(Details),new{id=s.MaSinhVien});}catch(DbUpdateException){ModelState.AddModelError("","Tài khoản đã được liên kết hoặc dữ liệu không còn hợp lệ.");}}
        await Accounts(model);return View(model);}
    [AuthorizeRole(Roles.Admin)]
    public async Task<IActionResult> Edit(int id){var s=await db.SinhViens.FindAsync(id);if(s==null)return NotFound();var m=SinhVienAdminFormViewModel.FromAdmin(s);await Accounts(m,id);return View(m);}
    [HttpPost,ValidateAntiForgeryToken,AuthorizeRole(Roles.Admin)]
    public async Task<IActionResult> Edit(int id,SinhVienAdminFormViewModel model){var s=await db.SinhViens.FindAsync(id);if(s==null)return NotFound();await CheckAccount(model,id);
        if(s.Version!=model.Version)ModelState.AddModelError("","Dữ liệu đã thay đổi. Tải lại trang trước khi lưu.");
        if(ModelState.IsValid){model.Apply(s);s.TrangThai=model.TrangThai;s.MaTaiKhoan=model.MaTaiKhoan;
            try{await db.SaveChangesAsync();TempData["SuccessMessage"]="Đã cập nhật sinh viên.";return RedirectToAction(nameof(Details),new{id});}catch(DbUpdateException){ModelState.AddModelError("","Không thể lưu: dữ liệu vừa thay đổi hoặc tài khoản đã được liên kết.");}}
        await Accounts(model,id);return View(model);}
    [AuthorizeRole(Roles.Admin)]
    public async Task<IActionResult> Delete(int id){var s=await db.SinhViens.AsNoTracking().SingleOrDefaultAsync(s=>s.MaSinhVien==id);return s==null?NotFound():View(s);}
    [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken,AuthorizeRole(Roles.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id,Guid version){var s=await db.SinhViens.FindAsync(id);if(s==null)return NotFound();
        if(s.Version!=version){TempData["ErrorMessage"]="Dữ liệu đã thay đổi. Hãy kiểm tra lại.";return RedirectToAction(nameof(Details),new{id});}
        if(await db.HoSoDangKyDeTais.AnyAsync(h=>h.MaSinhVien==id)){TempData["ErrorMessage"]="Không được xóa sinh viên đã có hồ sơ. Có thể ngừng hoạt động để giữ lịch sử.";return RedirectToAction(nameof(Details),new{id});}
        db.Remove(s);try{await db.SaveChangesAsync();TempData["SuccessMessage"]="Đã xóa hồ sơ sinh viên; tài khoản vẫn được giữ.";}catch(DbUpdateException){TempData["ErrorMessage"]="Không thể xóa vì dữ liệu vừa thay đổi hoặc có hồ sơ liên quan.";}
        return RedirectToAction(nameof(Index));}
    private async Task CheckAccount(SinhVienAdminFormViewModel m,int id){if(m.MaTaiKhoan.HasValue && (!await db.TaiKhoans.AnyAsync(t=>t.MaTaiKhoan==m.MaTaiKhoan && t.VaiTro==Roles.SinhVien) || await db.SinhViens.AnyAsync(s=>s.MaTaiKhoan==m.MaTaiKhoan && s.MaSinhVien!=id)))ModelState.AddModelError(nameof(m.MaTaiKhoan),"Chỉ chọn tài khoản Sinh viên chưa liên kết hồ sơ khác.");}
    private async Task Accounts(SinhVienAdminFormViewModel m,int id=0){m.Accounts=await db.TaiKhoans.AsNoTracking().Where(t=>t.VaiTro==Roles.SinhVien && !db.SinhViens.Any(s=>s.MaTaiKhoan==t.MaTaiKhoan && s.MaSinhVien!=id)).OrderBy(t=>t.TenDangNhap).Select(t=>new SelectListItem(t.TenDangNhap+" — "+t.HoTen,t.MaTaiKhoan.ToString())).ToListAsync();}
}
