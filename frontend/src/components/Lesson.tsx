import type { Lesson as LessonModel } from "../api/types";
import { fmt } from "../lib/format";

export function Lesson({ l }: { l: LessonModel }) {
  const clash = l.conflictCodes.length > 0;
  const badges = [
    ...l.conflictCodes.map((c) => ({ cls: "badge", text: c })),
    ...(l.groupId ? [{ cls: "badge pair", text: "exam pair" }] : []),
    ...(l.movedAfterCutoff ? [{ cls: "badge moved", text: "moved late" }] : []),
    ...(l.status !== "Booked" ? [{ cls: "badge cancelled", text: l.status }] : []),
  ];
  const cls = clash ? "clash" : l.movedAfterCutoff ? "moved-late" : "";

  return (
    <div className={`lesson ${cls}`.trimEnd()}>
      <span className="time">
        {fmt(l.startTime)}–{fmt(l.endTime)}
      </span>
      <div className="who">
        {l.studentName} · {l.tutorName}
      </div>
      {badges.length > 0 && (
        <div className="badges">
          {badges.map((b, i) => (
            <span key={i} className={b.cls}>
              {b.text}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
