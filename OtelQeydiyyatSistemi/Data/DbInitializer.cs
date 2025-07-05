using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtelQeydiyyatSistemi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OtelQeydiyyatSistemi.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // Verilənlər bazasını yarat və miqrasiyaları tətbiq et
                context.Database.EnsureCreated();
                
                // Rolları yarat
                await CreateRoles(roleManager);
                
                // Əgər admin istifadəçisi yoxdursa, yarat
                var adminExists = await userManager.FindByEmailAsync("admin@otel.az") != null;
                if (!adminExists)
                {
                    await CreateAdminUser(userManager);
                }
                
                // Əgər otaq növləri yoxdursa, yarat
                if (!context.RoomTypes.Any())
                {
                    await CreateRoomTypes(context);
                }
                
                // Əgər otaqlar yoxdursa, yarat
                if (!context.Rooms.Any())
                {
                    await CreateRooms(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Verilənlər bazası inizialisasiyasında xəta: {ex.Message}");
                Console.WriteLine($"Xətanın ətraflı məlumatı: {ex.StackTrace}");
            }
        }
        
        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Customer" };
            
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        
        private static async Task CreateAdminUser(UserManager<ApplicationUser> userManager)
        {
            try
            {
                var adminEmail = "admin@otel.az";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                
                if (adminUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true,
                        PhoneNumber = "+994501234567",
                        Address = "Bakı şəhəri",
                        RegistrationDate = DateTime.Now
                    };
                    
                    var result = await userManager.CreateAsync(user, "Admin123!");
                    
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                        Console.WriteLine("Admin istifadəçisi uğurla yaradıldı.");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"Admin yaradılması xətası: {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Admin istifadəçisi artıq mövcuddur.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Admin istifadəçisini yaradarkən xəta: {ex.Message}");
                Console.WriteLine($"Xətanın ətraflı məlumatı: {ex.StackTrace}");
            }
        }
        
        private static async Task CreateRoomTypes(ApplicationDbContext context)
        {
            if (!context.RoomTypes.Any())
            {
                var roomTypes = new RoomType[]
                {
                    new RoomType { Name = "Tək", Description = "Tək yataqlı otaq", BasePrice = 50 },
                    new RoomType { Name = "Cüt", Description = "Cüt yataqlı otaq", BasePrice = 80 },
                    new RoomType { Name = "Ailə", Description = "Ailə otağı (3-4 nəfər)", BasePrice = 120 },
                    new RoomType { Name = "Lüks", Description = "Lüks otaq", BasePrice = 200 },
                    new RoomType { Name = "Prezident", Description = "Prezident lüks otağı", BasePrice = 500 }
                };
                
                foreach (var roomType in roomTypes)
                {
                    context.RoomTypes.Add(roomType);
                }
                
                await context.SaveChangesAsync();
            }
        }
        
        private static async Task CreateRooms(ApplicationDbContext context)
        {
            if (!context.Rooms.Any())
            {
                // Otaq növlərini əldə et
                var roomTypes = await context.RoomTypes.ToListAsync();
                
                var rooms = new Room[]
                {
                    // Tək otaqlar (1-ci mərtəbə)
                    new Room { 
                        RoomNumber = "101", 
                        Floor = 1, 
                        Status = RoomStatus.Available, 
                        Description = "Tək yataqlı standart otaq", 
                        PricePerNight = 50, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Tək").Id,
                        ImageUrl = "/images/rooms/single1.jpg"
                    },
                    new Room { 
                        RoomNumber = "102", 
                        Floor = 1, 
                        Status = RoomStatus.Available, 
                        Description = "Tək yataqlı standart otaq", 
                        PricePerNight = 50, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Tək").Id,
                        ImageUrl = "/images/rooms/single2.jpg"
                    },
                    new Room { 
                        RoomNumber = "103", 
                        Floor = 1, 
                        Status = RoomStatus.Available, 
                        Description = "Tək yataqlı standart otaq", 
                        PricePerNight = 50, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Tək").Id,
                        ImageUrl = "/images/rooms/single3.jpg"
                    },
                    
                    // Cüt otaqlar (2-ci mərtəbə)
                    new Room { 
                        RoomNumber = "201", 
                        Floor = 2, 
                        Status = RoomStatus.Available, 
                        Description = "Cüt yataqlı standart otaq", 
                        PricePerNight = 80, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Cüt").Id,
                        ImageUrl = "/images/rooms/double1.jpg"
                    },
                    new Room { 
                        RoomNumber = "202", 
                        Floor = 2, 
                        Status = RoomStatus.Available, 
                        Description = "Cüt yataqlı standart otaq", 
                        PricePerNight = 80, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Cüt").Id,
                        ImageUrl = "/images/rooms/double2.jpg"
                    },
                    new Room { 
                        RoomNumber = "203", 
                        Floor = 2, 
                        Status = RoomStatus.Available, 
                        Description = "Cüt yataqlı standart otaq", 
                        PricePerNight = 80, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Cüt").Id,
                        ImageUrl = "/images/rooms/double3.jpg"
                    },
                    
                    // Ailə otaqları (3-cü mərtəbə)
                    new Room { 
                        RoomNumber = "301", 
                        Floor = 3, 
                        Status = RoomStatus.Available, 
                        Description = "Ailə otağı (3-4 nəfər)", 
                        PricePerNight = 120, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Ailə").Id,
                        ImageUrl = "/images/rooms/family1.jpg"
                    },
                    new Room { 
                        RoomNumber = "302", 
                        Floor = 3, 
                        Status = RoomStatus.Available, 
                        Description = "Ailə otağı (3-4 nəfər)", 
                        PricePerNight = 120, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Ailə").Id,
                        ImageUrl = "/images/rooms/family2.jpg"
                    },
                    
                    // Lüks otaqlar (4-cü mərtəbə)
                    new Room { 
                        RoomNumber = "401", 
                        Floor = 4, 
                        Status = RoomStatus.Available, 
                        Description = "Lüks otaq", 
                        PricePerNight = 200, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Lüks").Id,
                        ImageUrl = "/images/rooms/luxury1.jpg"
                    },
                    new Room { 
                        RoomNumber = "402", 
                        Floor = 4, 
                        Status = RoomStatus.Available, 
                        Description = "Lüks otaq", 
                        PricePerNight = 200, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Lüks").Id,
                        ImageUrl = "/images/rooms/luxury2.jpg"
                    },
                    
                    // Prezident lüks otağı (5-ci mərtəbə)
                    new Room { 
                        RoomNumber = "501", 
                        Floor = 5, 
                        Status = RoomStatus.Available, 
                        Description = "Prezident lüks otağı", 
                        PricePerNight = 500, 
                        RoomTypeId = roomTypes.First(rt => rt.Name == "Prezident").Id,
                        ImageUrl = "/images/rooms/presidential.jpg"
                    }
                };
                
                foreach (var room in rooms)
                {
                    context.Rooms.Add(room);
                }
                
                await context.SaveChangesAsync();
            }
        }
    }
}
