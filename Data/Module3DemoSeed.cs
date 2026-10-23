// Nguồn mã: Mã nền nhóm và/hoặc bản tham khảo Module 3 có hỗ trợ AI.
// Chức năng trong file: Dữ liệu mẫu phục vụ minh họa và kiểm thử Module 3.
// Thông tin người trực tiếp thực hiện: Chưa xác nhận; không gán tác giả cá nhân.
// Giữ nguyên thông tin tác giả mã nền khi hợp nhất với repository nhóm.
// Đối chiếu: DOI_CHIEU_DONG_GOP_HAI.md tại thư mục solution.
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Services;
namespace QuanLyDeTaiNghienCuu.Data;
public static class Module3DemoSeed
{
    public static void Initialize(ApplicationDbContext db)
    {
        if(db.TaiKhoans.Any(t=>t.TenDangNhap=="m3_sv02"))return;
        var accounts=Enumerable.Range(2,29).Select(i=>new TaiKhoan{TenDangNhap=$"m3_sv{i:00}",MatKhau="sv123456",HoTen=$"Sinh viên mẫu {i:00}",Email=$"sv{i:00}@example.test",VaiTro="SinhVien",TrangThai=true}).ToList();
        db.TaiKhoans.AddRange(accounts);db.SaveChanges();
        db.SinhViens.AddRange(accounts.Select((t,i)=>new SinhVien{MaTaiKhoan=t.MaTaiKhoan,HoTen=t.HoTen,Email=t.Email,NgaySinh=new DateTime(2005,1,1),GioiTinh="Khác",Nganh="Công nghệ thông tin",SoDienThoai="0901234567",DiemTrungBinh=3,TrangThai=i!=28}));db.SaveChanges();
        var topics=db.DeTais.OrderBy(d=>d.MaDeTai).Take(3).ToList();var students=db.SinhViens.OrderBy(s=>s.MaSinhVien).Take(20).ToList();
        if(topics.Count<3)return;
        var now=Module3Rules.VietnamNow(TimeProvider.System);
        for(var i=0;i<students.Count;i++)for(var j=0;j<2;j++){
            var state=Module3Rules.AllStatuses[(i+j)%8];
            var h=new HoSoDangKyDeTai{MaSinhVien=students[i].MaSinhVien,MaDeTai=topics[j].MaDeTai,NgayNop=now.AddDays(-7),DeCuongSoBo="Đề cương mẫu để kiểm thử Module 3.",TrangThai=state};
            if(state is Module3Rules.Scheduled or Module3Rules.Defended or Module3Rules.Assigned or Module3Rules.NotAssigned)
                h.LichBaoVeDeCuongs.Add(new LichBaoVeDeCuong{ThoiGianBatDau=now.AddDays(state==Module3Rules.Scheduled?5:-2).Date.AddHours(8+i),ThoiGianKetThuc=now.AddDays(state==Module3Rules.Scheduled?5:-2).Date.AddHours(8+i).AddMinutes(30),GiangVienPhanBien="Giảng viên mẫu",DiaDiemHoacLienKet="Phòng A301",TrangThai=state==Module3Rules.Scheduled?"Đã lên lịch":"Đã hoàn thành"});
            if(state is Module3Rules.Assigned or Module3Rules.NotAssigned)h.KetQuaGiaoDeTai=new KetQuaGiaoDeTai{KetQua=state,DiemDanhGia=state==Module3Rules.Assigned?85:45,NhanXet="Kết quả mẫu để kiểm thử hiển thị.",NgayCapNhat=now.AddDays(-1)};
            db.Add(h);
        }db.SaveChanges();
    }
}
