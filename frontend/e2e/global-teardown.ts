import { execSync } from "node:child_process";
import { existsSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

const MARKER_PATH = join(tmpdir(), "halcyon-e2e-cold-start.marker");

async function globalTeardown(): Promise<void> {
  if (process.env.CI || !existsSync(MARKER_PATH)) {
    return;
  }

  console.log(
    "[teardown] Cold-started AppHost detected, cleaning up containers...",
  );

  try {
    const containerIds = execSync(
      `docker ps -a --filter "name=sql-" --filter "name=meilisearch-" -q`,
    )
      .toString()
      .trim()
      .split(/\r?\n/)
      .filter(Boolean);

    if (containerIds.length > 0) {
      execSync(`docker rm -f ${containerIds.join(" ")}`, { stdio: "inherit" });
    }
  } catch (error) {
    const err = error as { stderr?: Buffer; message?: string };
    console.log(
      `[teardown] Failed to remove containers: ${err.stderr?.toString().trim() || err.message}`,
    );
  }

  rmSync(MARKER_PATH, { force: true });
}

export default globalTeardown;
