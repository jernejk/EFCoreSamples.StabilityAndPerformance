using Microsoft.AspNetCore.Mvc;

namespace EFCoreSamples.StabilityAndPerformance.Api.Controllers
{
    /// <summary>
    /// Testing how fast can Web API react without any code.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class NopController : Controller
    {

        [HttpGet]
        public void Ping()
        {
        }
    }
}
