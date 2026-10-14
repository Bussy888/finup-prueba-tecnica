"use client";

import { useState, type FormEvent } from "react";
import { Alert } from "@/components/Alert";
import { Field } from "@/components/Field";
import { ApiError } from "@/lib/api/http";
import { trackClaim } from "@/lib/api/public-claims";
import type { ClaimTracking } from "@/lib/types";
import { validateTracking, type FieldErrors } from "@/lib/validation";
import { TrackingResult } from "./TrackingResult";

export function TrackingForm() {
  const [folio, setFolio] = useState("");
  const [email, setEmail] = useState("");
  const [errors, setErrors] = useState<FieldErrors>({});
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [claim, setClaim] = useState<ClaimTracking | null>(null);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (loading) return;

    const found = validateTracking(folio, email);
    setErrors(found);
    setMessage(null);
    if (Object.keys(found).length > 0) return;

    setLoading(true);
    setClaim(null);
    try {
      setClaim(await trackClaim(folio, email));
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        setMessage(
          "No encontramos un reclamo con ese folio y correo. Revisa los datos e intenta de nuevo.",
        );
      } else {
        setMessage(
          error instanceof ApiError ? error.message : "Ocurrió un error inesperado.",
        );
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <form onSubmit={handleSubmit} noValidate className="card form">
        <Field
          id="folio"
          label="Folio"
          error={errors.folio}
          hint="Ejemplo: REC-2026-000123"
        >
          {(props) => (
            <input
              {...props}
              type="text"
              value={folio}
              onChange={(e) => setFolio(e.target.value)}
            />
          )}
        </Field>

        <Field id="email" label="Correo electrónico" error={errors.email}>
          {(props) => (
            <input
              {...props}
              type="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          )}
        </Field>

        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? "Consultando..." : "Consultar"}
        </button>
      </form>

      {message && <Alert>{message}</Alert>}
      {claim && <TrackingResult claim={claim} />}
    </>
  );
}
