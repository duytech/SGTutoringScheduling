import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";
import { Changes } from "./Changes";
import type { LessonEvent } from "../../api/types";

describe("Changes", () => {
  test("renders nothing when there are no events", () => {
    const { container } = render(<Changes events={[]} day="2026-03-05" />);

    expect(container).toBeEmptyDOMElement();
  });

  test("describes a lesson re-slotted within the same day", () => {
    const events: LessonEvent[] = [
      {
        lessonId: "lesson-1",
        type: "Rescheduled",
        occurredAt: "2026-03-04T15:30:00+07:00",
        fromDate: "2026-03-05",
        fromStartTime: "09:00:00",
        fromRoomId: "room-1",
        toDate: "2026-03-05",
        toStartTime: "10:00:00",
        toRoomId: "room-2",
        reason: "family request",
        afterCutoff: true,
      },
    ];

    render(<Changes events={events} day="2026-03-05" />);

    expect(screen.getByText("lesson-1")).toBeInTheDocument();
    expect(screen.getByText(/re-slotted 09:00 room-1 → 10:00 room-2/)).toBeInTheDocument();
    expect(screen.getByText(/family request/)).toBeInTheDocument();
  });

  test("describes a lesson moved onto this day from another day", () => {
    const events: LessonEvent[] = [
      {
        lessonId: "lesson-2",
        type: "Rescheduled",
        occurredAt: "2026-03-04T15:30:00+07:00",
        fromDate: "2026-03-04",
        fromStartTime: "09:00:00",
        fromRoomId: "room-1",
        toDate: "2026-03-05",
        toStartTime: "09:00:00",
        toRoomId: "room-1",
        reason: null,
        afterCutoff: true,
      },
    ];

    render(<Changes events={events} day="2026-03-05" />);

    expect(screen.getByText(/moved here from 2026-03-04 09:00 room-1/)).toBeInTheDocument();
  });

  test("describes a lesson moved off this day", () => {
    const events: LessonEvent[] = [
      {
        lessonId: "lesson-3",
        type: "Rescheduled",
        occurredAt: "2026-03-04T15:30:00+07:00",
        fromDate: "2026-03-05",
        fromStartTime: "09:00:00",
        fromRoomId: "room-1",
        toDate: "2026-03-06",
        toStartTime: "09:00:00",
        toRoomId: "room-1",
        reason: null,
        afterCutoff: true,
      },
    ];

    render(<Changes events={events} day="2026-03-05" />);

    expect(screen.getByText(/moved off this day to 2026-03-06 09:00 room-1/)).toBeInTheDocument();
  });
});
