using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SignEdgeService.Controllers
{
	public class AcmeChallengeController : Controller
    {

        private readonly ILogger<HomeController> _logger;

        public AcmeChallengeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                var name = "AuthKey.txt";
                return File($"/Files/{name}", "text/plain", name);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                return RedirectToAction(nameof(Index), "Home");
            }
        }
    }
}

