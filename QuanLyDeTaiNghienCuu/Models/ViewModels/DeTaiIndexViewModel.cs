using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyDeTaiNghienCuu.Models;
using System.Collections.Generic;

namespace QuanLyDeTaiNghienCuu.Models.ViewModels
{
    public class DeTaiIndexViewModel
    {
        // Search & Filter parameters
        public string? SearchTerm { get; set; }
        public int? MaBoMon { get; set; }
        public string? LinhVuc { get; set; }
        public string? TrangThai { get; set; }
        public double? MinDtb { get; set; }
        public string? ConHan { get; set; } // "all", "conHan", "hetHan"
        public string SortBy { get; set; } = "ten_asc";

        // Pagination
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Data Items
        public List<DeTaiItemDto> Items { get; set; } = new List<DeTaiItemDto>();

        // Select lists for View UI dropdowns
        public SelectList? BoMonList { get; set; }
        public List<SelectListItem>? LinhVucList { get; set; }
        public List<SelectListItem>? TrangThaiList { get; set; }
        public List<SelectListItem>? ConHanList { get; set; }
        public List<SelectListItem>? SortByList { get; set; }
    }

    public class DeTaiItemDto
    {
        public DeTai DeTai { get; set; } = null!;
        public int TotalHoSoCount { get; set; }
        public int AssignedHoSoCount { get; set; }
        public int ApprovedHoSoCount { get; set; }
        public bool IsRegistrationOpenNow => 
            DeTai.TrangThai == "Đang mở đăng ký" && 
            DateTime.Now.Date >= DeTai.NgayBatDauDangKy.Date && 
            DateTime.Now.Date <= DeTai.HanDangKy.Date;
        public bool IsFullGroups => AssignedHoSoCount >= DeTai.SoNhomToiDa;
    }
}
