using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
    
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofwork unitofwork;

        public HomeController(ILogger<HomeController> logger, IUnitofwork unitofwork)
        {
            _logger = logger;
            this.unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
          
            IEnumerable<Product> products = unitofwork.Product.GetAll(includeprops: "Category,Images");
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            var shoppingcart = new ShoppingCart();
            shoppingcart.Product = unitofwork.Product.Get(p=>p.Id==productId,includeprops: "Category,Images");
            shoppingcart.Count = 1;
            shoppingcart.ProductId = productId;
            return View(shoppingcart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingcart)
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var UserId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
           shoppingcart.UserId = UserId;
            var shoppingcartfromdb =unitofwork.ShoppingCart.Get(s=>s.UserId == shoppingcart.UserId && s.ProductId == shoppingcart.ProductId );
            if (shoppingcartfromdb != null)
            {
                shoppingcartfromdb.Count += shoppingcart.Count;
                unitofwork.ShoppingCart.Update(shoppingcartfromdb);
                unitofwork.Save();
            }
            else
            {

                unitofwork.ShoppingCart.Add(shoppingcart);
                unitofwork.Save();
                HttpContext.Session.SetInt32(SD.CartSession,unitofwork.ShoppingCart.GetAll(filter:s => s.UserId == shoppingcart.UserId).Count());
            }
           
           
            TempData["Success"] = "Shopping cart updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}