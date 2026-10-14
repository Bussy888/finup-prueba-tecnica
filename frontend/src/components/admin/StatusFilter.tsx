import Link from "next/link";
import { statusLabels } from "@/lib/claims";
import { claimStatuses, type ClaimStatus } from "@/lib/types";
import { adminUrl } from "./admin-url";

export function StatusFilter({ current }: { current?: ClaimStatus }) {
  return (
    <nav aria-label="Filtrar por estado" className="filters">
      <Link
        href={adminUrl({})}
        className={`chip${current ? "" : " chip-active"}`}
        aria-current={!current}
      >
        Todos
      </Link>
      {claimStatuses.map((status) => (
        <Link
          key={status}
          href={adminUrl({ status })}
          className={`chip${current === status ? " chip-active" : ""}`}
          aria-current={current === status}
        >
          {statusLabels[status]}
        </Link>
      ))}
    </nav>
  );
}
