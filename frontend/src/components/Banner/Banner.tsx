import type { Conflict } from "../../api/types";
import styles from "./Banner.module.css";

export function Banner({ conflicts }: { conflicts: Conflict[] }) {
  if (!conflicts.length) {
    return (
      <div className={styles.banner}>
        <div className={styles.conflict} style={{ borderLeftColor: "#1a7f37", background: "#e9f6ec" }}>
          <span className={styles.ok}>No conflicts for this day.</span>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.banner}>
      {conflicts.map((conflict, index) => (
        <div
          className={`${styles.conflict} ${conflict.severity === "warning" ? styles.warn : ""}`.trimEnd()}
          key={index}
        >
          <span className={styles.tag}>{conflict.code}</span> — {conflict.message}
          <div className={styles.ids}>bookings: {conflict.bookingIds.join(", ")}</div>
        </div>
      ))}
    </div>
  );
}
