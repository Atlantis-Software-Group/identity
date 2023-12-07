using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg.identity.Pages.Admin
{
    [SecurityHeaders]
    [Authorize]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}