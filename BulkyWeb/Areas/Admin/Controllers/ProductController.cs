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
                viewmodels.Product = _unitofwork.Product.Get(p => p.Id == id, includeprops: "Images") ;
                return View(viewmodels);
            }
            
        }
        //post 
        [HttpPost]
        public IActionResult Upsert(ProductViewModels productview,List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (productview.Product.Id == 0)
                {
                    _unitofwork.Product.Add(productview.Product);
                }
                else
                {
                    _unitofwork.Product.Update(productview.Product);
                }

                _unitofwork.Save();
                string wwwrootpath = _hostEnvironment.WebRootPath;
                if (files != null)
                {
                
                    foreach (IFormFile file in files)  
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productpath = @"Images\Products\Product-" + productview.Product.Id;
                        string finalpath = Path.Combine(wwwrootpath, productpath);
                        if (!Directory.Exists(finalpath))
                        {
                            Directory.CreateDirectory(finalpath);
                        }
                        using (var filestream = new FileStream(Path.Combine(finalpath, filename), FileMode.Create))
                        {
                            file.CopyTo(filestream);
                        }
                        Images productimg = new Images()
                        {
                            Url = @"\" + productpath + @"\" + filename,
                            productId = productview.Product.Id,
                        };
                       
                        if (productview.Product.Images == null)
                        {
                            productview.Product.Images = new List<Images>();
                        }
                        productview.Product.Images.Add(productimg);
                    }
                    _unitofwork.Product.Update(productview.Product);
                    _unitofwork.Save(); 
                }
                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");

            }
            else
            {
                productview.Categories = _unitofwork.Category.GetAll().Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            }
           
            return View(productview);
        }
        public IActionResult DeleteImage(int imageId) 
        {
            Images imagetodelete = _unitofwork.image.Get(x=>x.Id == imageId);
            int productid = imagetodelete.productId;
            if (imagetodelete != null ) 
            {
                if (!(imagetodelete.Url).IsNullOrEmpty())
                {
                    string wwwrootpath = _hostEnvironment.WebRootPath;
                    var oldimagepath = Path.Combine(wwwrootpath, imagetodelete.Url.TrimStart('\\'));
                    if (System.IO.File.Exists(oldimagepath))
                    {
                        System.IO.File.Delete(oldimagepath);
                    }
                    
                }
                _unitofwork.image.Remove(imagetodelete);
                
                _unitofwork.Save();
                TempData["success"] = "Deleted successfully";

            }
            return RedirectToAction(nameof(Upsert), new {id= productid });
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
            string productpath = @"Images\Products\Product-" + id;
            string finalpath = Path.Combine(wwwrootpath, productpath);
            if (Directory.Exists(finalpath))
            {
                string[] urls = Directory.GetFiles(finalpath);
                foreach (string url in urls) 
                {  
                    System.IO.File.Delete(url);
                }
                Directory.Delete(finalpath);
            }


            _unitofwork.Product.Remove(productToBeDeleted);
            _unitofwork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
