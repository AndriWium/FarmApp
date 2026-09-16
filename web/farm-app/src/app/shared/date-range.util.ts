// Small date-range helpers shared by every report screen with a from/to filter (income statement,
// sales analysis, stock movement summary, till sessions, cash flow) - all default to "the current
// month", per the task brief's instruction for the income statement, extended to every other
// date-ranged report for a consistent first-load experience.
//
// Every function here works in LOCAL calendar dates throughout - never `Date#toISOString()` and
// never `new Date(dateOnlyString)`. Both silently operate in UTC: `toISOString()` converts a local
// Date back to UTC before formatting, and the one-argument `Date` constructor parses a bare
// "yyyy-MM-dd" string as UTC midnight. Either one, mixed with local-time construction like
// `new Date(year, month, day)`, shifts the displayed date by the local UTC offset - caught live in
// this phase's own browser verification: South Africa is UTC+2, so a naive `currentMonthRange()`
// showed "2026-08-31" to "2026-09-29" instead of "2026-09-01" to "2026-09-30" for the current
// month. `parseLocalDate`/`formatLocalDate` below are the two chokepoints that keep every
// conversion in local time consistently.

function pad2(n: number): number | string {
  return n < 10 ? `0${n}` : n;
}

/** Formats a Date using its LOCAL year/month/day fields - never toISOString() (see file comment). */
export function formatLocalDate(d: Date): string {
  return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;
}

/** Parses a "yyyy-MM-dd" string as a local-midnight Date - never `new Date(str)` (see file comment). */
export function parseLocalDate(dateStr: string): Date {
  const [year, month, day] = dateStr.split('-').map(Number);
  return new Date(year, month - 1, day);
}

/** yyyy-MM-dd for today, local time (matches season-page.component.ts's own todayIso() helper). */
export function todayIso(): string {
  return formatLocalDate(new Date());
}

/** [first day, last day] of the current calendar month, as yyyy-MM-dd strings. */
export function currentMonthRange(): { from: string; to: string } {
  const now = new Date();
  const from = new Date(now.getFullYear(), now.getMonth(), 1);
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0);
  return { from: formatLocalDate(from), to: formatLocalDate(to) };
}

/**
 * The period of the same length immediately preceding [from, to] (inclusive on both ends), e.g.
 * for a calendar month this is "last month" - used by Sales Analysis's "avg price/kg vs last
 * period" spot-price-drift comparison (doc 04 §2).
 */
export function precedingPeriod(from: string, to: string): { from: string; to: string } {
  const fromDate = parseLocalDate(from);
  const toDate = parseLocalDate(to);
  const lengthDays = Math.round((toDate.getTime() - fromDate.getTime()) / 86400000);
  const prevTo = new Date(fromDate.getFullYear(), fromDate.getMonth(), fromDate.getDate() - 1);
  const prevFrom = new Date(prevTo.getFullYear(), prevTo.getMonth(), prevTo.getDate() - lengthDays);
  return { from: formatLocalDate(prevFrom), to: formatLocalDate(prevTo) };
}
