using coredemo.DataAccess;
using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace coredemoindi.Controllers
{
    [Area("Admin")]
    public class VariousController : Controller
    {
        private IUnitOfWork _unitofwork;

        public VariousController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            IEnumerable<Various> retrieve = _unitofwork.Various.GetAll();
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
        public IActionResult Create(Various obj)
        {
          
            if (ModelState.IsValid)
            {
                _unitofwork.Various.Add(obj);
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
            var Variousfromdb = _unitofwork.Various.GetFirstOrDefault(x=>x.Id == id);
            if (Variousfromdb == null)
            {
                return NotFound();
            }
            return View(Variousfromdb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Various obj)
        {

            if (ModelState.IsValid)
            {
                _unitofwork.Various.Update(obj);
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
            var Variousfromdb = _unitofwork.Various.GetFirstOrDefault(x => x.Id == id);

            if (Variousfromdb == null)
            {
                return NotFound();
            }
            return View(Variousfromdb);
        }

        //Post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var Variousfromdb = _unitofwork.Various.GetFirstOrDefault(x => x.Id == id);
            if (Variousfromdb == null)
            {
                return NotFound();
            }
            _unitofwork.Various.Delete(Variousfromdb);
            _unitofwork.Save();
            TempData["success"] = "Deleted successfully";
            return RedirectToAction("Index");
        }

    }
}
