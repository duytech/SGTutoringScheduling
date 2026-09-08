import { Fragment } from "react";
import type { TutorLoad } from "../api/types";

export function Loads({ tutorLoads }: { tutorLoads: TutorLoad[] }) {
  if (!tutorLoads.length) return <div className="loads" id="loads" />;
  return (
    <div className="loads" id="loads">
      Tutor load today:{" "}
      {tutorLoads.map((tutor, index) => (
        <Fragment key={tutor.tutorId}>
          {index > 0 && " · "}
          <span className={tutor.overLimit ? "over" : undefined}>
            {tutor.tutorName} {tutor.lessonCount}/{tutor.limit}
          </span>
        </Fragment>
      ))}
    </div>
  );
}
