import type { ScheduleDayResponse } from "./types";

// The board's default day. Matches Schedule:Now in appsettings.json — the
// server no longer supplies a default, GET /api/schedule needs an explicit date.
export const PINNED_TODAY = "2026-03-06";

export async function fetchSchedule(date: string): Promise<ScheduleDayResponse> {
  const response = await fetch(`/api/schedule?date=${date || PINNED_TODAY}`);
  if (!response.ok) throw new Error(`schedule request failed: ${response.status}`);
  return (await response.json()) as ScheduleDayResponse;
}
