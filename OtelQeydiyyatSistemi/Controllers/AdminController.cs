using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtelQeydiyyatSistemi.Data;
using OtelQeydiyyatSistemi.Models;
using OtelQeydiyyatSistemi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OtelQeydiyyatSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin
        public IActionResult Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalRooms = _context.Rooms.Count(),
                AvailableRooms = _context.Rooms.Count(r => r.Status == RoomStatus.Available),
                OccupiedRooms = _context.Rooms.Count(r => r.Status == RoomStatus.Occupied),
                TotalReservations = _context.Reservations.Count(),
                PendingReservations = _context.Reservations.Count(r => r.Status == ReservationStatus.Pending),
                TotalCustomers = _context.Users.Count(u => !_context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin")))
            };

            return View(viewModel);
        }


        
        // GET: Admin/Reservations
        public async Task<IActionResult> Reservations(string status)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Admin Rezervasiyalar səhifəsi açılır...");
                
                var query = _context.Reservations
                    .Include(r => r.Room)
                    .Include(r => r.User)
                    .AsQueryable();

                // Room.RoomType-ə ayrıca Include etmək lazımdır
                query = query.Include("Room.RoomType");
                    
                // Status filtri
                if (!string.IsNullOrEmpty(status))
                {
                    System.Diagnostics.Debug.WriteLine($"Status filtri tətbiq edilir: {status}");
                    if (Enum.TryParse<ReservationStatus>(status, out var reservationStatus))
                    {
                        query = query.Where(r => r.Status == reservationStatus);
                        System.Diagnostics.Debug.WriteLine($"Status filtri uğurla tətbiq edildi: {reservationStatus}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Status filtri tətbiq edilə bilmədi: {status}");
                    }
                }
                
                var reservations = await query
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Tapılan rezervasiyaların sayı: {reservations.Count}");
                
                // Rezervasiyaların mövcud olub-olmadığını yoxlayırıq
                if (reservations == null || !reservations.Any())
                {
                    System.Diagnostics.Debug.WriteLine("Heç bir rezervasiya tapılmadı");
                    TempData["InfoMessage"] = "Heç bir rezervasiya tapılmadı.";
                }
                
                return View(reservations);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin Rezervasiyalar yüklənərkən XƏTA: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Xəta stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Rezervasiyalar yüklənərkən xəta baş verdi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = string.Join(", ", roles)
                });
            }

            return View(userViewModels);
        }

        // GET: Admin/CreateAdmin
        public IActionResult CreateAdmin()
        {
            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Əgər "Admin" rolu yoxdursa, yarat
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                    // İstifadəçiyə "Admin" rolunu təyin et
                    await _userManager.AddToRoleAsync(user, "Admin");

                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                IsAdmin = roles.Contains("Admin")
            };

            return View(viewModel);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Address = model.Address;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Rol idarəsi
                    var roles = await _userManager.GetRolesAsync(user);
                    var isAdmin = roles.Contains("Admin");

                    if (model.IsAdmin && !isAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else if (!model.IsAdmin && isAdmin)
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Admin");
                    }

                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            // İstifadəçinin rezervasiyalarını yoxla
            var reservations = await _context.Reservations
                .Where(r => r.UserId == id)
                .ToListAsync();
                
            if (reservations.Any())
            {
                TempData["ErrorMessage"] = "Bu istifadəçinin aktiv rezervasiyaları var və silinə bilməz.";
                return RedirectToAction(nameof(Users));
            }
            
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "İstifadəçi uğurla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "İstifadəçi silinə bilmədi.";
            }
            
            return RedirectToAction(nameof(Users));
        }

        // Bu metod artıq yuxarıda var və parametrli versiyası ilə əvəz edilib

        // GET: Admin/CreateReservation
        public async Task<IActionResult> CreateReservation()
        {
            try
            {
                // Boş otaqları əldə et
                var availableRooms = await _context.Rooms
                    .Where(r => r.Status == RoomStatus.Available)
                    .Include(r => r.RoomType)
                    .ToListAsync();
                    
                var roomSelectList = availableRooms.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RoomNumber} - {r.RoomType.Name} - {r.PricePerNight}₼/gecə"
                }).ToList();
                
                ViewData["RoomId"] = new SelectList(roomSelectList, "Value", "Text");
                
                // Müştəriləri əldə et (admin olmayan istifadəçilər)
                var customers = await _userManager.GetUsersInRoleAsync("Customer");
                if (customers == null || !customers.Any())
                {
                    // Əgər Customer rolü yoxdursa, bütün qeyri-admin istifadəçiləri əldə et
                    var allUsers = await _userManager.Users.ToListAsync();
                    var admins = await _userManager.GetUsersInRoleAsync("Admin");
                    customers = allUsers.Except(admins).ToList();
                }
                
                var customerSelectList = customers.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})"
                }).ToList();
                
                ViewData["UserId"] = new SelectList(customerSelectList, "Value", "Text");
                
                var model = new AdminReservationCreateViewModel
                {
                    CheckInDate = DateTime.Today,
                    CheckOutDate = DateTime.Today.AddDays(1),
                    Adults = 1,
                    Children = 0
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Rezervasiya yaratma səhifəsi açılarkən xəta baş verdi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/CreateReservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(AdminReservationCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var availableRooms = await _context.Rooms
                        .Where(r => r.Status == RoomStatus.Available)
                        .Include(r => r.RoomType)
                        .ToListAsync();
                        
                    var roomSelectList = availableRooms.Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = $"{r.RoomNumber} - {r.RoomType.Name} - {r.PricePerNight}₼/gecə"
                    }).ToList();
                    
                    ViewData["RoomId"] = new SelectList(roomSelectList, "Value", "Text", model.RoomId);
                    
                    // Müştəriləri əldə et
                    var customers = await _userManager.GetUsersInRoleAsync("Customer");
                    if (customers == null || !customers.Any())
                    {
                        // Əgər Customer rolü yoxdursa, bütün qeyri-admin istifadəçiləri əldə et
                        var allUsers = await _userManager.Users.ToListAsync();
                        var admins = await _userManager.GetUsersInRoleAsync("Admin");
                        customers = allUsers.Except(admins).ToList();
                    }
                    
                    var customerSelectList = customers.Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = $"{u.FirstName} {u.LastName} ({u.Email})"
                    }).ToList();
                    
                    ViewData["UserId"] = new SelectList(customerSelectList, "Value", "Text", model.UserId);
                    
                    return View(model);
                }
                
                // Tarixləri yoxla
                if (model.CheckInDate < DateTime.Today)
                {
                    ModelState.AddModelError("CheckInDate", "Giriş tarixi bugündən əvvəl ola bilməz.");
                    PrepareViewDataForAdminReservation(model);
                    return View(model);
                }
                
                if (model.CheckOutDate <= model.CheckInDate)
                {
                    ModelState.AddModelError("CheckOutDate", "Çıxış tarixi giriş tarixinə bərabər və ya ondan əvvəl ola bilməz.");
                    PrepareViewDataForAdminReservation(model);
                    return View(model);
                }
                
                // Otağın uyğunluğunu yoxla
                var isRoomAvailable = await IsRoomAvailable(model.RoomId, model.CheckInDate, model.CheckOutDate);
                if (!isRoomAvailable)
                {
                    ModelState.AddModelError("RoomId", "Seçilmiş otaq bu tarixlərdə artıq rezervasiya edilib.");
                    PrepareViewDataForAdminReservation(model);
                    return View(model);
                }
                
                // Otaq məlumatlarını əldə et
                var room = await _context.Rooms
                    .Include(r => r.RoomType)
                    .FirstOrDefaultAsync(r => r.Id == model.RoomId);
                    
                if (room == null)
                {
                    ModelState.AddModelError("RoomId", "Seçilmiş otaq tapılmadı.");
                    PrepareViewDataForAdminReservation(model);
                    return View(model);
                }
                
                // Qalma müddətini hesabla
                var nights = (model.CheckOutDate - model.CheckInDate).Days;
                
                // Ümumi qiyməti hesabla
                var totalPrice = room.PricePerNight * nights;
                
                // Rezervasiya yarat
                var reservation = new Reservation
                {
                    UserId = model.UserId,
                    RoomId = model.RoomId,
                    CheckInDate = model.CheckInDate,
                    CheckOutDate = model.CheckOutDate,
                    Adults = model.Adults,
                    Children = model.Children,
                    SpecialRequests = model.SpecialRequests,
                    Status = ReservationStatus.Confirmed,
                    TotalPrice = totalPrice,
                    ReservationDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Rezervasiya uğurla yaradıldı.";
                return RedirectToAction(nameof(Reservations));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Rezervasiya yaradılarkən xəta baş verdi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Rooms
        public async Task<IActionResult> Rooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.RoomType)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            return View(rooms);
        }

        // GET: Admin/RoomTypes
        public async Task<IActionResult> RoomTypes()
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .ToListAsync();

            return View(roomTypes);
        }

        // GET: Admin/CreateRoomType
        public IActionResult CreateRoomType()
        {
            return View();
        }

        // POST: Admin/CreateRoomType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoomType(RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RoomTypes));
            }
            return View(roomType);
        }

        // GET: Admin/EditRoomType/5
        public async Task<IActionResult> EditRoomType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // POST: Admin/EditRoomType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoomType(int id, RoomType roomType)
        {
            if (id != roomType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomTypeExists(roomType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(RoomTypes));
            }
            return View(roomType);
        }

        // GET: Admin/DeleteRoomType/5
        public async Task<IActionResult> DeleteRoomType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // POST: Admin/DeleteRoomType/5
        [HttpPost, ActionName("DeleteRoomType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomTypeConfirmed(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType != null)
            {
                _context.RoomTypes.Remove(roomType);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(RoomTypes));
        }

        // GET: Admin/CreateRoom
        public IActionResult CreateRoom()
        {
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name");
            return View();
        }

        // POST: Admin/CreateRoom
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Rooms));
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", room.RoomTypeId);
            return View(room);
        }

        // GET: Admin/EditRoom/5
        public async Task<IActionResult> EditRoom(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", room.RoomTypeId);
            return View(room);
        }

        // POST: Admin/EditRoom/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Rooms));
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", room.RoomTypeId);
            return View(room);
        }

        // GET: Admin/DeleteRoom/5
        public async Task<IActionResult> DeleteRoom(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Admin/DeleteRoom/5
        [HttpPost, ActionName("DeleteRoom")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Rooms));
        }

        private bool RoomTypeExists(int id)
        {
            return _context.RoomTypes.Any(e => e.Id == id);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
        
        // Admin rezervasiya yaratma səhifəsi üçün ViewData hazırlamaq
        private async Task PrepareViewDataForAdminReservation(AdminReservationCreateViewModel model)
        {
            var availableRooms = await _context.Rooms
                .Where(r => r.Status == RoomStatus.Available)
                .Include(r => r.RoomType)
                .ToListAsync();
                
            var roomSelectList = availableRooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.RoomNumber} - {r.RoomType.Name} - {r.PricePerNight}₼/gecə"
            }).ToList();
            
            ViewData["RoomId"] = new SelectList(roomSelectList, "Value", "Text", model.RoomId);
            
            // Müştəriləri əldə et
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            if (customers == null || !customers.Any())
            {
                // Əgər Customer rolü yoxdursa, bütün qeyri-admin istifadəçiləri əldə et
                var allUsers = await _userManager.Users.ToListAsync();
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                customers = allUsers.Except(admins).ToList();
            }
            
            var customerSelectList = customers.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FirstName} {u.LastName} ({u.Email})"
            }).ToList();
            
            ViewData["UserId"] = new SelectList(customerSelectList, "Value", "Text", model.UserId);
        }
        
        // Otağın uyğunluğunu yoxlama metodu
        private async Task<bool> IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                // Giriş tarixi bugündən əvvəl ola bilməz
                if (checkIn.Date < DateTime.Today)
                {
                    return false;
                }
                
                // Çıxış tarixi giriş tarixinə bərabər və ya ondan əvvəl ola bilməz
                if (checkOut.Date <= checkIn.Date)
                {
                    return false;
                }
                
                // Əvvəlcə otağın statusunu yoxlayırıq
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Otaq tapılmadı: {roomId}");
                    return false;
                }
                
                // Mövcud rezervasiyaları yoxlayırıq
                var existingReservations = await _context.Reservations
                    .Where(r => r.RoomId == roomId && 
                           r.Status != ReservationStatus.Cancelled)
                    .ToListAsync();
                
                System.Diagnostics.Debug.WriteLine($"Otaq {roomId} üçün {existingReservations.Count} mövcud rezervasiya tapıldı");
                
                // Əgər heç bir rezervasiya yoxdursa, otaq uyğundur
                if (existingReservations.Count == 0)
                {
                    return true;
                }
                
                // Hər bir mövcud rezervasiyanı yoxlayırıq
                foreach (var reservation in existingReservations)
                {
                    System.Diagnostics.Debug.WriteLine($"Rezervasiya {reservation.Id}: {reservation.CheckInDate:yyyy-MM-dd} - {reservation.CheckOutDate:yyyy-MM-dd}, Status: {reservation.Status}");
                    
                    // Əgər rezervasiya artıq tamamlanıbsa (CheckedOut), yoxlamaya daxil etmə
                    if (reservation.Status == ReservationStatus.CheckedOut)
                    {
                        continue;
                    }
                    
                    // Tarix aralıqlarını yoxla
                    bool overlap = (
                        // Yeni giriş tarixi mövcud rezervasiya aralığındadır
                        (checkIn.Date >= reservation.CheckInDate.Date && checkIn.Date < reservation.CheckOutDate.Date) ||
                        // Yeni çıxış tarixi mövcud rezervasiya aralığındadır
                        (checkOut.Date > reservation.CheckInDate.Date && checkOut.Date <= reservation.CheckOutDate.Date) ||
                        // Yeni rezervasiya mövcud rezervasiyanı tam əhatə edir
                        (checkIn.Date <= reservation.CheckInDate.Date && checkOut.Date >= reservation.CheckOutDate.Date)
                    );
                    
                    if (overlap)
                    {
                        System.Diagnostics.Debug.WriteLine($"Tarix üst-üstə düşməsi aşkar edildi: {checkIn:yyyy-MM-dd} - {checkOut:yyyy-MM-dd} və {reservation.CheckInDate:yyyy-MM-dd} - {reservation.CheckOutDate:yyyy-MM-dd}");
                        return false;
                    }
                }
                
                // Bütün yoxlamalardan keçdiksə, otaq uyğundur
                return true;
            }
            catch (Exception ex)
            {
                // Xəta halında təhlükəsiz variant kimi false qaytaraq
                System.Diagnostics.Debug.WriteLine($"IsRoomAvailable metodu xətası: {ex.Message}");
                return false;
            }
        }
    }
}
