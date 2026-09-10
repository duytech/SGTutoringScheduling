import { Fragment, useEffect, useState } from "react";
import type { TutorLoad } from "../api/types";
import { fetchTutorLoads } from "../api/tutors";

export function Loads({ date }: { date: string }) {
  const [tutorLoads, setTutorLoads] = useState<TutorLoad[]>([]);

  useEffect(() => {
    let cancelled = false;

    fetchTutorLoads(date)
      .then((response) => {
        if (!cancelled) setTutorLoads(response.tutorLoads);
      })
      .catch((reason) => {
        if (!cancelled) console.error(`tutor loads fetch failed for ${date}`, reason);
      });

    return () => {
      cancelled = true;
    };
  }, [date]);

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
