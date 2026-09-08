import type { LessonEvent } from "../api/types";
import { fmt } from "../lib/format";

export function Changes({ events, day }: { events: LessonEvent[]; day: string }) {
  if (!events || !events.length) return null;
  return (
    <div id="changes">
      <h2>Changes since 16:00 the day before — tell the tutors</h2>
      {events.map((e, i) => {
        const from = `${e.fromDate === day ? "" : e.fromDate + " "}${fmt(e.fromStartTime)} ${e.fromRoomId}`;
        const to = `${e.toDate === day ? "" : e.toDate + " "}${fmt(e.toStartTime)} ${e.toRoomId}`;
        let detail: string;
        if (e.fromDate === day && e.toDate === day) detail = `re-slotted ${from.trim()} → ${to.trim()}`;
        else if (e.toDate === day) detail = `moved here from ${from.trim()}`;
        else detail = `moved off this day to ${to.trim()}`;
        return (
          <div className="change" key={i}>
            <strong>{e.lessonId}</strong> {detail}
            {e.reason ? ` — ${e.reason}` : ""}
            <div className="when">recorded {e.occurredAt.replace("T", " ").slice(0, 16)}</div>
          </div>
        );
      })}
    </div>
  );
}
