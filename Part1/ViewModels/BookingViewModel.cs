using System;

namespace EventEase.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }

        public string EventName { get; set; }

        public string VenueName { get; set; }

        public DateTime EventDate { get; set; }

        public DateTime BookingDate { get; set; }

        public string EventTypeName { get; set; }

        public bool VenueAvailable { get; set; }

        public bool IsAvailable { get; set; }
    }
}