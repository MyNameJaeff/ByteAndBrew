// This file is so large due to issues with Razor including partial views with their own scripts.
// Considered splitting into multiple files if it grows further.
// (Partial views cant send data as easily as if i would have placed all the forms in their own files :( )

// ------------------------------
// Notification system
// ------------------------------
function showNotification(message, type = 'success') {
    const container = document.getElementById('notification-container');
    const notification = document.createElement('div');
    notification.className = `
    flex items-center p-4 rounded-md shadow-md min-w-[250px]
    text-white ${type === 'success' ? 'bg-green-500' : 'bg-red-500'}
    animate-slide-in
    `;

    notification.innerHTML = `
    <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} mr-2"></i>
    <span>${message}</span>
    <button class="ml-auto hover:text-gray-200" onclick="this.parentElement.remove()">
        <i class="fas fa-times"></i>
    </button>
    `;

    container.appendChild(notification);

    // Auto-remove after 5 seconds
    setTimeout(() => notification.remove(), 15000);
}


// ------------------------------
// Load management panels
// ------------------------------
async function loadPanel(type) {
    const container = document.getElementById("management-panel");

    // Find the button that was clicked
    const btn = document.querySelector(`button[onclick="loadPanel('${type}')"]`);

    // Toggle: if already loaded and visible, hide it
    const currentType = container.dataset.panelType;
    if (currentType === type && container.innerHTML.trim() !== '') {
        container.innerHTML = '';
        container.removeAttribute('data-panel-type');
        btn.textContent = `Manage ${type}`; // restore original text
        return;
    }

    // Close any other open panel first
    const otherPanel = document.getElementById("management-panel");
    if (otherPanel.dataset.panelType && otherPanel.dataset.panelType !== type) {
        const otherType = otherPanel.dataset.panelType;
        const otherBtn = document.querySelector(`button[onclick="loadPanel('${otherType}')"]`);
        otherPanel.innerHTML = '';
        otherPanel.removeAttribute('data-panel-type');
        if (otherBtn) otherBtn.textContent = `Manage ${otherType}`;
    }

    // Set the current panel type
    container.dataset.panelType = type;
    container.innerHTML = `
    <div class="text-center p-8">
        <i class="fas fa-spinner fa-spin text-4xl text-gray-400"></i>
        <p class="text-gray-500 mt-4">Loading...</p>
    </div>
    `;
    btn.textContent = `Close ${type}`; // change text

    try {
        const response = await fetch(`/AdminPanel/Get${type}Panel`);
        const html = await response.text();
        container.innerHTML = html;
        initializePanelForms(container);
    } catch (err) {
        container.innerHTML = '<p class="text-red-500 p-4">Failed to load panel</p>';
        showNotification('Failed to load panel', 'error');
        btn.textContent = `Manage ${type}`; // restore text on error
    }
}

// ------------------------------
// Initialize forms in loaded panel
// ------------------------------
function initializePanelForms(container) {
    const forms = container.querySelectorAll('form');
    forms.forEach(form => {
        if (form.dataset.initialized) return;
        form.dataset.initialized = 'true';

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            await handleFormSubmit(form);
        });
    });
}

// ------------------------------
// Toggle quick forms
// ------------------------------
async function toggleQuickForm(btn, type) {
    const container = btn.nextElementSibling;
    const isVisible = !container.classList.contains('hidden');

    // Hide all other quick forms first
    document.querySelectorAll('.quick-form-container').forEach(c => {
        if (c !== container) {
            c.innerHTML = '';
            c.classList.add('hidden');
            const otherBtn = c.previousElementSibling;
            if (otherBtn) {
                // Restore button text (remove "Close")
                otherBtn.textContent = otherBtn.dataset.originalText || otherBtn.textContent;
                otherBtn.disabled = false;
            }
        }
    });

    if (isVisible) {
        container.innerHTML = '';
        container.classList.add('hidden');
        btn.textContent = btn.dataset.originalText || btn.textContent; // restore original text
        btn.disabled = false;
        return;
    }

    // Save original text on first open
    if (!btn.dataset.originalText) btn.dataset.originalText = btn.textContent;

    container.innerHTML = '<p class="text-gray-500 p-4">Loading form...</p>';
    container.classList.remove('hidden');

    try {
        const response = await fetch(`/AdminPanel/Get${type}Panel`);
        const html = await response.text();
        container.innerHTML = html;

        if (type === 'CreateBooking') {
            initializeBookingForm(container);
        }

        initializeQuickForm(container, type);

        // Change button text to "Close …"
        btn.textContent = `Close ${btn.dataset.originalText.replace(/^Add\s*/, '')}`;
        btn.disabled = false;
    } catch (err) {
        container.innerHTML = '<p class="text-red-500 p-4">Failed to load form</p>';
        showNotification('Failed to load form', 'error');
        btn.textContent = btn.dataset.originalText; // restore text on error
        btn.disabled = false;
    }
}

function initializeQuickForm(container, type) {
    const form = container.querySelector('form');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        if (type === 'CreateBooking') {
            await handleBookingFormSubmit(form, () => {
                container.innerHTML = '';
                container.classList.add('hidden');
                container.previousElementSibling.disabled = false;
            });
        } else {
            await handleRegularFormSubmit(form, () => {
                container.innerHTML = '';
                container.classList.add('hidden');
                container.previousElementSibling.disabled = false;
            });
        }
    });
}

