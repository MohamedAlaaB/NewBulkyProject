
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private IUnitofwork _context;

        public CategoryController(IUnitofwork context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> cl = _context.Category.GetAll();
            return View(cl);
        }
        ///Get

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]

        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "name and display order can't be the same");
            }
            if (ModelState.IsValid)
            {
                _context.Category.Add(category);
                _context.Save();
                TempData["Success"] = "Category Created successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Category not Created successfully";
            return View(category);


        }
        //get
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? category = _context.Category.Get(c => c.Id == id);

            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (category == null)
            {
                return NotFound();
            }
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "name and display order can't be the same");
            }
            if (ModelState.IsValid)
            {
                _context.Category.Update(category);
                _context.Save();
                TempData["Success"] = "Category Edited successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Category not Edited successfully";
            return View(category);
        }
        //get
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? category = _context.Category.Get(c => c.Id == id);
            return View(category);
        }
        [HttpPost]
        public IActionResult Delete(Category category)
        {
            if (category == null)
            {
                return NotFound();
            }
            _context.Category.Remove(category);
            _context.Save();
            TempData["Success"] = "Category deleted successfully";
            return RedirectToAction("Index");



        }
    }
}
