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
    [Route("api/Heater")]
    public class HeaterController : Controller
    {
        ServiceClientHelper _serviceClientHelper = new ServiceClientHelper("");
        
    }
}