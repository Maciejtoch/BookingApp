using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BookingApp.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data i godzina")]
        public DateTime Date { get; set; }

        [Required]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        [Required]
        [Display(Name = "Usługa")]
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public int? TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; }

        [Display(Name = "Status")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [Display(Name = "Notatki")]
        [StringLength(500)]
        public string Notes { get; set; }

        [Display(Name = "Data utworzenia")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum ReservationStatus
    {
        [Display(Name = "Oczekuje")] Pending,
        [Display(Name = "Zatwierdzona")] Approved,
        [Display(Name = "Anulowana")] Cancelled,
        [Display(Name = "Odrzucona")] Rejected
    }
}