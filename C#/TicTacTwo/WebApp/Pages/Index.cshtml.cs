using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnPostNewGame()
    {
        return RedirectToPage("/Games/Create");
    }

    public IActionResult OnPostPlayGame()
    {
        return RedirectToPage("/Games/Index");
    }
}