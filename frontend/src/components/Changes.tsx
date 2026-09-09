import type { LessonEvent } from "../api/types";
import { formatTime } from "../lib/format";

export function Changes({ events, day }: { events: LessonEvent[]; day: string }) {
  if (!events || !events.length) return null;

  return (
    <div id="changes">
      <h2>Changes since 16:00 the day before — tell the tutors</h2>
      {events.map((event, index) => {
        const from = `${event.fromDate === day ? "" : event.fromDate + " "}${formatTime(event.fromStartTime)} ${event.fromRoomId}`;
        const to = `${event.toDate === day ? "" : event.toDate + " "}${formatTime(event.toStartTime)} ${event.toRoomId}`;
        let detail: string;
        if (event.fromDate === day && event.toDate === day) detail = `re-slotted ${from.trim()} → ${to.trim()}`;
        else if (event.toDate === day) detail = `moved here from ${from.trim()}`;
        else detail = `moved off this day to ${to.trim()}`;

        return (
          <div className="change" key={index}>
            <strong>{event.lessonId}</strong> {detail}
            {event.reason ? ` — ${event.reason}` : ""}
            <div className="when">recorded {event.occurredAt.replace("T", " ").slice(0, 16)}</div>
          </div>
        );
      })}
    </div>
  );
}
