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
            Secure your spot at Byte & Brew in just a few simple steps. 
            We'll make sure everything is perfect for your visit.
          </p>
        </div>

        {/* Booking Steps Component */}
        <BookingSteps />

      </div>
    </div>
  );
}