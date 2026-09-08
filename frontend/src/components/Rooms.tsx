import type { RoomSchedule } from "../api/types";
import { Lesson } from "./Lesson";

export function Rooms({ rooms }: { rooms: RoomSchedule[] }) {
  return (
    <div className="rooms" id="rooms">
      {rooms.map((room) => (
        <div className="room" key={room.roomId}>
          <h2>{room.roomName}</h2>
          {room.lessons.length > 0 ? (
            room.lessons.map((lesson) => <Lesson key={lesson.id} lesson={lesson} />)
          ) : (
            <div className="empty">no lessons</div>
          )}
        </div>
      ))}
    </div>
  );
}
