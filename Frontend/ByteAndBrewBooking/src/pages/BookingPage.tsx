import BookingSteps from "../../components/BookingSteps";

export default function BookingPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50">
      <div className="max-w-4xl mx-auto px-6 py-12">
        {/* Header */}
        <div className="text-center mb-12">
          <div className="text-5xl mb-4">🍽️</div>
          <h1 className="text-4xl font-bold text-gray-800 mb-4">
            Reserve Your Table
          </h1>
          <p className="text-lg text-gray-600 max-w-2xl mx-auto">
            Secure your spot at My Café in just a few simple steps. 
            We'll make sure everything is perfect for your visit.
          </p>
        </div>

        {/* Booking Steps Component */}
        <BookingSteps />

        {/* Help Section */}
        <div className="mt-16 bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div className="text-center">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Need Help?</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-sm text-gray-600">
              <div className="flex items-center space-x-3">
                <div className="text-blue-500 text-xl">📞</div>
                <div>
                  <p className="font-medium">Call Us</p>
                  <p>(555) 123-4567</p>
                </div>
              </div>
              <div className="flex items-center space-x-3">
                <div className="text-blue-500 text-xl">✉️</div>
                <div>
                  <p className="font-medium">Email</p>
                  <p>reservations@mycafe.com</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}