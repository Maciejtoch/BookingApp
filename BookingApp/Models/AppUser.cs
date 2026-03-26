using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BookingApp.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Usługa")]
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        [Required]
        [Display(Name = "Data i godzina")]
        public DateTime DateTime { get; set; }

        [Display(Name = "Dostępny")]
        public bool IsAvailable { get; set; } = true;

        public List<Reservation> Reservations { get; set; } = new();
    }

    public class AppUser : IdentityUser
    {
        [Display(Name = "Imię")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public List<Reservation> Reservations { get; set; } = new();
    }
}