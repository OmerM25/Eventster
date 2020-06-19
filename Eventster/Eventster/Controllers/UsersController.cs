using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eventster.Models;
using Microsoft.AspNetCore.Http;
namespace Eventster.Controllers
{
    public class UsersController : Controller
    {
        public struct UserResult
        {
            public bool isLogin { get; set; }
            public string userName { get; set; }
        };

        public const string SessionName = "UserName";
        private readonly ModelCreator _context;

        public UsersController(ModelCreator context)
        {
            _context = context;
        }

        // GET: Users | Get all users
        public async Task<IActionResult> Index()
        {
            if (checkSession().isLogin)
            {
                return View(await _context.User.ToListAsync());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Users/Details/2 | Get all details for a specific user by id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FirstOrDefaultAsync(user => user.UserName == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Insert | Return the view for new user
        public IActionResult Insert()
        {
            return View();
        }

        // POST: Users/Insert | Insert a new user by a given user object
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Password")] User user)
        {
            // Check if user already logged in
            if (checkSession().isLogin)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Users/Update/2 | Get update view for user by id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Update/2 | Update a user by id with the parameters to update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserName,Password")] User user)
        {
            // Check if user already logged in
            if (checkSession().isLogin)
            {
                if (id != user.UserName)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // Check if user was found, in order to update it
                        if (!UserExists(user))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Users/Delete/2 | Get the delete view for user by id
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FirstOrDefaultAsync(user => user.UserName == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/2 | Delete user from db by id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (checkSession().isLogin)
            {
                var user = await _context.User.FindAsync(id);
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Check if a user exists in db
        private bool UserExists(User user)
        {
            return _context.User.Any(user => (user.UserName == user.UserName) && (user.Password == user.Password));
        }

        // Logins the user into the system, return the homepage view if success and throws an error if not
        public IActionResult Login([Bind("UserName,Password")] User user)
        {
            ViewData["ErrMessage"] = "";
            if (checkSession().isLogin)
            {
                return RedirectToAction("Index", "Home");

            }
            else if (ModelState.IsValid)
            {
                if (UserExists(user))
                {
                    HttpContext.Session.SetString(SessionName, user.UserName);
                    return RedirectToAction("Index", "Home");
                }
            }
            if (user.UserName != null && user.Password != null)
            {
                ViewData["ErrMessage"] = "Incorrect username or password";
            }

            return View("Login");
        }

        // Logoff the user and return to homepage
        public IActionResult Logoff()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Check if a user is already logged in, if yes - return a json containing his details
        [HttpGet]
        public UserResult checkSession()
        {
            string userName = HttpContext.Session.GetString(UsersController.SessionName);
            UserResult result = new UserResult();

            if (userName != null)
            {
                result.isLogin = true;
                result.userName = userName;
            }
            else
            {
                result.isLogin = false;
            }

            return (result);
        }

    }
}
