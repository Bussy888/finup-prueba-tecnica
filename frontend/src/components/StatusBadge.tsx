import { statusLabels } from "@/lib/claims";
import type { ClaimStatus } from "@/lib/types";

export function StatusBadge({ status }: { status: ClaimStatus }) {
  return (
    <span className={`badge badge-${status.toLowerCase()}`}>{statusLabels[status]}</span>
  );
}

export function OverdueBadge() {
  return <span className="badge badge-overdue">Vencido</span>;
}
