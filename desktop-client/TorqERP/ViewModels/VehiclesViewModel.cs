using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using TorqERP.Components.Pages;
using TorqERP.DataModels;
using TorqERP.Services;

namespace TorqERP.ViewModels
{
    public partial class VehiclesViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ISnackbar _snackbar;

        public VehiclesViewModel(ApiService apiService, ISnackbar snackbar)
        {
            _apiService = apiService;
            _snackbar = snackbar;
        }

        [ObservableProperty]
        private List<Vehicle> _vehicles = new();

        [ObservableProperty]
        private List<Customer> _customers = new();

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private string _searchString = string.Empty;

        [ObservableProperty]
        private bool _isDialogVisible;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private Vehicle _currentVehicle = new();

        [ObservableProperty]
        private Customer? _selectedCustomer;

        public DialogOptions DialogOptions { get; } = new() { MaxWidth = MaxWidth.Small, FullWidth = true };

        [RelayCommand]
        public async Task InitializeAsync()
        {
            await LoadVehiclesAsync();
            await LoadCustomersAsync();
        }

        [RelayCommand]
        public async Task LoadVehiclesAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _apiService.GetVehiclesAsync();
                Vehicles = result ?? new List<Vehicle>();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading vehicles: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadCustomersAsync()
        {
            try
            {
                var result = await _apiService.GetCustomersAsync();
                Customers = result ?? new List<Customer>();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading customers: {ex.Message}", Severity.Error);
            }
        }

        [RelayCommand]
        public void OpenCreateDialog()
        {
            IsEditMode = false;
            CurrentVehicle = new Vehicle();
            SelectedCustomer = null;
            IsDialogVisible = true;
        }

        [RelayCommand]
        public void CloseDialog() => IsDialogVisible = false;

        [RelayCommand]
        public void OnRowClick(TableRowClickEventArgs<Vehicle> args)
        {
            IsEditMode = true;
            CurrentVehicle = new Vehicle
            {
                Id = args.Item.Id,
                Plate = args.Item.Plate,
                Brand = args.Item.Brand,
                Model = args.Item.Model,
                Year = args.Item.Year,
                CustomerId = args.Item.CustomerId
            };
            SelectedCustomer = Customers.FirstOrDefault(c => c.Id == CurrentVehicle.CustomerId);
            IsDialogVisible = true;
        }

        [RelayCommand]
        public async Task SaveVehicleAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentVehicle.Plate) || SelectedCustomer == null)
            {
                _snackbar.Add("Plate and owner are required.", Severity.Warning);
                return;
            }

            CurrentVehicle.CustomerId = SelectedCustomer.Id;

            if (IsEditMode) await UpdateVehicleLogic();
            else await CreateVehicleLogic();
        }

        private async Task CreateVehicleLogic()
        {
            try
            {
                if (await _apiService.CreateVehicleAsync(CurrentVehicle))
                {
                    _snackbar.Add("Vehicle created successfully", Severity.Success);
                    await LoadVehiclesAsync();
                    CloseDialog();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task UpdateVehicleLogic()
        {
            try
            {
                if (await _apiService.UpdateVehicleAsync(CurrentVehicle))
                {
                    _snackbar.Add("Vehicle updated successfully", Severity.Success);
                    await LoadVehiclesAsync();
                    CloseDialog();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        public async Task<IEnumerable<Customer>> SearchCustomers(string value, CancellationToken token)
        {
            if (string.IsNullOrEmpty(value)) return Customers;
            return Customers.Where(x =>
                (x.Name?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Nif?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        public bool FilterVehicle(Vehicle vehicle)
        {
            if (string.IsNullOrWhiteSpace(SearchString)) return true;
            return (vehicle.Plate?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (vehicle.Brand?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (vehicle.Model?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}