import type { Conflict } from "../api/types";

export function Banner({ conflicts }: { conflicts: Conflict[] }) {
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