// ------------------------------
// Form submission handlers
// ------------------------------
async function handleBookingFormSubmit(form, onSuccess) {
    // Clear any previous error displays
    const errorContainer = form.querySelector('#form-errors');
    if (errorContainer) {
        errorContainer.classList.add('hidden');
        errorContainer.querySelector('ul').innerHTML = '';
    }

    // Validate required fields before submission
    const valOrEmpty = (sel) => form.querySelector(sel)?.value || "";

    const startTime = valOrEmpty('#hiddenBookingDateTime');
    const tableId = valOrEmpty('#tableSelect');
    const numberOfGuests = valOrEmpty('#NumberOfGuests');
    const name = valOrEmpty('#Name').trim();
    const phoneNumber = valOrEmpty('#PhoneNumber').trim();

    const errors = [];
    if (!name) errors.push('Customer name is required');
    if (!phoneNumber) errors.push('Phone number is required');
    if (!tableId) errors.push('Please select a table');
    if (!numberOfGuests || numberOfGuests < 1) errors.push('Number of guests must be at least 1');
    if (!startTime) errors.push('Please select a time slot');

    if (errors.length > 0) {
        displayFormErrors(form, errors);
        return;
    }

    // Create FormData and ensure all fields are included
    const formData = new FormData();
    formData.append('Name', name);
    formData.append('PhoneNumber', phoneNumber);
    formData.append('TableId', tableId);
    formData.append('NumberOfGuests', numberOfGuests);
    formData.append('StartTime', startTime);

    // Add anti-forgery token
    const token = form.querySelector('input[name="__RequestVerificationToken"]');
    if (token) {
        formData.append('__RequestVerificationToken', token.value);
    }

    try {
        const response = await fetch('/AdminPanel/CreateBooking', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            // Check if response is JSON (AJAX) or HTML (redirect)
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                const result = await response.json();
                showNotification(result.message || 'Booking created successfully!', 'success');
            } else {
                showNotification('Booking created successfully!', 'success');
            }

            if (onSuccess) onSuccess();

            // Reload any open panels that might need refreshing
            const managementPanel = document.getElementById('management-panel');
            if (managementPanel.innerHTML.trim() !== '') {
                // Refresh the current panel if one is loaded
                const panelContent = managementPanel.querySelector('[data-panel-type]');
                if (panelContent) {
                    const panelType = panelContent.dataset.panelType;
                    loadPanel(panelType);
                }
            }
        } else {
            // Handle error response
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                const errorData = await response.json();
                displayFormErrors(form, errorData.errors || ['Failed to create booking']);
            } else {
                const errorText = await response.text();
                console.error('Booking creation failed:', errorText);
                displayFormErrors(form, ['Failed to create booking. Please try again.']);
            }
        }
    } catch (err) {
        console.error('Form submission error:', err);
        displayFormErrors(form, ['Network error. Please check your connection and try again.']);
    }
}

