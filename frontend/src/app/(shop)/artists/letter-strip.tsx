import { letterAnchorId, type LetterKey } from "./letters";

interface LetterStripProps {
  letters: readonly LetterKey[];
  presentLetters: ReadonlySet<LetterKey>;
}

function LetterStrip({ letters, presentLetters }: LetterStripProps) {
  return (
    <nav
      aria-label="Jump to letter"
      className="flex flex-wrap gap-1.5 border-y border-line py-6"
    >
      {letters.map((letter) =>
        presentLetters.has(letter) ? (
          <a
            key={letter}
            href={`#${letterAnchorId(letter)}`}
            className="flex size-6.5 items-center justify-center font-heading text-sm font-extrabold text-ink"
          >
            {letter}
          </a>
        ) : (
          <span
            key={letter}
            aria-hidden
            className="flex size-6.5 items-center justify-center font-heading text-sm font-extrabold text-muted-foreground/40"
          >
            {letter}
          </span>
        ),
      )}
    </nav>
  );
}

export { LetterStrip };
