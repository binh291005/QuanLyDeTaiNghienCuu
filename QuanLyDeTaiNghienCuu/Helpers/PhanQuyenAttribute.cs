// Họ và tên: Phạm Tiến Phát
// Mã sinh viên: 23103100248
// Nội dung thực hiện: Module 5 - Attribute kiểm tra vai trò (Session) tại Controller.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace QuanLyDeTaiNghienCuu.Helpers
{
    public class PhanQuyenAttribute : ActionFilterAttribute
    {
        public static readonly bool BoQuaKiemTra = true;

        private readonly string[] _vaiTroChoPhep;

        public PhanQuyenAttribute(params string[] vaiTroChoPhep)
        {
            _vaiTroChoPhep = vaiTroChoPhep;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (BoQuaKiemTra) return;

            var vaiTro = context.HttpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(vaiTro) || !_vaiTroChoPhep.Contains(vaiTro))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này.";
                }
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}
