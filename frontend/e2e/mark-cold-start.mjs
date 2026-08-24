import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const markerPath = join(
  dirname(fileURLToPath(import.meta.url)),
  "../test-results/halcyon-e2e-cold-start.marker",
);

mkdirSync(dirname(markerPath), { recursive: true });
writeFileSync(markerPath, "");
