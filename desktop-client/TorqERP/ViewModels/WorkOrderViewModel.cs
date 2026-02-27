using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorqERP.Components.Pages;
using TorqERP.DataModels;
using TorqERP.Services;
using Color = MudBlazor.Color;

namespace TorqERP.ViewModels
{
    public partial class WorkOrdersViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ISnackbar _snackbar;

        public WorkOrdersViewModel(ApiService apiService, ISnackbar snackbar)
        {
            _apiService = apiService;
            _snackbar = snackbar;
        }

        [ObservableProperty]
        private List<WorkOrder> _workOrders = new();

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private string _searchString = string.Empty;

        [ObservableProperty]
        private bool _isDialogVisible;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private WorkOrder _currentWorkOrder = new();

        public DialogOptions DialogOptions { get; } = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        [ObservableProperty]
        private List<Vehicle> _vehicles = new();

        [ObservableProperty]
        private Vehicle? _selectedVehicle;

        [RelayCommand]
        public async Task InitializeAsync()
        {
            await Task.WhenAll(LoadWorkOrdersAsync(), LoadVehiclesAsync());
        }

        [RelayCommand]
        public async Task LoadVehiclesAsync()
        {
            try
            {
                Vehicles = await _apiService.GetVehiclesAsync();
            }
            catch (Exception ex)
            {
                _snackbar.Add("Error loading vehicles: " + ex.Message, Severity.Error);
            }
        }

        [RelayCommand]
        public async Task LoadWorkOrdersAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _apiService.GetWorkOrdersAsync();
                WorkOrders = result ?? new List<WorkOrder>();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading work orders: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void OpenCreateDialog()
        {
            IsEditMode = false;
            CurrentWorkOrder = new WorkOrder();
            SelectedVehicle = null;
            IsDialogVisible = true;
        }

        [RelayCommand]
        public void CloseDialog() => IsDialogVisible = false;

        [RelayCommand]
        public void OnRowClick(TableRowClickEventArgs<WorkOrder> args)
        {
            IsEditMode = true;
            CurrentWorkOrder = new WorkOrder
            {
                Id = args.Item.Id,
                OrderNumber = args.Item.OrderNumber,
                Description = args.Item.Description,
                Status = args.Item.Status,
                VehicleId = args.Item.VehicleId,
                CreatedAt = args.Item.CreatedAt,
                UpdatedAt = args.Item.UpdatedAt,
                CompletedAt = args.Item.CompletedAt,
                Lines = args.Item.Lines
            };
            IsDialogVisible = true;
        }
        partial void OnCurrentWorkOrderChanged(WorkOrder? value)
        {
            if (value != null && Vehicles.Any())
            {
                SelectedVehicle = Vehicles.FirstOrDefault(v => v.Id == value.VehicleId);
            }
        }
        public async Task<IEnumerable<Vehicle>> SearchVehicles(string value, CancellationToken token)
        {
            if (string.IsNullOrEmpty(value))
                return Vehicles;

            return Vehicles.Where(v =>
                v.Plate.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                v.Model.Contains(value, StringComparison.OrdinalIgnoreCase));
        }
        partial void OnSelectedVehicleChanged(Vehicle? value)
        {
            if (value != null && CurrentWorkOrder != null)
            {
                CurrentWorkOrder.VehicleId = value.Id;
            }
        }

        public bool FilterWorkOrder(WorkOrder workOrder)
        {
            if (string.IsNullOrWhiteSpace(SearchString)) return true;
            return (workOrder.OrderNumber?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (workOrder.Description?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   workOrder.Status.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
        }

        public static (Color color, string label) GetStatusDisplay(WorkOrderStatus status) => status switch
        {
            WorkOrderStatus.PENDING => (Color.Warning, "Pending"),
            WorkOrderStatus.IN_PROGRESS => (Color.Info, "In rogress"),
            WorkOrderStatus.COMPLETED => (Color.Success, "Completed"),
            WorkOrderStatus.CANCELLED => (Color.Error, "Cancelled"),
            _ => (Color.Default, status.ToString())
        };
    }
}
