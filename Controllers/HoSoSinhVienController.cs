// Nguồn mã: Mã nền nhóm và/hoặc bản tham khảo Module 3 có hỗ trợ AI.
// Chức năng trong file: Điều phối danh sách, chi tiết, nộp và hủy hồ sơ chính chủ.
// Thông tin người trực tiếp thực hiện: Chưa xác nhận; không gán tác giả cá nhân.
// Giữ nguyên thông tin tác giả mã nền khi hợp nhất với repository nhóm.
// Đối chiếu: DOI_CHIEU_DONG_GOP_HAI.md tại thư mục solution.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Filters;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;
using QuanLyDeTaiNghienCuu.Services;
namespace QuanLyDeTaiNghienCuu.Controllers;

[AuthorizeRole(Roles.SinhVien)]
public class HoSoSinhVienController(ApplicationDbContext db,HoSoSinhVienService service) : Controller
{
    private int AccountId=>HttpContext.Session.GetMaTaiKhoan()!.Value;
    private IQueryable<HoSoDangKyDeTai> Own()=>db.HoSoDangKyDeTais.Where(h=>h.SinhVien!.MaTaiKhoan==AccountId);
    public async Task<IActionResult> Index(string? status,int page=1) {
        var query=Own().AsNoTracking();if(Module3Rules.AllStatuses.Contains(status))query=query.Where(h=>h.TrangThai==status);else status=null;
        var pages=Math.Max(1,(int)Math.Ceiling(await query.CountAsync()/10d));page=Math.Clamp(page,1,pages);
        return View(new HoSoIndexViewModel{Items=await query.Include(h=>h.DeTai)!.ThenInclude(d=>d!.BoMon).OrderByDescending(h=>h.NgayNop).ThenByDescending(h=>h.MaHoSo).Skip((page-1)*10).Take(10).ToListAsync(),Status=status,Page=page,TotalPages=pages});
    }
    public async Task<IActionResult> Details(int id) {
        var h=await Own().AsNoTracking().Include(h=>h.DeTai)!.ThenInclude(d=>d!.BoMon).Include(h=>h.LichBaoVeDeCuongs).Include(h=>h.KetQuaGiaoDeTai).SingleOrDefaultAsync(h=>h.MaHoSo==id);
        return h==null?NotFound():View(h);
    }
    public async Task<IActionResult> Create(int maDeTai) {var d=await db.DeTais.AsNoTracking().Include(d=>d.BoMon).SingleOrDefaultAsync(d=>d.MaDeTai==maDeTai);
        return d==null?NotFound():View(new NopHoSoViewModel{MaDeTai=maDeTai,DeTai=d});}
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NopHoSoViewModel model) {
        if(ModelState.IsValid) {
            try {var result=await service.SubmitAsync(AccountId,model);if(result.Success){TempData["SuccessMessage"]=result.Message;return RedirectToAction(nameof(Details),new{id=result.Id});}
                ModelState.AddModelError("",result.Message);
            }catch(Exception ex) when(IsConflict(ex)) { ModelState.AddModelError("","Dữ liệu vừa thay đổi hoặc có yêu cầu nộp trùng. Hãy kiểm tra danh sách hồ sơ rồi thử lại."); }
        }
        model.DeTai=await db.DeTais.AsNoTracking().Include(d=>d.BoMon).SingleOrDefaultAsync(d=>d.MaDeTai==model.MaDeTai);
        return View(model);
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id,Guid version) {
        try {var result=await service.CancelAsync(AccountId,id,version);if(result.NotFound)return NotFound();TempData[result.Success?"SuccessMessage":"ErrorMessage"]=result.Message;}
        catch(Exception ex) when(IsConflict(ex)){TempData["ErrorMessage"]="Hồ sơ vừa được xử lý. Hãy tải lại và kiểm tra trạng thái.";}
        return RedirectToAction(nameof(Details),new{id});
    }
    private static bool IsConflict(Exception e) => e is DbUpdateConcurrencyException
        || e is SqlException sql && sql.Number is 1205 or 2601 or 2627
        || e is SqliteException lite && lite.SqliteErrorCode is 5 or 6 or 19
        || e.InnerException!=null && IsConflict(e.InnerException);
}
