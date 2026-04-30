using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TorqERP.DataModels;
using TorqERP.Services;
using Color = MudBlazor.Color;

namespace TorqERP.ViewModels
{
    public partial class SuppliersViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ISnackbar _snackbar;
        public readonly CultureInfo EuroCulture = CultureInfo.GetCultureInfo("es-ES");

        public SuppliersViewModel(ApiService apiService, ISnackbar snackbar)
        {
            _apiService = apiService;
            _snackbar = snackbar;
        }

        [ObservableProperty] private List<Supplier> _suppliers = new();
        [ObservableProperty] private bool _isLoading = true;
        [ObservableProperty] private string _searchString = string.Empty;

        [ObservableProperty] private bool _isSupplierDialogVisible;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private Supplier _currentSupplier = new();

        [ObservableProperty] private List<DeliveryNote> _deliveryNotes = new();
        [ObservableProperty] private bool _isNoteListVisible;
        [ObservableProperty] private Supplier? _selectedSupplier;

        [ObservableProperty] private bool _isNoteDialogVisible;
        [ObservableProperty] private bool _isNoteEditMode;
        [ObservableProperty] private DeliveryNote _currentNote = new();

        [ObservableProperty] private List<Product> _products = new();

        public DialogOptions SupplierDialogOptions { get; } = new() { MaxWidth = MaxWidth.Small, FullWidth = true };
        public DialogOptions NoteDialogOptions { get; } = new() { MaxWidth = MaxWidth.Large, FullWidth = true };

        [RelayCommand]
        public async Task InitializeAsync()
        {
            await Task.WhenAll(LoadSuppliersAsync(), LoadProductsAsync());
        }

