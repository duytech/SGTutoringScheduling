import { afterEach, describe, expect, test, vi } from "vitest";
import { fetchConflicts } from "./conflicts";
import { PINNED_TODAY } from "./schedule";
import type { ConflictsResponse } from "./types";

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

describe("fetchConflicts", () => {
  test("requests the given date as both from and to", async () => {
    const fetchMock = stubFetch({
      ok: true,
      data: { from: "2026-03-05", to: "2026-03-05", summary: { errors: 0, warnings: 0 }, conflicts: [] },
    });

    await fetchConflicts("2026-03-05");

    expect(fetchMock).toHaveBeenCalledWith("/api/conflicts?from=2026-03-05&to=2026-03-05");
  });

  test("falls back to the pinned today when no date is given", async () => {
    const fetchMock = stubFetch({
      ok: true,
      data: { from: PINNED_TODAY, to: PINNED_TODAY, summary: { errors: 0, warnings: 0 }, conflicts: [] },
    });

    await fetchConflicts("");

    expect(fetchMock).toHaveBeenCalledWith(`/api/conflicts?from=${PINNED_TODAY}&to=${PINNED_TODAY}`);
  });

  test("resolves with the parsed response body", async () => {
    const body: ConflictsResponse = {
      from: "2026-03-05",
      to: "2026-03-05",
      summary: { errors: 0, warnings: 0 },
      conflicts: [],
    };
    stubFetch({ ok: true, data: body });

    await expect(fetchConflicts("2026-03-05")).resolves.toEqual(body);
  });

  test("throws with the server's error message when the response is an error", async () => {
    stubFetch({ ok: false, status: 500, error: { code: "conflicts_failure", message: "boom" } });

    await expect(fetchConflicts("2026-03-05")).rejects.toThrow("boom");
  });
});
