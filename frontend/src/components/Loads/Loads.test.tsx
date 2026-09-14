import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";
import { Loads } from "./Loads";
import type { TutorLoad } from "../../api/types";

describe("Loads", () => {
  test("renders nothing but an empty container when there are no tutor loads", () => {
    const { container } = render(<Loads tutorLoads={[]} />);

    expect(container.firstChild).toBeEmptyDOMElement();
  });

  test("shows each tutor's lesson count against their limit", () => {
    const tutorLoads: TutorLoad[] = [
      { tutorId: "tutor-1", tutorName: "Ms. Linh", lessonCount: 5, limit: 6, overLimit: false },
      { tutorId: "tutor-2", tutorName: "Mr. Tuan", lessonCount: 7, limit: 6, overLimit: true },
    ];

    render(<Loads tutorLoads={tutorLoads} />);

    expect(screen.getByText(/Ms\. Linh 5\/6/)).toBeInTheDocument();
    expect(screen.getByText(/Mr\. Tuan 7\/6/)).toBeInTheDocument();
  });

  test("flags a tutor who is over their daily limit", () => {
    const tutorLoads: TutorLoad[] = [
      { tutorId: "tutor-2", tutorName: "Mr. Tuan", lessonCount: 7, limit: 6, overLimit: true },
    ];

    render(<Loads tutorLoads={tutorLoads} />);

    expect(screen.getByText(/Mr\. Tuan 7\/6/).className).toMatch(/_over_/);
  });
});
