using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using TorqERP.DataModels;
using TorqERP.Services;
using Color = MudBlazor.Color;

namespace TorqERP.ViewModels
{
    public partial class AppointmentsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ISnackbar _snackbar;

        public AppointmentsViewModel(ApiService apiService, ISnackbar snackbar)
        {
            _apiService = apiService;
            _snackbar = snackbar;
        }

        [ObservableProperty] 
        private List<Appointment> _appointments = new();

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private DateTime _currentMonth = DateTime.Today;

        [ObservableProperty]
        private DateTime? _selectedDate;

        [ObservableProperty]
        private List<Appointment> _selectedDayAppointments = new();

        [ObservableProperty]
        private bool _isDetailsPanelOpen;

        private static readonly string[] WeekDayHeaders = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        public IEnumerable<string> GetWeekDayHeaders() => WeekDayHeaders;

        public string CurrentMonthLabel =>
            CurrentMonth.ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

        public List<DateTime?> GetCalendarDays()
        {
            var days = new List<DateTime?>();
            var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;

            for (int i = 0; i < startOffset; i++) days.Add(null);
            for (int d = 1; d <= daysInMonth; d++) days.Add(new DateTime(CurrentMonth.Year, CurrentMonth.Month, d));

            return days;
        }

        private static DateTime ToLocalDate(DateTime dt) =>
            (dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt).Date;

        public bool HasAppointments(DateTime date) => Appointments.Any(a => ToLocalDate(a.ScheduledAt) == date.Date);
        public int AppointmentCount(DateTime date) => Appointments.Count(a => ToLocalDate(a.ScheduledAt) == date.Date);
        public bool IsToday(DateTime date) => date.Date == DateTime.Today;
        public bool IsSelectedDay(DateTime date) => SelectedDate.HasValue && SelectedDate.Value.Date == date.Date;

        [RelayCommand]
        public async Task InitializeAsync() => await LoadAppointmentsAsync();

        [RelayCommand]
        public async Task LoadAppointmentsAsync()
        {
            try
            {
                IsLoading = true;
                Appointments = await _apiService.GetAppointmentsAsync() ?? new();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading appointments: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void PreviousMonth() => CurrentMonth = CurrentMonth.AddMonths(-1);
        public void NextMonth() => CurrentMonth = CurrentMonth.AddMonths(1);
        public void GoToToday()
        {
            CurrentMonth = DateTime.Today;
            SelectDate(DateTime.Today);
        }

        public void SelectDate(DateTime date)
        {
            SelectedDate = date;
            SelectedDayAppointments = Appointments
                .Where(a => ToLocalDate(a.ScheduledAt) == date.Date)
                .OrderBy(a => a.ScheduledAt)
                .ToList();
            IsDetailsPanelOpen = true;
        }

        public void CloseDetailsPanel()
        {
            IsDetailsPanelOpen = false;
            SelectedDate = null;
            SelectedDayAppointments = new();
        }

        public Color GetStatusColor(string status) => status switch
        {
            "SCHEDULED" => Color.Primary,
            "COMPLETED" => Color.Success,
            "CANCELLED" => Color.Error,
            _ => Color.Default
        };

        public string GetStatusIcon(string status) => status switch
        {
            "SCHEDULED" => Icons.Material.Filled.Schedule,
            "COMPLETED" => Icons.Material.Filled.CheckCircle,
            "CANCELLED" => Icons.Material.Filled.Cancel,
            _ => Icons.Material.Filled.HelpOutline
        };

        public string GetStatusLabel(string status) => status switch
        {
            "SCHEDULED" => "Scheduled",
            "COMPLETED" => "Completed",
            "CANCELLED" => "Cancelled",
            _ => status
        };
    }
}























