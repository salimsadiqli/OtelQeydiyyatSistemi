using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtelQeydiyyatSistemi.Data;
using OtelQeydiyyatSistemi.Models;
using OtelQeydiyyatSistemi.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OtelQeydiyyatSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Ana səhifədə boş otaqları göstər
            var availableRooms = await _context.Rooms
                .Where(r => r.Status == RoomStatus.Available)
                .Include(r => r.RoomType)
                .ToListAsync();

            return View(availableRooms);
        }

        public async Task<IActionResult> Rooms()
        {
            // Bütün otaq növlərini göstər
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Rooms)
                .ToListAsync();

            return View(roomTypes);
        }

        public async Task<IActionResult> RoomDetails(int? id)
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

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
