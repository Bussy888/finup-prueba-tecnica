import Link from "next/link";
import { adminUrl, type AdminQuery } from "./admin-url";

interface PaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
  query: AdminQuery;
}

export function Pagination({ page, totalPages, totalCount, query }: PaginationProps) {
  if (totalPages <= 1) {
    return <p className="muted">{totalCount} reclamo(s)</p>;
  }

  return (
    <nav className="pagination" aria-label="Paginación">
      {page > 1 ? (
        <Link className="btn btn-small" href={adminUrl({ ...query, page: page - 1 })}>
          Anterior
        </Link>
      ) : (
        <span className="btn btn-small btn-disabled" aria-disabled="true">
          Anterior
        </span>
      )}
      <span>
        Página {page} de {totalPages} · {totalCount} reclamos
      </span>
      {page < totalPages ? (
        <Link className="btn btn-small" href={adminUrl({ ...query, page: page + 1 })}>
          Siguiente
        </Link>
      ) : (
        <span className="btn btn-small btn-disabled" aria-disabled="true">
          Siguiente
        </span>
      )}
    </nav>
  );
}
