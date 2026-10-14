import Link from "next/link";
import { HistoryList } from "@/components/HistoryList";
import { OverdueBadge, StatusBadge } from "@/components/StatusBadge";
import { claimTypeLabels } from "@/lib/claims";
import { formatDate, formatDateTime } from "@/lib/format";
import type { ClaimDetail } from "@/lib/types";
import { adminUrl, type AdminQuery } from "./admin-url";
import { ChangeStatusForm } from "./ChangeStatusForm";

interface ClaimDetailPanelProps {
  claim: ClaimDetail;
  query: AdminQuery;
}

export function ClaimDetailPanel({ claim, query }: ClaimDetailPanelProps) {
  return (
    <aside className="card detail-panel" aria-label={`Detalle de ${claim.folio}`}>
      <header className="result-header">
        <div>
          <h2>{claim.folio}</h2>
          <p className="muted">{claimTypeLabels[claim.claimType]}</p>
        </div>
        <Link
          href={adminUrl({ ...query, folio: undefined })}
          className="btn btn-small"
          scroll={false}
        >
          Cerrar
        </Link>
      </header>

      <div className="badges">
        <StatusBadge status={claim.status} />
        {claim.isOverdue && <OverdueBadge />}
      </div>

      <dl className="details">
        <div>
          <dt>Cliente</dt>
          <dd>{claim.fullName}</dd>
        </div>
        <div>
          <dt>CI</dt>
          <dd>{claim.documentId}</dd>
        </div>
        <div>
          <dt>Correo</dt>
          <dd>{claim.email}</dd>
        </div>
        {claim.phone && (
          <div>
            <dt>Teléfono</dt>
            <dd>{claim.phone}</dd>
          </div>
        )}
        <div>
          <dt>Registrado</dt>
          <dd>{formatDateTime(claim.createdAt)}</dd>
        </div>
        <div>
          <dt>Fecha límite</dt>
          <dd>{formatDate(claim.dueDate)}</dd>
        </div>
      </dl>

      <h3>Descripción</h3>
      <p className="description">{claim.description}</p>

      <h3>Historial</h3>
      <HistoryList history={claim.history} />

      <ChangeStatusForm
        key={`${claim.folio}-${claim.status}`}
        folio={claim.folio}
        currentStatus={claim.status}
      />
    </aside>
  );
}
