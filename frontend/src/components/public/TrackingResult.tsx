import { HistoryList } from "@/components/HistoryList";
import { OverdueBadge, StatusBadge } from "@/components/StatusBadge";
import { claimTypeLabels } from "@/lib/claims";
import { formatDate, formatDateTime } from "@/lib/format";
import type { ClaimTracking } from "@/lib/types";

export function TrackingResult({ claim }: { claim: ClaimTracking }) {
  return (
    <section className="card" aria-live="polite">
      <header className="result-header">
        <div>
          <h2>{claim.folio}</h2>
          <p className="muted">{claimTypeLabels[claim.claimType]}</p>
        </div>
        <div className="badges">
          <StatusBadge status={claim.status} />
          {claim.isOverdue && <OverdueBadge />}
        </div>
      </header>

      <dl className="details">
        <div>
          <dt>Registrado</dt>
          <dd>{formatDateTime(claim.createdAt)}</dd>
        </div>
        <div>
          <dt>Fecha límite</dt>
          <dd>{formatDate(claim.dueDate)}</dd>
        </div>
        {claim.resolvedAt && (
          <div>
            <dt>Cerrado</dt>
            <dd>{formatDateTime(claim.resolvedAt)}</dd>
          </div>
        )}
      </dl>

      <h3>Historial</h3>
      <HistoryList history={claim.history} />
    </section>
  );
}