async function handleRegularFormSubmit(form, onSuccess) {
    const formData = new FormData(form);
    const actionUrl = form.getAttribute('action');

    try {
        const response = await fetch(actionUrl, {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            showNotification('Operation completed successfully!', 'success');
            if (onSuccess) onSuccess();
        } else {
            const errorText = await response.text();
            console.error('Form submission failed:', errorText);
            showNotification('Operation failed', 'error');
        }
    } catch (err) {
        console.error('Form submission error:', err);
        showNotification('Failed to submit form', 'error');
    }
}

// Legacy handler for backward compatibility
async function handleFormSubmit(form, onSuccess) {
    return await handleRegularFormSubmit(form, onSuccess);
}

function displayFormErrors(form, errors) {
    const errorContainer = form.querySelector('#form-errors');
    if (errorContainer && errors.length > 0) {
        const errorList = errorContainer.querySelector('ul');
        errorList.innerHTML = errors.map(error => `<li>${error}</li>`).join('');
        errorContainer.classList.remove('hidden');

        // Scroll error container into view
        errorContainer.scrollIntoView({
            behavior: 'smooth',
            block: 'center'
        });
    }
}

// ------------------------------
// Enhanced Booking form (table + date + slots) - 24H FORMAT with table capacities
// ------------------------------
function initializeBookingForm(container) {
    const dateInput = container.querySelector('#bookingDate');
    const tableSelect = container.querySelector('#tableSelect');
    const guestsInput = container.querySelector('#NumberOfGuests');

    if (!dateInput || !tableSelect) {
        console.error('Required booking form elements not found');
        return;
    }

    // Get table capacities from the data attribute
    const createBookingPanel = container.querySelector('#createBookingPanel');
    let tableCapacities = {};

    if (createBookingPanel) {
        const tableCapacitiesData = createBookingPanel.dataset.tableCapacities;
        console.log('Raw table capacities data:', tableCapacitiesData);

        try {
            tableCapacities = JSON.parse(tableCapacitiesData || '{}');
            console.log('Parsed table capacities:', tableCapacities);
        } catch (e) {
            console.error('Error parsing table capacities:', e);
            tableCapacities = {};
        }
    } else {
        console.warn('createBookingPanel element not found for capacities');
    }

    // Set date constraints
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    // Set minimum date to today, default to tomorrow for better UX
    dateInput.min = today.toISOString().split('T')[0];
    dateInput.value = tomorrow.toISOString().split('T')[0];

    // Set maximum date (optional - 90 days from now)
    const maxDate = new Date(today);
    maxDate.setDate(maxDate.getDate() + 90);
    dateInput.max = maxDate.toISOString().split('T')[0];

    // Table capacity handling
    if (tableSelect && guestsInput) {
        tableSelect.addEventListener('change', function () {
            const selectedTableId = this.value;
            const capacity = tableCapacities[selectedTableId];

            if (capacity) {
                guestsInput.max = capacity;
                // If current guests exceed capacity, reset to capacity
                if (parseInt(guestsInput.value) > capacity) {
                    guestsInput.value = capacity;
                }

                // Update placeholder to show capacity
                guestsInput.placeholder = `Max ${capacity} guests`;
                console.log(`Table ${selectedTableId} selected, capacity: ${capacity}`);
            } else {
                guestsInput.max = 10; // Default fallback
                guestsInput.placeholder = "Number of guests";
                console.log(`No capacity data found for table ${selectedTableId}`);
            }
        });

        // Validate guests input
        guestsInput.addEventListener('input', function () {
            const selectedTableId = tableSelect.value;
            const capacity = tableCapacities[selectedTableId];
            const currentGuests = parseInt(this.value);

            if (capacity && currentGuests > capacity) {
                this.value = capacity;
                showNotification(`Maximum ${capacity} guests allowed for this table`, 'error');
            }
        });
    }

    // Event listeners with debouncing for better performance
    let debounceTimeout;
    const debouncedLoadSlots = () => {
        clearTimeout(debounceTimeout);
        debounceTimeout = setTimeout(loadAvailableSlots, 300);
    };

    dateInput.addEventListener('change', debouncedLoadSlots);
    tableSelect.addEventListener('change', debouncedLoadSlots);

    // Load initial slots if table is pre-selected
    if (tableSelect.value && dateInput.value) {
        setTimeout(loadAvailableSlots, 100);
    }
}

async function loadAvailableSlots() {
    const tableSelect = document.querySelector('#tableSelect');
    const dateInput = document.querySelector('#bookingDate');
    const slotContainer = document.querySelector('#timeSlotContainer');
    const hiddenInput = document.querySelector('#hiddenBookingDateTime');

    if (!tableSelect || !dateInput || !slotContainer || !hiddenInput) {
        console.error('Required form elements not found');
        return;
    }

    const tableId = tableSelect.value;
    const date = dateInput.value;

    if (!tableId || !date) {
        slotContainer.innerHTML = '<p class="text-gray-500">Please select a table and date first.</p>';
        hiddenInput.value = '';
        return;
    }

    slotContainer.innerHTML = '<p class="text-gray-500">Loading available slots...</p>';

    try {
        const response = await fetch(`/AdminPanel/GetAvailableSlots?tableId=${tableId}&date=${date}`);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const slots = await response.json();

        if (!slots || slots.length === 0) {
            slotContainer.innerHTML = '<p class="text-gray-500">No available slots for the selected date and table.</p>';
            hiddenInput.value = '';
            return;
        }

        // Create time slot HTML with 24-hour format
        const slotHTML = slots.map(slot => {
            const isAvailable = slot.available;
            const timeDisplay = slot.time; // Use 24-hour format directly

            return `
    <div class="time-slot px-3 py-2 rounded-lg text-center cursor-pointer border transition-all duration-200 ${isAvailable
                    ? 'bg-green-100 hover:bg-green-200 border-green-300 hover:shadow-md'
                    : 'bg-gray-200 border-gray-300 text-gray-500 line-through cursor-not-allowed opacity-60'
                }"
        data-time="${slot.time}"
        data-available="${isAvailable}"
        ${!isAvailable ? 'title="This time slot is already booked"' : ''}>
        <span class="font-medium">${timeDisplay}</span>
        ${!isAvailable ? '<br><small class="text-xs">Booked</small>' : ''}
    </div>
    `;
        }).join('');

        slotContainer.innerHTML = `
    <label class="block text-gray-700 font-medium mb-2">
        Select Time Slot *
        <small class="text-gray-500 font-normal">(Available slots in green)</small>
    </label>
    <div class="grid grid-cols-2 md:grid-cols-4 gap-2 mb-2">
        ${slotHTML}
    </div>
    <p class="text-sm text-gray-600 mt-2">
        <i class="fas fa-info-circle mr-1"></i>
        Click on a green time slot to select it
    </p>
    `;

        // Add click event listeners to available slots
        slotContainer.querySelectorAll('.time-slot[data-available="true"]').forEach(slot => {
            slot.addEventListener('click', function () {
                // Remove selection from all slots
                slotContainer.querySelectorAll('.time-slot').forEach(s => {
                    s.classList.remove('bg-blue-500', 'text-white', 'border-blue-600', 'shadow-lg');
                    if (s.dataset.available === 'true') {
                        s.classList.add('bg-green-100', 'border-green-300');
                    }
                });

                // Apply selection to clicked slot
                this.classList.remove('bg-green-100', 'border-green-300');
                this.classList.add('bg-blue-500', 'text-white', 'border-blue-600', 'shadow-lg');

                // Update hidden input with full datetime
                const selectedDateTime = `${date}T${this.dataset.time}:00`;
                hiddenInput.value = selectedDateTime;
            });
        });

        // Auto-select first available slot if none selected
        const firstAvailable = slotContainer.querySelector('.time-slot[data-available="true"]');
        if (firstAvailable && !hiddenInput.value) {
            firstAvailable.click();
        }

    } catch (error) {
        console.error('Error loading time slots:', error);
        slotContainer.innerHTML = `
    <div class="bg-red-50 border border-red-200 rounded-lg p-4">
        <p class="text-red-600">
            <i class="fas fa-exclamation-triangle mr-2"></i>
            Failed to load available time slots. Please try again.
        </p>
        <button onclick="loadAvailableSlots()" class="text-sm text-red-700 underline mt-2">
            Retry
        </button>
    </div>
    `;
        hiddenInput.value = '';
    }
}

// Close quick form function
function closeQuickForm(button) {
    const container = button.closest('.quick-form-container');
    const triggerButton = container.previousElementSibling;
    container.classList.add('hidden');
    container.innerHTML = '';
    triggerButton.disabled = false;
}

function editBooking(id) {
    const li = document.getElementById(`booking-${id}`);
    const booking = {
        CustomerName: li.dataset.customer,
        CustomerId: li.dataset.customerid,
        NumberOfGuests: li.dataset.guests,
        StartTime: li.dataset.start,
        TableId: li.dataset.table
    };

    li.innerHTML = `
    <div class="bg-blue-50 border-2 border-blue-200 rounded-xl p-6">
        <div class="flex items-center justify-between mb-4">
            <h4 class="text-lg font-bold text-blue-900 flex items-center">
                <i class="fas fa-edit mr-2"></i>Edit Booking
            </h4>
            <span class="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm font-medium">
                ID: ${id}
            </span>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
                <label class="block text-sm font-semibold text-gray-700 mb-2">
                    Customer Name
                </label>
                <input type="text" id="customer-${id}" value="${booking.CustomerName}" disabled
                    class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg bg-gray-100 cursor-not-allowed" />
            </div>

            <div>
                <label class="block text-sm font-semibold text-gray-700 mb-2">
                    Number of Guests *
                </label>
                <input type="number" id="guests-${id}" value="${booking.NumberOfGuests}" min="1"
                    class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all duration-200 font-bold text-lg" />
            </div>

            <div>
                <label class="block text-sm font-semibold text-gray-700 mb-2">
                    Booking Date *
                </label>
                <input type="date" id="bookingDate-${id}" 
                       value="${booking.StartTime.split('T')[0]}"
                       min="${new Date().toISOString().split('T')[0]}"
                       max="${new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]}"
                       class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all duration-200 font-bold text-lg" />
            </div>

            <div>
                <label class="block text-sm font-semibold text-gray-700 mb-2">
                    Table ID
                </label>
                <input type="number" id="table-${id}" value="${booking.TableId}" disabled
                    class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg bg-gray-100 cursor-not-allowed" />
            </div>
        </div>

        <div class="mb-4 mt-4" id="timeSlotContainer-${id}">
            <p class="text-gray-500 text-center py-4 bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
                <i class="fas fa-clock mr-2"></i>
                Please select a date to view available time slots.
            </p>
        </div>

        <input type="hidden" id="hiddenBookingDateTime-${id}" value="${booking.StartTime}" />

        <div class="flex justify-end space-x-3 mt-6 pt-4 border-t border-blue-200">
            <button onclick="cancelEditBooking(${id}, '${booking.NumberOfGuests}', '${booking.StartTime}')"
                class="bg-gray-500 hover:bg-gray-600 text-white px-6 py-3 rounded-lg font-medium flex items-center shadow-md">
                <i class="fas fa-times mr-2"></i>Cancel
            </button>
            <button onclick="saveBooking(${id})"
                class="bg-green-500 hover:bg-green-600 text-white px-6 py-3 rounded-lg font-medium flex items-center shadow-md">
                <i class="fas fa-save mr-2"></i>Save Changes
            </button>
        </div>
    </div>
    `;

    // Attach date change listener to load available slots
    const dateInput = document.getElementById(`bookingDate-${id}`);
    dateInput.addEventListener('change', () => loadAvailableSlotsForEdit(id));
    // Load slots initially
    loadAvailableSlotsForEdit(id);
}

async function loadAvailableSlotsForEdit(id) {
    const tableId = document.getElementById(`table-${id}`).value;
    const date = document.getElementById(`bookingDate-${id}`).value;
    const slotContainer = document.getElementById(`timeSlotContainer-${id}`);
    const hiddenInput = document.getElementById(`hiddenBookingDateTime-${id}`);

    if (!tableId || !date) {
        slotContainer.innerHTML = '<p class="text-gray-500">Please select a table and date first.</p>';
        hiddenInput.value = '';
        return;
    }

    slotContainer.innerHTML = '<p class="text-gray-500">Loading available slots...</p>';

    try {
        const res = await fetch(`/AdminPanel/GetAvailableSlots?tableId=${tableId}&date=${date}`);
        const slots = await res.json();

        if (!slots || slots.length === 0) {
            slotContainer.innerHTML = '<p class="text-gray-500">No available slots for this date.</p>';
            hiddenInput.value = '';
            return;
        }

        const slotHTML = slots.map(slot => {
            return `
            <div class="time-slot px-3 py-2 rounded-lg text-center cursor-pointer border transition-all duration-200 ${slot.available
                    ? 'bg-green-100 hover:bg-green-200 border-green-300 hover:shadow-md'
                    : 'bg-gray-200 border-gray-300 text-gray-500 line-through cursor-not-allowed opacity-60'}"
                data-time="${slot.time}" data-available="${slot.available}" ${!slot.available ? 'title="Booked"' : ''}>
                <span class="font-medium">${slot.time}</span>
                ${!slot.available ? '<br><small class="text-xs">Booked</small>' : ''}
            </div>`;
        }).join('');

        slotContainer.innerHTML = `
            <label class="block text-gray-700 font-medium mb-2">
                Select Time Slot *
            </label>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-2 mb-2">
                ${slotHTML}
            </div>`;

        // Attach click events
        slotContainer.querySelectorAll('.time-slot[data-available="true"]').forEach(slot => {
            slot.addEventListener('click', function () {
                slotContainer.querySelectorAll('.time-slot').forEach(s => {
                    s.classList.remove('bg-blue-500', 'text-white', 'border-blue-600', 'shadow-lg');
                    if (s.dataset.available === 'true') s.classList.add('bg-green-100', 'border-green-300');
                });
                this.classList.remove('bg-green-100', 'border-green-300');
                this.classList.add('bg-blue-500', 'text-white', 'border-blue-600', 'shadow-lg');
                hiddenInput.value = `${date}T${this.dataset.time}:00`;
            });
        });

        // Auto-select first available slot if current time matches
        const currentSlot = slots.find(s => `${date}T${s.time}:00` === hiddenInput.value);
        if (currentSlot) {
            const el = slotContainer.querySelector(`.time-slot[data-time="${currentSlot.time}"]`);
            if (el) el.click();
        } else {
            const first = slotContainer.querySelector('.time-slot[data-available="true"]');
            if (first) first.click();
        }

    } catch (err) {
        console.error(err);
        slotContainer.innerHTML = '<p class="text-red-500">Failed to load time slots.</p>';
        hiddenInput.value = '';
    }
}


async function saveBooking(id) {
    const li = document.getElementById(`booking-${id}`);
    const guests = parseInt(document.getElementById(`guests-${id}`).value);
    const startTime = document.getElementById(`hiddenBookingDateTime-${id}`).value;
    const tableId = li.dataset.table;
    const customerId = li.dataset.customerid;

    if (!startTime) {
        alert('Please select a date and time slot before saving.');
        return;
    }

    const res = await fetch(`/api/bookings/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            NumberOfGuests: guests,
            StartTime: startTime,
            TableId: tableId,
            CustomerId: customerId
        })
    });

    if (res.ok) {
        // Update the li content directly
        li.dataset.guests = guests;
        li.dataset.start = startTime;

        li.innerHTML = `
        <div class="booking-display flex items-center justify-between">
            <div class="flex items-center space-x-4">
                <div class="bg-blue-100 p-3 rounded-lg">
                    <i class="fas fa-user text-blue-600 text-xl"></i>
                </div>
                <div>
                    <h4 class="font-bold text-lg text-gray-900">${li.dataset.customer}</h4>
                    <div class="flex flex-wrap text-gray-600 mt-1 space-x-4">
                        <span>
                            <i class="fas fa-calendar-alt mr-1"></i>
                            ${new Date(startTime).toLocaleString('en-GB', {
                                day: '2-digit',
                                month: 'short',
                                year: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit',
                                hour12: false
                            })}
                        </span>
                        <span><i class="fas fa-chair mr-1"></i>Table: ${tableId}</span>
                        <span><i class="fas fa-users mr-1"></i>Guests: ${guests}</span>
                    </div>
                </div>
            </div>
            <div class="flex space-x-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                <button onclick="editBooking(${id})"
                    class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg font-medium flex items-center">
                    <i class="fas fa-edit mr-2"></i>Edit
                </button>
                <button onclick="deleteBooking(${id})"
                    class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg font-medium flex items-center">
                    <i class="fas fa-trash mr-2"></i>Delete
                </button>
            </div>
        </div>`;
    } else {
        const data = await res.json();
        alert('Failed to update booking: ' + (data.message || 'Unknown error'));
    }
}

function cancelEditBooking(id, originalGuests, originalStart) {
    const li = document.getElementById(`booking-${id}`);

    li.innerHTML = `
    <div class="booking-display flex items-center justify-between">
        <div class="flex items-center space-x-4">
            <div class="bg-blue-100 p-3 rounded-lg">
                <i class="fas fa-user text-blue-600 text-xl"></i>
            </div>
            <div>
                <h4 class="font-bold text-lg text-gray-900">${li.dataset.customer}</h4>
                <div class="flex flex-wrap text-gray-600 mt-1 space-x-4">
                    <i class="fas fa-calendar-alt mr-1"></i>
                        ${new Date(originalStart).toLocaleString('en-GB', {
                            day: '2-digit',
                            month: 'short',
                            year: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit',
                            hour12: false
                        })}
                    </span>
                    <span><i class="fas fa-chair mr-1"></i>Table: ${li.dataset.table}</span>
                    <span><i class="fas fa-users mr-1"></i>Guests: ${originalGuests}</span>
                </div>
            </div>
        </div>
        <div class="flex space-x-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
            <button onclick="editBooking(${id})"
                class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg font-medium flex items-center">
                <i class="fas fa-edit mr-2"></i>Edit
            </button>
            <button onclick="deleteBooking(${id})"
                class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg font-medium flex items-center">
                <i class="fas fa-trash mr-2"></i>Delete
            </button>
        </div>
    </div>
    `;
}


async function deleteBooking(id) {
    if (!confirm("Are you sure you want to delete this booking?")) return;

    const res = await fetch(`/api/bookings/${id}`, {
        method: 'DELETE'
    });
    if (res.ok) {
        document.getElementById(`booking-${id}`).remove();
    } else {
        const data = await res.json();
        alert('Failed to delete booking: ' + (data.message || 'Unknown error'));
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('form[action="/AdminPanel/CreateMenuItem"]');
    if (!form) return;

    const errorsDiv = form.querySelector('#form-errors');
    const successDiv = form.querySelector('#form-success');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        errorsDiv.classList.add('hidden');
        successDiv.classList.add('hidden');

        const formData = new FormData(form);

        const res = await fetch(form.action, {
            method: 'POST',
            body: formData
        });

        if (res.ok) {
            successDiv.querySelector('p').textContent = 'Menu item added successfully!';
            successDiv.classList.remove('hidden');
            form.reset();
        } else {
            const data = await res.json();
            const ul = errorsDiv.querySelector('ul');
            ul.innerHTML = '';
            data.errors?.forEach(err => {
                const li = document.createElement('li');
                li.textContent = err;
                ul.appendChild(li);
            });
            errorsDiv.classList.remove('hidden');
        }
    });
});



// ------------------------------
// Enhanced Menu Item Functions
// ------------------------------
function editMenuItem(id) {
    const div = document.getElementById(`menuitem-${id}`);

    const menuItem = {
        name: div.dataset.name,
        description: div.dataset.description,
        price: div.dataset.price,
        image: div.dataset.image,
        isPopular: div.dataset.popular === "True"
    };

    div.innerHTML = `
        <div class="bg-blue-50 border-2 border-blue-200 rounded-xl p-6">
            <div class="flex items-center justify-between mb-4">
                <h4 class="text-lg font-bold text-blue-900 flex items-center">
                    <i class="fas fa-edit mr-2"></i>Edit Menu Item
                </h4>
                <span class="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm font-medium">
                    ID: ${id}
                </span>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="space-y-4">
                    <div>
                        <label class="block text-sm font-semibold text-gray-700 mb-2">
                            <i class="fas fa-signature mr-2 text-blue-600"></i>Item Name *
                        </label>
                        <input type="text" id="name-${id}" value="${menuItem.name}" placeholder="Enter item name"
                               class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all duration-200 font-medium" />
                    </div>

                    <div>
                        <label class="block text-sm font-semibold text-gray-700 mb-2">
                            <i class="fas fa-align-left mr-2 text-blue-600"></i>Description
                        </label>
                        <textarea id="description-${id}" placeholder="Enter item description" rows="3"
                                  class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all duration-200 resize-none">${menuItem.description || ''}</textarea>
                    </div>

                    <!-- Popular Toggle -->
                    <div class="flex items-center space-x-3">
                        <label class="flex items-center cursor-pointer">
                            <span class="mr-2 text-gray-700 font-semibold">Popular</span>
                            <input type="checkbox" id="popular-${id}" ${menuItem.isPopular ? "checked" : ""} 
                                   class="w-5 h-5 text-yellow-500 border-gray-300 rounded focus:ring-yellow-200 focus:ring-2" />
                        </label>
                    </div>
                </div>

                <div class="space-y-4">
                    <div>
                        <label class="block text-sm font-semibold text-gray-700 mb-2">
                            <i class="fas fa-dollar-sign mr-2 text-green-600"></i>Price *
                        </label>
                        <div class="relative">
                            <input type="number" id="price-${id}" value="${menuItem.price.replace(',', '.')}"
                                   step="1" min="0" placeholder="0"
                                   class="w-full px-4 py-3 pl-8 border-2 border-gray-300 rounded-lg focus:border-green-500 focus:ring-2 focus:ring-green-200 transition-all duration-200 font-bold text-green-700" />
                            <span class="absolute left-3 top-3.5 text-green-600 font-bold">kr</span>
                        </div>
                    </div>

                    <div>
                        <label class="block text-sm font-semibold text-gray-700 mb-2">
                            <i class="fas fa-image mr-2 text-purple-600"></i>Image URL
                        </label>
                        <input type="url" id="image-${id}" value="${menuItem.image || ''}" placeholder="https://example.com/image.jpg"
                               class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:border-purple-500 focus:ring-2 focus:ring-purple-200 transition-all duration-200" />
                        <p class="text-xs text-gray-500 mt-1">Optional: Add a URL to display an image</p>
                    </div>
                </div>
            </div>

            <div class="flex justify-end space-x-3 mt-6 pt-4 border-t border-blue-200">
                <button onclick="cancelEditMenuItem(${id}, '${menuItem.name}', '${menuItem.description}', ${menuItem.price}, '${menuItem.image || ''}', ${menuItem.isPopular})"
                        class="bg-gray-500 hover:bg-gray-600 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center shadow-md hover:shadow-lg">
                    <i class="fas fa-times mr-2"></i>Cancel
                </button>
                <button onclick="saveMenuItem(${id})"
                        class="bg-green-500 hover:bg-green-600 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center shadow-md hover:shadow-lg">
                    <i class="fas fa-save mr-2"></i>Save Changes
                </button>
            </div>
        </div>
    `;
}

async function saveMenuItem(id) {
    const div = document.getElementById(`menuitem-${id}`);
    const name = document.getElementById(`name-${id}`).value.trim();
    const description = document.getElementById(`description-${id}`).value.trim();
    const price = parseFloat(document.getElementById(`price-${id}`).value);
    console.log(price);
    console.log(price.replace(",", "."))
    const image = document.getElementById(`image-${id}`).value.trim();
    const isPopular = document.getElementById(`popular-${id}`).checked;

    if (!name || price < 0) {
        showNotification('Name is required and price must be non-negative', 'error');
        return;
    }

    const saveButton = div.querySelector('button[onclick*="saveMenuItem"]');
    const originalHTML = saveButton.innerHTML;
    saveButton.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Saving...';
    saveButton.disabled = true;

    try {
        const response = await fetch(`/AdminPanel/UpdateMenuItem/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                Name: name,
                Description: description,
                Price: price,
                ImageUrl: image,
                IsPopular: isPopular
            })
        });

        if (response.ok) {
            div.dataset.name = name;
            div.dataset.description = description;
            div.dataset.price = price;
            div.dataset.image = image;
            div.dataset.popular = isPopular;

            // Re-render menu item display (optional: show a "Popular" badge)
            div.innerHTML = `
                <div class="menuitem-display flex items-start justify-between group">
                    <div class="flex space-x-4 flex-1">
                        ${image ?
                                `<div class="flex-shrink-0">
                            <img src="${image}" alt="${name}" class="w-20 h-20 object-cover rounded-lg border-2 border-gray-200" />
                        </div>` :
                                `<div class="flex-shrink-0">
                            <div class="w-20 h-20 bg-green-100 rounded-lg flex items-center justify-center border-2 border-gray-200">
                                <i class="fas fa-image text-green-600 text-2xl"></i>
                            </div>
                        </div>`
                            }
                        <div class="flex-1 min-w-0">
                            <h4 class="font-bold text-lg text-gray-900 mb-1 flex items-center space-x-2">
                                <span>${name}</span>
                                ${isPopular ? `<span class="bg-yellow-100 text-yellow-800 px-2 py-0.5 rounded-full text-xs font-bold">Popular</span>` : ''}
                            </h4>
                            <p class="text-gray-600 text-sm mb-2 line-clamp-2">${description}</p>
                            <div class="flex items-center space-x-4">
                                <span class="bg-green-100 text-green-800 px-3 py-1 rounded-full font-bold text-sm">
                                    ${price.toFixed(2)} kr
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="flex space-x-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200 ml-4">
                        <button onclick="editMenuItem(${id})"
                                class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center">
                            <i class="fas fa-edit mr-2"></i>Edit
                        </button>
                        <button onclick="deleteMenuItem(${id})"
                                class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center">
                            <i class="fas fa-trash mr-2"></i>Delete
                        </button>
                    </div>
                </div>
            `;
            showNotification('Menu item updated successfully!', 'success');
        } else {
            const data = await response.json();
            showNotification('Failed to update menu item: ' + (data.message || 'Unknown error'), 'error');
            saveButton.innerHTML = originalHTML;
            saveButton.disabled = false;
        }
    } catch (error) {
        showNotification('Network error occurred', 'error');
        console.error('Error updating menu item:', error);
        saveButton.innerHTML = originalHTML;
        saveButton.disabled = false;
    }
}

