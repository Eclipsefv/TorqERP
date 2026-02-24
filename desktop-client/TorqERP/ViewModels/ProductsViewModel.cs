using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using System.Globalization;
using TorqERP.DataModels;
using TorqERP.Services;

namespace TorqERP.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ISnackbar _snackbar;
        public readonly CultureInfo EuroCulture = CultureInfo.GetCultureInfo("es-ES");

        public ProductsViewModel(ApiService apiService, ISnackbar snackbar)
        {
            _apiService = apiService;
            _snackbar = snackbar;
        }

        [ObservableProperty] private List<Product> _products = new();
        [ObservableProperty] private bool _isLoading = true;
        [ObservableProperty] private string _searchString = string.Empty;
        [ObservableProperty] private bool _isDialogVisible;
        [ObservableProperty] private Product _currentProduct = new();
        [ObservableProperty] private bool _isEditMode;

        public DialogOptions DialogOptions { get; } = new() { MaxWidth = MaxWidth.Small, FullWidth = true };

        [RelayCommand]
        public async Task InitializeAsync() => await LoadProductsAsync();

        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                Products = await _apiService.GetProductsAsync() ?? new();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading products: {ex.Message}", Severity.Error);
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
            CurrentProduct = new Product { Type = ProductType.ITEM };
            IsDialogVisible = true;
        }

        [RelayCommand]
        public void CloseDialog() => IsDialogVisible = false;

        [RelayCommand]
        public async Task SaveProductAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentProduct.Name) || string.IsNullOrWhiteSpace(CurrentProduct.Sku))
            {
                _snackbar.Add("Name and SKU are required", Severity.Warning);
                return;
            }

            if (CurrentProduct.SellPrice < CurrentProduct.BuyPrice)
            {
                _snackbar.Add("Can't sell lower than buy price", Severity.Warning);
                return;
            }

            if (IsEditMode) await UpdateProductLogic();
            else await CreateProductLogic();
        }

        private async Task CreateProductLogic()
        {
            try
            {
                if (await _apiService.CreateProductAsync(CurrentProduct))
                {
                    _snackbar.Add("Product created successfully", Severity.Success);
                    await LoadProductsAsync();
                    CloseDialog();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
        }

        private async Task UpdateProductLogic()
        {
            try
            {
                if (await _apiService.UpdateProductAsync(CurrentProduct))
                {
                    _snackbar.Add("Product updated successfully", Severity.Success);
                    await LoadProductsAsync();
                    CloseDialog();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
        }

        public void SetupEditMode(Product product)
        {
            IsEditMode = true;
            CurrentProduct = new Product
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                Type = product.Type,
                Location = product.Location,
                BuyPrice = product.BuyPrice,
                SellPrice = product.SellPrice,
                TaxRate = product.TaxRate,
                Stock = product.Stock,
                MinStock = product.MinStock,
                Description = product.Description
            };
            IsDialogVisible = true;
        }

        public bool FilterProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(SearchString)) return true;
            return (product.Name?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (product.Sku?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (product.Location?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}