using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineStore.Pages.Account
{
    [Authorize]  // Только для авторизованных
    public class ProfileModel : PageModel
    {
        public string UserName { get; set; } = string.Empty;

        public void OnGet()
        {
            UserName = User.Identity?.Name ?? "Пользователь";
        }
    }
}