const DEFAULT_WINDOW_SIZE = 5;

export function pageWindow(current: number, total: number, size = DEFAULT_WINDOW_SIZE): number[] {
  const length = Math.max(Math.min(size, total), 0);
  const lastStart = Math.max(total - length + 1, 1);
  const start = Math.min(Math.max(current - Math.floor(length / 2), 1), lastStart);

  return Array.from({ length }, (_, index) => start + index);
}
