import { writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

writeFileSync(join(tmpdir(), "halcyon-e2e-cold-start.marker"), "");
