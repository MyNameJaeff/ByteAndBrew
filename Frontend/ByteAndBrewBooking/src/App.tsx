// src/App.tsx
import { Routes, Route, NavLink } from "react-router-dom";
import { useState } from "react";
import LandingPage from "./pages/LandingPage";
import BookingPage from "./pages/BookingPage";

function App() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  const closeMobileMenu = () => {
    setIsMobileMenuOpen(false);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Enhanced Mobile-Responsive Navigation */}
      <nav className="bg-white shadow-sm border-b border-gray-200 sticky top-0 z-50">
        <div className="max-w-6xl mx-auto px-4 sm:px-6">
          <div className="flex justify-between items-center h-16">
            {/* Logo */}
            <div className="flex items-center space-x-2">
              <span className="text-2xl">☕</span>
              <span className="text-lg sm:text-xl font-bold text-gray-800">Byte & Brew</span>
            </div>

            {/* Desktop Navigation Links */}
            <div className="hidden md:flex items-center space-x-8">
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

            {/* Mobile menu button */}
            <div className="md:hidden">
              <button
                onClick={toggleMobileMenu}
                className="inline-flex items-center justify-center p-2 rounded-md text-gray-700 hover:text-blue-600 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-500 transition-colors"
                aria-expanded="false"
              >
                <span className="sr-only">Open main menu</span>
                {/* Hamburger icon */}
                <svg
                  className={`${isMobileMenuOpen ? 'hidden' : 'block'} h-6 w-6`}
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  aria-hidden="true"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                </svg>
                {/* Close icon */}
                <svg
                  className={`${isMobileMenuOpen ? 'block' : 'hidden'} h-6 w-6`}
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  aria-hidden="true"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>
        </div>

        {/* Mobile menu */}
        <div className={`md:hidden transition-all duration-300 ease-in-out ${
          isMobileMenuOpen 
            ? 'max-h-64 opacity-100' 
            : 'max-h-0 opacity-0 overflow-hidden'
        }`}>
          <div className="px-2 pt-2 pb-3 space-y-1 bg-white border-t border-gray-200 shadow-lg">
            <NavLink
              to="/"
              onClick={closeMobileMenu}
              className={({ isActive }) =>
                `block px-3 py-2 rounded-md text-base font-medium transition-all duration-200 ${
                  isActive
                    ? "text-blue-600 bg-blue-50"
                    : "text-gray-700 hover:text-blue-600 hover:bg-gray-50"
                }`
              }
            >
              Home
            </NavLink>
            <NavLink
              to="/booking"
              onClick={closeMobileMenu}
              className={({ isActive }) =>
                `block px-3 py-2 rounded-md text-base font-medium transition-all duration-200 ${
                  isActive
                    ? "text-blue-600 bg-blue-50"
                    : "text-gray-700 hover:text-blue-600 hover:bg-gray-50"
                }`
              }
            >
              Book Table
            </NavLink>
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

      {/* Mobile-Responsive Footer */}
      <footer className="bg-gray-800 text-white py-8 sm:py-12">
        <div className="max-w-6xl mx-auto px-4 sm:px-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 sm:gap-8">
            {/* Logo and description */}
            <div className="sm:col-span-2 lg:col-span-1">
              <div className="flex items-center space-x-2 mb-4">
                <span className="text-2xl">☕</span>
                <span className="text-xl font-bold">Byte & Brew</span>
              </div>
              <p className="text-gray-400 text-sm sm:text-base">
                Your neighborhood café serving premium coffee and delicious treats since 2022.
              </p>
            </div>
           
            {/* Contact Info */}
            <div>
              <h4 className="font-semibold mb-3 sm:mb-4 text-base sm:text-lg">Contact Info</h4>
              <div className="space-y-2 text-gray-400 text-sm sm:text-base">
                <p className="flex items-start space-x-2">
                  <span>📍</span>
                  <span>123 Coffee Street, City, State 12345</span>
                </p>
                <p className="flex items-center space-x-2">
                  <span>📞</span>
                  <a href="tel:5551234567" className="hover:underline hover:text-white transition-colors">
                    (555) 123-4567
                  </a>
                </p>
                <p className="flex items-center space-x-2">
                  <span>✉️</span>
                  <a 
                    href="mailto:hello@mycafe.com" 
                    className="hover:underline hover:text-white transition-colors break-all" 
                    target="_blank" 
                    rel="noopener noreferrer"
                  >
                    hello@mycafe.com
                  </a>
                </p>
              </div>
            </div>
           
            {/* Hours */}
            <div>
              <h4 className="font-semibold mb-3 sm:mb-4 text-base sm:text-lg">Hours</h4>
              <div className="space-y-2 text-gray-400 text-sm sm:text-base">
                <p>Mon - Fri: 10:00 AM - 8:00 PM</p>
                <p>Sat - Sun: 10:00 AM - 8:00 PM</p>
              </div>
            </div>
          </div>
         
          <div className="border-t border-gray-700 mt-6 sm:mt-8 pt-6 sm:pt-8 text-center text-gray-400">
            <p className="text-sm sm:text-base">&copy; 2024 Byte & Brew. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  );
}

export default App;