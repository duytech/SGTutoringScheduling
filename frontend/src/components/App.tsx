import { useEffect, useState } from "react";
import type { ScheduleDayResponse } from "../api/types";
import { fetchSchedule, PINNED_TODAY } from "../api/schedule";
import { Banner } from "./Banner";
import { Changes } from "./Changes";
import { Loads } from "./Loads";
import { Rooms } from "./Rooms";

export function App() {
  const [date, setDate] = useState(PINNED_TODAY);
  const [data, setData] = useState<ScheduleDayResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [status, setStatus] = useState("");

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    fetchSchedule(date)
      .then((day) => {
        if (cancelled) return;

        setLoading(false);
        setData(day);
        setStatus(day.isMonday ? "centre closed (Monday)" : "");
        if (day.date !== date) setDate(day.date);
      })
      .catch((reason) => {
        if (cancelled) return;

        setLoading(false);
        console.error(`schedule fetch failed for ${date}`, reason);
        setStatus("failed to load");
      });

    return () => {
      cancelled = true;
    };
  }, [date]);

  return (
    <>
      <header>
        <h1>Bright Path Learning Centre</h1>
        <span className="muted">Room board for</span>
        <input
          type="date"
          value={date}
          disabled={loading}
          onChange={(event) => setDate(event.target.value || PINNED_TODAY)}
        />
        <span className="muted">{status}</span>
      </header>
      <main aria-busy={loading}>
        {loading && (
          <div className="loading-overlay">
            <div className="spinner" />
          </div>
        )}
        <Banner date={date} />
        {data && <Changes events={data.changes} day={data.date} />}
        <Loads date={date} />
        {data && <Rooms rooms={data.rooms} />}
      </main>
    </>
  );
}
