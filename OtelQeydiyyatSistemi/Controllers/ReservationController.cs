using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtelQeydiyyatSistemi.Data;
using OtelQeydiyyatSistemi.Models;
using OtelQeydiyyatSistemi.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OtelQeydiyyatSistemi.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservation
        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Rezervasiyalar səhifəsi açılır...");
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                System.Diagnostics.Debug.WriteLine($"Cari istifadəçi ID: {userId}");
                
                // Admin bütün rezervasiyaları görə bilər, normal istifadəçilər isə yalnız öz rezervasiyalarını
                IQueryable<Reservation> query;
                
                if (User.IsInRole("Admin"))
                {
                    System.Diagnostics.Debug.WriteLine("Admin rolu aşkarlandı, bütün rezervasiyalar göstəriləcək");
                    query = _context.Reservations
                        .Include(r => r.Room)
                        .Include(r => r.User);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Normal istifadəçi, yalnız öz rezervasiyaları göstəriləcək");
                    query = _context.Reservations
                        .Include(r => r.Room)
                        .Where(r => r.UserId == userId);
                }
                
                // Room.RoomType-ə ayrıca Include etmək lazımdır
                query = query.Include("Room.RoomType");
                
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
                System.Diagnostics.Debug.WriteLine($"Rezervasiyalar yüklənərkən XƏTA: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Xəta stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Rezervasiyalarınız yüklənərkən xəta baş verdi: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Reservation/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (reservation == null)
            {
                return NotFound();
            }
            
            // Müştəri yalnız öz rezervasiyalarını görə bilər
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && reservation.UserId != userId)
            {
                return Forbid();
            }

            return View(reservation);
        }

        // GET: Reservation/Create
        [Authorize]
        public async Task<IActionResult> Create()
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
                
                var model = new ReservationCreateViewModel
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
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ReservationCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PrepareViewDataForReservation(model);
                    return View(model);
                }
                
                // Tarixləri yoxla
                if (model.CheckInDate < DateTime.Today)
                {
                    ModelState.AddModelError("CheckInDate", "Giriş tarixi bugündən əvvəl ola bilməz.");
                    await PrepareViewDataForReservation(model);
                    return View(model);
                }
                
                if (model.CheckOutDate <= model.CheckInDate)
                {
                    ModelState.AddModelError("CheckOutDate", "Çıxış tarixi giriş tarixinə bərabər və ya ondan əvvəl ola bilməz.");
                    await PrepareViewDataForReservation(model);
                    return View(model);
                }
                
                // Otağın uyğunluğunu yoxla
                var isRoomAvailable = await IsRoomAvailable(model.RoomId, model.CheckInDate, model.CheckOutDate);
                if (!isRoomAvailable)
                {
                    ModelState.AddModelError("RoomId", "Seçilmiş otaq bu tarixlərdə artıq rezervasiya edilib.");
                    await PrepareViewDataForReservation(model);
                    return View(model);
                }
                
                // Otaq məlumatlarını əldə et
                var room = await _context.Rooms
                    .Include(r => r.RoomType)
                    .FirstOrDefaultAsync(r => r.Id == model.RoomId);
                    
                if (room == null)
                {
                    ModelState.AddModelError("RoomId", "Seçilmiş otaq tapılmadı.");
                    
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
                    return View(model);
                }
                
                // Qalma müddətini hesabla
                var nights = (model.CheckOutDate - model.CheckInDate).Days;
                
                // Ümumi qiyməti hesabla
                var totalPrice = room.PricePerNight * nights;
                
                // Cari istifadəçini əldə et
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Rezervasiya yarat
                var reservation = new Reservation
                {
                    UserId = userId,
                    RoomId = model.RoomId,
                    CheckInDate = model.CheckInDate,
                    CheckOutDate = model.CheckOutDate,
                    Adults = model.Adults,
                    Children = model.Children,
                    SpecialRequests = model.SpecialRequests,
                    PaymentMethod = model.PaymentMethod,
                    Status = ReservationStatus.Confirmed,
                    TotalPrice = totalPrice,
                    ReservationDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Rezervasiya uğurla yaradıldı.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Rezervasiya yaradılarkən xəta baş verdi: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Reservation/Cancel/5
        [Authorize]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (reservation == null)
            {
                return NotFound();
            }

            // Müştəri yalnız öz rezervasiyalarını ləğv edə bilər
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && reservation.UserId != userId)
            {
                return Forbid();
            }
            
            // Artıq başlamış rezervasiyanı ləğv etmək olmaz (admin istisna olmaqla)
            if (!User.IsInRole("Admin") && (reservation.Status == ReservationStatus.CheckedIn || reservation.Status == ReservationStatus.CheckedOut))
            {
                TempData["ErrorMessage"] = "Artıq başlamış və ya tamamlanmış rezervasiyanı ləğv etmək olmaz.";
                return RedirectToAction(nameof(Index));
            }

            return View(reservation);
        }

        // POST: Reservation/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            try 
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Room)
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (reservation == null)
                {
                    return NotFound();
                }
                
                // Müştəri yalnız öz rezervasiyalarını ləğv edə bilər
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!User.IsInRole("Admin") && reservation.UserId != userId)
                {
                    return Forbid();
                }
                
                // Artıq başlamış rezervasiyanı ləğv etmək olmaz (admin istisna olmaqla)
                if (!User.IsInRole("Admin") && (reservation.Status == ReservationStatus.CheckedIn || reservation.Status == ReservationStatus.CheckedOut))
                {
                    TempData["ErrorMessage"] = "Artıq başlamış və ya tamamlanmış rezervasiyanı ləğv etmək olmaz.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Rezervasiyanı ləğv et
                reservation.Status = ReservationStatus.Cancelled;
                _context.Update(reservation);
                
                // Otağın statusunu yenilə
                var room = reservation.Room;
                room.Status = RoomStatus.Available;
                _context.Update(room);
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Rezervasiya uğurla ləğv edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Rezervasiya ləğv edilərkən xəta baş verdi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Reservation/CheckIn/5 (Admin üçün)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckIn(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (reservation == null)
            {
                return NotFound();
            }
            
            return View(reservation);
        }

        // POST: Reservation/CheckIn/5 (Admin üçün)
        [HttpPost, ActionName("CheckIn")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckInConfirmed(int id)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Room)
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (reservation == null)
                {
                    return NotFound();
                }
                
                // Rezervasiyanın statusunu yenilə
                reservation.Status = ReservationStatus.CheckedIn;
                _context.Update(reservation);
                
                // Otağın statusunu yenilə
                var room = reservation.Room;
                room.Status = RoomStatus.Occupied;
                _context.Update(room);
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Müştəri uğurla qeydiyyatdan keçdi.";
                return RedirectToAction("Reservations", "Admin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Check-in əməliyyatı zamanı xəta baş verdi: {ex.Message}";
                return RedirectToAction("Reservations", "Admin");
            }
        }

        // GET: Reservation/CheckOut/5 (Admin üçün)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckOut(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (reservation == null)
            {
                return NotFound();
            }
            
            return View(reservation);
        }

        // POST: Reservation/CheckOut/5 (Admin üçün)
        [HttpPost, ActionName("CheckOut")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckOutConfirmed(int id)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Room)
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (reservation == null)
                {
                    return NotFound();
                }
                
                // Rezervasiyanın statusunu yenilə
                reservation.Status = ReservationStatus.CheckedOut;
                _context.Update(reservation);
                
                // Otağın statusunu yenilə
                var room = reservation.Room;
                room.Status = RoomStatus.Available;
                _context.Update(room);
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Müştəri uğurla çıxış etdi.";
                return RedirectToAction("Reservations", "Admin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Check-out əməliyyatı zamanı xəta baş verdi: {ex.Message}";
                return RedirectToAction("Reservations", "Admin");
            }
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
        
        // Rezervasiya yaratma və redaktə səhifələri üçün ViewData hazırlamaq
        private async Task PrepareViewDataForReservation(ReservationCreateViewModel model)
        {
            try
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
            }
            catch (Exception ex)
            {
                // Xəta halında boş bir SelectList yaradırıq ki, səhifə çökməsin
                ViewData["RoomId"] = new SelectList(new List<SelectListItem>(), "Value", "Text");
                ModelState.AddModelError(string.Empty, $"Otaq məlumatları yüklənərkən xəta baş verdi: {ex.Message}");
            }
        }
    }
}
