import Link from "next/link";
import { formatDate } from "@/lib/format";
import type { CreateClaimResult } from "@/lib/types";

interface ClaimConfirmationProps {
  result: CreateClaimResult;
  onNewClaim: () => void;
}

export function ClaimConfirmation({ result, onNewClaim }: ClaimConfirmationProps) {
  return (
    <section className="card confirmation" aria-live="polite">
      <h2>Tu reclamo fue registrado</h2>
      <p>Guarda este folio: lo necesitarás para consultar el estado.</p>

      <p className="folio">{result.folio}</p>

      <div className="due">
        <span>Fecha límite de atención</span>
        <strong>{formatDate(result.dueDate)}</strong>
      </div>

      <div className="actions">
        <Link href="/seguimiento" className="btn btn-primary">
          Consultar seguimiento
        </Link>
        <button type="button" className="btn" onClick={onNewClaim}>
          Registrar otro reclamo
        </button>
      </div>
    </section>
  );
}
