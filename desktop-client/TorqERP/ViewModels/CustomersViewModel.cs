using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TorqERP.Services;
using TorqERP.DataModels;
using MudBlazor;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TorqERP.ViewModels
{
    /*
     * Toolkit generated code
     * needs to be partial to be able to use ObservableProperty
     * I don't want to use the preview version (code would look something like this: public partial bool Loading { get; set; } = true;)
     * So I have to cope with the warnings
     */
    public partial class CustomersViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ISnackbar _snackbar;
        [ObservableProperty]
        private bool _loading = true;

        [ObservableProperty]
        private string _searchString = "";

        [ObservableProperty]
        private List<Customer> _customers = new();

        [ObservableProperty]
        private bool _isDialogVisible;

        [ObservableProperty]
        private Customer _currentCustomer = new();

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private int _activeTabIndex;

        public DialogOptions DialogOptions { get; } = new() { MaxWidth = MaxWidth.Small, FullWidth = true };

        public CustomersViewModel(ApiService apiService, ISnackbar snackbar)
        {
            _apiService = apiService;
            _snackbar = snackbar;
        }

        [RelayCommand]
        public async Task InitializeAsync() => await LoadCustomersAsync();

        [RelayCommand]
        public void OpenCreateDialog()
        {
            IsEditMode = false;
            CurrentCustomer = new Customer();
            ActiveTabIndex = 1;
            IsDialogVisible = true;
        }

        [RelayCommand]
        public void CloseDialog()
        {
            IsDialogVisible = false;
            ActiveTabIndex = 0;
        }

        [RelayCommand]
        public async Task LoadCustomersAsync()
        {
            Loading = true;
            try
            {
                Customers = await _apiService.GetCustomersAsync();
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                Loading = false;
            }
        }

        [RelayCommand]
        public async Task SaveCustomerAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentCustomer.Name) || string.IsNullOrWhiteSpace(CurrentCustomer.Nif))
            {
                _snackbar.Add("Name and NIF are required.", Severity.Warning);
                return;
            }

            if (IsEditMode) await UpdateCustomerAsync();
            else await CreateCustomerAsync();
        }

        private async Task CreateCustomerAsync()
        {
            try
            {
                if (await _apiService.CreateCustomerAsync(CurrentCustomer))
                {
                    _snackbar.Add("Customer created successfully", Severity.Success);
                    await LoadCustomersAsync();
                    CloseDialog();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task UpdateCustomerAsync()
        {
            try
            {
                if (await _apiService.UpdateCustomerAsync(CurrentCustomer))
                {
                    _snackbar.Add("Customer updated successfully", Severity.Success);
                    await LoadCustomersAsync();
                    CloseDialog();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        public void OnRowClick(Customer selectedCustomer)
        {
            IsEditMode = true;
            CurrentCustomer = new Customer
            {
                Id = selectedCustomer.Id,
                Nif = selectedCustomer.Nif,
                Name = selectedCustomer.Name,
                Address = selectedCustomer.Address,
                Phonenumber = selectedCustomer.Phonenumber,
                Email = selectedCustomer.Email,
                Vehicles = selectedCustomer.Vehicles
            };
            ActiveTabIndex = 0;
            IsDialogVisible = true;
        }

        public bool FilterCustomer(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(SearchString)) return true;
            return customer.Name?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) == true ||
                   customer.Nif?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) == true ||
                   customer.Email?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) == true ||
                   customer.Phonenumber?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}