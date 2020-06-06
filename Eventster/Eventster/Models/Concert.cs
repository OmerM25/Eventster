using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Eventster.Models
{
    public class Concert
    {
        [Display(Name = "Concert Id")]
        [Required]
        public int Id { get; set; }

        [Display(Name = "Artist Name")]
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Concert Country")]
        [StringLength(20)]
        [Required]
        public string Country { get; set; }

        [Display(Name = "Concert City")]
        [StringLength(30)]
        [Required]
        public string City { get; set; }

        [Display(Name = "Concert Address")]
        [StringLength(50)]
        [Required]
        public string Address { get; set; }

        [Display(Name = "Concert DateTime")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy - HH:mm:ss}")]
        [Required]
        public DateTime DateTime { get; set; }

        [Display(Name = "Artist Rank")]
        [Range(1, 5)]
        [Required]
        public double ArtistRank { get; set; }

        [Display(Name = "XCord")]
        [Required]
        public double XCord { get; set; }

        [Display(Name = "YCord")]
        [Required]
        public double YCord { get; set; }
    }
}
