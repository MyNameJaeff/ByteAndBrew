// src/features/bookingSlice.ts
import { createSlice } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";

interface BookingState {
  step: number;
  date: string;
  time: string;
  table: number;
  guests: number;
  name: string;
  phone: string;
}

const initialState: BookingState = {
  step: 1,
  date: "",
  time: "",
  table: 0,
  guests: 1,
  name: "",
  phone: "",
};

const bookingSlice = createSlice({
  name: "booking",
  initialState,
  reducers: {
    nextStep: (state) => {
      state.step += 1;
    },
    prevStep: (state) => {
      state.step -= 1;
    },
    updateBooking: (state, action: PayloadAction<Partial<BookingState>>) => {
      return { ...state, ...action.payload };
    },
    resetBooking: () => initialState,
  },
});

export const { nextStep, prevStep, updateBooking, resetBooking } =
  bookingSlice.actions;

export default bookingSlice.reducer;
