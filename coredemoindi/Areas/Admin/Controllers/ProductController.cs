using coredemo.DataAccess;
using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using coredemo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace coredemoindi.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private IUnitOfWork _unitofwork;
        private IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitofwork, IWebHostEnvironment hostEnvironment)
        {
            _unitofwork = unitofwork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

       

        //Get
        public IActionResult Upsert(int? id)
        {
            ProductVM ProductVm = new()
            {
                product = new(),
                CategoryList = _unitofwork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VariousList = _unitofwork.Various.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
            };

            if (id==0 || id == null)
            {
                //create
                //ViewBag.CategoryList = CategoryList;
                //ViewBag.VariousList = VariousList;
                return View(ProductVm);
            }
            else
            {
                //edit
                ProductVm.product = _unitofwork.Product.GetFirstOrDefault(x=>x.Productid==id);
                return View(ProductVm);
            }
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            //if (obj.Displayorderid == 0)
            //{
            //    ModelState.AddModelError("displayorderid", "Displayorder can not equal to zero.");
            //}
            if (ModelState.IsValid)
            {
                string wwwroot = _hostEnvironment.WebRootPath;
                if (file!= null)
                {
                    string fileName= Guid.NewGuid().ToString();
                    string uploads=Path.Combine(wwwroot, @"images\products");
                    string extension= Path.GetExtension(file.FileName);

                    if (obj.product.ImgURL!=null)
                    {
                        var oldimg = Path.Combine(wwwroot, obj.product.ImgURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldimg))
                        {
                            System.IO.File.Delete(oldimg);
                        }
                    }
                    using(var filestreams= new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(filestreams);
                    }
                    obj.product.ImgURL = @"\images\products\" + fileName + extension;
                }

                if (obj.product.Productid == 0)
                {
                    _unitofwork.Product.Add(obj.product);
                    TempData["success"] = "Created successfully";
                }
                else 
                {
                    _unitofwork.Product.Update(obj.product);
                    TempData["success"] = "Updated successfully";
                }

            }
            _unitofwork.Save();
            return RedirectToAction("Index");
        }

    

        #region API calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitofwork.Product.GetAll(includeProperties: "Category,Various");
            return Json(new {data = productList});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitofwork.Product.GetFirstOrDefault(x => x.Productid == id);
            if (obj == null)
            {
                return Json(new {success = false, message = "Error while deleting !"});
            }
            if (obj.ImgURL != null)
            {
                var oldimg = Path.Combine(_hostEnvironment.WebRootPath, obj.ImgURL.TrimStart('\\'));
                if (System.IO.File.Exists(oldimg))
                {
                    System.IO.File.Delete(oldimg);
                }
            }
            _unitofwork.Product.Delete(obj);
            _unitofwork.Save();
            return Json(new {success = true, message = "Deleted scucessfully !"});
        }
        #endregion
    }
}
