// Mirrors the server DTOs in Application/Contracts (System.Text.Json, camelCase).
// DateOnly -> "2026-03-06", TimeOnly -> "09:00:00", DateTimeOffset -> ISO-8601 with offset.

export type LessonStatus = "Booked" | "Cancelled" | "NoShow";
export type ConflictSeverity = "error" | "warning";

export interface Lesson {
  id: string;
  date: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  studentName: string;
  tutorId: string;
  tutorName: string;
  roomId: string;
  status: LessonStatus;
  groupId: string | null;
  movedAfterCutoff: boolean;
  conflictCodes: string[];
}

export interface RoomSchedule {
  roomId: string;
  roomName: string;
  lessons: Lesson[];
}

export interface TutorLoad {
  tutorId: string;
  tutorName: string;
  lessonCount: number;
  limit: number;
  overLimit: boolean;
}

export interface Conflict {
  code: string;
  severity: ConflictSeverity;
  date: string;
  message: string;
  bookingIds: string[];
  tutorId: string | null;
  roomId: string | null;
  studentName: string | null;
}

export interface ConflictSummary {
  errors: number;
  warnings: number;
}

export interface ConflictsResponse {
  from: string | null;
  to: string | null;
  summary: ConflictSummary;
  conflicts: Conflict[];
}

export interface LessonEvent {
  lessonId: string;
  type: string;
  occurredAt: string;
  fromDate: string | null;
  fromStartTime: string | null;
  fromRoomId: string | null;
  toDate: string | null;
  toStartTime: string | null;
  toRoomId: string | null;
  reason: string | null;
  afterCutoff: boolean;
}

export interface ScheduleDayResponse {
  isMonday: boolean;
  rooms: RoomSchedule[];
  changes: LessonEvent[];
}

export interface TutorLoadsResponse {
  date: string;
  tutorLoads: TutorLoad[];
}
