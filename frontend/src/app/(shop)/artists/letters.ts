export const LETTERS = [
  "A",
  "B",
  "C",
  "D",
  "E",
  "F",
  "G",
  "H",
  "I",
  "J",
  "K",
  "L",
  "M",
  "N",
  "O",
  "P",
  "Q",
  "R",
  "S",
  "T",
  "U",
  "V",
  "W",
  "X",
  "Y",
  "Z",
  "#",
] as const;

export type LetterKey = (typeof LETTERS)[number];

export function letterKeyFor(name: string): LetterKey {
  const first = name.trim().charAt(0).toUpperCase();
  return (LETTERS as readonly string[]).includes(first)
    ? (first as LetterKey)
    : "#";
}

export function letterAnchorId(letter: LetterKey): string {
  return letter === "#" ? "misc" : `letter-${letter}`;
}
