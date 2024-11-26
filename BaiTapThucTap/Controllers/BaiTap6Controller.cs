using BaiTapThucTap.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaiTapThucTap.Controllers
{
    public class BaiTap6Controller : Controller
    {
        private readonly DBContext _db;
        private readonly IHostingEnvironment _hosting;
        private readonly UserManager<ApplicationUser> _userManager;
        public BaiTap6Controller(DBContext db, IHostingEnvironment hosting, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _hosting = hosting;
            _userManager = userManager;
        }
        private void LoadViewBag()
        {
            ViewBag.UserList = _userManager.Users.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Email
            }).ToList();

            ViewBag.KhoList = _db.tbl_DM_Kho.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Ten_Kho
            }).ToList();
        }
        public IActionResult Index()
        {
            var listKho = _db.tbl_DM_Kho_User.Include(x => x.Kho).Include(x=>x.User).ToList();
            LoadViewBag();
            return View(listKho);
        }

        public IActionResult Add()
        {
            LoadViewBag();
            return View();
        }

        [HttpPost]
        public IActionResult Add(BaiTap6 model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = Guid.NewGuid().ToString(); 
            }

            if (string.IsNullOrWhiteSpace(model.Ma_Dang_Nhap))
            {
                ModelState.AddModelError("Ma_Dang_Nhap", "Chọn User");
            }
            else
            {
                var ktrMa = _db.tbl_DM_Kho_User
                              .FirstOrDefault(d => d.Ma_Dang_Nhap.Trim().ToLower() == model.Ma_Dang_Nhap.Trim().ToLower());

                if (ktrMa != null)
                {
                    ModelState.AddModelError("Ma_Dang_Nhap", "mã đăng nhập đã tồn tại.");
                }
            }
            if (model.Kho_ID <= 0)
            {
                ModelState.AddModelError("Kho_ID", "Vui lòng chọn nhà Kho");
            }
            if (ModelState.IsValid)
            {
                _db.tbl_DM_Kho_User.Add(model);
                _db.SaveChanges();
                TempData["success"] = "Thêm Thành Công";
                return RedirectToAction("Index");
            }
            LoadViewBag();
            return View(model);
        }
    }
}
