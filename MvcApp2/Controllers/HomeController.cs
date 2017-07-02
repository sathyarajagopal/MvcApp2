using System.Web.Mvc;

namespace MvcApp2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index");
        }
    }
}