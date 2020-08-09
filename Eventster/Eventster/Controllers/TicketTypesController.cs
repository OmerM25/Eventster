using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eventster.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Eventster.Controllers
{
    public class TicketTypesController : Controller
    {
        private readonly ModelCreator _context;

        public TicketTypesController(ModelCreator context)
        {
            _context = context;
        }

        // GET: TicketTypes
        public async Task<IActionResult> Index(string searchString)
        {
            var ticketTypes = from tt in _context.TicketType
                            select tt;

            if (!String.IsNullOrEmpty(searchString))
            {
                int numberSearch;

                ticketTypes = ticketTypes.Where(ticket =>
                ticket.Type.Contains(searchString) ||
                ticket.Description.Contains(searchString) ||
                (Int32.TryParse(searchString, out numberSearch) &&
                (ticket.Id == numberSearch || ticket.Price == numberSearch)));
            }

            return View(await ticketTypes.ToListAsync());
        }

        // GET: TicketTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketType.FirstOrDefaultAsync(tt => tt.Id == id);
            if (ticketType == null)
            {
                return NotFound();
            }

            return View(ticketType);
        }

        // GET: TicketTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TicketTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,Description,Price")] TicketType ticketType)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(ticketType);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(ticketType);
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: TicketTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketType.FindAsync(id);
            if (ticketType == null)
            {
                return NotFound();
            }
            return View(ticketType);
        }

        // POST: TicketTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,Description,Price")] TicketType ticketType)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (id != ticketType.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(ticketType);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TicketTypeExists(ticketType.Id))
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
                return View(ticketType);
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: TicketTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            bool isTicketTypeOnBooking =
                (_context.Booking.Where(booking => booking.Ticket.TicketType.Id == id).Count() > 0);

            var ticketType = await _context.TicketType.FirstOrDefaultAsync(tt => tt.Id == id);

            if (ticketType == null)
            {
                return NotFound();
            }
            else if (isTicketTypeOnBooking)
            {
                return BadRequest("This ticket-type is in use on booking");
            }

            return View(ticketType);
        }

        // POST: TicketTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                var ticketType = await _context.TicketType.FindAsync(id);
                _context.TicketType.Remove(ticketType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        private bool TicketTypeExists(int id)
        {
            return _context.TicketType.Any(tt => tt.Id == id);
        }

        public int GetTicketTypePrice(int ticketTypeId)
        {
            return _context.TicketType.Where(ticket => ticket.Id == ticketTypeId).Select(ticket => ticket.Price).FirstOrDefault();
        }

        public dynamic GetLastTypeId()
        {
            return _context.TicketType.Select(ticket => ticket.Id).Max();
        }

        public List<string> GetAllTicketTypesType()
        {
            return _context.TicketType.Select(ticket => ticket.Type).ToList();
        }
    }
}
