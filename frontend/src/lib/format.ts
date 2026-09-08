// "HH:mm:ss" -> "HH:mm". React escapes text nodes, so the old escapeHtml() is gone.
export function formatTime(time: string | null | undefined): string {
  return (time || "").slice(0, 5);
}
