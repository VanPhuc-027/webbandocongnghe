using _2280613193_webdocongnghe.Models;
using _2280613193_webdocongnghe.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace _2280613193_webdocongnghe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository,ICategoryRepository categoryRepository,IBrandRepository brandRepository,ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecificationsByCategory(int categoryId)
        {
            var specAttributes = await _context.CategorySpecificationAttributes
                .Where(cs => cs.CategoryId == categoryId)
                .Select(cs => new {
                    cs.SpecificationAttribute.Id,
                    cs.SpecificationAttribute.Name
                }).ToListAsync();

            return Json(specAttributes);
        }

        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();

            var specAttributes = await _context.SpecificationAttributes.ToListAsync();
            ViewBag.SpecAttributes = specAttributes;

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            return View(new ProductCreateViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Add(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepository.GetAllAsync();
                var brands = await _brandRepository.GetAllAsync();
                var specAttributes = await _context.SpecificationAttributes.ToListAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                ViewBag.Brands = new SelectList(brands, "Id", "Name");
                ViewBag.SpecAttributes = specAttributes;
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                CategoryId = model.CategoryId,
                BrandId = model.BrandId,
                ImageUrl = ""
            };

            if (model.ImageFile != null)
            {
                product.ImageUrl = await SaveImage(model.ImageFile);
            }

            await _productRepository.AddAsync(product);

            // Lưu các thông số kỹ thuật nếu có
            foreach (var spec in model.Specifications)
            {
                var ps = new ProductSpecification
                {
                    ProductId = product.Id,
                    SpecificationAttributeId = spec.SpecificationAttributeId,
                    Value = spec.Value
                };
                _context.ProductSpecifications.Add(ps);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }

		public async Task<IActionResult> Display(int id)
		{
			var product = await _context.Products
				.Include(p => p.Category)
				.Include(p => p.Brand)
				.Include(p => p.Specifications)
					.ThenInclude(ps => ps.SpecificationAttribute)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}

		public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name",product.CategoryId);
            ViewBag.Brands = new SelectList(brands, "Id", "Name", product.BrandId);
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product,IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl"); 
            if (id != product.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var existingProduct = await
                _productRepository.GetByIdAsync(id);
                if (imageUrl == null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    product.ImageUrl = await SaveImage(imageUrl);

                }
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;
                existingProduct.ImageUrl = product.ImageUrl;
                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
