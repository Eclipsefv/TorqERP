using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TorqERP.DataModels;
using TorqERP.Services;

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
            CalculateGrid();
        }

        [ObservableProperty]
        private List<Appointment> _appointments = new();

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private DateTime _currentDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private bool _isDialogVisible;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private Appointment _currentAppointment = new();

        public DialogOptions DialogOptions { get; } = new() { MaxWidth = MaxWidth.Small, FullWidth = true };

        [ObservableProperty]
        private List<Vehicle> _vehicles = new();

        [ObservableProperty]
        private List<Customer> _customers = new();

        [ObservableProperty]
        private DateTime? _selectedDate = DateTime.Today;

        [ObservableProperty]
        private TimeSpan? _selectedTime = DateTime.Now.TimeOfDay;

        public List<string> StatusOptions { get; } = new() { "SCHEDULED", "CONFIRMED", "COMPLETED", "CANCELLED" };

        [RelayCommand]
        public async Task InitializeAsync()
        {
            await Task.WhenAll(LoadAppointmentsAsync(), LoadVehiclesAsync(), LoadCustomersAsync());
        }

        [RelayCommand]
        public async Task LoadAppointmentsAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _apiService.GetAppointmentsAsync();
                Appointments = result ?? new List<Appointment>();
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

        private async Task LoadVehiclesAsync() {
           
        }
        private async Task LoadCustomersAsync() { 
        
        }


        [RelayCommand]
        private void NextMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
            CalculateGrid();
        }

        [RelayCommand]
        private void PreviousMonth()
        {
            CurrentDate = CurrentDate.AddMonths(-1);
            CalculateGrid();
        }

        private void CalculateGrid()
        {
            var firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            int offset = (int)firstDayOfMonth.DayOfWeek - (int)DayOfWeek.Monday;
            if (offset < 0) offset += 7;
            StartDate = firstDayOfMonth.AddDays(-offset);
        }

        public List<Appointment> GetAppointmentsForDay(DateTime date)
            => Appointments.Where(a => a.ScheduledAt.Date == date.Date).ToList();

        [RelayCommand]
        public void OpenCreateDialog(DateTime? date = null)
        {
            IsEditMode = false;
            CurrentAppointment = new Appointment();
            SelectedDate = date ?? DateTime.Today;
            SelectedTime = new TimeSpan(9, 0, 0);
            IsDialogVisible = true;
        }

        [RelayCommand]
        public void OpenEditDialog(Appointment appointment)
        {
            IsEditMode = true;
            CurrentAppointment = new Appointment
            {
                Id = appointment.Id,
                VehicleId = appointment.VehicleId,
                CustomerId = appointment.CustomerId,
                Description = appointment.Description,
                Status = appointment.Status,
                ScheduledAt = appointment.ScheduledAt
            };
            SelectedDate = appointment.ScheduledAt.Date;
            SelectedTime = appointment.ScheduledAt.TimeOfDay;
            IsDialogVisible = true;
        }

        [RelayCommand]
        public void CloseDialog() => IsDialogVisible = false;

        [RelayCommand]
        public async Task SaveAppointmentAsync()
        {
            if (CurrentAppointment.VehicleId == 0 || CurrentAppointment.CustomerId == 0)
            {
                _snackbar.Add("Please select vehicle and customer", Severity.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                if (SelectedDate.HasValue && SelectedTime.HasValue)
                {
                    CurrentAppointment.ScheduledAt = SelectedDate.Value.Date + SelectedTime.Value;
                }

                if (!IsEditMode)
                {
                    // insert logic
                    _snackbar.Add("Appointment scheduled", Severity.Success);
                }
                else
                {
                    // update logic
                    _snackbar.Add("Appointment updated", Severity.Success);
                }

                IsDialogVisible = false;
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}