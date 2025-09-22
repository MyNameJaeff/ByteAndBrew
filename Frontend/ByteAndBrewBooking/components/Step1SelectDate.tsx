import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "../app/store";
import { updateBooking } from "../features/bookingSlice";

export default function Step1SelectDate() {
  const dispatch = useDispatch();
  const date = useSelector((state: RootState) => state.booking.date);
  const time = useSelector((state: RootState) => state.booking.time);
  
  const today = new Date().toISOString().split('T')[0];
  const maxDate = new Date();
  maxDate.setMonth(maxDate.getMonth() + 3);
  const maxDateString = maxDate.toISOString().split('T')[0];

  const isWeekend = (dateString: string) => {
    const day = new Date(dateString).getDay();
    return day === 0 || day === 6; // Sunday or Saturday
  };

  const formatDateDisplay = (dateString: string) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const months = ['January', 'February', 'March', 'April', 'May', 'June',
                   'July', 'August', 'September', 'October', 'November', 'December'];
        
    return `${days[date.getDay()]}, ${months[date.getMonth()]} ${date.getDate()}, ${date.getFullYear()}`;
  };

  // Generate available time slots (2-hour intervals from 10:00 to 18:00)
  const getAvailableTimeSlots = () => {
    const slots = [];
    for (let hour = 10; hour <= 18; hour += 2) {
      const timeString = `${hour.toString().padStart(2, '0')}:00`;
      const endHour = hour + 2;
      const endTimeString = `${endHour.toString().padStart(2, '0')}:00`;
      slots.push({
        value: timeString,
        label: `${timeString} - ${endTimeString}`,
        display: `${timeString} - ${endTimeString}`
      });
    }
    return slots;
  };

  const timeSlots = getAvailableTimeSlots();

  const isTimeSlotPassed = (timeSlot: string) => {
    if (!date || date !== today) return false;
    
    const now = new Date();
    const currentHour = now.getHours();
    const slotHour = parseInt(timeSlot.split(':')[0]);
    
    return slotHour <= currentHour;
  };

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Select Your Date & Time</h2>
        <p className="text-gray-600">Choose when you'd like to visit our café</p>
      </div>

      <div className="space-y-4">
        {/* Date Selection */}
        <div>
          <label htmlFor="date-picker" className="block text-sm font-medium text-gray-700 mb-2">
            Preferred Date
          </label>
          <input
            id="date-picker"
            type="date"
            value={date}
            min={today}
            max={maxDateString}
            onChange={(e) => {
              dispatch(updateBooking({ date: e.target.value }));
              // Clear time selection when date changes
              if (time) {
                dispatch(updateBooking({ time: '' }));
              }
            }}
            className="w-full p-4 border-2 border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200 text-lg"
          />
        </div>

        {/* Time Selection */}
        {date && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Preferred Time (2-hour slots)
            </label>
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
              {timeSlots.map((slot) => {
                const isPassed = isTimeSlotPassed(slot.value);
                return (
                  <button
                    key={slot.value}
                    onClick={() => dispatch(updateBooking({ time: slot.value }))}
                    disabled={isPassed}
                    className={`p-3 rounded-lg border-2 transition-all duration-200 text-sm font-medium ${
                      time === slot.value
                        ? 'border-blue-500 bg-blue-50 text-blue-700'
                        : isPassed
                        ? 'border-gray-200 bg-gray-100 text-gray-400 cursor-not-allowed'
                        : 'border-gray-200 hover:border-blue-300 hover:bg-blue-50 text-gray-700'
                    }`}
                  >
                    {slot.display}
                    {isPassed && (
                      <div className="text-xs text-gray-400 mt-1">Past</div>
                    )}
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* Selected Date and Time Display */}
        {date && time && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div className="flex items-center space-x-3">
              <div className="text-blue-500">
                📅
              </div>
              <div>
                <p className="font-medium text-blue-800">{formatDateDisplay(date)}</p>
                <p className="text-blue-700">
                  🕐 {timeSlots.find(slot => slot.value === time)?.display}
                </p>
                {isWeekend(date) && (
                  <p className="text-blue-600 text-sm mt-1">
                    🎉 Weekend special menu available!
                  </p>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Empty State */}
        {(!date || !time) && (
          <div className="text-center py-8">
            <div className="text-gray-400 text-6xl mb-4">📅</div>
            <p className="text-gray-500">
              {!date ? 'Select a date to continue' : 'Select a time slot to continue'}
            </p>
          </div>
        )}
      </div>

      {/* Opening Hours */}
      <div className="bg-gray-50 rounded-lg p-4">
        <h3 className="font-medium text-gray-800 mb-2">Opening Hours & Booking Info</h3>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-sm text-gray-600">
          <div>
            <p className="font-medium">Daily Hours</p>
            <p>10:00 AM - 8:00 PM (10:00 - 20:00)</p>
          </div>
          <div>
            <p className="font-medium">Booking Duration</p>
            <p>2 hours per reservation</p>
          </div>
        </div>
        <div className="mt-3 text-xs text-gray-500">
          <p>• Each booking includes a 2-hour time slot</p>
          <p>• Last booking starts at 6:00 PM</p>
        </div>
      </div>
    </div>
  );
}