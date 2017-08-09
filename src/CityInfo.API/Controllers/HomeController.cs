using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CityInfo.API.Controllers
{
    [Route("Home")]
    public class HomeController : Controller
    {
        // GET: /<controller>/
        [HttpGet("Index")]
        public ViewResult Index()
        {
            var model = new CityDto {Id = 1, Name = "Beirut", Description = "Beirut city"};

            return View(model);
        }
    }
}
