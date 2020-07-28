using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Eventster.Models
{
    public class Booking
    {
        [Display(Name = "Booking Id")]
        [Range(1, 5000)]
        [Required]
        public int Id { get; set; }

        [Display(Name = "Client Id")]
        [Required]
        public string ClientId { get; set; }

        [Display(Name = "Concert Id")]
        [Required]
        public int ConcertId { get; set; }

        [Display(Name = "Ticket Id")]
        [Required]
        public int TicketId { get; set; }

        [Display(Name = "Number of Tickets")]
        [Range(1, 10)]
        [Required]
        public int TicketsAmount { get; set; }

        public Concert Concert { get; set; }
        public Ticket Ticket{ get; set; }
        public Client Client { get; set; }
    }
}
