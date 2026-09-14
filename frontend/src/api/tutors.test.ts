import { afterEach, describe, expect, test, vi } from "vitest";
import { fetchTutorLoads } from "./tutors";
import { PINNED_TODAY } from "./schedule";
import type { TutorLoadsResponse } from "./types";

afterEach(() => {
  vi.unstubAllGlobals();
});

function stubFetch(responseInit: { ok: boolean; status?: number; body?: unknown }) {
  const fetchMock = vi.fn().mockResolvedValue({
    ok: responseInit.ok,
    status: responseInit.status ?? 200,
    json: () => Promise.resolve(responseInit.body),
  });

  vi.stubGlobal("fetch", fetchMock);

  return fetchMock;
}

describe("fetchTutorLoads", () => {
  test("requests the given date", async () => {
    const fetchMock = stubFetch({ ok: true, body: { date: "2026-03-05", tutorLoads: [] } });

    await fetchTutorLoads("2026-03-05");

    expect(fetchMock).toHaveBeenCalledWith("/api/tutors/loads?date=2026-03-05");
  });

  test("falls back to the pinned today when no date is given", async () => {
    const fetchMock = stubFetch({ ok: true, body: { date: PINNED_TODAY, tutorLoads: [] } });

    await fetchTutorLoads("");

    expect(fetchMock).toHaveBeenCalledWith(`/api/tutors/loads?date=${PINNED_TODAY}`);
  });

  test("resolves with the parsed response body", async () => {
    const body: TutorLoadsResponse = { date: "2026-03-05", tutorLoads: [] };
    stubFetch({ ok: true, body });

    await expect(fetchTutorLoads("2026-03-05")).resolves.toEqual(body);
  });

  test("throws when the response is not ok", async () => {
    stubFetch({ ok: false, status: 404 });

    await expect(fetchTutorLoads("2026-03-05")).rejects.toThrow("tutor loads request failed: 404");
  });
});
