namespace OtelQeydiyyatSistemi.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int TotalReservations { get; set; }
        public int PendingReservations { get; set; }
        public int TotalCustomers { get; set; }
    }
}
