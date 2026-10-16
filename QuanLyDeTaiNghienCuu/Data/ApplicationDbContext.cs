using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Models;

namespace QuanLyDeTaiNghienCuu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<BoMon> BoMons { get; set; } = null!;
        public DbSet<DeTai> DeTais { get; set; } = null!;
        public DbSet<SinhVien> SinhViens { get; set; } = null!;
        public DbSet<HoSoDangKyDeTai> HoSoDangKyDeTais { get; set; } = null!;
        public DbSet<LichBaoVeDeCuong> LichBaoVeDeCuongs { get; set; } = null!;
        public DbSet<KetQuaGiaoDeTai> KetQuaGiaoDeTais { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Constraints
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            modelBuilder.Entity<BoMon>()
                .HasIndex(b => b.TenBoMon)
                .IsUnique();

            // BoMon - DeTai (1 - N)
            modelBuilder.Entity<DeTai>()
                .HasOne(d => d.BoMon)
                .WithMany(b => b.DeTais)
                .HasForeignKey(d => d.MaBoMon)
                .OnDelete(DeleteBehavior.Restrict);

            // SinhVien - TaiKhoan (0..1 - 1)
            modelBuilder.Entity<SinhVien>()
                .HasOne(s => s.TaiKhoan)
                .WithOne(t => t.SinhVien)
                .HasForeignKey<SinhVien>(s => s.MaTaiKhoan)
                .OnDelete(DeleteBehavior.SetNull);

            // SinhVien - HoSoDangKyDeTai (1 - N)
            modelBuilder.Entity<HoSoDangKyDeTai>()
                .HasOne(h => h.SinhVien)
                .WithMany(s => s.HoSoDangKyDeTais)
                .HasForeignKey(h => h.MaSinhVien)
                .OnDelete(DeleteBehavior.Restrict);

            // DeTai - HoSoDangKyDeTai (1 - N)
            modelBuilder.Entity<HoSoDangKyDeTai>()
                .HasOne(h => h.DeTai)
                .WithMany(d => d.HoSoDangKyDeTais)
                .HasForeignKey(h => h.MaDeTai)
                .OnDelete(DeleteBehavior.Restrict);

            // HoSoDangKyDeTai - LichBaoVeDeCuong (1 - N)
            modelBuilder.Entity<LichBaoVeDeCuong>()
                .HasOne(l => l.HoSo)
                .WithMany(h => h.LichBaoVeDeCuongs)
                .HasForeignKey(l => l.MaHoSo)
                .OnDelete(DeleteBehavior.Cascade);

            // HoSoDangKyDeTai - KetQuaGiaoDeTai (1 - 0..1)
            modelBuilder.Entity<KetQuaGiaoDeTai>()
                .HasOne(k => k.HoSo)
                .WithOne(h => h.KetQuaGiaoDeTai)
                .HasForeignKey<KetQuaGiaoDeTai>(k => k.MaHoSo)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
