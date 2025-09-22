// src/components/BookingSteps.tsx
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "../app/store";
import Step1SelectDate from "./Step1SelectDate";
import Step2SelectGuests from "./Step2SelectGuests";
import Step3SelectTable from "./Step3SelectTable";
import Step4ContactInfo from "./Step4ContactInfo";
import Step5Confirm from "./Step5Confirm";
import { prevStep, nextStep, resetBooking } from "../features/bookingSlice";

export default function BookingSteps() {
  const { step, date, time, table, guests, name, phone, submitted } = useSelector((state: RootState) => state.booking);
  const dispatch = useDispatch();

  const isStepValid = (currentStep: number): boolean => {
    switch (currentStep) {
      case 1:
        return !!date && new Date(date) >= new Date(new Date().toDateString()) && !!time;
      case 2:
        return guests > 0 && guests <= 20;
      case 3:
        return !!table && table > 0;
      case 4:
        return !!name.trim() && !!phone.trim() && phone.replace(/[-\D]/g, '').length >= 10;
      default:
        return true;
    }
  };

  const getStepStatus = (stepNumber: number) => {
    if (stepNumber < step) return 'completed';
    if (stepNumber === step) return 'current';
    return 'upcoming';
  };

  const handleNext = () => {
    if (isStepValid(step)) {
      dispatch(nextStep());
    }
  };

  const renderStep = () => {
    switch (step) {
      case 1:
        return <Step1SelectDate />;
      case 2:
        return <Step2SelectGuests />;
      case 3:
        return <Step3SelectTable />;
      case 4:
        return <Step4ContactInfo />;
      case 5:
        return <Step5Confirm />;
      default:
        return <Step1SelectDate />;
    }
  };

  const stepTitles = ['Date', 'Guests', 'Table', 'Contact', 'Confirm'];

  return (
    <div className="max-w-2xl mx-auto px-4 sm:px-6">
      {/* Mobile Progress Indicator - Compact horizontal version */}
      <div className="mb-6 sm:mb-8">
        {/* Mobile: Show current step info */}
        <div className="block sm:hidden mb-4">
          <div className="text-center">
            <div className="text-sm font-medium text-gray-500 mb-1">
              Step {step} of 5
            </div>
            <div className="text-lg font-semibold text-gray-800">
              {stepTitles[step - 1]}
            </div>
          </div>
        </div>

        {/* Progress bar for mobile */}
        <div className="block sm:hidden mb-4">
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div
              className="bg-blue-500 h-2 rounded-full transition-all duration-300"
              style={{ width: `${(step / 5) * 100}%` }}
            />
          </div>
        </div>

        {/* Desktop: Full progress indicator */}
        <div className="hidden sm:flex items-center justify-center space-x-2 lg:space-x-4">
          {[1, 2, 3, 4, 5].map((stepNumber) => {
            const status = getStepStatus(stepNumber);

            return (
              <div key={stepNumber} className="flex items-center">
                <div className="flex flex-col items-center">
                  <div
                    className={`w-8 h-8 lg:w-10 lg:h-10 rounded-full flex items-center justify-center text-xs lg:text-sm font-medium transition-all duration-300 ${status === 'completed'
                        ? 'bg-green-500 text-white shadow-lg'
                        : status === 'current'
                          ? 'bg-blue-500 text-white shadow-lg ring-2 lg:ring-4 ring-blue-200'
                          : 'bg-gray-200 text-gray-600'
                      }`}
                  >
                    {status === 'completed' ? '✓' : stepNumber}
                  </div>
                  <span className={`mt-2 text-xs font-medium ${status === 'current' ? 'text-blue-600' : 'text-gray-500'
                    }`}>
                    {stepTitles[stepNumber - 1]}
                  </span>
                </div>
                {stepNumber < 5 && (
                  <div
                    className={`w-8 lg:w-12 h-0.5 mx-1 lg:mx-2 transition-all duration-300 ${stepNumber < step ? 'bg-green-500' : 'bg-gray-300'
                      }`}
                  />
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Step Content */}
      <div className="bg-white rounded-lg sm:rounded-xl shadow-lg border border-gray-100 p-4 sm:p-6 lg:p-8 min-h-[280px] sm:min-h-[300px]">
        {renderStep()}
      </div>

      {/* Navigation Buttons */}
      <div className="flex justify-between items-center mt-6 sm:mt-8 gap-4">
        <div className="flex-1 sm:flex-initial">
          {step > 1 && (
            <button
              onClick={() => {
                if (submitted) {
                  dispatch(resetBooking()); // ✅ clear everything if already submitted
                } else {
                  dispatch(prevStep());
                }
              }}
              className="cursor-pointer"
            >
              {
                submitted ? (
                  <div className="w-full sm:w-auto px-4 sm:px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center justify-center sm:justify-start space-x-2 shadow-sm bg-gray-500 text-white hover:bg-gray-600 hover:shadow-md">
                    <span className="hidden sm:inline">Make New Booking</span>
                    <span className="sm:hidden">New Booking</span>
                    <span>＋</span>
                  </div>
                ) : (
                  <div className="w-full sm:w-auto px-4 sm:px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center justify-center sm:justify-start space-x-2 shadow-sm bg-gray-200 text-gray-700 hover:bg-gray-300 hover:shadow-md">
                    <span className="hidden sm:inline">Back</span>
                    <span className="sm:hidden">Back</span>
                    <span>←</span>
                  </div>
                )
              }
            </button>

          )}
        </div>

        <div className="flex-1 sm:flex-initial">
          {step < 5 && (
            <button
              className={`w-full sm:w-auto px-4 sm:px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center justify-center sm:justify-start space-x-2 shadow-sm ${isStepValid(step)
                  ? 'cursor-pointer bg-blue-500 text-white hover:bg-blue-600 hover:shadow-md'
                  : 'cursor-not-allowed bg-gray-300 text-gray-500 cursor-not-allowed'
                }`}
              onClick={handleNext}
              disabled={!isStepValid(step)}
            >
              <span className="hidden sm:inline">Next</span>
              <span className="sm:hidden">Continue</span>
              <span>→</span>
            </button>
          )}
        </div>
      </div>

      {/* Validation Message */}
      {step < 5 && !isStepValid(step) && (
        <div className="mt-4 p-3 bg-amber-50 border border-amber-200 rounded-lg">
          <p className="text-amber-800 text-sm">
            {step === 1 && "Please select a valid date (today or later)"}
            {step === 2 && "Please select between 1-20 guests"}
            {step === 3 && "Please select a table"}
            {step === 4 && "Please provide your name and a valid phone number (min 10 digits)"}
          </p>
        </div>
      )}
    </div>
  );
}