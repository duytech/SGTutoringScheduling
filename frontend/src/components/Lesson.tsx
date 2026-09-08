import type { Lesson as LessonModel } from "../api/types";
import { fmt } from "../lib/format";

export function Lesson({ lesson }: { lesson: LessonModel }) {
  const clash = lesson.conflictCodes.length > 0;
  const badges = [
    ...lesson.conflictCodes.map((code) => ({ cls: "badge", text: code })),
    ...(lesson.groupId ? [{ cls: "badge pair", text: "exam pair" }] : []),
    ...(lesson.movedAfterCutoff ? [{ cls: "badge moved", text: "moved late" }] : []),
    ...(lesson.status !== "Booked" ? [{ cls: "badge cancelled", text: lesson.status }] : []),
  ];
  const cls = clash ? "clash" : lesson.movedAfterCutoff ? "moved-late" : "";

  return (
    <div className={`lesson ${cls}`.trimEnd()}>
      <span className="time">
        {fmt(lesson.startTime)}–{fmt(lesson.endTime)}
      </span>
      <div className="who">
        {lesson.studentName} · {lesson.tutorName}
      </div>
      {badges.length > 0 && (
        <div className="badges">
          {badges.map((badge, index) => (
            <span key={index} className={badge.cls}>
              {badge.text}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
