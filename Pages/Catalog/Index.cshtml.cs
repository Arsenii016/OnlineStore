using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Pages.Catalog
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Product> Products { get; set; } = new List<Product>();
        public IList<Category> Categories { get; set; } = new List<Category>();

        // Параметры для фильтрации и пагинации
        public int? CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 8; // по 8 товаров на страницу (можно менять)
        public int TotalProducts { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalProducts / PageSize);

        public async Task OnGetAsync(int? categoryId, int pageNumber = 1)
        {
            CategoryId = categoryId;
            PageNumber = pageNumber < 1 ? 1 : pageNumber;

            // Загружаем категории для фильтра (только на ПК будет видно)
            Categories = await _context.Categories.ToListAsync();

            // Основной запрос
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Фильтрация по категории
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Подсчёт общего количества для пагинации
            TotalProducts = await query.CountAsync();

            // Пагинация
            Products = await query
                .OrderBy(p => p.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // Обработчик добавления в корзину (чтобы кнопка работала прямо из каталога)
        public async Task<IActionResult> OnPostAddToCartAsync(int productId)
        {
            // Здесь будет логика добавления в корзину (пока просто редирект обратно)
            // Полноценную реализацию сделаем на странице Cart
            // Можно временно сделать TempData сообщение
            TempData["Message"] = "Товар добавлен в корзину! (функционал в разработке)";

            // Вернёмся на ту же страницу с сохранением фильтров
            return RedirectToPage(new { categoryId = CategoryId, pageNumber = PageNumber });
        }
    }
}