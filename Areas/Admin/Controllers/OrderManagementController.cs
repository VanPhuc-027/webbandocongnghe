using _2280613193_webdocongnghe.Helpers;
using _2280613193_webdocongnghe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _2280613193_webdocongnghe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            string currentStatus = order.Status;

            // Kiểm tra trạng thái hợp lệ
            if (!OrderStatusFlow.ValidTransitions.ContainsKey(currentStatus))
                return BadRequest("Trạng thái hiện tại không hợp lệ.");

            var allowedNextStatuses = OrderStatusFlow.ValidTransitions[currentStatus];

            if (!allowedNextStatuses.Contains(status))
                return BadRequest($"Không thể chuyển từ '{currentStatus}' sang '{status}'.");

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ReturnDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.Status == "Trả lại hàng");

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(int id, string decision)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null || order.Status != "Trả lại hàng")
                return NotFound();

            if (decision == "accept")
            {
                order.Status = "Đã hoàn trả";
            }
            else if (decision == "reject")
            {
                order.Status = "Từ chối trả lại";
            }
            else
            {
                return BadRequest("Giá trị không hợp lệ.");
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xử lý yêu cầu đổi trả.";
            return RedirectToAction("Index");
        }


    }

}
