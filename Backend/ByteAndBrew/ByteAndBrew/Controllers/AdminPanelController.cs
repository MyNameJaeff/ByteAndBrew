using ByteAndBrew.Dtos.Admin;
using ByteAndBrew.Dtos.Booking;
using ByteAndBrew.Dtos.Customer;
using ByteAndBrew.Dtos.MenuItem;
using ByteAndBrew.Dtos.Table;
using ByteAndBrew.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Net.Http.Headers;

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
            var token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        private bool IsAuthenticated()
        {
            return Request.Cookies.ContainsKey("jwtToken");
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
                var response = await client.PostAsJsonAsync("Admins/login", model);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AdminResponses>();

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        Response.Cookies.Append("jwtToken", result.Token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTime.UtcNow.AddHours(2)
                        });

                        TempData["Success"] = $"Welcome {result.Username}";
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
            catch (HttpRequestException)
            {
                ViewBag.Error = "Unable to connect to the server. Please try again later.";
                return View(model);
            }
            catch
            {
                ViewBag.Error = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Remove the cookie
            Response.Cookies.Delete("jwtToken");
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

            if (!IsAuthenticated())
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Unauthorized();
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.ContentType?.Contains("application/json") == true)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }

            if (model.StartTime <= DateTime.Now)
            {
                var errorMsg = "Booking time must be in the future.";
                ModelState.AddModelError("StartTime", errorMsg);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest(new { errors = new[] { errorMsg } });
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }

            if (model.NumberOfGuests <= 0)
            {
                var errorMsg = "Number of guests must be greater than 0.";
                ModelState.AddModelError("NumberOfGuests", errorMsg);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest(new { errors = new[] { errorMsg } });
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }

            try
            {
                using var httpClient = CreateAuthenticatedClient();

                // Step 1: Validate table
                var tableResponse = await httpClient.GetAsync($"Tables/{model.TableId}");
                if (!tableResponse.IsSuccessStatusCode)
                {
                    var error = "Selected table does not exist.";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return BadRequest(new { errors = new[] { error } });
                    TempData["Error"] = error;
                    await LoadTablesForBookingForm();
                    return PartialView("_CreateBookingPanel", model);
                }

                var table = await tableResponse.Content.ReadFromJsonAsync<TableReadDto>();
                if (table != null && model.NumberOfGuests > table.Capacity)
                {
                    var error = $"Number of guests ({model.NumberOfGuests}) exceeds table capacity ({table.Capacity}).";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return BadRequest(new { errors = new[] { error } });
                    ModelState.AddModelError("NumberOfGuests", error);
                    await LoadTablesForBookingForm();
                    return PartialView("_CreateBookingPanel", model);
                }

                // Step 2: Check time slot availability
                var existingBookingsResponse = await httpClient.GetAsync("Bookings");
                if (existingBookingsResponse.IsSuccessStatusCode)
                {
                    var existingBookings = await existingBookingsResponse.Content.ReadFromJsonAsync<List<Booking>>();
                    var conflict = existingBookings?.FirstOrDefault(b =>
                        b.TableId == model.TableId &&
                        b.StartTime.Date == model.StartTime.Date &&
                        b.StartTime.Hour == model.StartTime.Hour);
                    if (conflict != null)
                    {
                        var error = "Selected time slot is no longer available. Please choose another time.";
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            return BadRequest(new { errors = new[] { error } });
                        ModelState.AddModelError("StartTime", error);
                        await LoadTablesForBookingForm();
                        return PartialView("_CreateBookingPanel", model);
                    }
                }

                // Step 3: Find or create customer
                var customerDto = new CustomerCreateDto
                {
                    Name = model.Name?.Trim(),
                    PhoneNumber = model.PhoneNumber?.Trim()
                };

                CustomerReadDto customer = null;

                var existingCustomerResponse = await httpClient.GetAsync($"Customers/search?phoneNumber={customerDto.PhoneNumber}");
                if (existingCustomerResponse.IsSuccessStatusCode && existingCustomerResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    customer = await existingCustomerResponse.Content.ReadFromJsonAsync<CustomerReadDto>();
                }

                if (customer == null)
                {
                    var customerResponse = await httpClient.PostAsJsonAsync("Customers", customerDto);
                    if (!customerResponse.IsSuccessStatusCode)
                    {
                        var error = "Failed to create customer. Please try again.";
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            return BadRequest(new { errors = new[] { error } });
                        TempData["Error"] = error;
                        await LoadTablesForBookingForm();
                        return PartialView("_CreateBookingPanel", model);
                    }

                    customer = await customerResponse.Content.ReadFromJsonAsync<CustomerReadDto>();
                    if (customer == null)
                    {
                        var error = "Customer creation failed (invalid response).";
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            return BadRequest(new { errors = new[] { error } });
                        TempData["Error"] = error;
                        await LoadTablesForBookingForm();
                        return PartialView("_CreateBookingPanel", model);
                    }
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
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Ok(new { success = true, message = "Booking created successfully!" });

                    TempData["Success"] = "Booking created successfully!";
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    var error = "Failed to create booking. Please try again.";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return BadRequest(new { errors = new[] { error } });
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
                    return BadRequest(new { errors = new[] { error } });
                TempData["Error"] = error;
                await LoadTablesForBookingForm();
                return PartialView("_CreateBookingPanel", model);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                var error = "An unexpected error occurred. Please try again.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest(new { errors = new[] { error } });
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
            return View(new MenuItemCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(MenuItemCreateDto model)
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
            return View(new TableCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTable(TableCreateDto model)
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
            return PartialView("_CreateMenuItemPanel", new MenuItemCreateDto());
        }

        public IActionResult GetCreateTablePanel()
        {
            return PartialView("_CreateTablePanel", new TableCreateDto());
        }

        public async Task<IActionResult> GetBookingsPanel()
        {
            Debug.WriteLine("GetBookingsPanel");

            using var client = CreateAuthenticatedClient();

            // Call the detailed bookings endpoint (requires authorization)
            var response = await client.GetAsync("Bookings/detailed");

            List<BookingReadDetailedDto> bookings;

            if (response.IsSuccessStatusCode)
            {
                bookings = await response.Content
                    .ReadFromJsonAsync<List<BookingReadDetailedDto>>() ?? new List<BookingReadDetailedDto>();
            }
            else
            {
                bookings = new List<BookingReadDetailedDto>();
                Debug.WriteLine($"Failed to fetch bookings: {response.StatusCode}");
            }

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
            try
            {
                using var client = CreateAuthenticatedClient();
                var response = await client.GetAsync("Bookings");
                var bookings = response.IsSuccessStatusCode
                    ? await response.Content.ReadFromJsonAsync<List<Booking>>()
                    : new List<Booking>();

                bookings ??= new List<Booking>();

                // Define restaurant operating hours (10:00 to 20:00, 2-hour intervals)
                var timeSlots = new List<object>();

                for (int hour = 10; hour <= 20; hour += 2) // 10:00 to 20:00 (last booking)
                {
                    var timeString = $"{hour:00}:00";

                    var isAvailable = !bookings.Any(b =>
                        b.TableId == tableId &&
                        b.StartTime.Date == date.Date &&
                        b.StartTime.Hour == hour);

                    timeSlots.Add(new
                    {
                        Time = timeString,
                        Available = isAvailable
                    });
                }

                return Json(timeSlots);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAvailableSlots: {ex.Message}");
                return StatusCode(500, new { error = "Failed to load available time slots" });
            }
        }

        [Authorize]
        [HttpDelete("Bookings/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            using var client = CreateAuthenticatedClient();
            var response = await client.DeleteAsync($"Bookings/{id}");

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { success = true, message = "Booking deleted successfully" });
            }
            return BadRequest(new { success = false, message = "Failed to delete booking" });
        }

        [Authorize]
        [HttpPut("Bookings/{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            using var client = CreateAuthenticatedClient();
            var response = await client.PutAsJsonAsync($"Bookings/{id}", dto);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { success = true, message = "Booking updated successfully" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { success = false, message = $"Failed to update booking: {errorContent}" });
        }
    }
}