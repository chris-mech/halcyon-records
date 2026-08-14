export function getPageNumbers(
  current: number,
  totalPages: number,
): (number | "ellipsis")[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  const windowStart = Math.max(1, current - 2);
  const windowEnd = Math.min(totalPages, current + 2);
  const pages = new Set<number>([1, totalPages]);
  for (let page = windowStart; page <= windowEnd; page++) {
    pages.add(page);
  }

  const sorted = [...pages].sort((a, b) => a - b);
  const result: (number | "ellipsis")[] = [];
  sorted.forEach((page, index) => {
    if (index > 0 && page - sorted[index - 1] > 1) {
      result.push("ellipsis");
    }
    result.push(page);
  });
  return result;
}
