using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Repository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        private IWebHostEnvironment _hostEnvironment;
        public CompanyController(IUnitofwork unitofwork, IWebHostEnvironment host)
        {
            _unitofwork = unitofwork;
            _hostEnvironment = host;
        }
        public IActionResult Index()
        {
            IEnumerable<Company> list = _unitofwork.Company.GetAll();

            return View(list);
        }
        //get 
        public IActionResult Upsert(int? id)
        {

            if (id == 0 || id == null)
            {
                //create
                var company = new Company();
                return View(company);
            }
            else
            {
                //update
                var company = _unitofwork.Company.Get(c => c.Id == id);
                return View(company);
            }

        }
        //post 
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                //string wwwrootpath = _hostEnvironment.WebRootPath;
                //if (file != null)
                //{
                //    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                //    string productpath = Path.Combine(wwwrootpath, @"Images\Products");
                //    if (!string.IsNullOrEmpty(productview.Product.ImageUrl))
                //    {
                //        var oldimagepath = Path.Combine(wwwrootpath, productview.Product.ImageUrl.TrimStart('\\'));
                //        if (System.IO.File.Exists(oldimagepath))
                //        {
                //            System.IO.File.Delete(oldimagepath);
                //        }
                //    }
                //    using (var filestream = new FileStream(Path.Combine(productpath, filename), FileMode.Create))
                //    {
                //        file.CopyTo(filestream);
                //    }
                //    productview.Product.ImageUrl = @"\Images\Products\" + filename;
                //}
                if (company.Id == 0)
                {
                    _unitofwork.Company.Add(company);
                }
                else
                {
                    _unitofwork.Company.Update(company);
                }

                _unitofwork.Save();
                return RedirectToAction("Index");
            }

            return View(company);
        }
        //get

        //public IActionResult Delete(int? id)
        //{
        //    if (id != null)
        //    {
        //        Company x = _unitofwork.Company.Get(p => p.Id == id);
        //        return View(x);
        //    }
        //    return NotFound();

        //}
        ////post 
        //[HttpPost]
        //public IActionResult Delete(Company company)
        //{

        //    _unitofwork.Company.Remove(company);
        //    _unitofwork.Save();
        //    return RedirectToAction("Index");


        //}
        #region Api calls
        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<Company> list = _unitofwork.Company.GetAll();
            return Json(new { data = list });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return Json(new { Success = "false", Message = "Error" });
            }

            var companytodelete = _unitofwork.Company.Get(p => p.Id == id);
            if (companytodelete == null)
            {
                return Json(new { Success = "false", Message = "Error" });
            }

            _unitofwork.Company.Remove(companytodelete);
            _unitofwork.Save();
            return Json(new { Success = "true", Message = "deleted successfully" });
          
        }
    }


    #endregion
}

