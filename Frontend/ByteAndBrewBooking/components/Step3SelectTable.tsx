import { useEffect, useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "../app/store";
import { updateBooking } from "../features/bookingSlice";

interface Table {
    id: number;
    tableNumber: number;
    capacity: number;
    bookings: any[];
    isBooked: boolean;
}

export default function Step3SelectTable() {
    const booking = useSelector((state: RootState) => state.booking);
    const dispatch = useDispatch();

    const [availableTables, setAvailableTables] = useState<Table[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [selectedTable, setSelectedTable] = useState<number | null>(null);

    useEffect(() => {
        if (!booking.date || !booking.time || !booking.guests) return;

        const fetchTables = async () => {
            setLoading(true);
            setError(null);
            try {
                const params = new URLSearchParams({
                    date: booking.date,
                    time: booking.time,
                    people: booking.guests.toString(),
                });

                const res = await fetch(`https://localhost:7145/api/Tables/available?${params}`);

                if (res.status === 404) {
                    setAvailableTables([]); // no tables available
                } else if (!res.ok) {
                    throw new Error("Failed to fetch available tables");
                } else {
                    const data: Table[] = await res.json();
                    setAvailableTables(data);
                }
            } catch (err: any) {
                setError(err.message || "Something went wrong");
            } finally {
                setLoading(false);
            }
        };

        fetchTables();
    }, [booking.date, booking.time, booking.guests]);

    const handleSelectTable = (tableNumber: number) => {
        setSelectedTable(tableNumber);
        dispatch(updateBooking({ table: tableNumber }));
    };

    return (
        <div className="space-y-6">
            <h2 className="text-xl font-semibold text-gray-700">Select a Table</h2>

            {loading && <p className="text-gray-500">Loading available tables...</p>}
            {error && <p className="text-red-500">{error}</p>}
            {!loading && !error && availableTables.length === 0 && (
                <p className="text-gray-500">No tables available for this date/time.</p>
            )}
            {!loading && !error && availableTables.length > 0 && (
                <div className="grid grid-cols-[repeat(auto-fit,minmax(100px,1fr))] gap-3">
                    {availableTables.map((table) => (
                        <button
                            key={table.id}
                            onClick={() => handleSelectTable(table.tableNumber)}
                            disabled={table.isBooked}
                            className={`p-4 rounded-lg border-2 font-medium text-center transition-all duration-200 ${table.isBooked
                                ? "border-gray-300 bg-gray-100 text-gray-400 cursor-not-allowed"
                                : selectedTable === table.tableNumber
                                    ? "border-blue-500 bg-blue-50 text-blue-700 shadow-inner"
                                    : "border-gray-200 bg-white text-gray-700 hover:border-gray-300 hover:bg-gray-50"
                                }`}
                        >
                            <div>Table {table.tableNumber}</div>
                            <div className="text-sm text-gray-500">{table.capacity} seats</div>
                        </button>
                    ))}
                </div>
            )}
        </div>
    );
}
