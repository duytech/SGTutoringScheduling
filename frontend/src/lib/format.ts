// "HH:mm:ss" -> "HH:mm". React escapes text nodes, so the old escapeHtml() is gone.
export function fmt(t: string | null | undefined): string {
  return (t || "").slice(0, 5);
}
