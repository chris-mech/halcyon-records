import { afterEach, beforeEach, describe, expect, test, vi } from "vitest";
import { act, render, screen } from "@testing-library/react";

import { ColdStartOverlay, pickNextMessageIndex } from "./cold-start-overlay";
import { LoadingState } from "./loading-state";

const ESCALATION_DELAY_MS = 6000;
const ROTATION_INTERVAL_MS = 10000;
const FADE_DURATION_MS = 400;

async function flushMicrotasks() {
  await act(async () => {
    await Promise.resolve();
  });
}

describe("ColdStartOverlay", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    document.body.innerHTML = "";
  });

  test("stays hidden when no skeleton is on the page", async () => {
    render(<ColdStartOverlay />);
    await flushMicrotasks();

    act(() => {
      vi.advanceTimersByTime(ESCALATION_DELAY_MS + 1000);
    });

    expect(screen.queryByText(/scale to zero/i)).not.toBeInTheDocument();
  });

  test("shows the overlay once a skeleton has been present past the escalation delay", async () => {
    render(
      <>
        <ColdStartOverlay />
        <LoadingState>
          <span />
        </LoadingState>
      </>,
    );
    await flushMicrotasks();

    act(() => {
      vi.advanceTimersByTime(ESCALATION_DELAY_MS);
    });

    expect(screen.getByText(/scale to zero/i)).toBeInTheDocument();
  });

  test("never shows if the skeleton resolves before the escalation delay", async () => {
    const { rerender } = render(
      <>
        <ColdStartOverlay />
        <LoadingState>
          <span />
        </LoadingState>
      </>,
    );
    await flushMicrotasks();

    rerender(
      <>
        <ColdStartOverlay />
        <div>Resolved content</div>
      </>,
    );
    await flushMicrotasks();

    act(() => {
      vi.advanceTimersByTime(ESCALATION_DELAY_MS);
    });

    expect(screen.queryByText(/scale to zero/i)).not.toBeInTheDocument();
  });

  test("clears the overlay as soon as the skeleton is replaced by real content", async () => {
    const { rerender } = render(
      <>
        <ColdStartOverlay />
        <LoadingState>
          <span />
        </LoadingState>
      </>,
    );
    await flushMicrotasks();

    act(() => {
      vi.advanceTimersByTime(ESCALATION_DELAY_MS);
    });
    expect(screen.getByText(/scale to zero/i)).toBeInTheDocument();

    rerender(
      <>
        <ColdStartOverlay />
        <div>Resolved content</div>
      </>,
    );
    await flushMicrotasks();

    expect(screen.queryByText(/scale to zero/i)).not.toBeInTheDocument();
  });

  test("applies inert to #app-content while visible, and clears it once resolved", async () => {
    document.body.innerHTML = '<div id="app-content"></div>';
    const appContent = document.getElementById("app-content")!;

    const { rerender } = render(
      <>
        <ColdStartOverlay />
        <LoadingState>
          <span />
        </LoadingState>
      </>,
    );
    await flushMicrotasks();

    act(() => {
      vi.advanceTimersByTime(ESCALATION_DELAY_MS);
    });
    expect(appContent).toHaveAttribute("inert");

    rerender(
      <>
        <ColdStartOverlay />
        <div>Resolved content</div>
      </>,
    );
    await flushMicrotasks();

    expect(appContent).not.toHaveAttribute("inert");
  });

  test("rotates to a different trivia line after its turn, without changing the fixed message", async () => {
    render(
      <>
        <ColdStartOverlay />
        <LoadingState>
          <span />
        </LoadingState>
      </>,
    );
    await flushMicrotasks();

    act(() => {
      vi.advanceTimersByTime(ESCALATION_DELAY_MS);
    });
    const firstTrivia = screen.getByTestId("cold-start-trivia").textContent;

    act(() => {
      vi.advanceTimersByTime(ROTATION_INTERVAL_MS + FADE_DURATION_MS);
    });
    const secondTrivia = screen.getByTestId("cold-start-trivia").textContent;

    expect(secondTrivia).not.toBe(firstTrivia);
    expect(screen.getByText(/scale to zero/i)).toBeInTheDocument();
  });
});

describe("pickNextMessageIndex", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  test("returns 0 for a single-entry pool instead of looping forever", () => {
    expect(pickNextMessageIndex(0, 1)).toBe(0);
  });

  test("retries until it finds an index different from the current one", () => {
    vi.spyOn(Math, "random").mockReturnValueOnce(0).mockReturnValueOnce(0.5);

    expect(pickNextMessageIndex(0, 3)).toBe(1);
    expect(Math.random).toHaveBeenCalledTimes(2);
  });
});
