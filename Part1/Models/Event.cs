using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }

        [Required]
        public int VenueId { get; set; }

        public virtual Venue? Venue { get; set; }

        public ICollection<Booking>? Bookings { get; set; }

        public int EventTypeId { get; set; }

        public EventType? EventType { get; set; }
    }
}