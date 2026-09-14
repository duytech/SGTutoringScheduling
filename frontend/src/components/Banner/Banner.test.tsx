import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";
import { Banner } from "./Banner";
import type { Conflict } from "../../api/types";

describe("Banner", () => {
  test("shows a no-conflicts message when there are none", () => {
    render(<Banner conflicts={[]} />);

    expect(screen.getByText("No conflicts for this day.")).toBeInTheDocument();
  });

  test("lists each conflict's code, message and booking ids", () => {
    const conflicts: Conflict[] = [
      {
        code: "ROOM_DOUBLE_BOOK",
        severity: "error",
        date: "2026-03-05",
        message: "Room 1 is double-booked",
        bookingIds: ["lesson-1", "lesson-2"],
        tutorId: null,
        roomId: "room-1",
        studentName: null,
      },
    ];

    render(<Banner conflicts={conflicts} />);

    expect(screen.getByText("ROOM_DOUBLE_BOOK")).toBeInTheDocument();
    expect(screen.getByText(/Room 1 is double-booked/)).toBeInTheDocument();
    expect(screen.getByText(/lesson-1, lesson-2/)).toBeInTheDocument();
  });
});
