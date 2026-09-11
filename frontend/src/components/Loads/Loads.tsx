import { Fragment } from "react";
import type { TutorLoad } from "../../api/types";
import styles from "./Loads.module.css";

export function Loads({ tutorLoads }: { tutorLoads: TutorLoad[] }) {
  if (!tutorLoads.length) return <div className={styles.loads} />;

  return (
    <div className={styles.loads}>
      Tutor load today:{" "}
      {tutorLoads.map((tutor, index) => (
        <Fragment key={tutor.tutorId}>
          {index > 0 && " · "}
          <span className={tutor.overLimit ? styles.over : undefined}>
            {tutor.tutorName} {tutor.lessonCount}/{tutor.limit}
          </span>
        </Fragment>
      ))}
    </div>
  );
}
