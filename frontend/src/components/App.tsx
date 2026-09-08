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
  const [status, setStatus] = useState("");

  useEffect(() => {
    let cancelled = false;
    setStatus("loading…");
    fetchSchedule(date)
      .then((d) => {
        if (cancelled) return;
        setData(d);
        setStatus(d.isMonday ? "centre closed (Monday)" : "");
        if (d.date !== date) setDate(d.date);
      })
      .catch(() => {
        if (!cancelled) setStatus("failed to load");
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
          onChange={(e) => setDate(e.target.value || PINNED_TODAY)}
        />
        <span className="muted">{status}</span>
      </header>
      <main>
        {data && (
          <>
            <Banner conflicts={data.conflicts} />
            <Changes events={data.changes} day={data.date} />
            <Loads tutorLoads={data.tutorLoads} />
            <Rooms rooms={data.rooms} />
          </>
        )}
      </main>
    </>
  );
}
