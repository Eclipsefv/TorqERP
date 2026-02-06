using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TorqERP.Services
{
    public class SimpleAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var email = await SecureStorage.Default.GetAsync("user_email");
                var role = await SecureStorage.Default.GetAsync("user_role");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
                {
                    return new AuthenticationState(_anonymous);
                }

                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, role)
                }, "CustomAuth");

                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception)
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyLogin(string email, string role)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            }, "CustomAuth");

            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyLogout()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymous));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}