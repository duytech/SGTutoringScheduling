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
      {conflicts.map((c, i) => (
        <div className={`conflict ${c.severity === "warning" ? "warn" : ""}`.trimEnd()} key={i}>
          <span className="tag">{c.code}</span> — {c.message}
          <div className="ids">bookings: {c.bookingIds.join(", ")}</div>
        </div>
      ))}
    </div>
  );
}
