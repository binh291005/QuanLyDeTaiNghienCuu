using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyDeTaiNghienCuu.Models;

namespace QuanLyDeTaiNghienCuu.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed BoMon if empty
            if (!context.BoMons.Any())
            {
                var boMons = new List<BoMon>
                {
                    new BoMon { TenBoMon = "Công nghệ phần mềm", MoTa = "Giảng dạy và nghiên cứu về quy trình phát triển phần mềm, Web, Mobile, DevOps.", EmailLienHe = "cnpm@uneti.edu.vn", TrangThai = true },
                    new BoMon { TenBoMon = "Hệ thống thông tin", MoTa = "Nghiên cứu về cơ sở dữ liệu, quản trị hệ thống, ERP và kiến trúc thông tin.", EmailLienHe = "httt@uneti.edu.vn", TrangThai = true },
                    new BoMon { TenBoMon = "Mạng máy tính & Truyền thông", MoTa = "Nghiên cứu an toàn thông tin, IoT, mạng máy tính và điện toán đám mây.", EmailLienHe = "mmt@uneti.edu.vn", TrangThai = true },
                    new BoMon { TenBoMon = "Trí tuệ nhân tạo & KH Dữ liệu", MoTa = "Nghiên cứu học máy, xử lý ngôn ngữ tự nhiên, thị giác máy tính và Big Data.", EmailLienHe = "aids@uneti.edu.vn", TrangThai = true },
                };
                context.BoMons.AddRange(boMons);
                context.SaveChanges();
            }

            // Seed TaiKhoan & SinhVien if empty
            if (!context.TaiKhoans.Any())
            {
                var tkAdmin = new TaiKhoan
                {
                    TenDangNhap = "admin",
                    MatKhau = "admin123",
                    HoTen = "Quản Trị Viên Hệ Thống",
                    Email = "admin@uneti.edu.vn",
                    VaiTro = "Admin",
                    TrangThai = true
                };

                var tkGiangVien = new TaiKhoan
                {
                    TenDangNhap = "giangvien",
                    MatKhau = "gv123456",
                    HoTen = "TS. Nguyễn Văn A",
                    Email = "nguyenvana@uneti.edu.vn",
                    VaiTro = "GiangVien",
                    TrangThai = true
                };

                var tkSinhVien1 = new TaiKhoan
                {
                    TenDangNhap = "sinhvien1",
                    MatKhau = "sv123456",
                    HoTen = "Trần Thị B",
                    Email = "tranthib@student.uneti.edu.vn",
                    VaiTro = "SinhVien",
                    TrangThai = true
                };

                context.TaiKhoans.AddRange(tkAdmin, tkGiangVien, tkSinhVien1);
                context.SaveChanges();

                var sv1 = new SinhVien
                {
                    MaTaiKhoan = tkSinhVien1.MaTaiKhoan,
                    HoTen = tkSinhVien1.HoTen,
                    Email = tkSinhVien1.Email,
                    SoDienThoai = "0987654321",
                    GioiTinh = "Nữ",
                    Nganh = "Công nghệ thông tin",
                    DiemTrungBinh = 3.45,
                    LinhVucQuanTam = "Lập trình Web, AI",
                    TrangThai = true
                };
                context.SinhViens.Add(sv1);
                context.SaveChanges();
            }

            // Seed DeTai if empty
            if (!context.DeTais.Any())
            {
                var boMonCNPM = context.BoMons.FirstOrDefault(b => b.TenBoMon.Contains("Công nghệ phần mềm"))?.MaBoMon ?? 1;
                var boMonHTTT = context.BoMons.FirstOrDefault(b => b.TenBoMon.Contains("Hệ thống thông tin"))?.MaBoMon ?? 2;
                var boMonMMT = context.BoMons.FirstOrDefault(b => b.TenBoMon.Contains("Mạng"))?.MaBoMon ?? 3;
                var boMonAI = context.BoMons.FirstOrDefault(b => b.TenBoMon.Contains("Trí tuệ"))?.MaBoMon ?? 4;

                var deTais = new List<DeTai>
                {
                    new DeTai
                    {
                        TenDeTai = "Xây dựng hệ thống quản lý đề tài NCKH và đăng ký đề tài sinh viên bằng ASP.NET Core 10 MVC",
                        MaBoMon = boMonCNPM,
                        SoNhomToiDa = 5,
                        LinhVucNghienCuu = "Phát triển phần mềm Web",
                        DiemTrungBinhToiThieu = 2.5,
                        NgayBatDauDangKy = DateTime.Today.AddDays(-10),
                        HanDangKy = DateTime.Today.AddDays(20),
                        MoTaDeTai = "Xây dựng hệ thống hoàn chỉnh cho phép công bố đề tài, tiếp nhận hồ sơ, xét duyệt, bảo vệ đề cương và ghi nhận kết quả giao đề tài.",
                        YeuCauSinhVien = "Thành thạo C#, ASP.NET Core MVC, EF Core, SQL Server, HTML/CSS/JS.",
                        TrangThai = "Đang mở đăng ký"
                    },
                    new DeTai
                    {
                        TenDeTai = "Ứng dụng Trí tuệ nhân tạo trong nhận dạng khuôn mặt hỗ trợ điểm danh tự động",
                        MaBoMon = boMonAI,
                        SoNhomToiDa = 3,
                        LinhVucNghienCuu = "Trí tuệ nhân tạo & Thị giác máy tính",
                        DiemTrungBinhToiThieu = 3.0,
                        NgayBatDauDangKy = DateTime.Today.AddDays(-5),
                        HanDangKy = DateTime.Today.AddDays(15),
                        MoTaDeTai = "Nghiên cứu các mô hình Deep Learning (YOLO, OpenCV, FaceNet) để tích hợp vào phần mềm nhận diện khuôn mặt sinh viên tại phòng học.",
                        YeuCauSinhVien = "Có kiến thức về Python, OpenCV, Machine Learning / Deep Learning, ĐTB >= 3.0.",
                        TrangThai = "Đang mở đăng ký"
                    },
                    new DeTai
                    {
                        TenDeTai = "Thiết kế hệ thống IoT giám sát chất lượng không khí và cảnh báo qua Telegram",
                        MaBoMon = boMonMMT,
                        SoNhomToiDa = 4,
                        LinhVucNghienCuu = "IoT & Mạng máy tính",
                        DiemTrungBinhToiThieu = 2.2,
                        NgayBatDauDangKy = DateTime.Today.AddDays(-15),
                        HanDangKy = DateTime.Today.AddDays(10),
                        MoTaDeTai = "Xây dựng mạch cảm biến ESP32 thu thập các chỉ số PM2.5, CO2, nhiệt độ, độ ẩm và gửi dữ liệu trực tiếp lên server Cloud Dashboard.",
                        YeuCauSinhVien = "Am hiểu lập trình Vi điều khiển, C/C++, MQTT, RESTful API.",
                        TrangThai = "Đang mở đăng ký"
                    },
                    new DeTai
                    {
                        TenDeTai = "Phân tích dữ liệu lớn và dự báo nhu cầu mua sắm thị trường thương mại điện tử",
                        MaBoMon = boMonHTTT,
                        SoNhomToiDa = 2,
                        LinhVucNghienCuu = "Khoa học dữ liệu & Big Data",
                        DiemTrungBinhToiThieu = 2.8,
                        NgayBatDauDangKy = DateTime.Today.AddDays(5),
                        HanDangKy = DateTime.Today.AddDays(35),
                        MoTaDeTai = "Sử dụng PySpark và Pandas để thu thập dữ liệu sản phẩm, phân tích xu hướng mua sắm của người dùng trên các nền tảng thương mại điện tử.",
                        YeuCauSinhVien = "Hiểu biết về SQL, Python Pandas, Data Analysis, Visualization.",
                        TrangThai = "Chưa mở"
                    },
                    new DeTai
                    {
                        TenDeTai = "Nghiên cứu công nghệ Blockchain trong xác thực văn bằng chứng chỉ đại học",
                        MaBoMon = boMonCNPM,
                        SoNhomToiDa = 2,
                        LinhVucNghienCuu = "Blockchain & An toàn thông tin",
                        DiemTrungBinhToiThieu = 3.2,
                        NgayBatDauDangKy = DateTime.Today.AddDays(-30),
                        HanDangKy = DateTime.Today.AddDays(-1),
                        MoTaDeTai = "Ứng dụng Smart Contract Ethereum/Polygon để lưu trữ mã băm văn bằng chứng chỉ, cho phép bên thứ ba xác thực trực tuyến.",
                        YeuCauSinhVien = "ĐTB >= 3.2, có kiến thức cơ bản về Solidity hoặc Cryptography.",
                        TrangThai = "Đã đóng"
                    },
                    new DeTai
                    {
                        TenDeTai = "Xây dựng Chatbot tư vấn tuyển sinh UNETI sử dụng kĩ thuật RAG và LLM",
                        MaBoMon = boMonAI,
                        SoNhomToiDa = 3,
                        LinhVucNghienCuu = "Xử lý ngôn ngữ tự nhiên (NLP)",
                        DiemTrungBinhToiThieu = 2.8,
                        NgayBatDauDangKy = DateTime.Today.AddDays(-2),
                        HanDangKy = DateTime.Today.AddDays(28),
                        MoTaDeTai = "Thu thập dữ liệu thông tin tuyển sinh UNETI, lưu trữ Vector DB (ChromaDB/Qdrant) và tích hợp LangChain hỗ trợ trả lời câu hỏi tự động.",
                        YeuCauSinhVien = "Yêu thích học máy, NLP, có khả năng đọc tài liệu tiếng Anh tốt.",
                        TrangThai = "Đang mở đăng ký"
                    },
                    new DeTai
                    {
                        TenDeTai = "Phát triển ứng dụng di động quản lý chi tiêu cá nhân tích hợp AI gợi ý tiết kiệm",
                        MaBoMon = boMonCNPM,
                        SoNhomToiDa = 4,
                        LinhVucNghienCuu = "Phát triển ứng dụng Di động",
                        DiemTrungBinhToiThieu = 2.0,
                        NgayBatDauDangKy = DateTime.Today.AddDays(-20),
                        HanDangKy = DateTime.Today.AddDays(5),
                        MoTaDeTai = "Viết app di động trên Flutter/React Native kết nối backend ASP.NET Core Web API.",
                        YeuCauSinhVien = "Đã từng làm đồ án ứng dụng di động hoặc Web API.",
                        TrangThai = "Tạm dừng"
                    },
                    new DeTai
                    {
                        TenDeTai = "Tối ưu hóa thuật toán định tuyến trong mạng cảm biến không dây WSN",
                        MaBoMon = boMonMMT,
                        SoNhomToiDa = 2,
                        LinhVucNghienCuu = "Mạng không dây",
                        DiemTrungBinhToiThieu = 2.6,
                        NgayBatDauDangKy = DateTime.Today.AddDays(-1),
                        HanDangKy = DateTime.Today.AddDays(30),
                        MoTaDeTai = "Mô phỏng mạng WSN bằng NS-3 hoặc Matlab, đề xuất cải tiến thuật toán LEACH giúp kéo dài tuổi thọ năng lượng của nút mạng.",
                        YeuCauSinhVien = "Yêu thích mô phỏng mạng và lý thuyết mạng nâng cao.",
                        TrangThai = "Đang mở đăng ký"
                    }
                };

                context.DeTais.AddRange(deTais);
                context.SaveChanges();
            }
            DuLieuMauModule5.Seed(context);
        }
    }
}
