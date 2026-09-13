import { useEffect, useState } from "react";
import type { Conflict, ScheduleDayResponse, TutorLoad } from "../../api/types";
import { fetchSchedule, PINNED_TODAY } from "../../api/schedule";
import { fetchTutorLoads } from "../../api/tutors";
import { fetchConflicts } from "../../api/conflicts";
import { Banner } from "../Banner";
import { Changes } from "../Changes";
import { Loads } from "../Loads";
import { Rooms } from "../Rooms";
import styles from "./App.module.css";

function shiftDate(date: string, delta: number): string {
  const shifted = new Date(`${date}T00:00:00Z`);
  shifted.setUTCDate(shifted.getUTCDate() + delta);

  return shifted.toISOString().slice(0, 10);
}

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
      <header className={styles.header}>
        <h1>Bright Path Learning Centre</h1>
        <span className={styles.muted}>Room board for</span>
        <button
          type="button"
          className={styles.navButton}
          disabled={loading}
          onClick={() => setDate((current) => shiftDate(current, -1))}
        >
          Back
        </button>
        <input
          type="date"
          value={date}
          disabled={loading}
          onChange={(event) => setDate(event.target.value || PINNED_TODAY)}
        />
        <button
          type="button"
          className={styles.navButton}
          disabled={loading}
          onClick={() => setDate((current) => shiftDate(current, 1))}
        >
          Next
        </button>
        <span className={styles.muted}>{status}</span>
      </header>
      <main className={styles.main} aria-busy={loading}>
        {loading && (
          <div className={styles.loadingOverlay}>
            <div className={styles.spinner} />
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
