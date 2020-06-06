using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Eventster.Models
{
    public class TicketType
    {
        [Display(Name = "TicketType Id")]
        [Required]
        public int Id { get; set; }

        [Display(Name = "Ticket Type")]
        [StringLength(50)]
        [Required]
        public string Type { get; set; }

        [Display(Name = "TicketType Description")]
        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Ticket Price")]
        [Range(1, 999)]
        [Required]
        public int Price { get; set; }
    }
}
