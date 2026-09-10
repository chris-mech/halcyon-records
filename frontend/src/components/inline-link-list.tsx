import { Fragment } from "react";
import Link from "next/link";

interface InlineLink {
  id: string;
  href: string;
  label: string;
}

interface InlineLinkListProps {
  items: InlineLink[];
  linkClassName?: string;
}

function InlineLinkList({ items, linkClassName }: InlineLinkListProps) {
  return (
    <>
      {items.map((item, index) => (
        <Fragment key={item.id}>
          {index > 0 && (
            <>
              <span className="sr-only">, </span>
              <span aria-hidden className="mx-1">
                ·
              </span>
            </>
          )}
          <Link href={item.href} className={linkClassName}>
            {item.label}
          </Link>
        </Fragment>
      ))}
    </>
  );
}

export { InlineLinkList };
export type { InlineLink };