        [RelayCommand]
        public async Task LoadSuppliersAsync()
        {
            try
            {
                IsLoading = true;
                Suppliers = await _apiService.GetSuppliersAsync() ?? new();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading suppliers: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            try
            {
                Products = await _apiService.GetProductsAsync() ?? new();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading products: {ex.Message}", Severity.Error);
            }
        }

        [RelayCommand]
        public async Task LoadDeliveryNotesForSupplierAsync(int supplierId)
        {
            try
            {
                var all = await _apiService.GetDeliveryNotesAsync() ?? new();
                DeliveryNotes = all.Where(n => n.SupplierId == supplierId).ToList();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading delivery notes: {ex.Message}", Severity.Error);
            }
        }

        [RelayCommand]
        public void OpenCreateSupplierDialog()
        {
            IsEditMode = false;
            CurrentSupplier = new Supplier();
            IsSupplierDialogVisible = true;
        }

        [RelayCommand]
        public void CloseSupplierDialog() => IsSupplierDialogVisible = false;

        [RelayCommand]
        public void OnSupplierRowClick(TableRowClickEventArgs<Supplier> args)
        {
            var s = args.Item;
            IsEditMode = true;
            CurrentSupplier = new Supplier
            {
                Id = s.Id,
                Cif = s.Cif,
                Name = s.Name,
                Address = s.Address,
                Phone = s.Phone,
                Email = s.Email,
                Notes = s.Notes,
                Active = s.Active
            };
            IsSupplierDialogVisible = true;
        }

        [RelayCommand]
        public async Task SaveSupplierAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentSupplier.Cif) || string.IsNullOrWhiteSpace(CurrentSupplier.Name))
            {
                _snackbar.Add("CIF and name are required.", Severity.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                if (!IsEditMode)
                {
                    if (await _apiService.CreateSupplierAsync(CurrentSupplier))
                    {
                        _snackbar.Add("Supplier created successfully.", Severity.Success);
                        CloseSupplierDialog();
                        await LoadSuppliersAsync();
                    }
                }
                else
                {
                    if (await _apiService.UpdateSupplierAsync(CurrentSupplier))
                    {
                        _snackbar.Add("Supplier updated successfully.", Severity.Success);
                        CloseSupplierDialog();
                        await LoadSuppliersAsync();
                    }
                }
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

        public bool FilterSupplier(Supplier supplier)
        {
            if (string.IsNullOrWhiteSpace(SearchString)) return true;
            return (supplier.Name?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (supplier.Cif?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (supplier.Email?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        [RelayCommand]
        public async Task OpenDeliveryNotesForSupplier(Supplier supplier)
        {
            SelectedSupplier = supplier;
            await LoadDeliveryNotesForSupplierAsync(supplier.Id);
            IsNoteListVisible = true;
        }

        [RelayCommand]
        public void CloseNoteList() => IsNoteListVisible = false;

        [RelayCommand]
        public void OpenCreateNoteDialog()
        {
            if (SelectedSupplier == null) return;
            IsNoteEditMode = false;
            CurrentNote = new DeliveryNote
            {
                SupplierId = SelectedSupplier.Id,
                Date = DateTime.Today
            };
            IsNoteDialogVisible = true;
        }

        [RelayCommand]
        public void OnNoteRowClick(TableRowClickEventArgs<DeliveryNote> args)
        {
            var n = args.Item;
            IsNoteEditMode = true;
            CurrentNote = new DeliveryNote
            {
                Id = n.Id,
                InternalNumber = n.InternalNumber,
                SupplierNoteNumber = n.SupplierNoteNumber,
                SupplierId = n.SupplierId,
                Date = n.Date,
                Notes = n.Notes,
                Lines = n.Lines?.ToList() ?? new(),
                Subtotal = n.Subtotal,
                TaxTotal = n.TaxTotal,
                Total = n.Total
            };
            IsNoteDialogVisible = true;
        }

        [RelayCommand]
        public void CloseNoteDialog() => IsNoteDialogVisible = false;

        [RelayCommand]
        public async Task SaveNoteAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentNote.SupplierNoteNumber))
            {
                _snackbar.Add("Supplier note number is required.", Severity.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                if (!IsNoteEditMode)
                {
                    var created = await _apiService.CreateDeliveryNoteAsync(CurrentNote);
                    if (created != null)
                    {
                        foreach (var line in CurrentNote.Lines)
                        {
                            line.DeliveryNoteId = created.Id;
                            await _apiService.AddDeliveryNoteLineAsync(line);
                        }
                        _snackbar.Add($"Delivery note {created.InternalNumber} created. Stock updated.", Severity.Success);
                        CloseNoteDialog();
                        await LoadDeliveryNotesForSupplierAsync(CurrentNote.SupplierId);
                    }
                }
                else
                {
                    var newLines = CurrentNote.Lines.Where(l => l.Id == 0).ToList();
                    if (await _apiService.UpdateDeliveryNoteAsync(CurrentNote))
                    {
                        foreach (var line in newLines)
                        {
                            line.DeliveryNoteId = CurrentNote.Id;
                            await _apiService.AddDeliveryNoteLineAsync(line);
                        }
                        _snackbar.Add(newLines.Any()
                            ? $"Note updated. {newLines.Count} new lines added and stock updated."
                            : "Delivery note updated.", Severity.Success);
                        CloseNoteDialog();
                        await LoadDeliveryNotesForSupplierAsync(CurrentNote.SupplierId);
                    }
                }
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

        [RelayCommand]
        public void AddEmptyNoteLine()
        {
            CurrentNote.Lines.Add(new DeliveryNoteLine
            {
                DeliveryNoteId = CurrentNote.Id,
                Quantity = 1,
                TaxRate = 21.0m
            });
            OnPropertyChanged(nameof(CurrentNote));
        }

        public async Task<IEnumerable<Product>> SearchProducts(string value, CancellationToken token)
        {
            if (!Products.Any()) await LoadProductsAsync();
            if (string.IsNullOrEmpty(value)) return Products.Where(p => p.Type == ProductType.ITEM);
            return Products.Where(p =>
                p.Type == ProductType.ITEM &&
                ((p.Name?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (p.Sku?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)));
        }

        public void OnProductSelected(DeliveryNoteLine line, Product selectedProduct)
        {
            if (selectedProduct == null) return;
            line.ProductId = selectedProduct.Id;
            line.UnitCost = selectedProduct.BuyPrice;
            line.TaxRate = selectedProduct.TaxRate;
            RecalculateLineTotals();
            OnPropertyChanged(nameof(CurrentNote));
        }

        public void RecalculateLineTotals()
        {
            decimal subtotal = 0, taxTotal = 0;
            foreach (var line in CurrentNote.Lines)
            {
                var base_ = line.UnitCost * (decimal)line.Quantity;
                var discounted = base_ - (base_ * line.Discount / 100);
                var tax = discounted * (line.TaxRate / 100);
                line.LineTotal = discounted + tax;
                subtotal += discounted;
                taxTotal += tax;
            }
            CurrentNote.Subtotal = subtotal;
            CurrentNote.TaxTotal = taxTotal;
            CurrentNote.Total = subtotal + taxTotal;
            OnPropertyChanged(nameof(CurrentNote));
        }
    }
}