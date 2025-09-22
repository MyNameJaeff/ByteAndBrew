import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "../app/store";
import { resetBooking } from "../features/bookingSlice";
import { useState } from "react";

// Define the DTO type for booking and customer creation
type BookingAndCustomerCreateDto = {
  StartTime: string;
  NumberOfGuests: number;
  TableId: number;
  Name: string;
  PhoneNumber: string;
};

export default function Step5Confirm() {
  const booking = useSelector((state: RootState) => state.booking);
  const dispatch = useDispatch();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isConfirmed, setIsConfirmed] = useState(false);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const months = ['January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'];

    return `${days[date.getDay()]}, ${months[date.getMonth()]} ${date.getDate()}, ${date.getFullYear()}`;
  };

  const generateBookingId = () => {
    return 'BK' + Date.now().toString().slice(-6) + Math.floor(Math.random() * 100).toString().padStart(2, '0');
  };

  const handleConfirm = async () => {
    if (!booking.date || !booking.time || !booking.table || !booking.name || !booking.phone) {
      alert("Please complete all booking details before confirming.");
      return;
    }

    setIsSubmitting(true);

    // Convert date + time to a single ISO string
    const localDate = new Date(`${booking.date}T${booking.time}`);
    const startTime = new Date(localDate.getTime() - localDate.getTimezoneOffset() * 60000).toISOString();

    console.log("Submitting booking:", {
      StartTime: startTime,
      NumberOfGuests: booking.guests,
      TableId: booking.table,
      Name: booking.name,
      PhoneNumber: booking.phone,
    });

    const dto: BookingAndCustomerCreateDto = {
      StartTime: startTime,
      NumberOfGuests: booking.guests,
      TableId: booking.table,
      Name: booking.name,
      PhoneNumber: booking.phone,
    };

    try {
      const response = await fetch("https://localhost:7145/api/Bookings/customerAndBooking", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(dto),
      });

      const data = await response.json();

      if (!response.ok) {
        console.error("Booking API error:", data);
        alert(data.errors?.join("\n") || "Failed to create booking");
      } else {
        console.log("Booking confirmed:", data);
        setIsConfirmed(true);

        // Auto-reset after showing confirmation
        setTimeout(() => {
          dispatch(resetBooking());
        }, 15000);
      }
    } catch (err) {
      console.error("Booking API exception:", err);
      alert("Failed to connect to the server. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };


  if (isConfirmed) {
    return (
      <div className="text-center space-y-6">
        <div className="text-6xl">🎉</div>
        <h2 className="text-3xl font-bold text-green-600">Booking Confirmed!</h2>
        <div className="bg-green-50 border border-green-200 rounded-lg p-6 max-w-md mx-auto">
          <p className="text-green-800 font-medium mb-2">Confirmation Details</p>
          <p className="text-green-700 text-sm">
            Booking ID: <span className="font-mono font-bold">{generateBookingId()}</span>
          </p>
          <p className="text-green-700 text-sm mt-2">
            A confirmation text will be sent to {booking.phone}
          </p>
        </div>
        <p className="text-gray-600">
          Thank you {booking.name}! We look forward to seeing you.
        </p>
        <p className="text-sm text-gray-500">
          This page will reset in a few seconds...
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Confirm Your Reservation</h2>
        <p className="text-gray-600">Please review your booking details</p>
      </div>

      {/* Booking Summary Card */}
      <div className="bg-gradient-to-br from-blue-50 to-indigo-50 border border-blue-200 rounded-xl p-6 shadow-sm">
        <h3 className="text-lg font-semibold text-gray-800 mb-4 flex items-center space-x-2">
          <span>📋</span>
          <span>Booking Summary</span>
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-3">
            <div className="flex items-center space-x-3">
              <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white text-sm">
                📅
              </div>
                <div>
                <p className="text-sm font-medium text-gray-700">Date</p>
                <p className="text-gray-900">
                  {formatDate(booking.date)} <br />
                  {booking.time} &ndash; {(() => {
                  const date = new Date(`${booking.date}T${booking.time}`);
                  date.setHours(date.getHours() + 2);
                  return date.toTimeString().slice(0, 5);
                  })()}
                </p>
                </div>
            </div>

            <div className="flex items-center space-x-3">
              <div className="w-8 h-8 bg-green-500 rounded-full flex items-center justify-center text-white text-sm">
                👥
              </div>
              <div>
                <p className="text-sm font-medium text-gray-700">Party Size</p>
                <p className="text-gray-900">{booking.guests} {booking.guests === 1 ? 'guest' : 'guests'}</p>
              </div>
            </div>
          </div>

          <div className="space-y-3">
            <div className="flex items-center space-x-3">
              <div className="w-8 h-8 bg-purple-500 rounded-full flex items-center justify-center text-white text-sm">
                👤
              </div>
              <div>
                <p className="text-sm font-medium text-gray-700">Name</p>
                <p className="text-gray-900">{booking.name}</p>
              </div>
            </div>

            <div className="flex items-center space-x-3">
              <div className="w-8 h-8 bg-orange-500 rounded-full flex items-center justify-center text-white text-sm">
                📞
              </div>
              <div>
                <p className="text-sm font-medium text-gray-700">Phone</p>
                <p className="text-gray-900">{booking.phone}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Additional Information */}
      <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
        <h4 className="font-medium text-amber-800 mb-2 flex items-center space-x-2">
          <span>💡</span>
          <span>Important Information</span>
        </h4>
        <div className="space-y-1 text-sm text-amber-700">
          <p>• Please arrive 5-10 minutes before your desired time</p>
          <p>• Tables are held for 15 minutes past reservation time</p>
          <p>• Contact us at (555) 123-4567 if you need to make changes</p>
        </div>
      </div>

      {/* Confirmation Button */}
      <div className="text-center">
        <button
          className={`px-8 py-4 rounded-xl font-semibold text-lg transition-all duration-300 transform ${isSubmitting
            ? 'bg-gray-400 text-white cursor-not-allowed'
            : 'bg-gradient-to-r from-green-500 to-green-600 text-white hover:from-green-600 hover:to-green-700 hover:scale-105 shadow-lg hover:shadow-xl'
            }`}
          onClick={handleConfirm}
          disabled={isSubmitting}
        >
          {isSubmitting ? (
            <div className="flex items-center space-x-2">
              <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
              <span>Confirming Reservation...</span>
            </div>
          ) : (
            <div className="flex items-center space-x-2">
              <span>✓</span>
              <span>Confirm Booking</span>
            </div>
          )}
        </button>
      </div>

      {/* Terms and Conditions */}
      <div className="text-center text-sm text-gray-500">
        <p>
          By confirming this reservation, you agree to our{' '}
          <button className="text-blue-500 hover:underline">cancellation policy</button>
          {' '}and{' '}
          <button className="text-blue-500 hover:underline">terms of service</button>.
        </p>
      </div>
    </div>
  );
}