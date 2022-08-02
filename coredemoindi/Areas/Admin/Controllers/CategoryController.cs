using coredemo.DataAccess;
using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace coredemoindi.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private IUnitOfWork _unitofwork;

        public CategoryController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> retrieve = _unitofwork.Category.GetAll();
            return View(retrieve);
        }

        //Get
        public IActionResult Create()
        {
            return View();
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Displayorderid ==0)
            {
                ModelState.AddModelError("displayorderid", "Displayorder can not equal to zero.");
            }
            if (ModelState.IsValid)
            {
                _unitofwork.Category.Add(obj);
                _unitofwork.Save();
                TempData["success"] = "Added successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //Get
        public IActionResult Edit(int? id)
        {
            if (id==0 || id == null)
            {
                return NotFound();
            }
            var categoryfromdb = _unitofwork.Category.GetFirstOrDefault(x=>x.Id == id);
            if (categoryfromdb == null)
            {
                return NotFound();
            }
            return View(categoryfromdb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Displayorderid == 0)
            {
                ModelState.AddModelError("displayorderid", "Displayorder can not equal to zero.");
            }
            if (ModelState.IsValid)
            {
                _unitofwork.Category.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //Get
        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var categoryfromdb = _unitofwork.Category.GetFirstOrDefault(x => x.Id == id);

            if (categoryfromdb == null)
            {
                return NotFound();
            }
            return View(categoryfromdb);
        }

        //Post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var categoryfromdb = _unitofwork.Category.GetFirstOrDefault(x => x.Id == id);
            if (categoryfromdb == null)
            {
                return NotFound();
            }
            _unitofwork.Category.Delete(categoryfromdb);
            _unitofwork.Save();
            TempData["success"] = "Deleted successfully";
            return RedirectToAction("Index");
        }

    }
}
