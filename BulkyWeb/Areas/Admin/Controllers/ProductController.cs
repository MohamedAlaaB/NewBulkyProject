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
using Microsoft.AspNetCore.Hosting;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        private IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitofwork unitofwork ,IWebHostEnvironment host)
        {
                _unitofwork = unitofwork;
                 _hostEnvironment= host;
        }
        public IActionResult Index()
        {
            IEnumerable<Product> list = _unitofwork.Product.GetAll(includeprops: "Category");
            
            return View(list);
        }
        //get 
        public IActionResult Upsert(int? id)
        {
            var viewmodels = new ProductViewModels();
            viewmodels.Product = new Product();
            viewmodels.Categories = _unitofwork.Category.GetAll().Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            if (id == 0 || id == null)
            {
                //create
                return View(viewmodels);
            }
            else
            {
                //update
                viewmodels.Product = _unitofwork.Product.Get(p  => p.Id == id);
                return View(viewmodels);
            }
            
        }
        //post 
        [HttpPost]
        public IActionResult Upsert(ProductViewModels productview,IFormFile file)
        {
            if(ModelState.IsValid)
            {
                string wwwrootpath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productpath = Path.Combine(wwwrootpath, @"Images\Products");
                    if (!string.IsNullOrEmpty(productview.Product.ImageUrl))
                    {
                        var oldimagepath = Path.Combine(wwwrootpath, productview.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldimagepath))
                        {
                            System.IO.File.Delete(oldimagepath);
                        }
                    }
                    using (var filestream = new FileStream(Path.Combine(productpath, filename), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productview.Product.ImageUrl = @"\Images\Products\" + filename;
                }
                if (productview.Product.Id == 0 )
                {
                    _unitofwork.Product.Add(productview.Product);
                }
                else
                {
                   _unitofwork.Product.Update(productview.Product);
                }
               
                _unitofwork.Save();
                return RedirectToAction("Index");
            }
            var viewmodels = new ProductViewModels();
            viewmodels.Product = new Product();
            viewmodels.Categories = _unitofwork.Category.GetAll().Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            return View(viewmodels);
        }
        //get 
       
        //public IActionResult Delete(int? id)
        //{
        //    if (id != null)
        //    {
        //        Product x = _unitofwork.Product.Get(p => p.Id == id);
        //        return View(x);
        //    }
        //    return NotFound();

        //}
        ////post 
        //[HttpPost]
        //public IActionResult Delete(Product product)
        //{
          
        //        _unitofwork.Product.Remove(product);
        //        _unitofwork.Save();
        //        return RedirectToAction("Index");
        
           
        //}
        #region Api calls
        [HttpGet]
        public IActionResult GetAll() 
        {
            IEnumerable<Product> list = _unitofwork.Product.GetAll(includeprops: "Category").ToList();
            return Json(new {data=list});
        }
        //[HttpDelete]
        //public IActionResult Delete(int? id)
        //{
           
           
        //    var producttodelete = _unitofwork.Product.Get(p => p.Id == id);
        //    if (producttodelete == null)
        //    {
        //        return Json(new { Success = "false", Message = "Error" });
        //    }
        //    string wwwrootpath = _hostEnvironment.WebRootPath;
        //    var oldimagepath = Path.Combine(wwwrootpath, producttodelete.ImageUrl.TrimStart('\\'));
        //    if (System.IO.File.Exists(oldimagepath))
        //    {
        //        System.IO.File.Delete(oldimagepath);
        //    }
        //    _unitofwork.Product.Remove(producttodelete);
        //    _unitofwork.Save();
        //    return Json(new { Success = "true", Message = "deleted successfully" });
        //}

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitofwork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string wwwrootpath = _hostEnvironment.WebRootPath;
            var oldimagepath = Path.Combine(wwwrootpath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldimagepath))
            {
                System.IO.File.Delete(oldimagepath);
            }


            _unitofwork.Product.Remove(productToBeDeleted);
            _unitofwork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
