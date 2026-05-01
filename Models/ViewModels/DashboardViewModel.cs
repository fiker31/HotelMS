namespace HotelMS.Models.ViewModels
{
    public class MonthlyRevenueData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalGuests { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalReservations { get; set; }
        public int ActiveReservations { get; set; }
        public decimal TotalRevenue { get; set; }
        public double OccupancyRate { get; set; }
        public List<ReservationViewModel> RecentReservations { get; set; } = new();
        public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();
        public Dictionary<string, int> RoomTypeBreakdown { get; set; } = new();
        public int TotalEmployees { get; set; }
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
    }
}
