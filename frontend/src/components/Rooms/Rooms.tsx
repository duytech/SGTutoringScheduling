import type { RoomSchedule } from "../../api/types";
import { Lesson } from "../Lesson";
import styles from "./Rooms.module.css";

export function Rooms({ rooms }: { rooms: RoomSchedule[] }) {
  return (
    <div className={styles.rooms}>
      {rooms.map((room) => (
        <div className={styles.room} key={room.roomId}>
          <h2>{room.roomName}</h2>
          {room.lessons.length > 0 ? (
            room.lessons.map((lesson) => <Lesson key={lesson.id} lesson={lesson} />)
          ) : (
            <div className={styles.empty}>no lessons</div>
          )}
        </div>
      ))}
    </div>
  );
}
