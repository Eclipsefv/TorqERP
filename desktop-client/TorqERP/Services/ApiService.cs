using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using TorqERP.DataModels;
using System.Text.Json;

namespace TorqERP.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        //product functions
        /*
         * Receives the Product class
         */
        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/products/getProducts");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
                }

                return new List<Product>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error - ApiService: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<bool> CreateProductAsync(Product newProduct)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                var response = await _httpClient.PostAsJsonAsync("/api/products/insert", newProduct, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error response from API: {errorBody}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product updatedProduct)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                if (string.IsNullOrEmpty(updatedProduct.Id.ToString()) || updatedProduct.Id == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Error, product id null");
                    return false;
                }

                string url = $"/api/products/updateProduct/{updatedProduct.Id}";

                var response = await _httpClient.PutAsJsonAsync(url, updatedProduct, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error response from api: {errorBody}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error on update: {ex.Message}");
                return false;
            }
        }

        //user functions
        /*
         * Receives User Class
         */

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users/getUsers");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
                }

                return new List<User>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<User>();
            }
        }


        //create user
        public async Task<bool> CreateUserAsync(User newUser)
        {
            try
            {
                //I have to do this because my API follows camelcase convention standard so in my data model in C# its "Email" But in my API i use "email"
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/register", newUser, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error response from API: {errorBody}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        //deactivate user
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/users/deleteUserById/{userId}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error on user deactivation: {ex.Message}");
                return false;
            }
        }

        //testing functions
        public async Task<string> TestProductsLogAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/products/getProducts");

                if (response.IsSuccessStatusCode)
                {
                    string jsonRaw = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine("Response:");
                    System.Diagnostics.Debug.WriteLine(jsonRaw);

                    return jsonRaw;
                }
                return "Error: " + response.StatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return ex.Message;
            }
        }

        //Customer functions
        public async Task<bool> CreateCustomerAsync(Customer newCustomer)
        {
            try
            {
                //I have to do this because my API follows camelcase convention standard so in my data model in C# its "Email" But in my API i use "email"
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                var response = await _httpClient.PostAsJsonAsync("/api/customers/insert", newCustomer, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error response from API: {errorBody}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/customers/getCustomers");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Customer>>() ?? new List<Customer>();
                }

                return new List<Customer>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<Customer>();
            }
        }
        public async Task<bool> UpdateCustomerAsync(Customer updatedCustomer)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                if (string.IsNullOrEmpty(updatedCustomer.Id.ToString()) || updatedCustomer.Id == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Error, product id null");
                    return false;
                }

                string url = $"/api/customers/updateCustomer/{updatedCustomer.Id}";

                var response = await _httpClient.PutAsJsonAsync(url, updatedCustomer, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error response from api: {errorBody}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error on update: {ex.Message}");
                return false;
            }
        }
    }
}