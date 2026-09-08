import { Fragment } from "react";
import type { TutorLoad } from "../api/types";

export function Loads({ tutorLoads }: { tutorLoads: TutorLoad[] }) {
  if (!tutorLoads.length) return <div className="loads" id="loads" />;
  return (
    <div className="loads" id="loads">
      Tutor load today:{" "}
      {tutorLoads.map((t, i) => (
        <Fragment key={t.tutorId}>
          {i > 0 && " · "}
          <span className={t.overLimit ? "over" : undefined}>
            {t.tutorName} {t.lessonCount}/{t.limit}
          </span>
        </Fragment>
      ))}
    </div>
  );
}
