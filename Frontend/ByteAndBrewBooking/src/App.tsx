// src/App.tsx
import { Routes, Route, NavLink } from "react-router-dom";
import LandingPage from "./pages/LandingPage";
import BookingPage from "./pages/BookingPage";

function App() {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Enhanced Navigation */}
      <nav className="bg-white shadow-sm border-b border-gray-200 sticky top-0 z-50">
        <div className="max-w-6xl mx-auto px-6">
          <div className="flex justify-between items-center h-16">
            {/* Logo */}
            <div className="flex items-center space-x-2">
              <span className="text-2xl">☕</span>
              <span className="text-xl font-bold text-gray-800">Byte & Brew</span>
            </div>

            {/* Navigation Links */}
            <div className="flex items-center space-x-8">
              <NavLink
                to="/"
                className={({ isActive }) =>
                  `px-4 py-2 rounded-lg font-medium transition-all duration-200 ${
                    isActive
                      ? "text-blue-600 bg-blue-50 shadow-sm"
                      : "text-gray-700 hover:text-blue-600 hover:bg-gray-50"
                  }`
                }
              >
                Home
              </NavLink>
              <NavLink
                to="/booking"
                className={({ isActive }) =>
                  `px-4 py-2 rounded-lg font-medium transition-all duration-200 ${
                    isActive
                      ? "text-blue-600 bg-blue-50 shadow-sm"
                      : "text-gray-700 hover:text-blue-600 hover:bg-gray-50"
                  }`
                }
              >
                Book Table
              </NavLink>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/booking" element={<BookingPage />} />
        </Routes>
      </main>

      {/* Footer */}
      <footer className="bg-gray-800 text-white py-12">
        <div className="max-w-6xl mx-auto px-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div>
              <div className="flex items-center space-x-2 mb-4">
                <span className="text-2xl">☕</span>
                <span className="text-xl font-bold">Byte & Brew</span>
              </div>
              <p className="text-gray-400">
                Your neighborhood café serving premium coffee and delicious treats since 2022.
              </p>
            </div>
            
            <div>
              <h4 className="font-semibold mb-4">Contact Info</h4>
              <div className="space-y-2 text-gray-400">
                <p>📍 123 Coffee Street, City, State 12345</p>
                <p>📞 (555) 123-4567</p>
                <p>✉️ hello@mycafe.com</p>
              </div>
            </div>
            
            <div>
              <h4 className="font-semibold mb-4">Hours</h4>
              <div className="space-y-2 text-gray-400">
                <p>Monday - Friday: 10:00 AM - 8:00 PM</p>
                <p>Saturday - Sunday: 10:00 AM - 8:00 PM</p>
              </div>
            </div>
          </div>
          
          <div className="border-t border-gray-700 mt-8 pt-8 text-center text-gray-400">
            <p>&copy; 2024 Byte & Brew. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  );
}

export default App;