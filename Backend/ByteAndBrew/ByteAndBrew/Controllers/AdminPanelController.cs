using ByteAndBrew.Dtos.Admin;
using ByteAndBrew.Dtos.Booking;
using ByteAndBrew.Dtos.Customer;
using ByteAndBrew.Dtos.Table;
using ByteAndBrew.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ByteAndBrew.Controllers
{
    public class AdminPanelController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AdminPanelController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("ByteAndBrewAPI");
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"));
        }

        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return RedirectToAction("Dashboard");
        }

        public IActionResult Login()
        {
            // If already authenticated, redirect to dashboard
            if (IsAuthenticated())
            {
                return RedirectToAction("Dashboard");
            }
            return View(new AdminLoginDto());
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View(model);
            }

            try
            {
                using var client = _httpClientFactory.CreateClient("ByteAndBrewAPI");
                var json = JsonSerializer.Serialize(model);
                Debug.WriteLine("Sending JSON: " + json);

                var response = await client.PostAsJsonAsync("Admins/login", model);
                Debug.WriteLine(response);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AdminResponses>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        HttpContext.Session.SetString("JWToken", result.Token);
                        HttpContext.Session.SetString("AdminUsername", result.Username);
                        HttpContext.Session.SetInt32("AdminId", result.AdminId);

                        return RedirectToAction("Dashboard");
                    }
                    else
                    {
                        ViewBag.Error = "Login response was invalid.";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Invalid username or password."
                        : $"Login failed: {response.StatusCode}";
                }

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = "Unable to connect to the server. Please try again later.";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been successfully logged out.";
            return RedirectToAction("Login");
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // Bookings
        public IActionResult CreateBooking()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login");
            return View(new BookingAndCustomerCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(BookingAndCustomerCreateDto model)
        {
            Debug.WriteLine("CreateBooking - Received model:");
            Debug.WriteLine($"Name: '{model.Name}'");
            Debug.WriteLine($"PhoneNumber: '{model.PhoneNumber}'");
            Debug.WriteLine($"StartTime: {model.StartTime}");
            Debug.WriteLine($"NumberOfGuests: {model.NumberOfGuests}");
            Debug.WriteLine($"TableId: {model.TableId}");

            // Check authentication
            if (!IsAuthenticated())
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Unauthorized();
                return RedirectToAction("Login");
            }

            // Validate the model
            if (!ModelState.IsValid)
            {
                // For AJAX requests, return validation errors
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.ContentType?.Contains("application/json") == true)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    return BadRequest(new { errors = errors });
                }

                // For regular form submissions, return partial view with tables loaded
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }

            // Additional validation
            if (model.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Booking time must be in the future.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new { errors = new[] { "Booking time must be in the future." } });
                }
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }

            if (model.NumberOfGuests <= 0)
            {
                ModelState.AddModelError("NumberOfGuests", "Number of guests must be greater than 0.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new { errors = new[] { "Number of guests must be greater than 0." } });
                }
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }

            try
            {
                using var httpClient = CreateAuthenticatedClient();

                // Step 1: Check if table exists and get its capacity
                var tableResponse = await httpClient.GetAsync($"Tables/{model.TableId}");
                if (!tableResponse.IsSuccessStatusCode)
                {
                    var error = "Selected table does not exist.";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return BadRequest(new { errors = new[] { error } });
                    }
                    TempData["Error"] = error;
                    await LoadTablesForBookingForm();
                    return PartialView("_CreateBookingPanel", model);
                }

                var table = await tableResponse.Content.ReadFromJsonAsync<TableReadDto>();
                if (table != null && model.NumberOfGuests > table.Capacity)
                {
                    var error = $"Number of guests ({model.NumberOfGuests}) exceeds table capacity ({table.Capacity}).";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return BadRequest(new { errors = new[] { error } });
                    }
                    ModelState.AddModelError("NumberOfGuests", error);
                    await LoadTablesForBookingForm();
                    return PartialView("_CreateBookingPanel", model);
                }

                // Step 2: Check if the time slot is still available
                var existingBookingsResponse = await httpClient.GetAsync("Bookings");
                if (existingBookingsResponse.IsSuccessStatusCode)
                {
                    var existingBookings = await existingBookingsResponse.Content.ReadFromJsonAsync<List<Booking>>();
                    var conflictingBooking = existingBookings?.FirstOrDefault(b =>
                        b.TableId == model.TableId &&
                        b.StartTime.Date == model.StartTime.Date &&
                        b.StartTime.Hour == model.StartTime.Hour);

                    if (conflictingBooking != null)
                    {
                        var error = "Selected time slot is no longer available. Please choose another time.";
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return BadRequest(new { errors = new[] { error } });
                        }
                        ModelState.AddModelError("StartTime", error);
                        await LoadTablesForBookingForm();
                        return PartialView("_CreateBookingPanel", model);
                    }
                }

                // Step 3: Create customer
                var customerDto = new CustomerCreateDto
                {
                    Name = model.Name?.Trim(),
                    PhoneNumber = model.PhoneNumber?.Trim()
                };

                var customerResponse = await httpClient.PostAsJsonAsync("Customers", customerDto);
                if (!customerResponse.IsSuccessStatusCode)
                {
                    var errorContent = await customerResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Customer creation failed: {errorContent}");

                    var error = "Failed to create customer. Please try again.";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return BadRequest(new { errors = new[] { error } });
                    }
                    TempData["Error"] = error;
                    await LoadTablesForBookingForm();
                    return PartialView("_CreateBookingPanel", model);
                }

                var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerReadDto>();
                if (customer == null)
                {
                    var error = "Customer creation failed (invalid response).";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return BadRequest(new { errors = new[] { error } });
                    }
                    TempData["Error"] = error;
                    await LoadTablesForBookingForm();
                    return PartialView("_CreateBookingPanel", model);
                }

                // Step 4: Create booking
                var bookingDto = new BookingCreateDto
                {
                    StartTime = model.StartTime,
                    NumberOfGuests = model.NumberOfGuests,
                    TableId = model.TableId,
                    CustomerId = customer.Id
                };

                var bookingResponse = await httpClient.PostAsJsonAsync("Bookings", bookingDto);
                if (bookingResponse.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Booking created successfully");

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Ok(new { success = true, message = "Booking created successfully!" });
                    }

                    TempData["Success"] = "Booking created successfully!";
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    var errorContent = await bookingResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Booking creation failed: {errorContent}");

                    var error = "Failed to create booking. Please try again.";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return BadRequest(new { errors = new[] { error } });
                    }
                    TempData["Error"] = error;
                    await LoadTablesForBookingForm();
                    return PartialView("_CreateBookingPanel", model);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP Exception: {ex.Message}");
                var error = "Unable to connect to the server. Please try again later.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new { errors = new[] { error } });
                }
                TempData["Error"] = error;
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                var error = "An unexpected error occurred. Please try again.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new { errors = new[] { error } });
                }
                TempData["Error"] = error;
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }
        }

        // Helper method to load tables for the booking form
        private async Task LoadTablesForBookingForm()
        {
            try
            {
                using var client = CreateAuthenticatedClient();
                var response = await client.GetAsync("Tables");
                var tables = response.IsSuccessStatusCode
                    ? await response.Content.ReadFromJsonAsync<List<TableReadDto>>()
                    : new List<TableReadDto>();

                ViewBag.Tables = tables?.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"Table {t.TableNumber} ({t.Capacity} seats)"
                }).ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load tables: {ex.Message}");
                ViewBag.Tables = new List<SelectListItem>();
            }
        }

        // Menu Items
        public IActionResult CreateMenuItem()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login");
            return View(new MenuItem());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(MenuItem model)
        {
            if (!ModelState.IsValid) return View(model);

            using var client = CreateAuthenticatedClient();
            var response = await client.PostAsJsonAsync("MenuItems", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Menu item added successfully!";
                return RedirectToAction("Dashboard");
            }
            TempData["Error"] = "Failed to add menu item.";
            return View(model);
        }

        // Tables
        public IActionResult CreateTable()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login");
            return View(new Table());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTable(Table model)
        {
            if (!ModelState.IsValid) return View(model);

            using var client = CreateAuthenticatedClient();
            var response = await client.PostAsJsonAsync("Tables", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Table added successfully!";
                return RedirectToAction("Dashboard");
            }
            TempData["Error"] = "Failed to add table.";
            return View(model);
        }

        public async Task<IActionResult> GetCreateBookingPanel()
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("Tables");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Tables = new List<SelectListItem>();
                return PartialView("_CreateBookingPanel", new BookingAndCustomerCreateDto());
            }

            // Deserialize the JSON into TableReadDto list
            var tables = await response.Content.ReadFromJsonAsync<List<TableReadDto>>();

            Debug.WriteLine(tables);

            // Map to SelectListItem for Razor dropdown
            ViewBag.Tables = tables?.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"Table {t.TableNumber} ({t.Capacity} seats)"
            }).ToList() ?? new List<SelectListItem>();

            return PartialView("_CreateBookingPanel", new BookingAndCustomerCreateDto());
        }


        public IActionResult GetCreateMenuItemPanel()
        {
            return PartialView("_CreateMenuItemPanel", new MenuItem());
        }

        public IActionResult GetCreateTablePanel()
        {
            return PartialView("_CreateTablePanel", new Table());
        }

        public async Task<IActionResult> GetBookingsPanel()
        {
            Debug.WriteLine("GetBookingsPanel");
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("Bookings");
            var bookings = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Booking>>()
                : new List<Booking>();
            return PartialView("_BookingsPanel", bookings);
        }

        public async Task<IActionResult> GetMenuItemsPanel()
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("MenuItems");
            var items = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<MenuItem>>()
                : new List<MenuItem>();
            return PartialView("_MenuItemsPanel", items);
        }

        public async Task<IActionResult> GetTablesPanel()
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("Tables");
            var tables = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Table>>()
                : new List<Table>();
            return PartialView("_TablesPanel", tables);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int tableId, DateTime date)
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("Bookings");
            var bookings = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Booking>>()
                : new List<Booking>();

            bookings ??= new List<Booking>(); // säkerställ att listan inte är null

            var slots = Enumerable.Range(10, 7)
                .Select(i => i * 2)
                .Select(hour => new
                {
                    Time = $"{hour:00}:00",
                    Display = $"{(hour % 12 == 0 ? 12 : hour % 12)}:00 {(hour >= 12 ? "PM" : "AM")}",
                    Available = !bookings.Any(b =>
                        b.TableId == tableId &&
                        b.StartTime.Date == date.Date &&
                        b.StartTime.Hour == hour)
                }).ToList();

            return Json(slots);
        }
    }
}