using _2280613193_webdocongnghe.Extensions;
using _2280613193_webdocongnghe.Models;
using _2280613193_webdocongnghe.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _2280613193_webdocongnghe.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(IProductRepository productRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    return Json(new { success = false, message = $"Sản phẩm không tồn tại (ID: {request.ProductId})" });
                }

                var item = cart.FirstOrDefault(i => i.ProductId == request.ProductId);
                if (item == null)
                {
                    cart.Add(new CartItem
                    {
                        ProductId = request.ProductId,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = request.Quantity
                    });
                }
                else
                {
                    item.Quantity += request.Quantity;
                }

                HttpContext.Session.SetObjectAsJson("Cart", cart);
                var cartCount = cart.Sum(i => i.Quantity);
                return Json(new { success = true, cartCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        public IActionResult Index(string? sortOrder)
        {
            var cart = GetCartItems();

            cart = sortOrder switch
            {
                "name_desc" => cart.OrderByDescending(c => c.Name).ToList(),
                "price_asc" => cart.OrderBy(c => c.Price).ToList(),
                "price_desc" => cart.OrderByDescending(c => c.Price).ToList(),
                _ => cart.OrderBy(c => c.Name).ToList(), // mặc định: tên tăng dần
            };

            ViewData["CurrentSort"] = sortOrder;
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int change)
        {
            var cart = GetCartItems();

            var item = cart.FirstOrDefault(p => p.ProductId == productId);
            if (item != null)
            {
                item.Quantity += change;
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
                SaveCartSession(cart);

                var updatedTotal = cart.Sum(i => i.Price * i.Quantity);
                return Json(new
                {
                    success = true,
                    quantity = item.Quantity,
                    itemTotal = item.Quantity * item.Price,
                    cartTotal = updatedTotal,
                    cartTotalQuantity = cart.Sum(i => i.Quantity)
                });
            }

            return Json(new { success = false });
        }



        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return Json(new
            {
                success = true,
                cartTotalQuantity = 0
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCartAjax(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    cart.Remove(item);
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                    var total = cart.Sum(i => i.Price * i.Quantity);
                    var quantity = cart.Sum(i => i.Quantity);
                    return Json(new
                    {
                        success = true,
                        cartTotal = total,
                        cartTotalQuantity = quantity
                    });
                }

                // Không tìm thấy sản phẩm, vẫn trả số lượng
                return Json(new
                {
                    success = false,
                    cartTotalQuantity = cart.Sum(i => i.Quantity)
                });
            }

            // Giỏ hàng null
            return Json(new
            {
                success = false,
                cartTotalQuantity = 0
            });
        }



        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);

            var model = new CheckoutViewModel
            {
                ShippingAddress = user?.Address ?? "",
                CartItems = cart,
                TotalPrice = cart.Sum(i => i.Price * i.Quantity)
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = model.ShippingAddress,
                Notes = model.Notes,
                Status = "Chờ xác nhận",
                PaymentMethod = "COD",
                TotalPrice = cart.Sum(i => i.Price * i.Quantity),
                OrderDetails = cart.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            return View("OrderCompleted", order.Id);
        }

		[Authorize]
		public async Task<IActionResult> MyOrders()
		{
			var user = await _userManager.GetUserAsync(User);
			var orders = _context.Orders
				.Where(o => o.UserId == user.Id)
				.OrderByDescending(o => o.OrderDate)
				.ToList();

			return View(orders);
		}

        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Tuỳ chọn: kiểm tra quyền người dùng (chỉ chủ sở hữu xem)
            var user = await _userManager.GetUserAsync(User);
            if (order.UserId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(order);
        }



        private async Task<Product> GetProductFromDatabase(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }

        private List<CartItem> GetCartItems()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        private void SaveCartSession(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (order.UserId != user.Id)
                return Forbid(); // Không được huỷ đơn người khác

            if (order.Status != "Chờ xác nhận" && order.Status != "Đã xác nhận")
            {
                TempData["Error"] = "Chỉ có thể huỷ đơn hàng chưa giao.";
                return RedirectToAction("MyOrders");
            }

            order.Status = "Đã hủy";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đơn hàng đã được huỷ.";
            return RedirectToAction("MyOrders");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReturn(int id, string reason)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (order.UserId != user.Id)
                return Forbid();

            if (order.Status != "Đã giao")
            {
                TempData["Error"] = "Chỉ có thể yêu cầu đổi trả khi đơn hàng đã giao.";
                return RedirectToAction("MyOrders");
            }

            order.Status = "Trả lại hàng";
            order.ReturnReason = reason;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã gửi yêu cầu đổi trả.";
            return RedirectToAction("MyOrders");
        }



    }
}
