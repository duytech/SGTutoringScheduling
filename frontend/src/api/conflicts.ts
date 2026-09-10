import type { ConflictsResponse } from "./types";
import { PINNED_TODAY } from "./schedule";

// Conflicts for one day. Split from the schedule board: /api/conflicts is its
// own read view, so the board response no longer carries a conflicts list.
export async function fetchConflicts(date: string): Promise<ConflictsResponse> {
  const day = date || PINNED_TODAY;
  const response = await fetch(`/api/conflicts?from=${day}&to=${day}`);
  if (!response.ok) throw new Error(`conflicts request failed: ${response.status}`);

  return (await response.json()) as ConflictsResponse;
}
