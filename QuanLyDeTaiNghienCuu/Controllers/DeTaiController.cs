using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiNghienCuu.Data;
using QuanLyDeTaiNghienCuu.Models;
using QuanLyDeTaiNghienCuu.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDeTaiNghienCuu.Controllers
{
    public class DeTaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeTaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DeTai (Index with Search, Filter, Sort, Pagination)
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? maBoMon,
            string? linhVuc,
            string? trangThai,
            double? minDtb,
            string? conHan,
            string sortBy = "ten_asc",
            int page = 1,
            int pageSize = 6)
        {
            if (page < 1) page = 1;

            // 1. Base Query using EF Core IQueryable
            var query = _context.DeTais
                .Include(d => d.BoMon)
                .Include(d => d.HoSoDangKyDeTais)
                .AsNoTracking()
                .AsQueryable();

            // 2. Search by TenDeTai or TenBoMon
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(d =>
                    d.TenDeTai.ToLower().Contains(term) ||
                    (d.BoMon != null && d.BoMon.TenBoMon.ToLower().Contains(term)));
            }

            // 3. Filters
            // Filter by BoMon
            if (maBoMon.HasValue && maBoMon.Value > 0)
            {
                query = query.Where(d => d.MaBoMon == maBoMon.Value);
            }

            // Filter by LinhVucNghienCuu
            if (!string.IsNullOrWhiteSpace(linhVuc))
            {
                query = query.Where(d => d.LinhVucNghienCuu == linhVuc);
            }

            // Filter by TrangThai
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            // Filter by Mức điểm trung bình tối thiểu (VD: lấy đề tài yêu cầu ĐTB <= minDtb)
            if (minDtb.HasValue)
            {
                query = query.Where(d => d.DiemTrungBinhToiThieu <= minDtb.Value);
            }

            // Filter by Còn hạn / Hết hạn nhận hồ sơ
            var today = DateTime.Today;
            if (conHan == "conHan")
            {
                query = query.Where(d =>
                    d.TrangThai == "Đang mở đăng ký" &&
                    d.NgayBatDauDangKy <= today &&
                    d.HanDangKy >= today);
            }
            else if (conHan == "hetHan")
            {
                query = query.Where(d => d.HanDangKy < today);
            }

            // 4. Sorting via LINQ
            query = sortBy switch
            {
                "ten_desc" => query.OrderByDescending(d => d.TenDeTai),
                "handangky_asc" => query.OrderBy(d => d.HanDangKy),
                "handangky_desc" => query.OrderByDescending(d => d.HanDangKy),
                "sonhom_asc" => query.OrderBy(d => d.SoNhomToiDa),
                "sonhom_desc" => query.OrderByDescending(d => d.SoNhomToiDa),
                _ => query.OrderBy(d => d.TenDeTai) // Default ten_asc
            };

            // 5. Pagination via LINQ Skip() and Take()
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var itemsOnPage = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DeTaiItemDto
                {
                    DeTai = d,
                    TotalHoSoCount = d.HoSoDangKyDeTais.Count,
                    AssignedHoSoCount = d.HoSoDangKyDeTais.Count(h => h.TrangThai == "Được giao đề tài"),
                    ApprovedHoSoCount = d.HoSoDangKyDeTais.Count(h => h.TrangThai == "Đạt xét duyệt")
                })
                .ToListAsync();

            // Populate Filter Dropdowns
            var boMons = await _context.BoMons.Where(b => b.TrangThai).OrderBy(b => b.TenBoMon).ToListAsync();
            var distinctLinhVucs = await _context.DeTais
                .Select(d => d.LinhVucNghienCuu)
                .Where(l => !string.IsNullOrEmpty(l))
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            var viewModel = new DeTaiIndexViewModel
            {
                SearchTerm = searchTerm,
                MaBoMon = maBoMon,
                LinhVuc = linhVuc,
                TrangThai = trangThai,
                MinDtb = minDtb,
                ConHan = conHan,
                SortBy = sortBy,
                PageIndex = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = itemsOnPage,
                BoMonList = new SelectList(boMons, "MaBoMon", "TenBoMon", maBoMon),
                LinhVucList = distinctLinhVucs.Select(l => new SelectListItem { Text = l, Value = l, Selected = l == linhVuc }).ToList(),
                TrangThaiList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Chưa mở", Value = "Chưa mở", Selected = trangThai == "Chưa mở" },
                    new SelectListItem { Text = "Đang mở đăng ký", Value = "Đang mở đăng ký", Selected = trangThai == "Đang mở đăng ký" },
                    new SelectListItem { Text = "Tạm dừng", Value = "Tạm dừng", Selected = trangThai == "Tạm dừng" },
                    new SelectListItem { Text = "Đã đóng", Value = "Đã đóng", Selected = trangThai == "Đã đóng" }
                },
                ConHanList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Tất cả hạn đăng ký", Value = "", Selected = string.IsNullOrEmpty(conHan) },
                    new SelectListItem { Text = "Còn hạn đăng ký", Value = "conHan", Selected = conHan == "conHan" },
                    new SelectListItem { Text = "Đã hết hạn", Value = "hetHan", Selected = conHan == "hetHan" }
                },
                SortByList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Tên đề tài (A -> Z)", Value = "ten_asc", Selected = sortBy == "ten_asc" },
                    new SelectListItem { Text = "Tên đề tài (Z -> A)", Value = "ten_desc", Selected = sortBy == "ten_desc" },
                    new SelectListItem { Text = "Hạn đăng ký (Tăng dần)", Value = "handangky_asc", Selected = sortBy == "handangky_asc" },
                    new SelectListItem { Text = "Hạn đăng ký (Giảm dần)", Value = "handangky_desc", Selected = sortBy == "handangky_desc" },
                    new SelectListItem { Text = "Số nhóm tối đa (Tăng dần)", Value = "sonhom_asc", Selected = sortBy == "sonhom_asc" },
                    new SelectListItem { Text = "Số nhóm tối đa (Giảm dần)", Value = "sonhom_desc", Selected = sortBy == "sonhom_desc" }
                }
            };

            return View(viewModel);
        }

        // GET: DeTai/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deTai = await _context.DeTais
                .Include(d => d.BoMon)
                .Include(d => d.HoSoDangKyDeTais)
                .ThenInclude(h => h.SinhVien)
                .FirstOrDefaultAsync(m => m.MaDeTai == id);

            if (deTai == null)
            {
                return NotFound();
            }

            ViewBag.AssignedCount = deTai.HoSoDangKyDeTais.Count(h => h.TrangThai == "Được giao đề tài");
            ViewBag.TotalCount = deTai.HoSoDangKyDeTais.Count;

            return View(deTai);
        }

        // GET: DeTai/Create
        public async Task<IActionResult> Create()
        {
            var boMons = await _context.BoMons.Where(b => b.TrangThai).OrderBy(b => b.TenBoMon).ToListAsync();

            var viewModel = new DeTaiFormViewModel
            {
                NgayBatDauDangKy = DateTime.Today,
                HanDangKy = DateTime.Today.AddDays(30),
                SoNhomToiDa = 3,
                DiemTrungBinhToiThieu = 2.0,
                TrangThai = "Chưa mở",
                BoMonList = new SelectList(boMons, "MaBoMon", "TenBoMon"),
                TrangThaiList = GetTrangThaiList("Chưa mở")
            };

            return View(viewModel);
        }

        // POST: DeTai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeTaiFormViewModel model)
        {
            // Business logic check: HanDangKy >= NgayBatDauDangKy
            if (model.HanDangKy.Date < model.NgayBatDauDangKy.Date)
            {
                ModelState.AddModelError("HanDangKy", "Hạn đăng ký phải sau hoặc bằng ngày bắt đầu nhận đăng ký.");
            }

            // Business logic check: Cannot open registration if mandatory fields are missing
            if (model.TrangThai == "Đang mở đăng ký")
            {
                if (string.IsNullOrWhiteSpace(model.TenDeTai) || string.IsNullOrWhiteSpace(model.LinhVucNghienCuu))
                {
                    ModelState.AddModelError("TrangThai", "Không thể mở đăng ký khi chưa hoàn thiện tên đề tài và lĩnh vực nghiên cứu.");
                }
            }

            if (ModelState.IsValid)
            {
                var deTai = new DeTai
                {
                    TenDeTai = model.TenDeTai.Trim(),
                    MaBoMon = model.MaBoMon,
                    SoNhomToiDa = model.SoNhomToiDa,
                    LinhVucNghienCuu = model.LinhVucNghienCuu.Trim(),
                    DiemTrungBinhToiThieu = model.DiemTrungBinhToiThieu,
                    NgayBatDauDangKy = model.NgayBatDauDangKy,
                    HanDangKy = model.HanDangKy,
                    MoTaDeTai = model.MoTaDeTai,
                    YeuCauSinhVien = model.YeuCauSinhVien,
                    TrangThai = model.TrangThai
                };

                _context.Add(deTai);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tạo mới đề tài '{deTai.TenDeTai}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Reload select lists on error
            var boMons = await _context.BoMons.Where(b => b.TrangThai).OrderBy(b => b.TenBoMon).ToListAsync();
            model.BoMonList = new SelectList(boMons, "MaBoMon", "TenBoMon", model.MaBoMon);
            model.TrangThaiList = GetTrangThaiList(model.TrangThai);
            return View(model);
        }

        // GET: DeTai/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deTai = await _context.DeTais.FindAsync(id);
            if (deTai == null)
            {
                return NotFound();
            }

            var boMons = await _context.BoMons.Where(b => b.TrangThai).OrderBy(b => b.TenBoMon).ToListAsync();

            var viewModel = new DeTaiFormViewModel
            {
                MaDeTai = deTai.MaDeTai,
                TenDeTai = deTai.TenDeTai,
                MaBoMon = deTai.MaBoMon,
                SoNhomToiDa = deTai.SoNhomToiDa,
                LinhVucNghienCuu = deTai.LinhVucNghienCuu,
                DiemTrungBinhToiThieu = deTai.DiemTrungBinhToiThieu,
                NgayBatDauDangKy = deTai.NgayBatDauDangKy,
                HanDangKy = deTai.HanDangKy,
                MoTaDeTai = deTai.MoTaDeTai,
                YeuCauSinhVien = deTai.YeuCauSinhVien,
                TrangThai = deTai.TrangThai,
                BoMonList = new SelectList(boMons, "MaBoMon", "TenBoMon", deTai.MaBoMon),
                TrangThaiList = GetTrangThaiList(deTai.TrangThai)
            };

            return View(viewModel);
        }

        // POST: DeTai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DeTaiFormViewModel model)
        {
            if (id != model.MaDeTai)
            {
                return NotFound();
            }

            if (model.HanDangKy.Date < model.NgayBatDauDangKy.Date)
            {
                ModelState.AddModelError("HanDangKy", "Hạn đăng ký phải sau hoặc bằng ngày bắt đầu nhận đăng ký.");
            }

            if (model.TrangThai == "Đang mở đăng ký")
            {
                if (string.IsNullOrWhiteSpace(model.TenDeTai) || string.IsNullOrWhiteSpace(model.LinhVucNghienCuu))
                {
                    ModelState.AddModelError("TrangThai", "Không thể chuyển sang 'Đang mở đăng ký' khi thông tin đề tài chưa hợp lệ.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var deTai = await _context.DeTais.FindAsync(id);
                    if (deTai == null) return NotFound();

                    deTai.TenDeTai = model.TenDeTai.Trim();
                    deTai.MaBoMon = model.MaBoMon;
                    deTai.SoNhomToiDa = model.SoNhomToiDa;
                    deTai.LinhVucNghienCuu = model.LinhVucNghienCuu.Trim();
                    deTai.DiemTrungBinhToiThieu = model.DiemTrungBinhToiThieu;
                    deTai.NgayBatDauDangKy = model.NgayBatDauDangKy;
                    deTai.HanDangKy = model.HanDangKy;
                    deTai.MoTaDeTai = model.MoTaDeTai;
                    deTai.YeuCauSinhVien = model.YeuCauSinhVien;
                    deTai.TrangThai = model.TrangThai;

                    _context.Update(deTai);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật đề tài '{deTai.TenDeTai}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeTaiExists(model.MaDeTai))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var boMons = await _context.BoMons.Where(b => b.TrangThai).OrderBy(b => b.TenBoMon).ToListAsync();
            model.BoMonList = new SelectList(boMons, "MaBoMon", "TenBoMon", model.MaBoMon);
            model.TrangThaiList = GetTrangThaiList(model.TrangThai);
            return View(model);
        }

        // POST: DeTai/QuickChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickChangeStatus(int id, string newStatus)
        {
            var deTai = await _context.DeTais.FindAsync(id);
            if (deTai == null)
            {
                return NotFound();
            }

            var allowedStatuses = new[] { "Chưa mở", "Đang mở đăng ký", "Tạm dừng", "Đã đóng" };
            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["ErrorMessage"] = "Trạng thái mới không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (newStatus == "Đang mở đăng ký")
            {
                if (string.IsNullOrWhiteSpace(deTai.TenDeTai) || string.IsNullOrWhiteSpace(deTai.LinhVucNghienCuu) || deTai.SoNhomToiDa <= 0)
                {
                    TempData["ErrorMessage"] = "Không thể mở đăng ký do đề tài chưa đủ thông tin bắt buộc.";
                    return RedirectToAction(nameof(Index));
                }
            }

            deTai.TrangThai = newStatus;
            _context.Update(deTai);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã đổi trạng thái đề tài sang '{newStatus}'.";
            return RedirectToAction(nameof(Index));
        }

        // GET: DeTai/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deTai = await _context.DeTais
                .Include(d => d.BoMon)
                .Include(d => d.HoSoDangKyDeTais)
                .FirstOrDefaultAsync(m => m.MaDeTai == id);

            if (deTai == null)
            {
                return NotFound();
            }

            ViewBag.HasApplications = deTai.HoSoDangKyDeTais.Any();
            return View(deTai);
        }

        // POST: DeTai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deTai = await _context.DeTais
                .Include(d => d.HoSoDangKyDeTais)
                .FirstOrDefaultAsync(d => d.MaDeTai == id);

            if (deTai == null)
            {
                return NotFound();
            }

            // Kiểm soát nghiệp vụ: Không cho phép xóa đề tài đã có hồ sơ đăng ký để bảo toàn dữ liệu lịch sử
            if (deTai.HoSoDangKyDeTais.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa đề tài '{deTai.TenDeTai}' vì đã có {deTai.HoSoDangKyDeTais.Count} hồ sơ đăng ký trong lịch sử. Vui lòng chuyển trạng thái sang 'Đã đóng' hoặc 'Tạm dừng'.";
                return RedirectToAction(nameof(Index));
            }

            _context.DeTais.Remove(deTai);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa đề tài '{deTai.TenDeTai}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool DeTaiExists(int id)
        {
            return _context.DeTais.Any(e => e.MaDeTai == id);
        }

        private List<SelectListItem> GetTrangThaiList(string selectedValue)
        {
            var statuses = new[] { "Chưa mở", "Đang mở đăng ký", "Tạm dừng", "Đã đóng" };
            return statuses.Select(s => new SelectListItem
            {
                Text = s,
                Value = s,
                Selected = s == selectedValue
            }).ToList();
        }
    }
}