function cancelEditMenuItem(id, originalName, originalDescription, originalPrice, originalImage, originalPopularity) {
    const div = document.getElementById(`menuitem-${id}`);
    div.innerHTML = `
        <div class="menuitem-display flex items-start justify-between group">
            <div class="flex space-x-4 flex-1">
                ${originalImage ?
            `<div class="flex-shrink-0">
                    <img src="${originalImage}" alt="${originalName}" class="w-20 h-20 object-cover rounded-lg border-2 border-gray-200" />
                </div>` :
            `<div class="flex-shrink-0">
                    <div class="w-20 h-20 bg-green-100 rounded-lg flex items-center justify-center border-2 border-gray-200">
                        <i class="fas fa-image text-green-600 text-2xl"></i>
                    </div>
                </div>`
        }
                <div class="flex-1 min-w-0">
                    <h4 class="font-bold text-lg text-gray-900 mb-1 flex items-center space-x-2">
                        <span>${originalName}</span>
                        ${originalPopularity ? `<span class="bg-yellow-100 text-yellow-800 px-2 py-0.5 rounded-full text-xs font-semibold">Popular!</span>` : ''}
                    </h4>
                    <p class="text-gray-600 text-sm mb-2 line-clamp-2">${originalDescription}</p>
                    <div class="flex items-center space-x-4">
                        <span class="bg-green-100 text-green-800 px-3 py-1 rounded-full font-bold text-sm">
                            ${parseFloat(originalPrice).toFixed(2)} kr
                        </span>
                    </div>
                </div>
            </div>
            <div class="flex space-x-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200 ml-4">
                <button onclick="editMenuItem(${id})"
                        class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center">
                    <i class="fas fa-edit mr-2"></i>Edit
                </button>
                <button onclick="deleteMenuItem(${id})"
                        class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center">
                    <i class="fas fa-trash mr-2"></i>Delete
                </button>
            </div>
        </div>
    `;
}

