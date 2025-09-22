import { useDispatch, useSelector } from "react-redux";
import { updateBooking } from "../features/bookingSlice";
import type { RootState } from "../app/store";
import { useState } from "react";

export default function Step4ContactInfo() {
  const dispatch = useDispatch();
  const { name, phone } = useSelector((state: RootState) => state.booking);
  const [errors, setErrors] = useState({ name: '', phone: '' });

  const validateName = (value: string) => {
    if (!value.trim()) return 'Name is required';
    if (value.trim().length < 2) return 'Name must be at least 2 characters';
    return '';
  };

  const validatePhone = (value: string) => {
    const cleanPhone = value.replace(/[-\D]/g, '');
    if (!cleanPhone) return 'Phone number is required';
    if (cleanPhone.length < 10) return 'Phone number must be at least 10 digits';
    return '';
  };

  const formatPhoneNumber = (value: string) => {
    const cleaned = value.replace(/\D/g, '');
    const match = cleaned.match(/^(\d{0,3})(\d{0,3})(\d{0,4})$/);
    if (!match) return value;
    
    const [, part1, part2, part3] = match;
    let formatted = part1;
    if (part2) formatted += `-${part2}`;
    if (part3) formatted += `-${part3}`;
    
    return formatted;
  };

  const handleNameChange = (value: string) => {
    dispatch(updateBooking({ name: value }));
    setErrors(prev => ({ ...prev, name: validateName(value) }));
  };

  const handlePhoneChange = (value: string) => {
    const formatted = formatPhoneNumber(value);
    dispatch(updateBooking({ phone: formatted }));
    setErrors(prev => ({ ...prev, phone: validatePhone(formatted) }));
  };

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Contact Information</h2>
        <p className="text-gray-600">We'll use this information to confirm your reservation</p>
      </div>

      <div className="space-y-6">
        {/* Name Field */}
        <div>
          <label htmlFor="name-input" className="block text-sm font-medium text-gray-700 mb-2">
            Full Name *
          </label>
          <div className="relative">
            <input
              id="name-input"
              type="text"
              placeholder="Enter your full name"
              value={name}
              onChange={(e) => handleNameChange(e.target.value)}
              className={`w-full p-4 pl-12 border-2 rounded-lg focus:outline-none transition-all duration-200 ${
                errors.name
                  ? 'border-red-300 focus:ring-2 focus:ring-red-500'
                  : 'border-gray-200 focus:ring-2 focus:ring-blue-500 focus:border-transparent'
              }`}
            />
            <div className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400">
              👤
            </div>
          </div>
          {errors.name && (
            <p className="mt-2 text-sm text-red-600 flex items-center space-x-1">
              <span>⚠️</span>
              <span>{errors.name}</span>
            </p>
          )}
        </div>

        {/* Phone Field */}
        <div>
          <label htmlFor="phone-input" className="block text-sm font-medium text-gray-700 mb-2">
            Phone Number *
          </label>
          <div className="relative">
            <input
              id="phone-input"
              type="tel"
              placeholder="123-456-7890"
              value={phone}
              onChange={(e) => handlePhoneChange(e.target.value)}
              className={`w-full p-4 pl-12 border-2 rounded-lg focus:outline-none transition-all duration-200 ${
                errors.phone
                  ? 'border-red-300 focus:ring-2 focus:ring-red-500'
                  : 'border-gray-200 focus:ring-2 focus:ring-blue-500 focus:border-transparent'
              }`}
            />
            <div className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400">
              📞
            </div>
          </div>
          {errors.phone && (
            <p className="mt-2 text-sm text-red-600 flex items-center space-x-1">
              <span>⚠️</span>
              <span>{errors.phone}</span>
            </p>
          )}
        </div>

        {/* Contact Preview */}
        {name && phone && !errors.name && !errors.phone && (
          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <div className="flex items-center space-x-3">
              <div className="text-green-500 text-xl">✅</div>
              <div>
                <p className="font-medium text-green-800">Contact Information Complete</p>
                <p className="text-green-700 text-sm">{name} • {phone}</p>
              </div>
            </div>
          </div>
        )}
      </div>

      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h3 className="font-medium text-blue-800 mb-2">📋 Why we need this information</h3>
        <div className="space-y-1 text-sm text-blue-700">
          <p>• Confirm your reservation via call or text</p>
          <p>• Notify you of any changes or delays</p>
          <p>• Contact you if we need to adjust your table</p>
        </div>
      </div>

      <div className="bg-gray-50 rounded-lg p-4">
        <h3 className="font-medium text-gray-800 mb-2">🔒 Privacy Notice</h3>
        <p className="text-sm text-gray-600">
          Your contact information is secure and will only be used for reservation purposes. 
          We never share your information with third parties.
        </p>
      </div>
    </div>
  );
}