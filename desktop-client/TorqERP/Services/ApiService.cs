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

        //Test Functions
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
    }
}