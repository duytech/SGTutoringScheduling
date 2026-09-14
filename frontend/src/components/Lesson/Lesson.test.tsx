import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";
import { Lesson } from "./Lesson";
import type { Lesson as LessonModel } from "../../api/types";

const baseLesson: LessonModel = {
  id: "lesson-1",
  date: "2026-03-05",
  startTime: "09:00:00",
  endTime: "10:00:00",
  durationMinutes: 60,
  studentName: "An Nguyen",
  tutorId: "tutor-1",
  tutorName: "Ms. Linh",
  roomId: "room-1",
  status: "Booked",
  groupId: null,
  movedAfterCutoff: false,
  conflictCodes: [],
};

describe("Lesson", () => {
  test("shows the formatted time range, student and tutor", () => {
    render(<Lesson lesson={baseLesson} />);

    expect(screen.getByText("09:00–10:00")).toBeInTheDocument();
    expect(screen.getByText("An Nguyen · Ms. Linh")).toBeInTheDocument();
  });

  test("renders badges for a lesson with conflicts", () => {
    render(<Lesson lesson={{ ...baseLesson, conflictCodes: ["ROOM_DOUBLE_BOOK"] }} />);

    expect(screen.getByText("ROOM_DOUBLE_BOOK")).toBeInTheDocument();
  });
});
