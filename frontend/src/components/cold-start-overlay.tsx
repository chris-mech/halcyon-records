"use client";

import { useEffect, useState } from "react";

import { cn } from "@/lib/utils";

const ESCALATION_DELAY_MS = 6000;
const ROTATION_INTERVAL_MS = 10000;
const FADE_DURATION_MS = 400;
const SKELETON_SELECTOR = "[data-cold-start-skeleton]";
const APP_CONTENT_ID = "app-content";

const HEADING =
  "Still loading, and thank you so much for taking a look at my store.";
const SUBHEADING =
  "Azure SQL, Meilisearch, and the API all scale to zero when idle, so the first request wakes the whole stack back up.";

const TECH_MESSAGES = [
  "Built with .NET 10 and vertical slice architecture: each feature owns its own path straight through to the database.",
  "Search runs on Meilisearch, self-hosted and typo-tolerant.",
  "The whole stack is orchestrated locally with .NET Aspire: API, database, search, and frontend, one command.",
  "Frontend is Next.js 16 with React Server Components and partial prerendering.",
  "Infrastructure as code with Azure Bicep, deployed through a GitHub Actions CI/CD pipeline on every merge.",
  "Type-safe error handling throughout, using the Result pattern instead of exceptions for expected failures.",
  "The frontend's API client is generated straight from the backend's OpenAPI spec: one contract, no drift between the two.",
];

function pickNextMessageIndex(currentIndex: number, poolSize: number) {
  if (poolSize <= 1) {
    return 0;
  }

  let next = currentIndex;
  while (next === currentIndex) {
    next = Math.floor(Math.random() * poolSize);
  }
  return next;
}

function ColdStartOverlay() {
  const [visible, setVisible] = useState(false);
  const [triviaIndex, setTriviaIndex] = useState(-1);
  const [triviaVisible, setTriviaVisible] = useState(true);

  useEffect(() => {
    let escalateTimeout: ReturnType<typeof setTimeout> | undefined;

    function sync() {
      const present = document.querySelector(SKELETON_SELECTOR) !== null;

      if (present) {
        if (!escalateTimeout) {
          escalateTimeout = setTimeout(() => {
            setTriviaIndex((current) =>
              pickNextMessageIndex(current, TECH_MESSAGES.length),
            );
            setVisible(true);
          }, ESCALATION_DELAY_MS);
        }
      } else {
        if (escalateTimeout) {
          clearTimeout(escalateTimeout);
          escalateTimeout = undefined;
        }
        setVisible(false);
      }
    }

    const observer = new MutationObserver(sync);
    observer.observe(document.body, { childList: true, subtree: true });
    sync();

    return () => {
      observer.disconnect();
      if (escalateTimeout) {
        clearTimeout(escalateTimeout);
      }
    };
  }, []);

  useEffect(() => {
    document.getElementById(APP_CONTENT_ID)?.toggleAttribute("inert", visible);
  }, [visible]);

  useEffect(() => {
    if (!visible) {
      return;
    }

    let rotateTimeout: ReturnType<typeof setTimeout>;
    let fadeTimeout: ReturnType<typeof setTimeout>;

    function scheduleNextRotation() {
      rotateTimeout = setTimeout(() => {
        setTriviaVisible(false);
        fadeTimeout = setTimeout(() => {
          setTriviaIndex((current) =>
            pickNextMessageIndex(current, TECH_MESSAGES.length),
          );
          setTriviaVisible(true);
          scheduleNextRotation();
        }, FADE_DURATION_MS);
      }, ROTATION_INTERVAL_MS);
    }

    scheduleNextRotation();

    return () => {
      clearTimeout(rotateTimeout);
      clearTimeout(fadeTimeout);
    };
  }, [visible]);

  if (!visible) {
    return null;
  }

  return (
    <div
      role="status"
      aria-live="polite"
      className="fixed inset-0 z-50 flex flex-col items-center justify-center gap-8 bg-ink/80 px-8 text-center"
    >
      <div className="max-w-xl">
        <h2 className="text-2xl font-medium text-paper">{HEADING}</h2>
        <p className="mt-3 text-base text-paper/90">{SUBHEADING}</p>
      </div>
      <p
        aria-hidden="true"
        data-testid="cold-start-trivia"
        className={cn(
          "min-h-10 max-w-md text-sm text-paper/70 transition-opacity motion-reduce:transition-none",
          triviaVisible ? "opacity-100" : "opacity-0",
        )}
        style={{ transitionDuration: `${FADE_DURATION_MS}ms` }}
      >
        {TECH_MESSAGES[triviaIndex]}
      </p>
    </div>
  );
}

export { ColdStartOverlay, pickNextMessageIndex };
