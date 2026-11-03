using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDeTaiNghienCuu.Models
{
    [Table("BoMon")]
    public class BoMon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBoMon { get; set; }

        [Required(ErrorMessage = "Tên bộ môn không được để trống")]
        [StringLength(150, ErrorMessage = "Tên bộ môn tối đa 150 ký tự")]
        [Display(Name = "Tên bộ môn")]
        public string TenBoMon { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string? MoTa { get; set; }

        [EmailAddress(ErrorMessage = "Email liên hệ không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email liên hệ")]
        public string? EmailLienHe { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true = Hoạt động, false = Tạm dừng

        public virtual ICollection<DeTai> DeTais { get; set; } = new List<DeTai>();
    }
}
