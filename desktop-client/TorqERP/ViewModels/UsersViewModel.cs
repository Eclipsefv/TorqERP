using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using TorqERP.DataModels;
using TorqERP.Services;

namespace TorqERP.ViewModels
{
    public partial class UsersViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ISnackbar _snackbar;
        private readonly IDialogService _dialogService;

        public UsersViewModel(ApiService apiService, ISnackbar snackbar, IDialogService dialogService)
        {
            _apiService = apiService;
            _snackbar = snackbar;
            _dialogService = dialogService;
        }

        [ObservableProperty]
        private List<User> _users = new();

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private string _searchString = string.Empty;

        [ObservableProperty]
        private bool _isDialogVisible;

        [ObservableProperty]
        private User _currentUser = new();

        public DialogOptions DialogOptions { get; } = new() { MaxWidth = MaxWidth.Small, FullWidth = true };

        [RelayCommand]
        public async Task InitializeAsync() => await LoadUsersAsync();

        [RelayCommand]
        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _apiService.GetUsersAsync();
                Users = result ?? new List<User>();
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error loading users: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void OpenCreateDialog()
        {
            CurrentUser = new User { Role = UserRole.USER };
            IsDialogVisible = true;
        }

        [RelayCommand]
        public void CloseDialog() => IsDialogVisible = false;

        [RelayCommand]
        public async Task CreateUserAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.Email) || string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                _snackbar.Add("Email and/or password are required", Severity.Warning);
                return;
            }

            await CreateUserLogic();
        }

        private async Task CreateUserLogic()
        {
            try
            {
                if (await _apiService.CreateUserAsync(CurrentUser))
                {
                    _snackbar.Add("User created successfully", Severity.Success);
                    await LoadUsersAsync();
                    CloseDialog();
                }
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteUserAsync(User user)
        {
            bool? result = await _dialogService.ShowMessageBox(
                "IMPORTANT",
                $"You are going to deactivate {user.Username} proceed?",
                yesText: "Delete", cancelText: "Cancel");

            if (result == true)
            {
                try
                {
                    if (await _apiService.DeleteUserAsync(user.Id))
                    {
                        _snackbar.Add("User deactivated successfully", Severity.Success);
                        await LoadUsersAsync();
                    }
                    else
                    {
                        _snackbar.Add("Error on deactivation", Severity.Error);
                    }
                }
                catch (Exception ex)
                {
                    _snackbar.Add(ex.Message, Severity.Error);
                }
            }
        }

        public bool FilterUser(User user)
        {
            if (string.IsNullOrWhiteSpace(SearchString)) return true;
            return (user.Email?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (user.Username?.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   user.Role.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
        }
    }
}