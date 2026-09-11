import { useEffect, useState } from "react";
import type { Conflict, ScheduleDayResponse, TutorLoad } from "../api/types";
import { fetchSchedule, PINNED_TODAY } from "../api/schedule";
import { fetchTutorLoads } from "../api/tutors";
import { fetchConflicts } from "../api/conflicts";
import { Banner } from "./Banner";
import { Changes } from "./Changes";
import { Loads } from "./Loads";
import { Rooms } from "./Rooms";

export function App() {
  const [date, setDate] = useState(PINNED_TODAY);
  const [data, setData] = useState<ScheduleDayResponse | null>(null);
  const [tutorLoads, setTutorLoads] = useState<TutorLoad[]>([]);
  const [conflicts, setConflicts] = useState<Conflict[]>([]);
  const [loading, setLoading] = useState(true);
  const [status, setStatus] = useState("");

  useEffect(() => {
    // Cancelled guard is a pattern to handle race condition
    let cancelled = false;
    setLoading(true);

    Promise.all([fetchSchedule(date), fetchTutorLoads(date), fetchConflicts(date)])
      .then(([day, loads, dayConflicts]) => {
        if (cancelled) return;

        setLoading(false);
        setData(day);
        setTutorLoads(loads.tutorLoads);
        setConflicts(dayConflicts.conflicts);
        setStatus(day.isMonday ? "centre closed (Monday)" : "");
      })
      .catch((reason) => {
        if (cancelled) return;

        setLoading(false);
        console.error(`board fetch failed for ${date}`, reason);
        setStatus("failed to load");
      });

    // cleanup: run when date change or unmount
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
        {data && (
          <>
            <Banner conflicts={conflicts} />
            <Changes events={data.changes} day={date} />
            <Loads tutorLoads={tutorLoads} />
            <Rooms rooms={data.rooms} />
          </>
        )}
      </main>
    </>
  );
}
