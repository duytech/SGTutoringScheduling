import { useEffect, useState } from "react";
import type { Conflict } from "../api/types";
import { fetchConflicts } from "../api/conflicts";

export function Banner({ date }: { date: string }) {
  const [conflicts, setConflicts] = useState<Conflict[] | null>(null);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setFailed(false);

    fetchConflicts(date)
      .then((response) => {
        if (!cancelled) setConflicts(response.conflicts);
      })
      .catch((reason) => {
        if (cancelled) return;

        console.error(`conflicts fetch failed for ${date}`, reason);
        setFailed(true);
      });

    return () => {
      cancelled = true;
    };
  }, [date]);

  if (failed) {
    return (
      <div id="banner">
        <div className="conflict">Couldn't load conflicts for this day.</div>
      </div>
    );
  }

  if (conflicts === null) {
    return null;
  }

  if (!conflicts.length) {
    return (
      <div id="banner">
        <div className="conflict" style={{ borderLeftColor: "#1a7f37", background: "#e9f6ec" }}>
          <span className="ok">No conflicts for this day.</span>
        </div>
      </div>
    );
  }

  return (
    <div id="banner">
      {conflicts.map((conflict, index) => (
        <div
          className={`conflict ${conflict.severity === "warning" ? "warn" : ""}`.trimEnd()}
          key={index}
        >
          <span className="tag">{conflict.code}</span> — {conflict.message}
          <div className="ids">bookings: {conflict.bookingIds.join(", ")}</div>
        </div>
      ))}
    </div>
  );
}
