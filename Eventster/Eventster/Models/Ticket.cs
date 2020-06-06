using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Eventster.Models
{
    public class Ticket
    {
        [Display(Name = "Ticket Id")]
        [Range(1, 5000)]
        [Required]
        public int Id { get; set; }

        [Display(Name = "Tickets left")]
        [Range(1, 5000)]
        [Required]
        public int TicketsLeft { get; set; }

        [Display(Name = "Concert Id")]
        [Required]
        public int ConcertId { get; set; }

        [Display(Name = "TicketType Id")]
        [Required]
        public int TicketTypeId { get; set; }

        public Concert Concert { get; set; }

        [Display(Name = "TicketType")]
        public TicketType TicketType { get; set; }

        public Ticket()
        {
            this.Id = 1;
            this.TicketsLeft = 5000;
        }
    }
}
