using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Bulky.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class OrderHeaderRepo : Repo<OrderHeader>, IOrderHeader
    {
        private readonly ApplicationDbContext _context;
       
        public OrderHeaderRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
            
            
        }

     
        public void Update(OrderHeader orderHeader)
        {
           _context.OrderHeaders.Update(orderHeader);
        }

		public void UpdateStatus(int id, string orderstatus, string? paymentstatus = null)
		{
			var orderheaderfromdb = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderheaderfromdb != null)
            {
                orderheaderfromdb.OrderStatus = orderstatus;
            }
            if (!string.IsNullOrEmpty(paymentstatus))
            {
                orderheaderfromdb.PaymentStatus = paymentstatus;
            }
		}

		public void UpdateStripePaymentId(int id, string sessionid, string paymentintentid)
		{
			var orderheaderfromdb = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(sessionid))
            {
				orderheaderfromdb.SessionId = sessionid;

			}
            if (string.IsNullOrEmpty(paymentintentid))
            {
                orderheaderfromdb.PaymentIntentId = paymentintentid;
                orderheaderfromdb.OrderDate = DateTime.Now;
            }
		}
	}
}
