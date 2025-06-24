using _2280613193_webdocongnghe.Models;
using _2280613193_webdocongnghe.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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
		public IActionResult Import()
		{
			return View();
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
            product.Specifications = product.Specifications ?? new List<ProductSpecification>();
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Update(ProductUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepository.GetAllAsync();
                var brands = await _brandRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
                ViewBag.Brands = new SelectList(brands, "Id", "Name", model.BrandId);
                return View(model);
            }

            var product = await _context.Products
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Price = model.Price;
            product.Description = model.Description;
            product.CategoryId = model.CategoryId;
            product.BrandId = model.BrandId;

            if (model.ImageFile != null)
            {
                product.ImageUrl = await SaveImage(model.ImageFile);
            }

            _context.ProductSpecifications.RemoveRange(product.Specifications);

            foreach (var spec in model.Specifications)
            {
                _context.ProductSpecifications.Add(new ProductSpecification
                {
                    ProductId = model.Id,
                    SpecificationAttributeId = spec.SpecificationAttributeId,
                    Value = spec.Value
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel hợp lệ.";
                return RedirectToAction("Import");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var productList = new List<Product>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var name = worksheet.Cells[row, 1].Text?.Trim();
                            var priceText = worksheet.Cells[row, 2].Text?.Trim();
                            var desc = worksheet.Cells[row, 3].Text?.Trim();
                            var imageUrl = worksheet.Cells[row, 4].Text?.Trim();
                            var catIdText = worksheet.Cells[row, 5].Text?.Trim();
                            var brandIdText = worksheet.Cells[row, 6].Text?.Trim();

                            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(priceText))
                                continue; // bỏ dòng trống hoặc thiếu dữ liệu

                            var product = new Product
                            {
                                Name = name,
                                Price = decimal.Parse(priceText),
                                Description = desc,
                                ImageUrl = imageUrl,
                                CategoryId = string.IsNullOrEmpty(catIdText) ? null : int.Parse(catIdText),
                                BrandId = string.IsNullOrEmpty(brandIdText) ? null : int.Parse(brandIdText),
                            };

                            productList.Add(product);
                        }
                        catch (Exception ex)
                        {
                            TempData["Error"] = $"Lỗi ở dòng {row}: {ex.Message}";
                            return RedirectToAction("Import");
                        }
                    }
                }
            }

            _context.Products.AddRange(productList);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã nhập thành công {productList.Count} sản phẩm.";
            return RedirectToAction("Import");
        }


    }
}
