using coredemo.DataAccess;
using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using coredemo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace coredemoindi.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private IUnitOfWork _unitofwork;


        public CompanyController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
           
        }

        public IActionResult Index()
        {
            return View();
        }

       

        //Get
        public IActionResult Upsert(int? id)
        {
            Company company = new();

            if (id==0 || id == null)
            {
                //create
                //ViewBag.CategoryList = CategoryList;
                //ViewBag.VariousList = VariousList;
                return View(company);
            }
            else
            {
                //edit
                company = _unitofwork.Company.GetFirstOrDefault(x=>x.Id==id);
                return View(company);
            }
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            //if (obj.Displayorderid == 0)
            //{
            //    ModelState.AddModelError("displayorderid", "Displayorder can not equal to zero.");
            //}
            if (ModelState.IsValid)
            {
                if (obj.Id== 0)
                {
                    _unitofwork.Company.Add(obj);
                    TempData["success"] = "Created successfully";
                }
                else 
                {
                    _unitofwork.Company.Update(obj);
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
            var companyList = _unitofwork.Company.GetAll();
            return Json(new {data = companyList});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitofwork.Company.GetFirstOrDefault(x => x.Id == id);
            if (obj == null)
            {
                return Json(new {success = false, message = "Error while deleting !"});
            }
            
            _unitofwork.Company.Delete(obj);
            _unitofwork.Save();
            return Json(new {success = true, message = "Deleted scucessfully !"});
        }
        #endregion
    }
}
