using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]

    public class OrderController : Controller
	{
		private readonly IUnitofwork _unitofwork;
		[BindProperty]
		public OrderVM order { get; set; }

        public OrderController(IUnitofwork unitofwork)
        {
			_unitofwork = unitofwork;
        }
        public IActionResult Index()
		{
			
			return View();
		}
		public IActionResult Detail(int orderid) 
		{
			 order = new()
			{
				OrderDetails=_unitofwork.OrderDetail.GetAll(filter:u=>u.OrderHeaderId == orderid,includeprops:"Product"),
				OrderHeader=_unitofwork.OrderHeader.Get(filter:u=>u.Id == orderid,includeprops: "ApplicationUser")
			};
			return View(order);
		}
		[HttpPost]
		[Authorize(Roles=SD.Role_Admin+","+SD.Role_Employee )]
		public IActionResult UpdateOrderDetail()
		{
			OrderHeader tobeupdated = _unitofwork.OrderHeader.Get(o => o.Id == order.OrderHeader.Id);
			tobeupdated.StreetAddress = order.OrderHeader.StreetAddress;
			tobeupdated.PhoneNumber = order.OrderHeader.PhoneNumber;
			tobeupdated.City = order.OrderHeader.City;
			tobeupdated.PostalCode = order.OrderHeader.PostalCode;
			tobeupdated.State = order.OrderHeader.State;
			tobeupdated.Name = order.OrderHeader.Name;
			if (!string.IsNullOrEmpty(order.OrderHeader.TrackingNumber))
			{
                tobeupdated.TrackingNumber = order.OrderHeader.TrackingNumber;
            }
            if (!string.IsNullOrEmpty(order.OrderHeader.Carrier))
            {
                tobeupdated.Carrier = order.OrderHeader.Carrier;
            }
           
			_unitofwork.OrderHeader.Update(tobeupdated);
			_unitofwork.Save();
			TempData["Success"] = "Data updated successfully";
			return RedirectToAction(nameof(Detail), new {orderid=order.OrderHeader.Id});
		}
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult StartProcessing()
		{
            OrderHeader tobeupdated = _unitofwork.OrderHeader.Get(o => o.Id == order.OrderHeader.Id);
			_unitofwork.OrderHeader.UpdateStatus(id:tobeupdated.Id, orderstatus:SD.StatusInProcess,null);
			
			_unitofwork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Detail), new { orderid = order.OrderHeader.Id });
        }
		[HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder() 
		{
            OrderHeader tobeupdated = _unitofwork.OrderHeader.Get(o => o.Id == order.OrderHeader.Id);
			tobeupdated.Carrier = order.OrderHeader.Carrier;
			tobeupdated.TrackingNumber = order.OrderHeader.TrackingNumber;
			tobeupdated.ShippingDate = order.OrderHeader.ShippingDate;
            tobeupdated.OrderStatus = SD.StatusShipped;
            if (tobeupdated.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
                tobeupdated.PaymentDueDate = DateTime.Now.AddDays(30);
			}
            _unitofwork.OrderHeader.Update(tobeupdated);
            _unitofwork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Detail), new { orderid = order.OrderHeader.Id });
        }
		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult CancelOrder()
		{
            OrderHeader tobeupdated = _unitofwork.OrderHeader.Get(o => o.Id == order.OrderHeader.Id);
			if (tobeupdated.PaymentStatus == SD.PaymentStatusApproved)
			{
				var options = new RefundCreateOptions() { Reason=RefundReasons.RequestedByCustomer , PaymentIntent =tobeupdated.PaymentIntentId};
				var service = new RefundService();
				Refund refund= service.Create(options);
				_unitofwork.OrderHeader.UpdateStatus(tobeupdated.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
                _unitofwork.OrderHeader.UpdateStatus(tobeupdated.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
			_unitofwork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Detail), new { orderid = order.OrderHeader.Id });
        }
        [ActionName("Detail")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            order.OrderHeader = _unitofwork.OrderHeader
                .Get(u => u.Id == order.OrderHeader.Id, includeprops: "ApplicationUser");
            order.OrderDetails = _unitofwork.OrderDetail
                .GetAll(filter:u => u.OrderHeaderId == order.OrderHeader.Id, includeprops: "Product");

            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={order.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={order.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in order.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            _unitofwork.OrderHeader.UpdateStripePaymentId(order.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitofwork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = _unitofwork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofwork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitofwork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitofwork.Save();
                }


            }


            return View(orderHeaderId);
        }


        #region Apicalls
        [HttpGet]
		public IActionResult GetAll(string? status)
		{
			IEnumerable<OrderHeader> orders;
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			if (userId == SD.Role_Employee || userId==SD.Role_Admin)
			{
				orders = _unitofwork.OrderHeader.GetAll( includeprops: "ApplicationUser").ToList();
			}
			else
			{
                orders = _unitofwork.OrderHeader.GetAll(filter: o => o.ApplicationUserId == userId ,includeprops:"ApplicationUser");
            }

            switch (status)
			{
				case ("pending"): orders = orders.Where(u => u.OrderStatus == SD.StatusPending);

                        break;
                case "approved":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusApproved);

                    break;
                case "inprocess":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusInProcess);

                    break;
                case "completed":
                    orders = orders.Where(u => u.OrderStatus == SD.StatusShipped);

                    break;
                default:
					break;
			}


			return Json(new { data = orders });
		}
		#endregion
	}
}
