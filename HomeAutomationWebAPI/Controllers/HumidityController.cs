using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationWebAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeAutomationAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Humidity")]
    public class HumidityController : Controller
    {
    }
}