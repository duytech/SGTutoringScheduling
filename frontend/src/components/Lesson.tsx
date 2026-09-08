import type { Lesson as LessonModel } from "../api/types";
import { formatTime } from "../lib/format";

export function Lesson({ lesson }: { lesson: LessonModel }) {
  const clash = lesson.conflictCodes.length > 0;
  const badges = [
    ...lesson.conflictCodes.map((code) => ({ className: "badge", text: code })),
    ...(lesson.groupId ? [{ className: "badge pair", text: "exam pair" }] : []),
    ...(lesson.movedAfterCutoff ? [{ className: "badge moved", text: "moved late" }] : []),
    ...(lesson.status !== "Booked" ? [{ className: "badge cancelled", text: lesson.status }] : []),
  ];
  const rowClassName = clash ? "clash" : lesson.movedAfterCutoff ? "moved-late" : "";

  return (
    <div className={`lesson ${rowClassName}`.trimEnd()}>
      <span className="time">
        {formatTime(lesson.startTime)}–{formatTime(lesson.endTime)}
      </span>
      <div className="who">
        {lesson.studentName} · {lesson.tutorName}
      </div>
      {badges.length > 0 && (
        <div className="badges">
          {badges.map((badge, index) => (
            <span key={index} className={badge.className}>
              {badge.text}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
