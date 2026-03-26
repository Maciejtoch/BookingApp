using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookingApp.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [StringLength(100)]
        [Display(Name = "Nazwa usługi")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Opis jest wymagany")]
        [Display(Name = "Opis")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana")]
        [Range(0, 10000, ErrorMessage = "Cena musi być między 0 a 10000")]
        [Display(Name = "Cena (zł)")]
        public decimal Price { get; set; }

        [Display(Name = "Czas trwania (min)")]
        public int DurationMinutes { get; set; } = 60;

        [Display(Name = "Aktywna")]
        public bool IsActive { get; set; } = true;

        public List<Reservation> Reservations { get; set; } = new();
        public List<TimeSlot> TimeSlots { get; set; } = new();
    }
}