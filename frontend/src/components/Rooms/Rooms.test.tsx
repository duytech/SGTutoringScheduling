import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";
import { Rooms } from "./Rooms";
import type { RoomSchedule } from "../../api/types";

describe("Rooms", () => {
  test("shows a room name and its lessons", () => {
    const rooms: RoomSchedule[] = [
      {
        roomId: "room-1",
        roomName: "Room 1",
        lessons: [
          {
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
          },
        ],
      },
    ];

    render(<Rooms rooms={rooms} />);

    expect(screen.getByText("Room 1")).toBeInTheDocument();
    expect(screen.getByText("An Nguyen · Ms. Linh")).toBeInTheDocument();
  });

  test("shows an empty-state message for a room with no lessons", () => {
    const rooms: RoomSchedule[] = [{ roomId: "room-2", roomName: "Room 2", lessons: [] }];

    render(<Rooms rooms={rooms} />);

    expect(screen.getByText("no lessons")).toBeInTheDocument();
  });
});
