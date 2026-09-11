import type { Lesson as LessonModel } from "../../api/types";
import { formatTime } from "../../lib/format";
import styles from "./Lesson.module.css";

export function Lesson({ lesson }: { lesson: LessonModel }) {
  const clash = lesson.conflictCodes.length > 0;
  const badges = [
    ...lesson.conflictCodes.map((code) => ({ className: styles.badge, text: code })),
    ...(lesson.groupId ? [{ className: `${styles.badge} ${styles.pair}`, text: "exam pair" }] : []),
    ...(lesson.movedAfterCutoff ? [{ className: `${styles.badge} ${styles.moved}`, text: "moved late" }] : []),
    ...(lesson.status !== "Booked" ? [{ className: `${styles.badge} ${styles.cancelled}`, text: lesson.status }] : []),
  ];
  const rowClassName = clash ? styles.clash : lesson.movedAfterCutoff ? styles.movedLate : "";

  return (
    <div className={`${styles.lesson} ${rowClassName}`.trimEnd()}>
      <span className={styles.time}>
        {formatTime(lesson.startTime)}–{formatTime(lesson.endTime)}
      </span>
      <div className={styles.who}>
        {lesson.studentName} · {lesson.tutorName}
      </div>
      {badges.length > 0 && (
        <div className={styles.badges}>
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
