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
    public class TicketsController : Controller
    {
        private readonly ModelCreator _context;

        public TicketsController(ModelCreator context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index(string searchString)
        {

            IQueryable<Ticket> tickets = _context.Set<Ticket>();

            tickets = tickets.Include(ticket => ticket.Concert).Include(ticket => ticket.TicketType);

            if (!String.IsNullOrEmpty(searchString))
            {
                int numberSearch;

                tickets = tickets.Where(ticket =>
                ticket.Concert.Name.Contains(searchString) ||
                ticket.Concert.Country.Contains(searchString) ||
                ticket.TicketType.Type.Contains(searchString) ||
                (Int32.TryParse(searchString, out numberSearch) &&
                (ticket.Id == numberSearch || ticket.TicketType.Price == numberSearch || ticket.TicketsLeft == numberSearch)));
            }


            tickets = tickets.OrderBy(ticket => ticket.ConcertId).ThenBy(ticket => ticket.Id);

            return View(await tickets.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id, int? concertId)
        {
            if (id == null || concertId == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.Include(ticket => ticket.Concert).Include(ticket => ticket.TicketType).FirstOrDefaultAsync(t => (t.Id == id) && (t.ConcertId == concertId));
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["ConcertId"] = new SelectList(_context.Concert, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Type");
            return View(new Ticket());
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ConcertId,TicketTypeId,TicketsLeft")] Ticket ticket)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(ticket);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["ConcertId"] = new SelectList(_context.Concert, "Id", "Id", ticket.ConcertId);
                ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id", ticket.TicketTypeId);
                return View(ticket);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id, int? concertId)
        {
            if (id == null || concertId == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id, concertId);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["ConcertId"] = new SelectList(_context.Concert, "Id", "Id", ticket.ConcertId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ConcertId,TicketTypeId,TicketsLeft")] Ticket ticket)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (id != ticket.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(ticket);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TicketExists(ticket.Id, ticket.ConcertId))
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
                ViewData["ConcertId"] = new SelectList(_context.Concert, "Id", "Id", ticket.ConcertId);
                ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id", ticket.TicketTypeId);
                return View(ticket);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id, int? concertId)
        {
            if (id == null || concertId == null)
            {
                return NotFound();
            }

            bool isTicketOnBooking =
                (_context.Booking.Where(ticket => ticket.TicketId == id && ticket.ConcertId == concertId).Count() > 0);

            var ticket = await _context.Ticket
                .Include(ticket => ticket.Concert)
                .Include(ticket => ticket.TicketType)
                .FirstOrDefaultAsync(t => (t.Id == id) && (t.ConcertId == concertId));

            if (ticket == null)
            {
                return NotFound();
            }
            else if (isTicketOnBooking)
            {
                return BadRequest("This ticket is in use on booking");
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int concertId)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                var ticket = await _context.Ticket.FindAsync(id, concertId);
                _context.Ticket.Remove(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private bool TicketExists(int id, int concertId)
        {
            return _context.Ticket.Any(t => (t.Id == id) && (t.ConcertId == concertId));
        }

        public int GetLastTicketIdInConcert(int concertId)
        {
            var query = _context.Ticket.Where(ticket => (ticket.ConcertId == concertId));

            if (query.Any())
            {
                return query.Max(ticket => ticket.Id);
            }
            else
            {
                return 0;
            }
        }

        public dynamic GetFreeTicketByParms(int concertId, int ticketTypeId)
        {
            return _context.Ticket.Where(e => (e.ConcertId == concertId) &&
            (e.TicketTypeId == ticketTypeId) && (e.TicketsLeft > 0)).ToList();
        }

        public async Task EditNumOfTicketById(int ticketId, int concertId, int numToReduce)
        {
            if (numToReduce == 0)
            {
                var ticket = await _context.Ticket.FindAsync(ticketId, concertId);
                if ((ticket != null) && (ticket.TicketsLeft - numToReduce >= 0))
                {
                    ticket.TicketsLeft = ticket.TicketsLeft - numToReduce;
                    _context.Ticket.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<IActionResult> MultiSearch(string concertCountry, string ticketTypeName, int price)
        {
            var tickets = _context.Ticket.Include(ticket => ticket.Concert).Include(ticket => ticket.TicketType).
                Where(ticket => ticket.Concert.Country.Equals(concertCountry) &&
                ticket.TicketType.Type.Equals(ticketTypeName) &&
                ticket.TicketType.Price <= price).
                OrderBy(ticket => ticket.ConcertId).ThenBy(ticket => ticket.Id);

            return View("Index", await tickets.ToListAsync());
        }
    }
}
