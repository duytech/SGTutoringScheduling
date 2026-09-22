import { fetchEnvelope } from "./http";
import type { TutorLoadsResponse } from "./types";
import { PINNED_TODAY } from "./schedule";

// Tutor workload for one day. Split from the schedule board: /api/tutors/loads
// is its own read view, so the board no longer carries a tutorLoads list.
export async function fetchTutorLoads(date: string): Promise<TutorLoadsResponse> {
  return fetchEnvelope<TutorLoadsResponse>(`/api/tutors/loads?date=${date || PINNED_TODAY}`);
}
