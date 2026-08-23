function SkipLink() {
  return (
    <a
      href="#main-content"
      className="sr-only outline-none focus:not-sr-only focus:fixed focus:top-4 focus:left-4 focus:z-50 focus:border focus:border-line focus:bg-paper focus:px-4 focus:py-2 focus:text-sm focus:font-semibold focus:text-ink focus:shadow-lg focus-visible:ring-3 focus-visible:ring-ring/50"
    >
      Skip to content
    </a>
  );
}

export { SkipLink };
