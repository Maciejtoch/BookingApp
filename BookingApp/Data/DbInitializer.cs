using BookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            // Seed roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed admin
            if (await userManager.FindByEmailAsync("admin@bookingapp.pl") == null)
            {
                var admin = new AppUser
                {
                    UserName = "admin@bookingapp.pl",
                    Email = "admin@bookingapp.pl",
                    FirstName = "Adam",
                    LastName = "Admin",
                    PhoneNumber = "",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed test user
            if (await userManager.FindByEmailAsync("jan@example.com") == null)
            {
                var user = new AppUser
                {
                    UserName = "jan@example.com",
                    Email = "jan@example.com",
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    PhoneNumber = "",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, "User123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "User");
            }

            // Seed services
            if (!context.Services.Any())
            {
                var services = new[]
                {
                    new Service { Name = "Strzyżenie męskie", Description = "Klasyczne strzyżenie włosów dla mężczyzn. Obejmuje mycie, strzyżenie nożyczkami lub maszynką oraz stylizację.", Price = 50, DurationMinutes = 30 },
                    new Service { Name = "Strzyżenie damskie", Description = "Strzyżenie i stylizacja włosów dla kobiet. Obejmuje konsultację, strzyżenie i suszenie.", Price = 80, DurationMinutes = 60 },
                    new Service { Name = "Koloryzacja", Description = "Profesjonalna koloryzacja włosów z użyciem farb premium. Cena od.", Price = 150, DurationMinutes = 120 },
                    new Service { Name = "Pasemka / Highlights", Description = "Rozjaśnianie lub ciemnienie wybranych pasm włosów dla efektu głębi.", Price = 200, DurationMinutes = 150 },
                    new Service { Name = "Keratynowe prostowanie", Description = "Profesjonalne prostowanie keratynowe eliminujące puszenie i zapewniające gładkość na tygodnie.", Price = 350, DurationMinutes = 180 }
                };
                context.Services.AddRange(services);
                await context.SaveChangesAsync();

                // Seed time slots for next 7 days
                var allServices = context.Services.ToList();
                var slots = new System.Collections.Generic.List<TimeSlot>();
                for (int day = 1; day <= 7; day++)
                {
                    var date = DateTime.Today.AddDays(day);
                    if (date.DayOfWeek == DayOfWeek.Sunday) continue;
                    foreach (var svc in allServices)
                    {
                        for (int hour = 9; hour <= 17; hour += 2)
                        {
                            slots.Add(new TimeSlot
                            {
                                ServiceId = svc.Id,
                                DateTime = date.AddHours(hour),
                                IsAvailable = true
                            });
                        }
                    }
                }
                context.TimeSlots.AddRange(slots);
                await context.SaveChangesAsync();
            }
        }
    }
}