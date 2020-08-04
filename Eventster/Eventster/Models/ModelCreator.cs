using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventster.Models
{
    public class ModelCreator : DbContext
    {
        public ModelCreator(DbContextOptions<ModelCreator> settings)
            : base(settings)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PK definition
            modelBuilder.Entity<Concert>().HasKey(concert => new { concert.Id });
            modelBuilder.Entity<Ticket>().HasKey(ticket => new { ticket.Id, ticket.ConcertId });
            modelBuilder.Entity<TicketType>().HasKey(ticketType => new { ticketType.Id });
            modelBuilder.Entity<Client>().HasKey(client => new { client.Id });
            modelBuilder.Entity<Booking>().HasKey(booking => new { booking.ClientId, booking.ConcertId, booking.TicketId });
            modelBuilder.Entity<User>().HasKey(user => new { user.UserName });

            // Relations definition
            modelBuilder.Entity<Ticket>().HasOne(ticket => ticket.Concert).WithMany().HasForeignKey(ticket => ticket.ConcertId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Ticket>().HasOne(ticket => ticket.TicketType).WithMany().HasForeignKey(ticket => ticket.TicketTypeId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Booking>().HasOne(booking => booking.Concert).WithMany().HasForeignKey(booking => booking.ConcertId);
            modelBuilder.Entity<Booking>().HasOne(booking => booking.Ticket).WithMany().HasForeignKey(booking => new { booking.TicketId, booking.ConcertId });
            modelBuilder.Entity<Booking>().HasOne(booking => booking.Client).WithMany().HasForeignKey(booking => booking.ClientId);
        }

        public DbSet<Concert> Concert { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketType> TicketType { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<User> User { get; set; }
    }
}
