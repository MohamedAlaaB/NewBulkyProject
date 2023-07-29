using Bulky.DataAccess.Repository.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.ViewComponents
{
    public class ShoppingCartViewComponent :ViewComponent
    {
        private readonly IUnitofwork _unitOfWork;
        public ShoppingCartViewComponent(IUnitofwork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {

                if (HttpContext.Session.GetInt32(SD.CartSession) == null)
                {
                    HttpContext.Session.SetInt32(SD.CartSession,
                    _unitOfWork.ShoppingCart.GetAll(filter:s => s.UserId == claim.Value).Count());
                }

                return View(HttpContext.Session.GetInt32(SD.CartSession));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
