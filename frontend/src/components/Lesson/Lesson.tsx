import type { Lesson as LessonModel } from "../../api/types";
import { formatTime } from "../../lib/format";
import { LessonBadges } from "../LessonBadges";
import styles from "./Lesson.module.css";

export function Lesson({ lesson }: { lesson: LessonModel }) {
  const clash = lesson.conflictCodes.length > 0;
  const rowClassName = clash ? styles.clash : lesson.movedAfterCutoff ? styles.movedLate : "";

  return (
    <div className={`${styles.lesson} ${rowClassName}`.trimEnd()}>
      <span className={styles.time}>
        {formatTime(lesson.startTime)}–{formatTime(lesson.endTime)}
      </span>
      <div className={styles.who}>
        {lesson.studentName} · {lesson.tutorName}
      </div>
      <LessonBadges lesson={lesson} />
    </div>
  );
}
