using coredemo.DataAccess.Repository;
using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using coredemo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace coredemoindi.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfwork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfwork)
        {
            _logger = logger;
            _unitOfwork = unitOfwork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfwork.Product.GetAll();
            return View(productList);
        }
        
        public IActionResult Details(int productId)
        {
            ShoppingCart productList = new()
            {
                Count = 1,
                ProductId = productId,
                product = _unitOfwork.Product.GetFirstOrDefault(x => x.Productid == productId, includeProperties: "Category,Various")
            };
            return View(productList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var identity = claim.Value;
            shoppingCart.ApplicationUserId = identity;

            var cartFromDb = _unitOfwork.ShoppingCart.GetFirstOrDefault(x => x.ApplicationUserId == claim.Value && x.ProductId == shoppingCart.ProductId);
            if (cartFromDb==null)
            {
                _unitOfwork.ShoppingCart.Add(shoppingCart);
            }
            else
            {
                _unitOfwork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
            }
            _unitOfwork.Save();
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