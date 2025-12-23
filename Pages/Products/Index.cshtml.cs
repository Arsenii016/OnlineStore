using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Pages.Products;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Product> Products { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Products = await _db.Products.OrderBy(p => p.Id).ToListAsync();
    }
}
