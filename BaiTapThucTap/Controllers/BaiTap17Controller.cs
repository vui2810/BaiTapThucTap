using BaiTapThucTap.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaiTapThucTap.Controllers
{
    public class BaiTap17Controller : Controller
    {
        private readonly DBContext _db;

        public BaiTap17Controller(DBContext db)
        {
            _db = db;
        }

        public class CombinedViewModel
        {
            public BaiTap17 SearchModel { get; set; }
            public List<BaiTap17> ListSearchSP { get; set; }

        }

        public IActionResult Index()
        {
            var model = new CombinedViewModel();
            // Lấy danh sách dữ liệu
            var listBai7 = _db.tbl_DM_Nhap_Kho.Include(x => x.Kho).Include(x => x.NCC);
            var listBai11 = _db.tbl_DM_Xuat_Kho.Include(x => x.Kho);
            var listBai7_2 = _db.tbl_DM_Nhap_Kho_Raw_Data.Include(x => x.sanpham);
            var listBai11_2 = _db.tbl_DM_Xuat_Kho_Raw_Data.Include(x => x.sanpham);
            var listBai3 = _db.tbl_DM_San_Pham.Include(x => x.DonViTinh).Include(x => x.LoaiSP).ToList();
    
            var ngayNhap = new BaiTap17
            {
                NgayBatDau = new DateTime(2024, 1, 1),
                NgayKetThuc = new DateTime(2024, 12, 30)
            };
            var dauKyTruoc = ngayNhap.NgayBatDau.Month == 1
                     ? new DateTime(ngayNhap.NgayBatDau.Year - 1, 12, ngayNhap.NgayBatDau.Day)
                     : new DateTime(ngayNhap.NgayBatDau.Year, ngayNhap.NgayBatDau.Month - 1, ngayNhap.NgayBatDau.Day);

            var cuoiKyTruoc = ngayNhap.NgayKetThuc.Month == 1
                ? new DateTime(ngayNhap.NgayKetThuc.Year - 1, 12, ngayNhap.NgayKetThuc.Day == 31 ? 30 : ngayNhap.NgayKetThuc.Day)
                : new DateTime(ngayNhap.NgayKetThuc.Year, ngayNhap.NgayKetThuc.Month - 1, ngayNhap.NgayKetThuc.Day == 31 ? 30 : ngayNhap.NgayKetThuc.Day);
            // Kết hợp danh sách
            var List7 = listBai7.Join(listBai7_2,
                bai7 => bai7.Id,
                bai7_2 => bai7_2.Id,
                (bai7, bai7_2) => new BaiTap17
                {
                    Bai7 = bai7,
                    Bai7_2 = bai7_2,
                    SL_Nhap = bai7_2.SL_Nhap
                }).Where(x => x.Bai7.Ngay_Nhap_Kho >= ngayNhap.NgayBatDau && x.Bai7.Ngay_Nhap_Kho <= ngayNhap.NgayKetThuc)
                    .ToList();

            var List11 = listBai11.Join(listBai11_2,
                bai11 => bai11.Id,
                bai11_2 => bai11_2.Id,
                (bai11, bai11_2) => new BaiTap17
                {
                    Bai11 = bai11,
                    Bai11_2 = bai11_2,
                    SL_Xuat =bai11_2.SL_Xuat
                }).Where(x => x.Bai11.Ngay_Xuat_Kho >= ngayNhap.NgayBatDau && x.Bai11.Ngay_Xuat_Kho <= ngayNhap.NgayKetThuc)
                    .ToList();
            // Tính toán số liệu
            var viewModelList = List7.Join(List11,
                bai7 => bai7.Bai7.Id,
                bai11_2 => bai11_2.Bai11.Id,
                (bai7, bai11) => new BaiTap17
                {
                    Bai7 = bai7.Bai7,
                    Bai7_2 = bai7.Bai7_2,
                    Bai11 = bai11.Bai11,
                    Bai11_2 = bai11.Bai11_2,
                    
                })
                .ToList(); // Dữ liệu đã được tải về bộ nhớ

            var viewBai17TheoSP = listBai3.Select(bai3=>  new BaiTap17
                {
                    Ma_San_Pham = bai3.Ma_San_Pham,
                    Ten_San_Pham = bai3.Ten_San_Pham,
                    // Tìm các giá trị từ viewModelList liên quan đến sản phẩm
                    SL_Dau_Ky =  0,
                    SL_Nhap = List7.Where(x => x.Bai7_2.sanpham.Id == bai3.Id).Sum(x=>x?.SL_Nhap ?? 0),
                    SL_Xuat = List11.Where(x => x.Bai11_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Xuat ?? 0),
                    SL_Cuoi_Ky = List7.Where(x => x.Bai7_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Nhap ?? 0)
                                -
                                List11.Where(x => x.Bai11_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Xuat ?? 0)
            })
                
                
                .ToList();
            model.ListSearchSP = viewBai17TheoSP;
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(CombinedViewModel model)
        {
            // Lấy danh sách dữ liệu
            var listBai7 = _db.tbl_DM_Nhap_Kho.Include(x => x.Kho).Include(x => x.NCC);
            var listBai11 = _db.tbl_DM_Xuat_Kho.Include(x => x.Kho);
            var listBai7_2 = _db.tbl_DM_Nhap_Kho_Raw_Data.Include(x => x.sanpham);
            var listBai11_2 = _db.tbl_DM_Xuat_Kho_Raw_Data.Include(x => x.sanpham);
            var listBai3 = _db.tbl_DM_San_Pham.Include(x => x.DonViTinh).Include(x => x.LoaiSP).ToList();

            var dauKyTruoc = model.SearchModel.NgayBatDau.Month == 1
                     ? new DateTime(model.SearchModel.NgayBatDau.Year - 1, 12, model.SearchModel.NgayBatDau.Day)
                     : new DateTime(model.SearchModel.NgayBatDau.Year, model.SearchModel.NgayBatDau.Month - 1, model.SearchModel.NgayBatDau.Day);

            var cuoiKyTruoc = model.SearchModel.NgayKetThuc.Month == 1
                ? new DateTime(model.SearchModel.NgayKetThuc.Year - 1, 12, model.SearchModel.NgayKetThuc.Day == 31 ? 30 : model.SearchModel.NgayKetThuc.Day)
                : new DateTime(model.SearchModel.NgayKetThuc.Year, model.SearchModel.NgayKetThuc.Month - 1, model.SearchModel.NgayKetThuc.Day == 31 ? 30 : model.SearchModel.NgayKetThuc.Day);
            // Kết hợp danh sách
            var List7KyTruoc = listBai7.Join(listBai7_2,
                bai7 => bai7.Id,
                bai7_2 => bai7_2.Id,
                (bai7, bai7_2) => new BaiTap17
                {
                    Bai7 = bai7,
                    Bai7_2 = bai7_2,
                    SL_Nhap = bai7_2.SL_Nhap
                }).Where(x => x.Bai7.Ngay_Nhap_Kho >= dauKyTruoc && x.Bai7.Ngay_Nhap_Kho <= cuoiKyTruoc)
                .ToList();

            var List11KyTruoc = listBai11.Join(listBai11_2,
                bai11 => bai11.Id,
                bai11_2 => bai11_2.Id,
                (bai11, bai11_2) => new BaiTap17
                {
                    Bai11 = bai11,
                    Bai11_2 = bai11_2,
                    SL_Xuat = bai11_2.SL_Xuat
                }).Where(x => x.Bai11.Ngay_Xuat_Kho >= dauKyTruoc && x.Bai11.Ngay_Xuat_Kho <= cuoiKyTruoc)
                .ToList();

            var viewKyTruocTheoSP = listBai3.Select(bai3 => new BaiTap17
            {
                Bai3=bai3,
                Ma_San_Pham = bai3.Ma_San_Pham,
                Ten_San_Pham = bai3.Ten_San_Pham,
                // Tìm các giá trị từ viewModelList liên quan đến sản phẩm
                SL_Dau_Ky = 0,
                SL_Nhap = List7KyTruoc.Where(x => x.Bai7_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Nhap ?? 0),
                SL_Xuat = List11KyTruoc.Where(x => x.Bai11_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Xuat ?? 0),
                SL_Cuoi_Ky = List7KyTruoc.Where(x => x.Bai7_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Nhap ?? 0)
                            -
                            List11KyTruoc.Where(x => x.Bai11_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Xuat ?? 0)
            }) .ToList();

            // Kết hợp danh sách
            var List7 = listBai7.Join(listBai7_2,
                bai7 => bai7.Id,
                bai7_2 => bai7_2.Id,
                (bai7, bai7_2) => new BaiTap17
                {
                    Bai7 = bai7,
                    Bai7_2 = bai7_2,
                    SL_Nhap = bai7_2.SL_Nhap
                }).Where(x => x.Bai7.Ngay_Nhap_Kho >= model.SearchModel.NgayBatDau && x.Bai7.Ngay_Nhap_Kho <= model.SearchModel.NgayKetThuc)
                .ToList();

            var List11 = listBai11.Join(listBai11_2,
                bai11 => bai11.Id,
                bai11_2 => bai11_2.Id,
                (bai11, bai11_2) => new BaiTap17
                {
                    Bai11 = bai11,
                    Bai11_2 = bai11_2,
                    SL_Xuat = bai11_2.SL_Xuat
                }).Where(x => x.Bai11.Ngay_Xuat_Kho >= model.SearchModel.NgayBatDau && x.Bai11.Ngay_Xuat_Kho <= model.SearchModel.NgayKetThuc)
                .ToList();
            var viewBai17TheoSP = listBai3.Select(bai3 => new BaiTap17
            {
                Bai3=bai3,
                Ma_San_Pham = bai3.Ma_San_Pham,
                Ten_San_Pham = bai3.Ten_San_Pham,
                // Tìm các giá trị từ viewModelList liên quan đến sản phẩm
                SL_Dau_Ky = viewKyTruocTheoSP.Where(x => x.Bai3.Id== bai3.Id).Sum(x => x?.SL_Cuoi_Ky ?? 0),
                SL_Nhap = List7.Where(x => x.Bai7_2.sanpham.Id == bai3.Id).Sum(x=>x?.SL_Nhap ?? 0),
                SL_Xuat = List11.Where(x => x.Bai11_2.sanpham.Id == bai3.Id).Sum(x=>x?.SL_Xuat ?? 0),
                SL_Cuoi_Ky = viewKyTruocTheoSP.Where(x => x.Bai3.Id == bai3.Id).Sum(x => x?.SL_Cuoi_Ky ?? 0)
                            +
                             List7.Where(x => x.Bai7_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Nhap ?? 0)
                            -
                             List11.Where(x => x.Bai11_2.sanpham.Id == bai3.Id).Sum(x => x?.SL_Xuat ?? 0)
            }).ToList();
            model.ListSearchSP = viewBai17TheoSP;
            return View(model);
        }
    }
}
