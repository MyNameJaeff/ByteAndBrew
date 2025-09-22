import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "../app/store";
import { resetBooking, updateBooking } from "../features/bookingSlice";
import { useState, useEffect } from "react";

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
  const [confirmationId, setConfirmationId] = useState<string | null>(null);
  const [countdown, setCountdown] = useState(15);
  const [autoReset, setAutoReset] = useState(true);

  // countdown effect
  useEffect(() => {
    if (!isConfirmed || !autoReset) return;

    if (countdown === 0) {
      dispatch(resetBooking());
      return;
    }

    const timer = setInterval(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);

    return () => clearInterval(timer);
  }, [isConfirmed, countdown, autoReset, dispatch]);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    return `${days[date.getDay()]}, ${months[date.getMonth()]} ${date.getDate()}, ${date.getFullYear()}`;
  };

  const generateBookingId = () => {
    return "BK" + Date.now().toString().slice(-6) + Math.floor(Math.random() * 100).toString().padStart(2, "0");
  };

  const handleConfirm = async () => {
    if (!booking.date || !booking.time || !booking.table || !booking.name || !booking.phone) {
      alert("Please complete all booking details before confirming.");
      return;
    }

    setIsSubmitting(true);

    const localDate = new Date(`${booking.date}T${booking.time}`);
    const startTime = new Date(localDate.getTime() - localDate.getTimezoneOffset() * 60000).toISOString();

    const dto: BookingAndCustomerCreateDto = {
      StartTime: startTime,
      NumberOfGuests: booking.guests,
      TableId: booking.table,
      Name: booking.name,
      PhoneNumber: booking.phone,
    };

    try {
      // force minimum wait (800ms)
      const [response] = await Promise.all([
        fetch("https://localhost:7145/api/Bookings/customerAndBooking", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(dto),
        }),
        new Promise((resolve) => setTimeout(resolve, 800)), // artificial delay
      ]);

      const data = await response.json();

      if (!response.ok) {
        console.error("Booking API error:", data);
        alert(data.errors?.join("\n") || "Failed to create booking");
      } else {
        setIsConfirmed(true);
        setCountdown(15);
        setAutoReset(true);
        setConfirmationId(generateBookingId());
        dispatch(updateBooking({ submitted: true }));
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
        <div className="text-6xl mb-4">🎉</div>
        <h2 className="text-2xl sm:text-3xl font-bold text-green-600">
          Booking Confirmed!
        </h2>

        <div className="bg-green-50 border border-green-200 rounded-lg p-6 max-w-md mx-auto">
          <p className="text-green-800 font-medium mb-2">Confirmation ID</p>
          <p className="text-green-900 font-mono font-bold text-xl">
            {confirmationId}
          </p>
          <p className="text-green-700 text-sm mt-3">
            A confirmation text will be sent to {booking.phone}
          </p>
        </div>

        <p className="text-gray-700">
          Thank you <span className="font-semibold">{booking.name}</span>! We look forward to seeing you.
        </p>

        {autoReset ? (
          <p className="text-sm text-gray-500">
            This page will reset in <span className="font-semibold">{countdown}</span> seconds...
          </p>
        ) : (
          <p className="text-sm text-gray-500">Auto reset stopped ✅</p>
        )}

        {autoReset && (
          <button
            onClick={() => setAutoReset(false)}
            className="cursor-pointer mt-3 px-6 py-2 rounded-lg bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium"
          >
            Stop Auto-Reload
          </button>
        )}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="text-center">
        <h2 className="text-xl sm:text-2xl font-bold text-gray-800 mb-2">
          Confirm Your Reservation
        </h2>
        <p className="text-gray-600">Please review your booking details</p>
      </div>

      {/* Booking Summary */}
      <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 sm:p-6">
        <h3 className="text-lg font-semibold text-gray-800 mb-4 flex items-center">
          <span className="mr-2">📋</span>
          Booking Summary
        </h3>

        <div className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {/* Date & Time */}
            <div className="space-y-3">
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">📅 Date</p>
                <p className="text-gray-900">{formatDate(booking.date)}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">⏰ Time</p>
                <p className="text-gray-900">{booking.time}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">🍽️ Table</p>
                <p className="text-gray-900">Table #{booking.table}</p>
              </div>
            </div>

            {/* Contact & Party Info */}
            <div className="space-y-3">
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">🙍 Name</p>
                <p className="text-gray-900">{booking.name}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">📱 Phone</p>
                <p className="text-gray-900">{booking.phone}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">👥 Party Size</p>
                <p className="text-gray-900">
                  {booking.guests} {booking.guests === 1 ? "guest" : "guests"}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Important Information */}
      <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
        <h4 className="font-medium text-amber-800 mb-3 flex items-center">
          <span className="mr-2">💡</span>
          Important Information
        </h4>
        <ul className="space-y-1 text-sm text-amber-700">
          <li>• Please arrive 5-10 minutes before your reservation time</li>
          <li>• Tables are held for 15 minutes past reservation time</li>
          <li>
            • Contact us at <span className="font-medium">(555) 123-4567</span> if you need to make changes
          </li>
        </ul>
      </div>

      {/* Confirmation Button */}
      <div className="text-center pt-2">
        <button
          className={`cursor-pointer w-full sm:w-auto px-8 py-4 rounded-lg font-semibold text-lg transition-all duration-200 ${isSubmitting
            ? "bg-gray-400 text-white cursor-not-allowed"
            : "bg-green-600 text-white hover:bg-green-700 shadow-sm hover:shadow-md"
            }`}
          onClick={handleConfirm}
          disabled={isSubmitting}
        >
          {isSubmitting ? (
            <span className="flex items-center justify-center">
              <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-3"></div>
              Confirming...
            </span>
          ) : (
            <span className="flex items-center justify-center">
              <span className="mr-2">✓</span>
              Confirm Booking
            </span>
          )}
        </button>
      </div>

      {/* Terms */}
      <div className="text-center text-sm text-gray-500">
        <p>
          By confirming this reservation, you agree to our{" "}
          <button className="text-blue-600 hover:underline cursor-pointer">cancellation policy</button>{" "}
          and{" "}
          <button className="text-blue-600 hover:underline cursor-pointer">terms of service</button>.
        </p>
      </div>
    </div>
  );
}
