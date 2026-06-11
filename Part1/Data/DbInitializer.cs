using EventEase.Models;
using System.Linq;

namespace EventEase.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Venues.Any())
                return; // DB has been seeded

            var venues = new Venue[]
            {
                new Venue
                {
                    VenueName = "FNB Stadium",
                    Location = "Johannesburg",
                    Capacity = 87436,
                    ImageUrl = "FnbStadium.jpeg"
                },
                new Venue
                {
                    VenueName = "Table Mountain National Park",
                    Location = "Cape Town",
                    Capacity = 11753,
                    ImageUrl = "TableMountNationalPark.jpg"
                },
                new Venue
                {
                    VenueName = "Durban Botanic Gardens",
                    Location = "Durban",
                    Capacity = 1234,
                    ImageUrl = "DurbanBotanicGarden.jpeg"
                }
                
            };

            foreach (var v in venues)
            {
                context.Venues.Add(v);
            }

            context.SaveChanges();
        }
    }
}