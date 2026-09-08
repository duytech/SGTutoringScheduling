// "HH:mm:ss" -> "HH:mm". React escapes text nodes, so the old escapeHtml() is gone.
export function fmt(time: string | null | undefined): string {
  return (time || "").slice(0, 5);
}
