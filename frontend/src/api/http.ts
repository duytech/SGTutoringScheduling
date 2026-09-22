import type { ApiEnvelope } from "./types";

export class ApiError extends Error {
  constructor(
    public readonly code: string,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

// Fetches `url`, unwraps the shared { data, error } envelope, and throws
// ApiError for either a non-2xx response or an error-shaped body.
export async function fetchEnvelope<T>(url: string): Promise<T> {
  const response = await fetch(url);
  const body = (await response.json()) as ApiEnvelope<T>;

  if (body.error) {
    throw new ApiError(body.error.code, body.error.message);
  }

  if (!response.ok || body.data === null) {
    throw new ApiError("request_failed", `request to ${url} failed: ${response.status}`);
  }

  return body.data;
}
