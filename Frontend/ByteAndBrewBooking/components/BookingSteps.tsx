// src/components/BookingSteps.tsx
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "../app/store";
import Step1SelectDate from "./Step1SelectDate";
import Step2SelectGuests from "./Step2SelectGuests";
import Step3SelectTable from "./Step3SelectTable";
import Step4ContactInfo from "./Step4ContactInfo";
import Step5Confirm from "./Step5Confirm";
import { prevStep, nextStep } from "../features/bookingSlice";

export default function BookingSteps() {
  const { step, date, table, guests, name, phone } = useSelector((state: RootState) => state.booking);
  const dispatch = useDispatch();

  const isStepValid = (currentStep: number): boolean => {
    switch (currentStep) {
      case 1:
        return !!date && new Date(date) >= new Date(new Date().toDateString());
      case 2:
        return guests > 0 && guests <= 20;
      case 3:
        return !!table;
      case 4:
        return !!name.trim() && !!phone.trim() && phone.length >= 10;
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
    if(step === 3){
      console.log("Selected Table:", table);
    }
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

  return (
    <div className="max-w-2xl mx-auto">
      {/* Progress Indicator */}
      <div className="mb-8">
        <div className="flex items-center justify-center space-x-4">
          {[1, 2, 3, 4, 5].map((stepNumber) => {
            const status = getStepStatus(stepNumber);
            const stepTitles = ['Date', 'Guests', 'Table', 'Contact', 'Confirm'];
            
            return (
              <div key={stepNumber} className="flex items-center">
                <div className="flex flex-col items-center">
                  <div
                    className={`w-10 h-10 rounded-full flex items-center justify-center text-sm font-medium transition-all duration-300 ${
                      status === 'completed'
                        ? 'bg-green-500 text-white shadow-lg'
                        : status === 'current'
                        ? 'bg-blue-500 text-white shadow-lg ring-4 ring-blue-200'
                        : 'bg-gray-200 text-gray-600'
                    }`}
                  >
                    {status === 'completed' ? '✓' : stepNumber}
                  </div>
                  <span className={`mt-2 text-xs font-medium ${
                    status === 'current' ? 'text-blue-600' : 'text-gray-500'
                  }`}>
                    {stepTitles[stepNumber - 1]}
                  </span>
                </div>
                {stepNumber < 5 && (
                  <div
                    className={`w-12 h-0.5 mx-2 transition-all duration-300 ${
                      stepNumber < step ? 'bg-green-500' : 'bg-gray-300'
                    }`}
                  />
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Step Content */}
      <div className="bg-white rounded-xl shadow-lg border border-gray-100 p-8 min-h-[300px]">
        {renderStep()}
      </div>

      {/* Navigation Buttons */}
      <div className="flex justify-between items-center mt-8">
        <div>
          {step > 1 && (
            <button
              className="px-6 py-3 bg-gray-100 text-gray-700 rounded-lg font-medium hover:bg-gray-200 transition-all duration-200 flex items-center space-x-2 shadow-sm"
              onClick={() => dispatch(prevStep())}
            >
              <span>←</span>
              <span>Back</span>
            </button>
          )}
        </div>
        
        <div>
          {step < 5 && (
            <button
              className={`px-6 py-3 rounded-lg font-medium transition-all duration-200 flex items-center space-x-2 shadow-sm ${
                isStepValid(step)
                  ? 'bg-blue-500 text-white hover:bg-blue-600 hover:shadow-md'
                  : 'bg-gray-300 text-gray-500 cursor-not-allowed'
              }`}
              onClick={handleNext}
              disabled={!isStepValid(step)}
            >
              <span>Next</span>
              <span>→</span>
            </button>
          )}
        </div>
      </div>

      {/* Validation Message */}
      {step < 4 && !isStepValid(step) && (
        <div className="mt-4 p-3 bg-amber-50 border border-amber-200 rounded-lg">
          <p className="text-amber-800 text-sm">
            {step === 1 && "Please select a valid date (today or later)"}
            {step === 2 && "Please select between 1-20 guests"}
            {step === 3 && "Please provide your name and a valid phone number (min 10 digits)"}
          </p>
        </div>
      )}
    </div>
  );
}