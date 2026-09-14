import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";
import { LessonBadges } from "./LessonBadges";

describe("LessonBadges", () => {
  test("renders nothing when there is nothing to flag", () => {
    const { container } = render(
      <LessonBadges lesson={{ conflictCodes: [], groupId: null, movedAfterCutoff: false, status: "Booked" }} />,
    );

    expect(container).toBeEmptyDOMElement();
  });

  test("renders a badge per conflict code", () => {
    render(
      <LessonBadges
        lesson={{ conflictCodes: ["ROOM_DOUBLE_BOOK", "TUTOR_DOUBLE_BOOK"], groupId: null, movedAfterCutoff: false, status: "Booked" }}
      />,
    );

    expect(screen.getByText("ROOM_DOUBLE_BOOK")).toBeInTheDocument();
    expect(screen.getByText("TUTOR_DOUBLE_BOOK")).toBeInTheDocument();
  });

  test("flags an exam pair", () => {
    render(
      <LessonBadges lesson={{ conflictCodes: [], groupId: "group-1", movedAfterCutoff: false, status: "Booked" }} />,
    );

    expect(screen.getByText("exam pair")).toBeInTheDocument();
  });

  test("flags a lesson moved after the cutoff", () => {
    render(
      <LessonBadges lesson={{ conflictCodes: [], groupId: null, movedAfterCutoff: true, status: "Booked" }} />,
    );

    expect(screen.getByText("moved late")).toBeInTheDocument();
  });

  test("shows the status when it is not Booked", () => {
    render(
      <LessonBadges lesson={{ conflictCodes: [], groupId: null, movedAfterCutoff: false, status: "Cancelled" }} />,
    );

    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });
});
