using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventster.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace Eventster.Controllers
{
    public class StatisticsController : Controller
    {

        private readonly ModelCreator _context;

        public StatisticsController(ModelCreator context)
        {
            _context = context;
        }

        // GET: Statistics
        public ActionResult Index()
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                var artistsRank = _context.Concert.GroupBy(c => c.ArtistRank).Select(c => new { artistRank = (c.Key), count = c.Count() }).ToList();
                var artistsRankData = JsonConvert.SerializeObject(artistsRank);
                ViewBag.artistRankInConcertsData = artistsRankData;

                var concerts = _context.Ticket.GroupBy(t => t.Concert.Name).Select(c => new { name = (c.Key), ticketsAmount = c.Count() }).ToList();
                var concertsData = JsonConvert.SerializeObject(concerts);
                ViewBag.ticketsAmountInConcertsData = concertsData;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
