using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Stripe.Checkout;
using System.Data;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]

    [Authorize]
    public class CartController : Controller
    {
        private IUnitofwork _context;
        [BindProperty]
        public ShoppingCartVM shoppingmodel { get; set; }


		public CartController(IUnitofwork context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var shoppingmodel = new ShoppingCartVM
            {
                ShoppingCartList = _context.ShoppingCart.GetAll(filter: c => c.UserId == userId, includeprops: "Product"),
                OrderHeader=new OrderHeader ()
            };
            foreach (var item in shoppingmodel.ShoppingCartList)
            {
                item.Price = pricebasedonquantity(item);
                shoppingmodel.OrderHeader.OrderTotal += item.Price * item.Count;
            }
            return View(shoppingmodel);
        }
        public IActionResult Plus(int cartid) 
        { 
            var cart = _context.ShoppingCart.Get(filter: c => c.Id == cartid);
            cart.Count += 1;
            _context.ShoppingCart.Update(cart);
            _context.Save();
          
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartid) 
        {
            var cart = _context.ShoppingCart.Get(filter: c => c.Id == cartid,tracking:true);
            if (cart.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.CartSession, _context.ShoppingCart.GetAll(filter: s => s.UserId == cart.UserId).Count() - 1);
                _context.ShoppingCart.Remove(cart);
            }
            else
            {
                cart.Count -= 1;
                _context.ShoppingCart.Update(cart);
            }
           
            _context.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int cartid) 
        {
            var cart = _context.ShoppingCart.Get(filter: c => c.Id == cartid,tracking:true) ;
            HttpContext.Session.SetInt32(SD.CartSession, _context.ShoppingCart.GetAll(filter: s => s.UserId == cart.UserId).Count() - 1);
            _context.ShoppingCart.Remove(cart);
      
            _context.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var shoppingmodel = new ShoppingCartVM
            {
                ShoppingCartList = _context.ShoppingCart.GetAll(filter: c => c.UserId == userId, includeprops: "Product"),
                OrderHeader = new OrderHeader()
            };
            shoppingmodel.OrderHeader.ApplicationUser =_context.AppUser.Get(u=>u.Id == userId);

            shoppingmodel.OrderHeader.Name = shoppingmodel.OrderHeader.ApplicationUser.Name;
            shoppingmodel.OrderHeader.PhoneNumber = shoppingmodel.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingmodel.OrderHeader.StreetAddress = shoppingmodel.OrderHeader.ApplicationUser.StreetAddress;
            shoppingmodel.OrderHeader.City = shoppingmodel.OrderHeader.ApplicationUser.City;
            shoppingmodel.OrderHeader.State = shoppingmodel.OrderHeader.ApplicationUser.State;
            shoppingmodel.OrderHeader.PostalCode = shoppingmodel.OrderHeader.ApplicationUser.PostalCode;
            foreach (var item in shoppingmodel.ShoppingCartList)
            {
                item.Price = pricebasedonquantity(item);
                shoppingmodel.OrderHeader.OrderTotal += item.Price * item.Count;
            }
            return View(shoppingmodel);
        }

        [HttpPost]
        [ActionName("Summary")]

        public IActionResult SummaryOnPost()
        {
			var claimsidentity = (ClaimsIdentity)User.Identity;
			var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingmodel.ShoppingCartList = _context.ShoppingCart.GetAll(filter: c => c.UserId == userId, includeprops: "Product");
			Appuser ApplicationUser = _context.AppUser.Get(u => u.Id == userId);
            shoppingmodel.OrderHeader.ApplicationUserId= userId;
			shoppingmodel.OrderHeader.OrderDate = DateTime.Now;
			foreach (var item in shoppingmodel.ShoppingCartList)
			{
				item.Price = pricebasedonquantity(item);
				shoppingmodel.OrderHeader.OrderTotal += item.Price * item.Count;
			}
            if (ApplicationUser.CompanyId.GetValueOrDefault()==0)
            {
                ///change status to pending
                shoppingmodel.OrderHeader.OrderStatus = SD.StatusPending;
				///change payment to pending
				shoppingmodel.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			}
			else
            {
				////no payment required
				
				shoppingmodel.OrderHeader.OrderStatus = SD.StatusApproved;
				
				shoppingmodel.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
			}
             _context.OrderHeader.Add(shoppingmodel.OrderHeader);
            _context.Save();
            foreach (var item in shoppingmodel.ShoppingCartList)
            {
                OrderDetail orderdetail = new()
                {
                    OrderHeaderId = shoppingmodel.OrderHeader.Id,
                    ProductId= item.ProductId,
                    Price = item.Price,
                    Count= item.Count
                    

                };
               _context.OrderDetail.Add(orderdetail);
                _context.Save();
                
            }
			if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                //redirect tostrip payment
                string Domain = "https://localhost:7108/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = Domain+$"Customer/Cart/OrderConfirmation?id={shoppingmodel.OrderHeader.Id}",
                    CancelUrl = Domain+"Customer/Cart/Index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};
                foreach (var item in shoppingmodel.ShoppingCartList)
                {
                    var sessionlineitem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title,
                                
                            }
                        },Quantity=item.Count
                    
					
					};
                    options.LineItems.Add(sessionlineitem);
                }
				
				var service = new SessionService();
				Session session = service.Create(options);
                _context.OrderHeader.UpdateStripePaymentId(shoppingmodel.OrderHeader.Id,session.Id,session.PaymentIntentId);
                _context.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}
			else
			{
				return RedirectToAction(nameof(OrderConfirmation),new { id = shoppingmodel.OrderHeader.Id });
			}
			return View(shoppingmodel);

		}
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderheader = _context.OrderHeader.Get(o=>o.Id==id);
            if (orderheader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
				var service = new SessionService();
                Session session = service.Get(orderheader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _context.OrderHeader.UpdateStripePaymentId(id,session.Id,session.PaymentIntentId);
                    _context.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _context.Save();
                }
                HttpContext.Session.Clear();
			}
            List<ShoppingCart> listtodelete = _context.ShoppingCart.GetAll(filter:c => c.UserId == orderheader.ApplicationUserId).ToList();

            _context.ShoppingCart.RemoveRange(listtodelete);
            _context.Save();
            return View(id);
        }

        private double pricebasedonquantity(ShoppingCart cart)
        {
          
                if(cart.Count <= 50)
                {
                return cart.Product.Price;
                }
                else
                {
                    if (cart.Count <= 100)
                {
                    return cart.Product.Price50;
                }
                     else
                    {
                    return cart.Product.Price100;
                    }
                }
            
        }
    }
    
}
