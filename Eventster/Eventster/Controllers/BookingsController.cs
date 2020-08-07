using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eventster.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.ML.Legacy;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Transforms;
using Microsoft.ML.Legacy.Trainers;
using System.IO;
using Microsoft.AspNetCore.Authentication;

namespace Eventster.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ModelCreator _context;
        public string dataPath = "ML/train.txt";

        public BookingsController(ModelCreator context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string searchString)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (CountOfTicketTypesOnBooking() > 1)
                {
                    TrainBookingsData();
                }

                IQueryable<Booking> bookings = _context.Set<Booking>();

                bookings = bookings.Include(b => b.Client).
                    Include(b => b.Concert).
                    Include(b => b.Ticket).
                    Include(b => b.Ticket.TicketType);

                if (!String.IsNullOrEmpty(searchString))
                {
                    int numberSearch;

                    bookings = bookings.Where(b =>
                    b.Concert.Name.Contains(searchString) ||
                    b.Concert.Country.Contains(searchString) ||
                    b.Client.Id.Contains(searchString) ||
                    (Int32.TryParse(searchString, out numberSearch) && b.TicketId == numberSearch));
                }

                bookings = bookings.OrderByDescending(b => b.Concert.DateTime);

                return View(await bookings.ToListAsync());
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Bookings/Details/5
        public IActionResult Details(string ClientId, int ConcertId, int TicketId, int TicketsAmount)
        {

            TicketsAmount = int.Parse(ModelState["TicketsAmount"].AttemptedValue);


            Booking booking = _context.Booking.Include(b => b.Client).Include(b => b.Concert).Include(b => b.Ticket)
                .Where(e => (e.TicketsAmount.Equals(TicketsAmount)) && (e.ClientId == ClientId) && (e.ConcertId == ConcertId) && (e.TicketId == TicketId)).FirstOrDefault();

            if (booking == null)
            {
                return NotFound("Booking was not found");
            }
            else
            {
                booking.Ticket.TicketType = _context.TicketType.Where(b => b.Id == booking.Ticket.TicketTypeId).FirstOrDefault();
            }


            ViewData["TicketsAmount"] = TicketsAmount;

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            TempData["ErrMessageBooking"] = "";
            ViewData["ConcertId"] = new SelectList(_context.Concert, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name");
            ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Id");
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id");

            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,ConcertId,TicketId,TicketsAmount")] Booking booking)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Booking parameters are not valid");
                }
                else
                {
                    TicketsController ticketsController = new TicketsController(_context);
                    await ticketsController.EditNumOfTicketById(booking.TicketId, booking.ConcertId, booking.TicketsAmount);
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Bookings/Edit/5
        public IActionResult Edit(string ClientId, int ConcertId, int TicketId, int TicketsAmount)
        {
            TicketsAmount = int.Parse(ModelState["TicketsAmount"].AttemptedValue);

            var booking = _context.Booking.Where(e => (e.TicketsAmount.Equals(TicketsAmount)) && (e.ClientId == ClientId) && (e.ConcertId == ConcertId) && (e.TicketId == TicketId)).FirstOrDefault();

            if (booking == null)
            {
                return NotFound("Booking was not found");
            }
            else if (!CalculatePrice(booking))
            {

                return NotFound("Error on price calculation");
            }

            ViewData["TicketsAmount"] = TicketsAmount;

            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ClientId,ConcertId,TicketId,TicketsAmount")] Booking booking)
        {
            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                TicketsController ticket = new TicketsController(_context);

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(booking);
                        await ticket.EditNumOfTicketById(booking.TicketId, booking.ConcertId, 0);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!BookingExists(booking.ClientId))
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
                else
                {
                    TempData["ErrMessageBooking"] = "Unable to book with these parameters.";
                }

                ViewData["ClientId"] = new SelectList(_context.Client, "Id", "Id", booking.ClientId);
                ViewData["ConcertId"] = new SelectList(_context.Concert, "Id", "Name", booking.ConcertId);
                ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Id", booking.TicketId);
                ViewData["TicketsAmount"] = booking.TicketsAmount;
                return View(booking);
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Bookings/Delete/5
        public IActionResult Delete(string ClientId, int ConcertId, int TicketId, int TicketsAmount)
        {
            TicketsAmount = int.Parse(ModelState["TicketsAmount"].AttemptedValue);

            var booking = _context.Booking.Where(e => (e.TicketsAmount.Equals(TicketsAmount)) && (e.ClientId == ClientId) && (e.ConcertId == ConcertId) && (e.TicketId == TicketId)).ToList();

            if (booking.Count == 0)
            {
                return NotFound();
            }

            var concert = _context.Concert.Where(e => (e.Id == ConcertId)).ToList();
            ViewData["TicketsAmount"] = TicketsAmount;

            return View(booking[0]);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([Bind("ClientId,ConcertId,TicketId,TicketsAmount")] Booking booking)
        {

            if (HttpContext.Session.GetString(UsersController.SessionName) != null)
            {
                _context.Booking.Remove(booking);
                TicketsController ticket = new TicketsController(_context);
                await ticket.EditNumOfTicketById(booking.TicketId, booking.ConcertId, booking.TicketsAmount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["msg"] = "<script>alert('Please login.');</script>";
                return RedirectToAction("Index", "Home");
            }
        }

        private bool BookingExists(string id)
        {
            return _context.Booking.Any(e => e.ClientId == id);
        }

        private bool CalculatePrice(Booking booking)
        {
            // Ticket validation
            if (booking.TicketsAmount > 0)
            {
                // Query the booking's ticket object.
                Ticket ticket = _context.Ticket.Where(e => (e.Id == booking.TicketId) && (e.ConcertId == booking.ConcertId)).Include(b => b.TicketType).FirstOrDefault();

                // In case the ticket has not found
                if ((ticket == null) || (ticket.TicketsLeft < booking.TicketsAmount))
                {
                    return false;
                }

                int pricePerTicket = ticket.TicketType.Price;
                int total_booking_price = pricePerTicket * booking.TicketsAmount;

                ViewBag.total_booking_price = total_booking_price;
                ViewBag.pricePerTicket = pricePerTicket;

                return true;
            }
            else
            {
                return false;
            }
        }

        public int CountOfTicketTypesOnBooking()
        {
            return _context.Booking.Include(b => b.Ticket).GroupBy(b => b.Ticket.TicketTypeId).Count();
        }

        public List<IGrouping<int, Booking>> GetTicketTypeOnBookings()
        {
            return _context.Booking.Include(b => b.Ticket).GroupBy(b => b.Ticket.TicketTypeId).ToList();
        }

        // Getting concert id and calculate by ML recommendation for favorite ticket type.
        public IActionResult GetRecommendedTicketTypeByConcertId(int concertId)
        {
            string country = _context.Concert.Where(c => c.Id == concertId).Select(c => c.Country).FirstOrDefault();

            if (country == null)
            {
                return NotFound("Concert country not found for concert with id: " + concertId);
            }
            else
            {
                int ticketTypeId;
                var TicketTypesList = GetTicketTypeOnBookings();

                if (TicketTypesList.Count == 1)
                {
                    ticketTypeId = TicketTypesList[0].Key;
                }
                else if (TicketTypesList.Count > 1)
                {
                    ticketTypeId = PredictTicketByBooking(concertId, country);
                }
                else
                {
                    return BadRequest();
                }

                string predictedTicketType = _context.TicketType.Where(t => t.Id == ticketTypeId).Select(t => t.Type).FirstOrDefault();

                if (predictedTicketType == null)
                {
                    return NotFound("There is no ticket type id " + ticketTypeId);
                }

                return Ok(predictedTicketType);
            }
        }

        // Getting two features and predict favorite ticket type id (label).
        public int PredictTicketByBooking(float concertId, string concertCountry)
        {
            // Create a pipeline and load your data.
            var pipeline = new LearningPipeline();

            // Load the data from the training file by path.
            pipeline.Add(new TextLoader(this.dataPath).CreateFrom<TrainData>(separator: ','));

            // Assign numeric values to text in the "Label" column, because only
            // numbers can be processed during model training
            pipeline.Add(new Dictionarizer("Label"));
            pipeline.Add(new CategoricalOneHotVectorizer("ConcertCountry"));

            // Puts all features into a vector
            pipeline.Add(new ColumnConcatenator("Features", "ConcertId", "ConcertCountry"));

            // Add learner
            // Add a learning algorithm to the pipeline. 
            // This is a classification scenario.
            pipeline.Add(new StochasticDualCoordinateAscentClassifier());

            // Convert the Label back into original text (after converting to number).
            pipeline.Add(new PredictedLabelColumnOriginalValueConverter() { PredictedLabelColumn = "PredictedLabel" });

            // Train our model based on the data set.
            var model = pipeline.Train<TrainData, Prediction>();

            // Use our model to make a prediction.
            return model.Predict(new TrainData()
            {
                ConcertId = concertId,
                ConcertCountry = concertCountry
            }).TicketTypeId;

        }

        // This function prepare file with training data.
        public void TrainBookingsData()
        {
            var bookings = _context.Booking.Include(b => b.Concert).Include(b => b.Ticket).
                Select(b => new { b.ConcertId, b.Concert.Country, b.Ticket.TicketTypeId }).ToList();
            using (StreamWriter outputFile = new StreamWriter(this.dataPath))
            {
                foreach (var booking in bookings)
                {
                    int concertId = booking.ConcertId;
                    string concertCountry = booking.Country;
                    outputFile.WriteLine(concertId + "," + concertCountry+ "," + booking.TicketTypeId);
                }

                outputFile.Close();
            }
        }

    }

    // This class is the train data vector structure.
    public class TrainData
    {
        [Column("0")]
        [ColumnName("ConcertId")]
        public float ConcertId;

        [Column("1")]
        [ColumnName("ConcertCountry")]
        public string ConcertCountry;

        [Column("2")]
        [ColumnName("Label")]
        public int TicketTypeId;
    }

    // This clss is the prediction object.
    public class Prediction
    {
        [ColumnName("PredictedLabel")]
        public int TicketTypeId;
    }
}