async function deleteMenuItem(id) {
    if (!confirm("Are you sure you want to delete this menu item?")) return;

    try {
        const response = await fetch(`/AdminPanel/DeleteMenuItem/${id}`, {
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        });

        if (response.ok) {
            const div = document.getElementById(`menuitem-${id}`);
            div.style.transform = 'scale(0.95)';
            div.style.opacity = '0';
            div.style.transition = 'all 0.3s ease';

            setTimeout(() => {
                div.remove();
                showNotification('Menu item deleted successfully!', 'success');
            }, 300);
        } else {
            const data = await response.json();
            showNotification('Failed to delete menu item: ' + (data.message || 'Unknown error'), 'error');
        }
    } catch (error) {
        showNotification('Network error occurred', 'error');
        console.error('Error deleting menu item:', error);
    }
}

// ------------------------------
// Enhanced Table Functions
// ------------------------------
function editTable(id) {
    const div = document.getElementById(`table-${id}`);
    const table = {
        tableNumber: div.dataset.tablenumber,
        capacity: div.dataset.capacity
    };

    div.innerHTML = `
                <div class="bg-yellow-50 border-2 border-yellow-200 rounded-xl p-6">
                    <div class="flex items-center justify-between mb-4">
                        <h4 class="text-lg font-bold text-yellow-900 flex items-center">
                            <i class="fas fa-edit mr-2"></i>Edit Table
                        </h4>
                        <span class="bg-yellow-100 text-yellow-800 px-3 py-1 rounded-full text-sm font-medium">
                            ID: ${id}
                        </span>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-2">
                                <i class="fas fa-hashtag mr-2 text-yellow-600"></i>Table Number *
                            </label>
                            <input type="number" id="tablenumber-${id}" value="${table.tableNumber}" min="1" placeholder="1"
                                   class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:border-yellow-500 focus:ring-2 focus:ring-yellow-200 transition-all duration-200 font-bold text-lg" />
                            <p class="text-xs text-gray-500 mt-1">Unique table identifier</p>
                        </div>

                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-2">
                                <i class="fas fa-users mr-2 text-yellow-600"></i>Seating Capacity *
                            </label>
                            <input type="number" id="capacity-${id}" value="${table.capacity}" min="1" max="20" placeholder="4"
                                   class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:border-yellow-500 focus:ring-2 focus:ring-yellow-200 transition-all duration-200 font-bold text-lg" />
                            <p class="text-xs text-gray-500 mt-1">Maximum number of guests (1-20)</p>
                        </div>
                    </div>

                    <div class="flex justify-end space-x-3 mt-6 pt-4 border-t border-yellow-200">
                        <button onclick="cancelEditTable(${id}, ${table.tableNumber}, ${table.capacity})"
                                class="bg-gray-500 hover:bg-gray-600 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center shadow-md hover:shadow-lg">
                            <i class="fas fa-times mr-2"></i>Cancel
                        </button>
                        <button onclick="saveTable(${id})"
                                class="bg-green-500 hover:bg-green-600 text-white px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center shadow-md hover:shadow-lg">
                            <i class="fas fa-save mr-2"></i>Save Changes
                        </button>
                    </div>
                </div>
            `;
}

