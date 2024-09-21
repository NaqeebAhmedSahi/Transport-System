using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class GuardController : Controller
    {
        private readonly RoutedSystemContext _context;

        public GuardController(RoutedSystemContext context)
        {
            _context = context;
        }

        // Action method for handling the upper input (in)
        [HttpPost]
        public IActionResult ClickIn(string incomingPlateTextBox)
        {
            if (!string.IsNullOrEmpty(incomingPlateTextBox))
            {
                // Create a new VehicleInfo object with "in" status and current datetime
                var vehicleInfo = new VehicleInfo
                {
                    NumberPlate = incomingPlateTextBox,
                    InOrOut = "in",
                    DateTime = DateTime.Now
                };

                // Add the vehicle info to the database and save changes
                _context.VehicleInfos.Add(vehicleInfo);
                _context.SaveChanges();
            }

            // Redirect to the GuardDashboard page
            return RedirectToAction(nameof(GuardDashboard));
        }

        // Action method for handling the lower input (out)
        [HttpPost]
        public IActionResult ClickOut(string outgoingPlateTextBox)
        {
            if (!string.IsNullOrEmpty(outgoingPlateTextBox))
            {
                // Create a new VehicleInfo object with "out" status and current datetime
                var vehicleInfo = new VehicleInfo
                {
                    NumberPlate = outgoingPlateTextBox,
                    InOrOut = "out",
                    DateTime = DateTime.Now
                };

                // Add the vehicle info to the database and save changes
                _context.VehicleInfos.Add(vehicleInfo);
                _context.SaveChanges();
            }

            // Redirect to the GuardDashboard page
            return RedirectToAction(nameof(GuardDashboard));
        }

        // Action method for rendering the GuardDashboard view
        public IActionResult GuardDashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index", "Home");
            }
            // Return the view
            return View();
        }
    }
}
