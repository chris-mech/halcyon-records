import Link from "next/link";
import type { ReactNode } from "react";

import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { cn } from "@/lib/utils";

interface AccountShellProps {
  active: "orders" | "details";
  children: ReactNode;
}

const tabs = [
  { key: "orders", label: "Order history", href: "/account" },
  { key: "details", label: "Account details", href: "/account/details" },
] as const;

function AccountShell({ active, children }: AccountShellProps) {
  return (
    <div className="mx-auto w-full max-w-275 px-16 py-11">
      <Breadcrumb className="mb-6">
        <BreadcrumbList className="gap-2 text-xs font-semibold text-muted-foreground sm:gap-2">
          <BreadcrumbItem>
            <BreadcrumbLink
              render={<Link href="/">Home</Link>}
              className="hover:text-ink"
            />
          </BreadcrumbItem>
          <BreadcrumbSeparator className="text-line">/</BreadcrumbSeparator>
          <BreadcrumbItem>
            <BreadcrumbPage className="text-ink">Account</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      <h1 className="mb-1.5 font-serif text-4xl font-medium italic">
        Your account
      </h1>
      <p className="mb-8 text-[0.8125rem] text-muted-foreground">
        Demo account: nothing here is tied to a real identity
      </p>

      <div className="grid grid-cols-[220px_1fr] items-start gap-12">
        <nav aria-label="Account" className="border-r border-line">
          {tabs.map((tab) => (
            <Link
              key={tab.key}
              href={tab.href}
              aria-current={tab.key === active ? "page" : undefined}
              className={cn(
                "block border-l-2 py-3.5 pl-4 text-[0.8125rem] font-semibold tracking-wide uppercase",
                tab.key === active
                  ? "border-rust font-bold text-ink"
                  : "border-transparent text-muted-foreground",
              )}
            >
              {tab.label}
            </Link>
          ))}
        </nav>

        <div>{children}</div>
      </div>
    </div>
  );
}

export { AccountShell };
