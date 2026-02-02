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