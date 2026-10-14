import Link from "next/link";
import { OverdueBadge, StatusBadge } from "@/components/StatusBadge";
import { claimTypeLabels } from "@/lib/claims";
import { formatDate } from "@/lib/format";
import type { ClaimSummary } from "@/lib/types";
import { adminUrl, type AdminQuery } from "./admin-url";

interface ClaimsTableProps {
  claims: ClaimSummary[];
  query: AdminQuery;
}

export function ClaimsTable({ claims, query }: ClaimsTableProps) {
  if (claims.length === 0) {
    return (
      <div className="empty">
        <p>No hay reclamos para mostrar.</p>
        {query.status && <Link href={adminUrl({})}>Quitar el filtro</Link>}
      </div>
    );
  }

  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th scope="col">Folio</th>
            <th scope="col">Tipo</th>
            <th scope="col">Cliente</th>
            <th scope="col">Estado</th>
            <th scope="col">Fecha límite</th>
          </tr>
        </thead>
        <tbody>
          {claims.map((claim) => (
            <tr
              key={claim.folio}
              className={[
                claim.isOverdue ? "row-overdue" : "",
                claim.folio === query.folio ? "row-selected" : "",
              ].join(" ")}
            >
              <td>
                <Link href={adminUrl({ ...query, folio: claim.folio })} scroll={false}>
                  {claim.folio}
                </Link>
              </td>
              <td>{claimTypeLabels[claim.claimType]}</td>
              <td>{claim.fullName}</td>
              <td>
                <div className="badges">
                  <StatusBadge status={claim.status} />
                  {claim.isOverdue && <OverdueBadge />}
                </div>
              </td>
              <td>{formatDate(claim.dueDate)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
