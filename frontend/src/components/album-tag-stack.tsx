import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

interface AlbumTagStackProps {
  isNew: boolean;
  isOnSale: boolean;
  isStaffPick: boolean;
  orientation?: "horizontal" | "vertical";
  className?: string;
}

const tagClassName =
  "h-auto px-3 py-1 text-[0.625rem] font-bold tracking-wide uppercase shadow-sm";

function AlbumTagStack({
  isNew,
  isOnSale,
  isStaffPick,
  orientation = "horizontal",
  className,
}: AlbumTagStackProps) {
  if (!isNew && !isOnSale && !isStaffPick) {
    return null;
  }

  return (
    <div
      className={cn(
        "flex gap-1",
        orientation === "vertical" ? "flex-col items-end" : "flex-row",
        className,
      )}
    >
      {isNew && <Badge className={tagClassName}>New</Badge>}
      {isOnSale && (
        <Badge className={cn(tagClassName, "bg-gold text-ink")}>On sale</Badge>
      )}
      {isStaffPick && (
        <Badge variant="secondary" className={tagClassName}>
          Staff pick
        </Badge>
      )}
    </div>
  );
}

export { AlbumTagStack };
