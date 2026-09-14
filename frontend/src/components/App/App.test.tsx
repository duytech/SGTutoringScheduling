import { describe, expect, test, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { App } from "./App";
import type { ConflictsResponse, ScheduleDayResponse, TutorLoadsResponse } from "../../api/types";

const { scheduleResponse, tutorLoadsResponse, conflictsResponse } = vi.hoisted(() => ({
  scheduleResponse: {
    isMonday: false,
    rooms: [{ roomId: "room-1", roomName: "Room 1", lessons: [] }],
    changes: [],
  } as ScheduleDayResponse,
  tutorLoadsResponse: { date: "2026-03-06", tutorLoads: [] } as TutorLoadsResponse,
  conflictsResponse: {
    from: "2026-03-06",
    to: "2026-03-06",
    summary: { errors: 0, warnings: 0 },
    conflicts: [],
  } as ConflictsResponse,
}));

vi.mock("../../api/schedule", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../api/schedule")>();

  return { ...actual, fetchSchedule: vi.fn().mockResolvedValue(scheduleResponse) };
});
vi.mock("../../api/tutors", () => ({ fetchTutorLoads: vi.fn().mockResolvedValue(tutorLoadsResponse) }));
vi.mock("../../api/conflicts", () => ({ fetchConflicts: vi.fn().mockResolvedValue(conflictsResponse) }));

describe("App", () => {
  test("loads the pinned today's board and renders the rooms", async () => {
    render(<App />);

    await waitFor(() => expect(screen.getByText("Room 1")).toBeInTheDocument());

    expect(screen.getByText("No conflicts for this day.")).toBeInTheDocument();
  });

  test("shows a message when the centre is closed on the loaded day", async () => {
    const { fetchSchedule } = await import("../../api/schedule");
    vi.mocked(fetchSchedule).mockResolvedValueOnce({ ...scheduleResponse, isMonday: true });

    render(<App />);

    await waitFor(() => expect(screen.getByText("centre closed (Monday)")).toBeInTheDocument());
  });

  test("shows a failure message when the board fails to load", async () => {
    const { fetchSchedule } = await import("../../api/schedule");
    vi.mocked(fetchSchedule).mockRejectedValueOnce(new Error("network down"));
    vi.spyOn(console, "error").mockImplementation(() => {});

    render(<App />);

    await waitFor(() => expect(screen.getByText("failed to load")).toBeInTheDocument());
  });
});
