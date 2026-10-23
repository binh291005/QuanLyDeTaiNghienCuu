// Nguồn mã: Mã nền nhóm và/hoặc bản tham khảo Module 3 có hỗ trợ AI.
// Chức năng trong file: Kiểm tra và lưu nghiệp vụ nộp/hủy hồ sơ trong transaction.
// Thông tin người trực tiếp thực hiện: Chưa xác nhận; không gán tác giả cá nhân.
// Giữ nguyên thông tin tác giả mã nền khi hợp nhất với repository nhóm.
// Đối chiếu: DOI_CHIEU_DONG_GOP_HAI.md tại thư mục solution.
using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Helpers;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;
namespace QuanLyDeTaiNghienCuu.Services;

public record HoSoOperation(bool Success, string Message, int? Id=null, bool NotFound=false);
public class HoSoSinhVienService(ApplicationDbContext db, TimeProvider clock)
{
    public async Task<HoSoOperation> SubmitAsync(int accountId, NopHoSoViewModel form)
    {
        // SERIALIZABLE giữ kiểm tra và ghi cùng transaction; filtered unique index là hàng rào cuối chống nộp đồng thời.
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var sv=await db.SinhViens.Include(s=>s.TaiKhoan).SingleOrDefaultAsync(s=>s.MaTaiKhoan==accountId);
        if(sv==null || !sv.TrangThai || sv.TaiKhoan is not { TrangThai:true, VaiTro:Roles.SinhVien })
            return new(false,"Sinh viên không tồn tại, bị ngừng hoạt động hoặc tài khoản không hợp lệ.");
        if(!Module3Rules.ProfileComplete(sv)) return new(false,"Vui lòng khai báo đầy đủ thông tin cá nhân bắt buộc.");
        var topic=await db.DeTais.SingleOrDefaultAsync(t=>t.MaDeTai==form.MaDeTai);
        var now=Module3Rules.VietnamNow(clock);
        if(topic==null || topic.TrangThai!="Đang mở đăng ký" || now.Date<topic.NgayBatDauDangKy.Date || now.Date>topic.HanDangKy.Date)
            return new(false,"Đề tài không tồn tại, chưa mở, tạm dừng, đã đóng hoặc ngoài thời hạn nhận hồ sơ.");
        if(await db.HoSoDangKyDeTais.AnyAsync(h=>h.MaSinhVien==sv.MaSinhVien && h.MaDeTai==form.MaDeTai && Module3Rules.Processing.Contains(h.TrangThai)))
            return new(false,"Bạn đã có hồ sơ đang xử lý cho đề tài này.");
        if(string.IsNullOrWhiteSpace(form.DeCuongSoBo) || form.DeCuongSoBo.Length>10000 || form.GhiChu?.Length>2000)
            return new(false,"Đề cương sơ bộ hoặc ghi chú không hợp lệ.");
        var h=new HoSoDangKyDeTai {MaSinhVien=sv.MaSinhVien,MaDeTai=topic.MaDeTai,NgayNop=now,
            DeCuongSoBo=form.DeCuongSoBo.Trim(),GhiChu=form.GhiChu?.Trim(),TrangThai=Module3Rules.Pending};
        db.Add(h);await db.SaveChangesAsync();await tx.CommitAsync();
        return new(true,"Nộp hồ sơ thành công. Hồ sơ đang chờ xét duyệt.",h.MaHoSo);
    }
    public async Task<HoSoOperation> CancelAsync(int accountId,int id,Guid version)
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var h=await db.HoSoDangKyDeTais.Include(h=>h.SinhVien)!.ThenInclude(s=>s!.TaiKhoan)
            .Include(h=>h.LichBaoVeDeCuongs).Include(h=>h.KetQuaGiaoDeTai)
            .SingleOrDefaultAsync(h=>h.MaHoSo==id && h.SinhVien!.MaTaiKhoan==accountId);
        if(h==null) return new(false,"Không tìm thấy hồ sơ.",NotFound:true);
        if(h.SinhVien?.TaiKhoan is not { TrangThai:true, VaiTro:Roles.SinhVien }) return new(false,"Tài khoản không hợp lệ.");
        if(h.Version!=version) return new(false,"Hồ sơ đã thay đổi. Hãy tải lại trang trước khi hủy.");
        if(!Module3Rules.CanCancel(h)) return new(false,"Chỉ hủy hồ sơ Chờ duyệt/Đạt xét duyệt chưa có lịch bảo vệ và chưa có kết quả.");
        h.TrangThai=Module3Rules.Cancelled;await db.SaveChangesAsync();await tx.CommitAsync();
        return new(true,"Đã hủy hồ sơ và giữ lại lịch sử.",id);
    }
}