async function saveTable(id) {
    const div = document.getElementById(`table-${id}`);
    const tableNumber = parseInt(document.getElementById(`tablenumber-${id}`).value);
    const capacity = parseInt(document.getElementById(`capacity-${id}`).value);

    if (!tableNumber || tableNumber < 1 || !capacity || capacity < 1 || capacity > 20) {
        showNotification('Table number and capacity must be positive integers (capacity max 20)', 'error');
        return;
    }

    // Add loading state
    const saveButton = div.querySelector('button[onclick*="saveTable"]');
    const originalHTML = saveButton.innerHTML;
    saveButton.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Saving...';
    saveButton.disabled = true;

    try {
        const response = await fetch(`/AdminPanel/UpdateTable/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                TableNumber: tableNumber,
                Capacity: capacity
            })
        });

        if (response.ok) {
            // Update the div data attributes and content
            div.dataset.tablenumber = tableNumber;
            div.dataset.capacity = capacity;

            div.innerHTML = `
                        <div class="table-display flex items-center justify-between">
                            <div class="flex items-center space-x-4">
                                <div class="bg-yellow-100 p-3 rounded-lg">
                                    <i class="fas fa-chair text-yellow-600 text-xl"></i>
                                </div>
                                <div>
                                    <h4 class="font-bold text-lg text-gray-900">Table ${tableNumber}</h4>
                                    <div class="flex items-center text-gray-600 mt-1">
                                        <i class="fas fa-users mr-2 text-sm"></i>
                                        <span>Capacity: ${capacity} seats</span>
                                    </div>
                                </div>
                            </div>
                            <div class="flex space-x-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                                <button onclick="editTable(${id})"
                                        class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center">
                                    <i class="fas fa-edit mr-2"></i>Edit
                                </button>
                                <button onclick="deleteTable(${id})"
                                        class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center">
                                    <i class="fas fa-trash mr-2"></i>Delete
                                </button>
                            </div>
                        </div>
                    `;
            showNotification('Table updated successfully!', 'success');
        } else {
            const data = await response.json();
            showNotification('Failed to update table: ' + (data.message || 'Unknown error'), 'error');
            saveButton.innerHTML = originalHTML;
            saveButton.disabled = false;
        }
    } catch (error) {
        showNotification('Network error occurred', 'error');
        console.error('Error updating table:', error);
        saveButton.innerHTML = originalHTML;
        saveButton.disabled = false;
    }
}

function cancelEditTable(id, originalTableNumber, originalCapacity) {
    const div = document.getElementById(`table-${id}`);
    div.innerHTML = `
                <div class="table-display flex items-center justify-between">
                    <div class="flex items-center space-x-4">
                        <div class="bg-yellow-100 p-3 rounded-lg">
                            <i class="fas fa-chair text-yellow-600 text-xl"></i>
                        </div>
                        <div>
                            <h4 class="font-bold text-lg text-gray-900">Table ${originalTableNumber}</h4>
                            <div class="flex items-center text-gray-600 mt-1">
                                <i class="fas fa-users mr-2 text-sm"></i>
                                <span>Capacity: ${originalCapacity} seats</span>
                            </div>
                        </div>
                    </div>
                    <div class="flex space-x-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                        <button onclick="editTable(${id})"
                                class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center">
                            <i class="fas fa-edit mr-2"></i>Edit
                        </button>
                        <button onclick="deleteTable(${id})"
                                class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center">
                            <i class="fas fa-trash mr-2"></i>Delete
                        </button>
                    </div>
                </div>
            `;
}

async function deleteTable(id) {
    if (!confirm("Are you sure you want to delete this table? This may affect existing bookings.")) return;

    try {
        const response = await fetch(`/AdminPanel/DeleteTable/${id}`, {
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        });

        if (response.ok) {
            const div = document.getElementById(`table-${id}`);
            div.style.transform = 'scale(0.95)';
            div.style.opacity = '0';
            div.style.transition = 'all 0.3s ease';

            setTimeout(() => {
                div.remove();
                showNotification('Table deleted successfully!', 'success');
            }, 300);
        } else {
            const data = await response.json();
            showNotification('Failed to delete table: ' + (data.message || 'Unknown error'), 'error');
        }
    } catch (error) {
        showNotification('Network error occurred', 'error');
        console.error('Error deleting table:', error);
    }
}