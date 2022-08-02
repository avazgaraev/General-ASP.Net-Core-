using coredemo.DataAccess.Repository;
using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using coredemo.Models.ViewModels;
using coredemo.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace coredemoindi.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
                _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(r => r.ApplicationUserId == claim.Value, includeProperties:"product"),
                OrderHeader = new()
            };
            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                item.Price = GetEndPrice(item.Count, item.product.Price, item.product.Price50, item.product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(r => r.ApplicationUserId == claim.Value, includeProperties: "product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x=>x.Id==claim.Value);

            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                item.Price = GetEndPrice(item.Count, item.product.Price, item.product.Price50, item.product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            return View(ShoppingCartVM);
        } 
        
        
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(r => r.ApplicationUserId == claim.Value, includeProperties: "product");

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                item.Price = GetEndPrice(item.Count, item.product.Price, item.product.Price50, item.product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);

            if (applicationUser.CompanyId.GetValueOrDefault() ==0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus= SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                OrderDetail orderDetail = new()
                {
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count,
                    ProductId = item.ProductId,

                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }



            if (applicationUser.CompanyId == null)
            {


                //Stripe Settings
                var url = "https://localhost:44325";
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),

                    Mode = "payment",
                    SuccessUrl = url + $"/customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = url + $"/customer/cart/index",
                };

                foreach (var item in ShoppingCartVM.ShoppingCarts)
                {
                    var sessionListItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.product.Title,
                            },

                        },
                        Quantity = 1,
                    };
                    options.LineItems.Add(sessionListItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripeSettings(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation" , "Cart", new {id=ShoppingCartVM.OrderHeader.Id});
                
            }


            //_unitOfWork.ShoppingCart.DeleteMore(ShoppingCartVM.ShoppingCarts);
            //_unitOfWork.Save();
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(x=>x.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.DeleteMore(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }


        public IActionResult Plus(int cartId)
        {
            var spcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(spcart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var spcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (spcart.Count<=1)
            {
                _unitOfWork.ShoppingCart.Delete(spcart);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(spcart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult Remove(int cartId)
        {
            var spcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Delete(spcart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        private double GetEndPrice(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if (quantity  <= 100)
                {
                    return price50;
                }
                return price100;
            }
        }
    }
}
