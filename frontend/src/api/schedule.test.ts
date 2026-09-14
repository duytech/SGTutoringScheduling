import { afterEach, describe, expect, test, vi } from "vitest";
import { fetchSchedule, PINNED_TODAY } from "./schedule";
import type { ScheduleDayResponse } from "./types";

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

describe("fetchSchedule", () => {
  test("requests the given date", async () => {
    const fetchMock = stubFetch({ ok: true, body: { rooms: [] } as unknown as ScheduleDayResponse });

    await fetchSchedule("2026-03-05");

    expect(fetchMock).toHaveBeenCalledWith("/api/schedule?date=2026-03-05");
  });

  test("falls back to the pinned today when no date is given", async () => {
    const fetchMock = stubFetch({ ok: true, body: { rooms: [] } as unknown as ScheduleDayResponse });

    await fetchSchedule("");

    expect(fetchMock).toHaveBeenCalledWith(`/api/schedule?date=${PINNED_TODAY}`);
  });

  test("resolves with the parsed response body", async () => {
    const body: ScheduleDayResponse = { isMonday: false, rooms: [], changes: [] };
    stubFetch({ ok: true, body });

    await expect(fetchSchedule("2026-03-05")).resolves.toEqual(body);
  });

  test("throws when the response is not ok", async () => {
    stubFetch({ ok: false, status: 500 });

    await expect(fetchSchedule("2026-03-05")).rejects.toThrow("schedule request failed: 500");
  });
});
