using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorqERP.DataModels;
using TorqERP.Services;
using Color = MudBlazor.Color;

namespace TorqERP.ViewModels
{
    public partial class InvoicesViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ISnackbar _snackbar;

        public InvoicesViewModel(ApiService apiService, ISnackbar snackbar)
        {
            _apiService = apiService;
            _snackbar = snackbar;
        }

        [ObservableProperty]
        private List<Invoice> _invoices = new();

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private string _searchString = string.Empty;

        [ObservableProperty]
        private bool _isDialogVisible;

        [ObservableProperty]
        private Invoice _currentInvoice = new();

        public DialogOptions DialogOptions { get; } = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

        [RelayCommand]
        public async Task InitializeAsync()
        {
            await LoadInvoicesAsync();
        }

        [RelayCommand]
        public async Task LoadInvoicesAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _apiService.GetInvoicesAsync();
                Invoices = result ?? new List<Invoice>();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading invoices: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void OnRowClick(TableRowClickEventArgs<Invoice> args)
        {
            CurrentInvoice = args.Item;
            IsDialogVisible = true;
        }

        [RelayCommand]
        public void CloseDialog() => IsDialogVisible = false;

        public bool FilterInvoice(Invoice invoice)
        {
            if (string.IsNullOrWhiteSpace(SearchString)) return true;
            return (invoice.InvoiceNumber?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   invoice.Status.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase) ||
                   invoice.CustomerId.ToString().Contains(SearchString);
        }

        public static (Color color, string label) GetStatusDisplay(InvoiceStatus status) => status switch
        {
            InvoiceStatus.DRAFT     => (Color.Default,  "Draft"),
            InvoiceStatus.ISSUED    => (Color.Info,     "Issued"),
            InvoiceStatus.PAID      => (Color.Success,  "Paid"),
            InvoiceStatus.CANCELLED => (Color.Error,    "Cancelled"),
            _                       => (Color.Default,  status.ToString())
        };
    }
}
