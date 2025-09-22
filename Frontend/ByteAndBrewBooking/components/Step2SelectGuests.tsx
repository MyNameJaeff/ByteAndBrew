import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "../app/store";
import { updateBooking } from "../features/bookingSlice";

export default function Step2SelectGuests() {
  const dispatch = useDispatch();
  const guests = useSelector((state: RootState) => state.booking.guests);

  const handleGuestChange = (newGuests: number) => {
    if (newGuests >= 1 && newGuests <= 12) {
      dispatch(updateBooking({ guests: newGuests }));
    }
  };

  const getTableSize = (guestCount: number) => {
    if (guestCount <= 2) return "Small table (1-2 people)";
    if (guestCount <= 4) return "Medium table (3-4 people)";
    if (guestCount <= 6) return "Large table (5-6 people)";
    if (guestCount <= 8) return "Extra large table (7-8 people)";
    if (guestCount <= 10) return "Party table (9-10 people)";
    if (guestCount <= 12) return "Group table (11-12 people)";
    return "Private dining area (12+ people)";
  };

  const quickSelectOptions = [1, 2, 4, 6, 8, 10, 12];

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">How Many Guests?</h2>
        <p className="text-gray-600">Select the number of people in your party</p>
      </div>

      {/* Quick Select Buttons */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-3">
          Quick Select
        </label>
        <div className="grid grid-cols-[repeat(auto-fit,minmax(120px,1fr))] gap-3">
          {quickSelectOptions.map((option) => (
            <button
              key={option}
              onClick={() => handleGuestChange(option)}
              className={`p-3 rounded-xl border-2 font-medium text-center transition-all duration-200 ${guests === option
                  ? 'border-blue-500 bg-blue-50 text-blue-700 shadow-inner'
                  : 'border-gray-200 bg-white text-gray-700 hover:border-gray-300 hover:bg-gray-50'
                }`}
            >
              {option}
            </button>
          ))}
        </div>
      </div>

      {/* Manual Input */}
      <div>
        <label htmlFor="guest-input" className="block text-sm font-medium text-gray-700 mb-2">
          Or Enter Custom Number
        </label>
        <div className="flex items-center space-x-4">
          <button
            onClick={() => handleGuestChange(Math.max(1, guests - 1))}
            className="w-12 h-12 rounded-lg bg-gray-100 hover:bg-gray-200 flex items-center justify-center text-xl font-medium transition-colors"
            disabled={guests <= 1}
          >
            −
          </button>
          <input
            id="guest-input"
            type="number"
            min={1}
            max={12}
            value={guests}
            onChange={(e) => handleGuestChange(Number(e.target.value))}
            className="w-24 p-4 border-2 border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200 text-center text-xl font-medium"
          />
          <button
            onClick={() => handleGuestChange(Math.min(20, guests + 1))}
            className={`w-12 h-12 rounded-lg bg-gray-100 hover:bg-gray-200 flex items-center justify-center text-xl font-medium transition-colors ${guests >= 12 ? 'opacity-50 cursor-not-allowed' : ''}`}
            disabled={guests >= 12}
          >
            +
          </button>
        </div>
      </div>

      {/* Guest Count Display */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div className="flex items-center space-x-3">
          <div className="text-blue-500 text-2xl">
            👥
          </div>
          <div>
            <p className="font-medium text-blue-800">
              {guests} {guests === 1 ? 'Guest' : 'Guests'}
            </p>
            <p className="text-blue-600 text-sm">{getTableSize(guests)}</p>
          </div>
        </div>
      </div>

      {/* Special Notices */}
      {guests >= 11 && (
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
          <div className="flex items-start space-x-3">
            <div className="text-amber-500 text-xl">ℹ️</div>
            <div>
              <p className="font-medium text-amber-800">Large Party Notice</p>
              <p className="text-amber-700 text-sm mt-1">
                For parties of 12 or more, we may require a deposit and recommend calling ahead to ensure availability.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="bg-gray-50 rounded-lg p-4">
        <h3 className="font-medium text-gray-800 mb-2">Party Size Information</h3>
        <div className="space-y-2 text-sm text-gray-600">
          <p>• Maximum party size: 12 people</p>
          <p>• Large parties (12+) may have limited seating options</p>
          <p>• High chairs available for children upon request</p>
        </div>
      </div>
    </div>
  );
}