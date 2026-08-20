function safeNextPath(
  value: string | string[] | undefined,
): string | undefined {
  return typeof value === "string" &&
    value.startsWith("/") &&
    !value.startsWith("//")
    ? value
    : undefined;
}

export { safeNextPath };
