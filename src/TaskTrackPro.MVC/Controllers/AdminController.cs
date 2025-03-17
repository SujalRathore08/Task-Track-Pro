using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TaskTrackPro.Mvc.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            // Overview of system stats (Total Users, Active Tasks, Pending Tasks, Completed Tasks, etc.)
            // Graphs and Charts for task status and user activity
            return View();
        }

        public IActionResult Members()
        {
            // User Management 
            return View();
        }

        public IActionResult Task()
        {
            // User Management 
            return View();
        }

        public IActionResult Messages()
        {
            // User Management 
            return View();
        }

    }
}