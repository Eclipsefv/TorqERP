using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TorqERP.DataModels;

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

                var errorMessage = await GetErrorMessageAsync(response);
                throw new Exception(errorMessage);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error: {httpEx.Message}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateProductAsync(Product updatedProduct)
        {
            try
            {
                if (updatedProduct.Id == 0)
                    throw new Exception("Product ID is not valid for update");

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                string url = $"/api/products/updateProduct/{updatedProduct.Id}";
                var response = await _httpClient.PutAsJsonAsync(url, updatedProduct, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorMessage = await GetErrorMessageAsync(response);
                throw new Exception(errorMessage);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error: {httpEx.Message}");
            }
            catch (Exception)
            {
                throw;
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
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/register", newUser, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                var errorMessage = await GetErrorMessageAsync(response);
                throw new Exception(errorMessage);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error: {httpEx.Message}");
            }
            catch (Exception)
            {
                throw;
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

                var response = await _httpClient.PostAsJsonAsync("/api/customers/insertCustomer", newCustomer, options);

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

        //Vehicle funcs
        public async Task<List<Vehicle>> GetVehiclesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/vehicles/getVehicles");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Vehicle>>() ?? new List<Vehicle>();
                }

                return new List<Vehicle>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<Vehicle>();
            }
        }
        public async Task<bool> CreateVehicleAsync(Vehicle newVehicle)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                var response = await _httpClient.PostAsJsonAsync("/api/vehicles/insert", newVehicle, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorMessage = await GetErrorMessageAsync(response);
                throw new Exception(errorMessage);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Connection Error: {httpEx.Message}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateVehicleAsync(Vehicle updatedVehicle)
        {
            try
            {
                if (updatedVehicle.Id == 0)
                    throw new Exception("Vehicle ID is not valid");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                string url = $"/api/vehicles/updateVehicle/{updatedVehicle.Id}";

                var response = await _httpClient.PutAsJsonAsync(url, updatedVehicle, options);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                var errorMessage = await GetErrorMessageAsync(response);
                throw new Exception(errorMessage);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Error: {httpEx.Message}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        //work orders
        public async Task<List<WorkOrder>> GetWorkOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/workOrders/getWorkOrders");
                if (response.IsSuccessStatusCode)
                {

                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() },
                        PropertyNameCaseInsensitive = true
                    };
                    return await response.Content.ReadFromJsonAsync<List<WorkOrder>>(options)
                           ?? new List<WorkOrder>();
                }
                return new List<WorkOrder>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<WorkOrder>();
            }
        }

        public async Task<WorkOrder?> CreateWorkOrderAsync(WorkOrder workOrder)
        {
            var payload = new
            {
                //work order id will be autogenerated by API
                vehicleId = workOrder.VehicleId,
                description = workOrder.Description
            };

            var response = await _httpClient.PostAsJsonAsync("api/workOrders/insert", payload);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WorkOrder>();
            }

            var errorMessage = await GetErrorMessageAsync(response);
            throw new Exception(errorMessage);
        }

        public async Task<WorkOrderLine?> AddWorkOrderLineAsync(WorkOrderLine line)
        {
            var payload = new
            {
                workOrderId = line.WorkOrderId,
                productId = line.ProductId,
                quantity = line.Quantity,
                price = line.Price,
                discount = line.Discount
            };

            var response = await _httpClient.PostAsJsonAsync("api/workOrders/addLine", payload);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WorkOrderLine>();
            }

            var errorMessage = await GetErrorMessageAsync(response);
            throw new Exception(errorMessage);
        }
        public async Task<bool> UpdateWorkOrderAsync(int id, WorkOrder order)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workOrders/update/{order.Id}", order);
            return response.IsSuccessStatusCode;
        }

        public async Task<Invoice?> ConvertToInvoiceAsync(int workOrderId)
        {
            var response = await _httpClient.PutAsync($"api/workOrders/convertToInvoice/{workOrderId}", null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Invoice>();
            }
            var errorMessage = await GetErrorMessageAsync(response);
            throw new Exception(errorMessage);
        }

        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/workOrders/getInvoices");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() },
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };

                    return await response.Content.ReadFromJsonAsync<List<Invoice>>(options)
                           ?? new List<Invoice>();
                }

                return new List<Invoice>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error - GetInvoicesAsync: {ex.Message}");
                return new List<Invoice>();
            }
        }


        //Appointments
        public async Task<List<Appointment>> GetAppointmentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/appointments/getAllAppointments");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await response.Content.ReadFromJsonAsync<List<Appointment>>(options)
                           ?? new List<Appointment>();
                }
                return new List<Appointment>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task<bool> CreateAppointmentAsync(Appointment appointment)
        {
            try
            {
                var payload = new
                {
                    vehicleId = appointment.VehicleId,
                    customerId = appointment.CustomerId,
                    scheduledAt = appointment.ScheduledAt.ToUniversalTime().ToString("o"),
                    description = appointment.Description
                };

                var response = await _httpClient.PostAsJsonAsync("api/appointments/createAppointment", payload);

                if (response.IsSuccessStatusCode)
                    return true;

                var errorMessage = await GetErrorMessageAsync(response);
                throw new Exception(errorMessage);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Network error: {httpEx.Message}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/suppliers/getSuppliers");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<Supplier>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> CreateSupplierAsync(Supplier supplier)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var response = await _httpClient.PostAsJsonAsync("api/suppliers/insert", supplier, options);
                if (response.IsSuccessStatusCode) return true;
                throw new Exception(await GetErrorMessageAsync(response));
            }
            catch (HttpRequestException httpEx) { throw new Exception($"Network error: {httpEx.Message}"); }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            try
            {
                if (supplier.Id == 0) throw new Exception("Supplier ID is not valid");
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var response = await _httpClient.PutAsJsonAsync($"api/suppliers/update/{supplier.Id}", supplier, options);
                if (response.IsSuccessStatusCode) return true;
                throw new Exception(await GetErrorMessageAsync(response));
            }
            catch (HttpRequestException httpEx) { throw new Exception($"Network error: {httpEx.Message}"); }
            catch (Exception) { throw; }
        }

        public async Task<List<DeliveryNote>> GetDeliveryNotesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/suppliers/getDeliveryNotes");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<List<DeliveryNote>>() ?? new();
                return new();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new();
            }
        }

        public async Task<DeliveryNote?> CreateDeliveryNoteAsync(DeliveryNote note)
        {
            var payload = new
            {
                supplierId = note.SupplierId,
                supplierNoteNumber = note.SupplierNoteNumber,
                date = note.Date.ToUniversalTime().ToString("o"),
                notes = note.Notes
            };

            var response = await _httpClient.PostAsJsonAsync("api/suppliers/insertDeliveryNote", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DeliveryNote>();

            throw new Exception(await GetErrorMessageAsync(response));
        }

        public async Task<DeliveryNoteLine?> AddDeliveryNoteLineAsync(DeliveryNoteLine line)
        {
            var payload = new
            {
                deliveryNoteId = line.DeliveryNoteId,
                productId = line.ProductId,
                quantity = line.Quantity,
                unitCost = line.UnitCost,
                taxRate = line.TaxRate,
                discount = line.Discount
            };

            var response = await _httpClient.PostAsJsonAsync("api/suppliers/addDeliveryNoteLine", payload);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DeliveryNoteLine>();

            throw new Exception(await GetErrorMessageAsync(response));
        }

        public async Task<bool> UpdateDeliveryNoteAsync(DeliveryNote note)
        {
            var payload = new
            {
                supplierNoteNumber = note.SupplierNoteNumber,
                date = note.Date.ToUniversalTime().ToString("o"),
                notes = note.Notes
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var response = await _httpClient.PutAsJsonAsync($"api/suppliers/updateDeliveryNote/{note.Id}", payload, options);
            return response.IsSuccessStatusCode;
        }

        //Private functions
        private async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
        {
            var errorBody = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(errorBody))
                return $"Error: {response.StatusCode}";

            try
            {
                using var doc = JsonDocument.Parse(errorBody);

                if (doc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetString() ?? "Error has no message str";
                }

                if (doc.RootElement.TryGetProperty("errors", out var errorsElement))
                {
                    return "Multiple errors detected";
                }
            }
            catch (JsonException)
            {
                return errorBody.Length > 100 ? errorBody.Substring(0, 100) : errorBody;
            }

            return errorBody;
        }
    }
}