using _2280613193_webdocongnghe.Models;
using _2280613193_webdocongnghe.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _2280613193_webdocongnghe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

		private readonly ApplicationDbContext _context;

		public CategoryController(ICategoryRepository categoryRepository, ApplicationDbContext context)
		{
			_categoryRepository = categoryRepository;
			_context = context;
		}

		// Danh sách
		public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

		// Form Thêm
		public IActionResult Add()
		{
			var model = new CategoryCreateViewModel
			{
				AllSpecificationAttributes = _context.SpecificationAttributes.ToList()
			};
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Add(CategoryCreateViewModel model)
		{
			if (ModelState.IsValid)
			{
				var category = new Category { Name = model.Name };
				await _categoryRepository.AddAsync(category);

				foreach (var specId in model.SelectedSpecificationAttributeIds)
				{
					var link = new CategorySpecificationAttribute
					{
						CategoryId = category.Id,
						SpecificationAttributeId = specId
					};
					_context.CategorySpecificationAttributes.Add(link);
				}

				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			// Nếu có lỗi thì nạp lại danh sách thông số
			model.AllSpecificationAttributes = _context.SpecificationAttributes.ToList();
			return View(model);
		}


        // Form Cập nhật
        public async Task<IActionResult> Update(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // Lấy các ID thông số hiện tại liên kết với category
            var selectedSpecIds = await _context.CategorySpecificationAttributes
                .Where(cs => cs.CategoryId == id)
                .Select(cs => cs.SpecificationAttributeId)
                .ToListAsync();

            ViewBag.AllSpecifications = await _context.SpecificationAttributes.ToListAsync();
            ViewBag.SelectedSpecIds = selectedSpecIds;

            return View(category);
        }


        [HttpPost]
        public async Task<IActionResult> Update(Category model, List<int> SelectedSpecificationIds)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllSpecifications = await _context.SpecificationAttributes.ToListAsync();
                ViewBag.SelectedSpecIds = SelectedSpecificationIds;
                return View(model);
            }

            _context.Update(model);

            // Xóa các liên kết cũ
            var existing = _context.CategorySpecificationAttributes.Where(cs => cs.CategoryId == model.Id);
            _context.CategorySpecificationAttributes.RemoveRange(existing);

            // Thêm lại các liên kết mới
            foreach (var specId in SelectedSpecificationIds)
            {
                _context.CategorySpecificationAttributes.Add(new CategorySpecificationAttribute
                {
                    CategoryId = model.Id,
                    SpecificationAttributeId = specId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Xác nhận Xoá
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                TempData["ErrorMessage"] = "Không thể xoá danh mục vì vẫn còn sản phẩm thuộc danh mục này.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var categorySpecs = _context.CategorySpecificationAttributes
                .Where(cs => cs.CategoryId == id);
            _context.CategorySpecificationAttributes.RemoveRange(categorySpecs);

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
