using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using coredemo.Models.ViewModels;
using coredemo.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace coredemoindi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM _orderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            _orderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderId == orderId, includeProperties: "Product")
            }; 
            return View(_orderVM);
        }

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_pay_now()
        {
            _orderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            _orderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderId == _orderVM.OrderHeader.Id, includeProperties: "Product");
            var url = "https://localhost:44325";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = url + $"/Admin/order/PaymentConfirmation?orderHeaderid={_orderVM.OrderHeader.Id}",
                CancelUrl = url + $"/Admin/order/details?orderId={_orderVM.OrderHeader.Id}",
            };

            foreach (var item in _orderVM.OrderDetails)
            {
                var sessionListItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },

                    },
                    Quantity = 1,
                };
                options.LineItems.Add(sessionListItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripeSettings(_orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderHeaderid);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
           
            return View(orderHeaderid);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDetails()
        {
            var orderheaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _orderVM.OrderHeader.Id, tracked: false);
            orderheaderFromDb.State = _orderVM.OrderHeader.State;
            orderheaderFromDb.Name = _orderVM.OrderHeader.Name;
            orderheaderFromDb.PhoneNumber = _orderVM.OrderHeader.PhoneNumber;
            orderheaderFromDb.StreetAddress = _orderVM.OrderHeader.StreetAddress;
            orderheaderFromDb.City = _orderVM.OrderHeader.City;
            orderheaderFromDb.PostalCode = _orderVM.OrderHeader.PostalCode;

            if (_orderVM.OrderHeader.TrackingNumber != null)
            {
                orderheaderFromDb.TrackingNumber = _orderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderheaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Updated successfully";
            return RedirectToAction("Details", "Order", new {orderId= _orderVM.OrderHeader.Id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult startProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(_orderVM.OrderHeader.Id, SD.StatusInProcess, SD.PaymentStatusDelayedPayment);
            _unitOfWork.Save();
            TempData["success"] = "Status Updated to Processing successfully";
            return RedirectToAction("Details", "Order", new { orderId = _orderVM.OrderHeader.Id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ordershipped()
        {
            var orderheaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _orderVM.OrderHeader.Id, tracked: false);
            orderheaderFromDb.TrackingNumber = _orderVM.OrderHeader.TrackingNumber;
            orderheaderFromDb.ShippingDate = DateTime.Now;
            orderheaderFromDb.OrderStatus = SD.StatusShipped;
            if (orderheaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderheaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unitOfWork.OrderHeader.Update(orderheaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Status Shipped successfully";
            return RedirectToAction("Details", "Order", new { orderId = _orderVM.OrderHeader.Id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderheaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == _orderVM.OrderHeader.Id, tracked: false);
            if (orderheaderFromDb.PaymentStatus==SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderheaderFromDb.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderheaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);

            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderheaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["success"] = "Status Shipped successfully";
            return RedirectToAction("Details", "Order", new { orderId = _orderVM.OrderHeader.Id });
        }


        #region API calls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.Role_Admin)  || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(x=>x.ApplicationUserId==claim.Value,includeProperties: "ApplicationUser");

            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
