using _2280613193_webdocongnghe.Models;
using Microsoft.AspNetCore.Mvc;

namespace _2280613193_webdocongnghe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SpecificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpecificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị form nhập thông số
        public IActionResult Add(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            ViewBag.SpecAttributes = _context.SpecificationAttributes.ToList();
            ViewBag.Product = product;

            return View(new ProductSpecificationViewModel { ProductId = productId });
        }

        // POST: Lưu thông số
        [HttpPost]
        public IActionResult Add(ProductSpecificationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            foreach (var spec in model.Specifications)
            {
                var ps = new ProductSpecification
                {
                    ProductId = model.ProductId,
                    SpecificationAttributeId = spec.SpecificationAttributeId,
                    Value = spec.Value
                };

                _context.ProductSpecifications.Add(ps);
            }

            _context.SaveChanges();
            return RedirectToAction("Details", "Product", new { id = model.ProductId });
        }
    }


}
