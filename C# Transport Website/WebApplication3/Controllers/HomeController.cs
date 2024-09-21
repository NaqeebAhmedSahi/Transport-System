using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly RoutedSystemContext _context;

        public HomeController(RoutedSystemContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SuperAdminDashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            return View();
        }

    

        public IActionResult AddAdmin()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                // Fetch users where role is 2 (Admin)
                List<User> admins = _context.Users.Where(u => u.Role == 2).ToList();
                return View(admins);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while fetching admins.";
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult AddAdmin(string username, string password)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                // Create a new user with the role of Admin (role = 2)
                var newUser = new User
                {
                    Username = username,
                    Password = password,
                    Role = 2 // Set the role to Admin
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();

                return RedirectToAction("AddAdmin");
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while adding the admin.";
                return View("Error");
            }
        }

        public IActionResult AddDriver()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            return View();
        }

        public IActionResult GenerateReport()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                // Your generate report logic here
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while generating the report.";
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult SignIn(string username, string password, int role)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Invalid username or password.";
                    return View("Index");
                }


                // Verify the role matches
                if (user.Role != role)
                {
                    ViewBag.ErrorMessage = "Invalid role selected.";
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId", user.Id);

                // Assuming role-based redirection
                if (role == 1) // Assuming role 1 is Superadmin
                {
                    return RedirectToAction("SuperAdminDashboard");
                }
                else if (role == 2) // Assuming role 2 is Admin
                {
                    return RedirectToAction("AdminDashboard");
                }
                else if (role == 3) // Assuming role 3 is Guard
                {
                    return RedirectToAction("GuardDashboard");
                }

                return View("Index"); // Fallback to index
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred during sign-in.";
                return View("Error");
            }
        }

        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            return View();
        }

        public IActionResult GuardDashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            return View();
        }

        public ActionResult LoginPage()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                List<User> users = DbHandler.GetAllUsers();
                return View(users);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while fetching users.";
                return View("Error");
            }
        }

        public async Task<IActionResult> UpdateAdmin(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while fetching the admin.";
                return View("Error");
            }
        }

        [HttpPost]
        
        
        public async Task<IActionResult> UpdateAdmin(int id, string username, string password)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                // Find the user in the database
                var user = await _context.Users.FindAsync(id);

                // If user not found, return Not Found status
                if (user == null)
                {
                    return NotFound();
                }

                // Update the user properties
                user.Username = username;
                user.Password = password;

                // Save changes to the database
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Redirect to the AddAdmin action method after successful update
                return RedirectToAction(nameof(AddAdmin));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the update operation results in a concurrency conflict
                // return a NotFound status
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    // Log the exception and show an error view
                    // LogException(ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while updating the admin.";
                return View("Error");
            }
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                var admin = await _context.Users.FindAsync(id);
                if (admin == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(admin);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(AddAdmin));
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while deleting the admin.";
                return View("Error");
            }
        }

        public IActionResult AddGuard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                // Fetch users where role is 3 (Guard)
                List<User> guards = _context.Users.Where(u => u.Role == 3).ToList();
                return View(guards);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while fetching guards.";
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult AddGuard(string username, string password)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                // Create a new user with the role of Guard (role = 3)
                var newUser = new User
                {
                    Username = username,
                    Password = password,
                    Role = 3 // Set the role to Guard
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();

                return RedirectToAction("AddGuard");
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while adding the guard.";
                return View("Error");
            }
        }

        public async Task<IActionResult> UpdateGuard(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while fetching the guard.";
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult UpdateGuard(int id, string Username, string Password)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                // Find the user in the database
                var user = _context.Users.Find(id);

                // If user not found, return Not Found status
                if (user == null)
                {
                    return NotFound();
                }

                // Update the user properties
                user.Username = Username;
                user.Password = Password;

                // Save changes to the database
                _context.Users.Update(user);
                _context.SaveChanges();

                // Redirect to the AddGuard action method after successful update
                return RedirectToAction(nameof(AddGuard));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the update operation results in a concurrency conflict
                // Log the exception and show a user-friendly message
                ViewBag.ErrorMessage = "Failed to update the guard due to a concurrency conflict. Please try again.";
                return View("Error");
            }
            catch (Exception ex)
            {
                // Log the exception and show a generic error message
                ViewBag.ErrorMessage = "An error occurred while updating the guard.";
                return View("Error");
            }
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGuard(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to login page if session variables are not found
                return RedirectToAction("index");
            }
            try
            {
                var guard = await _context.Users.FindAsync(id);
                if (guard == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(guard);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(AddGuard));
            }
            catch (Exception ex)
            {
                // Log the exception and show an error view
                // LogException(ex);
                ViewBag.ErrorMessage = "An error occurred while deleting the guard.";
                return View("Error");
            }
        }
    }
}
