import { describe, expect, test } from "vitest";
import { formatTime } from "./format";

describe("formatTime", () => {
  test("trims seconds off a HH:mm:ss string", () => {
    expect(formatTime("09:30:00")).toBe("09:30");
  });

  test("returns an empty string for null", () => {
    expect(formatTime(null)).toBe("");
  });

  test("returns an empty string for undefined", () => {
    expect(formatTime(undefined)).toBe("");
  });

  test("returns an empty string for an empty string", () => {
    expect(formatTime("")).toBe("");
  });
});
