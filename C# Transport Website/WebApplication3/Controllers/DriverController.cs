using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class DriverController : Controller
    {
        private readonly RoutedSystemContext _context;

        public DriverController(RoutedSystemContext context)
        {
            _context = context;
        }

        public IActionResult AddDriver()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index","Home");
            }
            // Fetch drivers from the database
            var drivers = _context.Drivers.ToList();
            return View(drivers);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDriver(Driver driver)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(driver);
                _context.SaveChanges();
                return RedirectToAction(nameof(AddDriver));
            }
            return View(driver);
        }

        public IActionResult UpdateDriver(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index", "Home");
            }
            var driver = _context.Drivers.Find(id);
            if (driver == null)
            {
                return NotFound();
            }
            return View(driver);
        }

        [HttpPost]

       
        public IActionResult UpdateDriver(int id, string firstName, string lastName, DateTime dateOfBirth, string numberPlate)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index", "Home");
            }
            var driver = _context.Drivers.Find(id);

            if (driver == null)
            {
                return NotFound();
            }

            driver.FirstName = firstName;
            driver.LastName = lastName;
            driver.DateOfBirth = DateOnly.FromDateTime(dateOfBirth);// Convert DateTime to DateOnly
            driver.NumberPlate = numberPlate;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driver);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(AddDriver));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception and show an error view
                    // LogException(ex);
                    ViewBag.ErrorMessage = "An error occurred while updating the driver.";
                    return View("Error");
                }
            }
            return View(driver);
        }



        public IActionResult DeleteDriver(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index", "Home");
            }
            var driver = _context.Drivers.Find(id);
            if (driver == null)
            {
                return NotFound();
            }

            _context.Drivers.Remove(driver);
            _context.SaveChanges();

            return RedirectToAction(nameof(AddDriver));
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.DriverId == id);
        }
    }
}
