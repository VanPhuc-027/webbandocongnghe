﻿using _2280613193_webdocongnghe.Models;
using _2280613193_webdocongnghe.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace _2280613193_webdocongnghe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;

        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository, ApplicationDbContext context)
        {
            _logger = logger;
            _productRepository = productRepository;
            _context = context;
        }

        public IActionResult Index(string search, string priceFilter, string category, string brand)
        {
            var products = _context.Products.Include(p => p.Category).Include(p => p.Brand).AsQueryable();

            // Lọc theo tên
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search));
            }

            // Lọc theo giá
            switch (priceFilter)
            {
                case "lt5":
                    products = products.Where(p => p.Price < 5000000);
                    break;
                case "5to10":
                    products = products.Where(p => p.Price >= 5000000 && p.Price <= 10000000);
                    break;
                case "gt10":
                    products = products.Where(p => p.Price > 10000000);
                    break;
            }

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(category) && category != "all")
            {
                products = products.Where(p => p.Category.Name == category);
            }

            // Lọc theo hãng
            if (!string.IsNullOrEmpty(brand) && brand != "all")
            {
                products = products.Where(p => p.Brand.Name == brand);
            }

            // Truyền dữ liệu để render lại các dropdown
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Search = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedBrand = brand;

            return View(products.ToList());
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
