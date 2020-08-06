using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eventster.Models;
using Microsoft.AspNetCore.Http;

namespace Eventster.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ModelCreator _context;

        public ClientsController(ModelCreator context)
        {
            _context = context;
        }

        // This function returns the view of clients
        // param: searchString - for searching a client 
        public async Task<IActionResult> Index(string searchString)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                var clients = from m in _context.Client select m;

                if (!String.IsNullOrEmpty(searchString))
                {
                    clients = clients.Where(client =>
                    client.FirstName.Contains(searchString) ||
                    client.LastName.Contains(searchString) ||
                    client.Id.Contains(searchString) ||
                    client.PhoneNumber.Contains(searchString) ||
                    client.CreditCard.Contains(searchString));
                }

                clients = clients.OrderBy(c => c.FirstName).ThenBy(c => c.LastName);

                return View(await clients.ToListAsync());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // This function returns the view of details for client
        // param: id - client id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Client.FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        // This function returns the view of create page
        public IActionResult Create()
        {
            ViewData["ErrClient"] = "";
            return View();
        }

        // POST: Clients/Create
        // This function add a new client to db
        // param: Client - new client to create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,PhoneNumber,CreditCard")] Client client)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(client);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(client);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Clients/Edit/5
        // This function returns the view of edit for client
        // param: id - client id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Client.FindAsync(id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        // POST: Clients/Edit/5
        // This function update client by id in the db
        // param: id     - client id
        //        client - client with new values 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,LastName,PhoneNumber,CreditCard")] Client client)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (id != client.Id)
                    return NotFound();

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(client);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ClientExists(client.Id))
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
                return View(client);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Clients/Delete/5
        // This function returns the view of delete for client
        // param: id - client id
        public async Task<IActionResult> Delete(string id)
        {
            ViewData["ErrDeleteClient"] = "";

            if (id == null)
                return NotFound();

            var client = await _context.Client.FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        // POST: Clients/Delete/5
        // This function deleting client by id from the db.
        // param: id - client id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                bool doesClientHasBooking = (_context.Booking.Where(r => r.ClientId == id).Count() > 0);

                if (doesClientHasBooking)
                {
                    ViewData["ErrDeleteClient"] = "This client has an existing booking.";
                    return View();
                }
                else
                {
                    var client = await _context.Client.FindAsync(id);
                    _context.Client.Remove(client);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public dynamic GetLastClientId()
        {
            return _context.Client.OrderByDescending(client => client.Id).FirstOrDefault();
        }

        // This function checks if a client is already exist in the db
        // param: id - client id
        private bool ClientExists(string id)
        {
            return _context.Client.Any(e => e.Id == id);
        }


    }
}
