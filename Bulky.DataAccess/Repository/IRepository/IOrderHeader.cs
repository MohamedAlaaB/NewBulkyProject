using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bulky.Models;
namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IOrderHeader : IRepo<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, string orderstatus, string? paymentstatus);
        void UpdateStripePaymentId(int id, string sessionid ,string paymentintentid);
    }
}
