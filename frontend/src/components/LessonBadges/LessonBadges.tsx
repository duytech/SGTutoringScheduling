import type { Lesson as LessonModel } from "../../api/types";
import styles from "./LessonBadges.module.css";

type LessonBadgesProps = {
  lesson: Pick<LessonModel, "conflictCodes" | "groupId" | "movedAfterCutoff" | "status">;
};

export function LessonBadges({ lesson }: LessonBadgesProps) {
  const badges = [
    ...lesson.conflictCodes.map((code) => ({ className: styles.badge, text: code })),
    ...(lesson.groupId ? [{ className: `${styles.badge} ${styles.pair}`, text: "exam pair" }] : []),
    ...(lesson.movedAfterCutoff ? [{ className: `${styles.badge} ${styles.moved}`, text: "moved late" }] : []),
    ...(lesson.status !== "Booked" ? [{ className: `${styles.badge} ${styles.cancelled}`, text: lesson.status }] : []),
  ];

  if (badges.length === 0) return null;

  return (
    <div className={styles.badges}>
      {badges.map((badge, index) => (
        <span key={index} className={badge.className}>
          {badge.text}
        </span>
      ))}
    </div>
  );
}
