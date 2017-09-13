using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HomeAutomationAPI.Controllers
{
 [Produces("application/json")]
    [Route("api/Weather")]
    public class WeatherController : Controller
    {

    }
}