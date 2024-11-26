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
    public class BaiTap7Controller : Controller
    {
        private readonly DBContext _db;
        private readonly IHostingEnvironment _hosting;
        private readonly UserManager<ApplicationUser> _userManager;
        public BaiTap7Controller(DBContext db, IHostingEnvironment hosting,UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _hosting = hosting;
            _userManager = userManager;
        }
        public class CombinedViewModel
        {
            public ViewModelBai7 Bai7ViewModel { get; set; }
            public List<ViewModelBai7> ViewModelList { get; set; }
        }
        

        private void LoadViewBag()
        {
            ViewBag.SPList = _db.tbl_DM_San_Pham.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Ten_San_Pham
            }).ToList();

            ViewBag.NCCList = _db.tbl_DM_NCC.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Ten_NCC
            }).ToList();

            ViewBag.KhoList = _db.tbl_DM_Kho.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Ten_Kho
            }).ToList();
        }
        public IActionResult Index(int? IdKho)
        {
            // Lấy thông tin người dùng đã đăng nhập
            var currentUser = _userManager.GetUserAsync(User).Result;
            var userKhoId = currentUser?.Kho_ID;
            ViewBag.UserKhoId = userKhoId.ToString();
            var listBai7 = _db.tbl_DM_Nhap_Kho.Include(x => x.Kho).Include(x => x.NCC);
            var listBai7_2 = _db.tbl_DM_Nhap_Kho_Raw_Data.Include(x => x.sanpham);
            var viewModelList = listBai7.Join(listBai7_2,
            bai7 => bai7.Id,
            bai7_2 => bai7_2.Id,
            (bai7, bai7_2) => new ViewModelBai7
            {
                Bai7 = bai7,
                Bai7_2 = bai7_2,
                TriGia = (decimal)bai7_2.Don_Gia_Nhap * (decimal)bai7_2.SL_Nhap
            }).Where(x=>x.Bai7.Kho_ID == userKhoId).ToList();
            LoadViewBag();
            return View(viewModelList);
        }
        public IActionResult Add()
        {
            LoadViewBag();
            return View();
        }
        [HttpPost]
        public IActionResult Add(ViewModelBai7 model)
        {
            if (string.IsNullOrWhiteSpace(model.Bai7.So_Phieu_Nhap_Kho))
            {
                ModelState.AddModelError("Bai7.So_Phieu_Nhap_Kho", "Số Phiếu Nhập không được để trống.");
            }
            else
            {
                var ktrMa = _db.tbl_DM_Nhap_Kho
                              .FirstOrDefault(d => d.So_Phieu_Nhap_Kho.Trim().ToLower() == model.Bai7.So_Phieu_Nhap_Kho.Trim().ToLower());

                if (ktrMa != null)
                {
                    ModelState.AddModelError("Bai7.So_Phieu_Nhap_Kho", "Số Phiếu Nhấp đã tồn tại.");
                }
            }
            LoadViewBag();
            if (model.Bai7.Kho_ID <= 0)
            {
                ModelState.AddModelError("Bai7.Kho_ID", "Vui lòng chọn Kho");
            }

            if (model.Bai7.NCC_ID <= 0)
            {
                ModelState.AddModelError("Bai7.NCC_ID", "Vui lòng chọn nhà cung cấp");
            }
            if (model.Bai7_2.San_Pham_ID <= 0)
            {
                ModelState.AddModelError("Bai7.San_Pham_ID", "Vui lòng chọn nhà cung cấp");
            }
            if (model.Bai7_2.SL_Nhap < 0)
            {
                ModelState.AddModelError("Bai7.SL_Nhap", "Số lượng không được âm");
            }
            if (ModelState.IsValid)
            {
                _db.tbl_DM_Nhap_Kho.Add(model.Bai7);
                _db.SaveChanges();
                if (model.Bai7.Id > 0)
                {
                    model.Bai7_2.Nhap_Kho_ID = model.Bai7.Id;
                    _db.tbl_DM_Nhap_Kho_Raw_Data.Add(model.Bai7_2);
                    _db.SaveChanges();
                    TempData["success"] = "Thêm Thành Công";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Không thể lấy Id từ Bai7.");
                }
            }
            return View(model);
        }
        public IActionResult Delete(int bai7Id, int bai7_2Id)
        {
            var nk = _db.tbl_DM_Nhap_Kho.FirstOrDefault(x => x.Id == bai7Id);
            var nkr = _db.tbl_DM_Nhap_Kho_Raw_Data.FirstOrDefault(x => x.Id == bai7_2Id);
            if (nk == null || nkr == null)
            {
                return NotFound();
            }
            var viewModel = new ViewModelBai7
            {
                Bai7 = nk,
                Bai7_2 = nkr
            };
            return View(viewModel);
        }
        public IActionResult DeleteConfirmed(int bai7Id, int bai7_2Id)
        {
            var nk = _db.tbl_DM_Nhap_Kho.FirstOrDefault(x => x.Id == bai7Id);
            var nkr = _db.tbl_DM_Nhap_Kho_Raw_Data.FirstOrDefault(x => x.Id == bai7_2Id);
            if (nk == null || nkr == null)
            {
                return NotFound();
            }
            _db.tbl_DM_Nhap_Kho.Remove(nk);
            _db.tbl_DM_Nhap_Kho_Raw_Data.Remove(nkr);
            _db.SaveChanges();
            TempData["success"] = "Xóa Thành Công";
            return RedirectToAction("Index");
        }
    }
}
