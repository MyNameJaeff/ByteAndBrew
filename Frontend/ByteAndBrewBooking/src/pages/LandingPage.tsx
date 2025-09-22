import { Link } from "react-router-dom";

export default function LandingPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-amber-50 via-white to-orange-50">
      {/* Hero Section */}
      <div className="text-center pt-16 pb-12">
        <div className="mb-8">
          <div className="text-8xl mb-4">☕</div>
          <h1 className="text-5xl md:text-6xl font-bold mb-6 bg-gradient-to-r from-amber-600 to-orange-600 bg-clip-text text-transparent">
            Byte & Brew
          </h1>
          <p className="text-xl text-gray-600 mb-8 max-w-2xl mx-auto leading-relaxed">
            Experience the perfect blend of artisanal coffee, fresh pastries, and warm hospitality. 
            Reserve your table today for an unforgettable café experience.
          </p>
        </div>

        <Link to="/booking">
          <button className="group px-8 py-4 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white rounded-xl font-semibold text-lg shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-300">
            <div className="flex items-center space-x-2">
              <span>Book Your Table</span>
              <span className="group-hover:translate-x-1 transition-transform duration-300">→</span>
            </div>
          </button>
        </Link>
      </div>

      {/* Features Section */}
      <div className="max-w-6xl mx-auto px-6 py-12">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div className="text-center p-6 bg-white rounded-xl shadow-md hover:shadow-lg transition-shadow duration-300">
            <div className="text-4xl mb-4">🌟</div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Premium Quality</h3>
            <p className="text-gray-600">
              Sourced from the finest coffee beans and fresh, locally-sourced ingredients
            </p>
          </div>
          
          <div className="text-center p-6 bg-white rounded-xl shadow-md hover:shadow-lg transition-shadow duration-300">
            <div className="text-4xl mb-4">🪑</div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Cozy Atmosphere</h3>
            <p className="text-gray-600">
              Comfortable seating and warm ambiance perfect for any occasion
            </p>
          </div>
          
          <div className="text-4xl mb-4 text-center p-6 bg-white rounded-xl shadow-md hover:shadow-lg transition-shadow duration-300">
            <div className="text-4xl mb-4">📱</div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Easy Booking</h3>
            <p className="text-gray-600">
              Simple online reservation system - book your table in just a few clicks
            </p>
          </div>
        </div>
      </div>

      {/* Stats Section */}
      <div className="bg-gradient-to-r from-amber-500 to-orange-500 text-white py-16">
        <div className="max-w-4xl mx-auto px-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 text-center">
            <div>
              <div className="text-4xl font-bold mb-2">2,500+</div>
              <p className="text-amber-100">Happy Customers</p>
            </div>
            <div>
              <div className="text-4xl font-bold mb-2">4.9★</div>
              <p className="text-amber-100">Average Rating</p>
            </div>
            <div>
              <div className="text-4xl font-bold mb-2">3</div>
              <p className="text-amber-100">Years Serving</p>
            </div>
          </div>
        </div>
      </div>

      {/* Call to Action */}
      <div className="text-center py-16">
        <h2 className="text-3xl font-bold text-gray-800 mb-4">Ready for Your Perfect Coffee Experience?</h2>
        <p className="text-gray-600 mb-8 max-w-lg mx-auto">
          Join thousands of satisfied customers who have made My Café their favorite destination
        </p>
        <Link to="/booking">
          <button className="px-8 py-4 bg-gradient-to-r from-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 text-white rounded-xl font-semibold text-lg shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-300">
            Reserve Now - It's Free!
          </button>
        </Link>
      </div>
    </div>
  );
}