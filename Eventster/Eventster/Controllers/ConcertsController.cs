using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eventster.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Eventster.Controllers
{
    public class ConcertsController : Controller
    {
        private readonly ModelCreator _context;

        public ConcertsController(ModelCreator context)
        {
            _context = context;
        }

        // GET: Concerts | View of concerts by the search parameters
        public async Task<IActionResult> Index(string searchString)
        {
            var concerts = from concert in _context.Concert select concert;

            // Filter in case the search parameters are not empty
            if (!String.IsNullOrEmpty(searchString))
            {
                int artistRankSearch;
                DateTime dateSearch;

                concerts = concerts.Where(concert =>
                concert.Name.Contains(searchString) ||
                concert.Country.Contains(searchString) ||
                concert.City.Contains(searchString) ||
                concert.Address.Contains(searchString) ||
                (DateTime.TryParse(searchString, out dateSearch) && concert.DateTime == dateSearch) ||
                (Int32.TryParse(searchString, out artistRankSearch) && concert.ArtistRank == artistRankSearch));
            }

            return View(await concerts.ToListAsync());
        }

        // GET: Concerts/Details/2 | Get details for a specific concert by id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concert = await _context.Concert.FirstOrDefaultAsync(concert => concert.Id == id);
            if (concert == null)
            {
                return NotFound();
            }

            return View(concert);
        }

        // GET: Concerts/Create | Return the insert page view
        public IActionResult Create()
        {
            return View();
        }

        // POST: Concerts/Create | Insert new concert to db
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Country,City,Address,DateTime,ArtistRank,XCord,YCord")] Concert concert)
        {
            // Make sure user connected
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                // Set the concert's last sequence ID
                concert.Id = this.GetLastConcertId() + 1;

                if (ModelState.IsValid)
                {
                    _context.Add(concert);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(concert);
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Concerts/Edit/2 | Return the update view of the concert
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concert = await _context.Concert.FindAsync(id);
            if (concert == null)
            {
                return NotFound();
            }
            return View(concert);
        }

        // POST: Concerts/Edit/2 | Update the concert by id and the new concert object
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Country,City,Address,DateTime,ArtistRank,XCord,YCord")] Concert concert)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (id != concert.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(concert);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ConcertExists(concert.Id))
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
                return View(concert);
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Concerts/Delete/2 | Return the delete view
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concert = await _context.Concert.FirstOrDefaultAsync(concert => concert.Id == id);

            if (concert == null)
            {
                return NotFound();
            }

            return View(concert);
        }

        // POST: Concerts/Delete/2 | Delete a concert from the DB by id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                var concert = await _context.Concert.FindAsync(id);
                _context.Concert.Remove(concert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        // Check if a concert exists for a specific id
        private bool ConcertExists(int id)
        {
            return _context.Concert.Any(concert => concert.Id == id);
        }

        // Get concert coordinates for all the concerts in db
        public dynamic GetConcertCoords()
        {
            return _context.Concert.Select(concert => new { concert.Name, concert.XCord, concert.YCord }).ToList();
        }

        private int GetLastConcertId()
        {
            if (_context.Concert.Count() == 0)
            {
                return 0;
            }
            return _context.Concert.Select(concert => concert.Id).Max();
        }

        // Get concert city by coordinates from database
        public string GetConcertCityByXY()
        {
            double x = Double.Parse(Request.Query["lat"].ToString());
            double y = Double.Parse(Request.Query["lon"].ToString());

            return _context.Concert.Select(concert => new { concert.City, concert.XCord, concert.YCord }).ToList()
                .Where(concert => concert.XCord == x && concert.YCord == y).FirstOrDefault().City;
        }

        // Get a list of all countries of concerts
        public List<string> GetAllConcertCountries()
        {
            return _context.Concert.GroupBy(concert => concert.Country).Select(concert => concert.Key).ToList();
        }

        // Get all concerts cities by country
        public List<string> GetAllConcertsCitiesByCountry(string country)
        {
            //return _context.Concert.Where(concert => concert.Country == country).GroupBy((object concert) => concert.City).Select((object concert) => concert.Key).ToList();
            return _context.Concert.Where(concert => concert.Country == country).GroupBy(concert => concert.City).Select(concert => concert.Key).ToList();
        }

        // Search for concerts by multiple parameters (Country, City, ArtistRank)
        public async Task<IActionResult> MultiSearch(string country, string city, int artistRank)
        {
            var concerts = _context.Concert.Where(concert => concert.Country == country &&
            concert.City == city &&
            concert.ArtistRank>= artistRank);

            return View("Index", await concerts.ToListAsync());
        }
    }
}
