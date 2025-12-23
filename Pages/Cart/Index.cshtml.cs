using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;
using System.Security.Claims;

namespace OnlineStore.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalPrice { get; set; } = 0;

        public async Task OnGetAsync()
        {
            await LoadCartAsync();
        }

        private async Task LoadCartAsync()
        {
            string cartId = GetCartId();

            CartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.SessionId == cartId || ci.UserId == cartId)
                .ToListAsync();

            TotalPrice = CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
        }

        // Добавление товара в корзину (из главной или каталога)
        public async Task<IActionResult> OnPostAddAsync(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity)
                return NotFound();

            string cartId = GetCartId();

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == productId &&
                    (ci.SessionId == cartId || ci.UserId == cartId));

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    SessionId = User.Identity.IsAuthenticated ? null : cartId,
                    UserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            await _context.SaveChangesAsync();
            await LoadCartAsync();

            return RedirectToPage(); // обновляем корзину
        }

        // Обновление количества (+/- или удаление)
        public async Task<IActionResult> OnPostUpdateAsync(int id, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null) return NotFound();

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            await LoadCartAsync();

            return RedirectToPage();
        }

        // Оформить заказ
        public IActionResult OnPostCheckout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Заглушка: просто очищаем корзину
            _context.CartItems.RemoveRange(CartItems);
            _context.SaveChanges();

            TempData["Success"] = "Заказ успешно оформлен! (заглушка)";
            return RedirectToPage();
        }

        private string GetCartId()
        {
    if (User.Identity?.IsAuthenticated == true)
            {
        // Безопасно получаем UserId
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
                {
            // Запасной вариант — если claims не загрузились
            return HttpContext.Session.Id;
                }
                return userId;
            }

    // Для анонимов — сессия
    var sessionId = HttpContext.Session.GetString("CartId");
    if (string.IsNullOrEmpty(sessionId))
            {
        sessionId = Guid.NewGuid().ToString();
        HttpContext.Session.SetString("CartId", sessionId);
            }
    return sessionId;
        }
    }
}