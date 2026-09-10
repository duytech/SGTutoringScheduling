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
  const [status, setStatus] = useState("");

  useEffect(() => {
    let cancelled = false;
    setStatus("loading…");
    Promise.all([fetchSchedule(date), fetchTutorLoads(date), fetchConflicts(date)])
      .then(([day, loads, dayConflicts]) => {
        if (cancelled) return;
        setData(day);
        setTutorLoads(loads.tutorLoads);
        setConflicts(dayConflicts.conflicts);
        setStatus(day.isMonday ? "centre closed (Monday)" : "");
        if (day.date !== date) setDate(day.date);
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
          onChange={(event) => setDate(event.target.value || PINNED_TODAY)}
        />
        <span className="muted">{status}</span>
      </header>
      <main>
        {data && (
          <>
            <Banner conflicts={conflicts} />
            <Changes events={data.changes} day={data.date} />
            <Loads tutorLoads={tutorLoads} />
            <Rooms rooms={data.rooms} />
          </>
        )}
      </main>
    </>
  );
}
