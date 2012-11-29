using System.Web.Mvc;

namespace Xlnt.Web.SampleSite.Controllers
{
    public class EchoController : Controller
    {
        public ActionResult Index(int status) {
            return new HttpStatusCodeResult(status);
		}
    }
}
