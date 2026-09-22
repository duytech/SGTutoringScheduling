import { afterEach, describe, expect, test, vi } from "vitest";
import { fetchTutorLoads } from "./tutors";
import { PINNED_TODAY } from "./schedule";
import type { TutorLoadsResponse } from "./types";

afterEach(() => {
  vi.unstubAllGlobals();
});

function stubFetch(responseInit: { ok: boolean; status?: number; data?: unknown; error?: { code: string; message: string } }) {
  const fetchMock = vi.fn().mockResolvedValue({
    ok: responseInit.ok,
    status: responseInit.status ?? 200,
    json: () => Promise.resolve({ data: responseInit.data ?? null, error: responseInit.error ?? null }),
  });

  vi.stubGlobal("fetch", fetchMock);

  return fetchMock;
}

describe("fetchTutorLoads", () => {
  test("requests the given date", async () => {
    const fetchMock = stubFetch({ ok: true, data: { date: "2026-03-05", tutorLoads: [] } });

    await fetchTutorLoads("2026-03-05");

    expect(fetchMock).toHaveBeenCalledWith("/api/tutors/loads?date=2026-03-05");
  });

  test("falls back to the pinned today when no date is given", async () => {
    const fetchMock = stubFetch({ ok: true, data: { date: PINNED_TODAY, tutorLoads: [] } });

    await fetchTutorLoads("");

    expect(fetchMock).toHaveBeenCalledWith(`/api/tutors/loads?date=${PINNED_TODAY}`);
  });

  test("resolves with the parsed response body", async () => {
    const body: TutorLoadsResponse = { date: "2026-03-05", tutorLoads: [] };
    stubFetch({ ok: true, data: body });

    await expect(fetchTutorLoads("2026-03-05")).resolves.toEqual(body);
  });

  test("throws with the server's error message when the response is an error", async () => {
    stubFetch({ ok: false, status: 404, error: { code: "tutor_loads_failure", message: "boom" } });

    await expect(fetchTutorLoads("2026-03-05")).rejects.toThrow("boom");
  });
});
