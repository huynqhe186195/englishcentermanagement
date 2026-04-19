using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnglishCenter.Web.Pages.Account;

public class LogoutModel : PageModel
{
    public void OnGet()
    {
        HttpContext.Session.Clear();
    }
}
