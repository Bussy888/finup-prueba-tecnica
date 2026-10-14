"use client";

import { useRef, useState, type FormEvent } from "react";
import { Alert } from "@/components/Alert";
import { Field } from "@/components/Field";
import { createClaim } from "@/lib/api/public-claims";
import { ApiError } from "@/lib/api/http";
import { claimTypeLabels } from "@/lib/claims";
import { claimTypes, type CreateClaimInput, type CreateClaimResult } from "@/lib/types";
import { validateClaim, type FieldErrors } from "@/lib/validation";
import { ClaimConfirmation } from "./ClaimConfirmation";

const emptyForm: CreateClaimInput = {
  fullName: "",
  documentId: "",
  email: "",
  phone: "",
  claimType: "Producto",
  description: "",
};

const fieldOrder = [
  "fullName",
  "documentId",
  "email",
  "phone",
  "claimType",
  "description",
];

export function ClaimForm() {
  const [values, setValues] = useState<CreateClaimInput>(emptyForm);
  const [errors, setErrors] = useState<FieldErrors>({});
  const [serverError, setServerError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState<CreateClaimResult | null>(null);
  const formRef = useRef<HTMLFormElement>(null);

  function update<K extends keyof CreateClaimInput>(key: K, value: CreateClaimInput[K]) {
    setValues((current) => ({ ...current, [key]: value }));
    if (errors[key]) setErrors((current) => ({ ...current, [key]: undefined }));
  }

  function focusFirstError(found: FieldErrors) {
    const first = fieldOrder.find((name) => found[name]);
    if (first) formRef.current?.querySelector<HTMLElement>(`#${first}`)?.focus();
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (submitting) return;

    const found = validateClaim(values);
    setErrors(found);
    setServerError(null);
    if (Object.keys(found).length > 0) {
      focusFirstError(found);
      return;
    }

    setSubmitting(true);
    try {
      setResult(await createClaim(values));
    } catch (error) {
      if (error instanceof ApiError && Object.keys(error.fieldErrors).length > 0) {
        setErrors(error.fieldErrors);
        focusFirstError(error.fieldErrors);
      } else {
        setServerError(
          error instanceof ApiError ? error.message : "Ocurrió un error inesperado.",
        );
      }
    } finally {
      setSubmitting(false);
    }
  }

  if (result) {
    return (
      <ClaimConfirmation
        result={result}
        onNewClaim={() => {
          setResult(null);
          setValues(emptyForm);
        }}
      />
    );
  }

  return (
    <form ref={formRef} onSubmit={handleSubmit} noValidate className="card form">
      {serverError && <Alert>{serverError}</Alert>}

      <Field id="fullName" label="Nombre completo" error={errors.fullName}>
        {(props) => (
          <input
            {...props}
            type="text"
            autoComplete="name"
            value={values.fullName}
            onChange={(e) => update("fullName", e.target.value)}
          />
        )}
      </Field>

      <div className="form-row">
        <Field
          id="documentId"
          label="Documento de identidad (CI)"
          error={errors.documentId}
        >
          {(props) => (
            <input
              {...props}
              type="text"
              value={values.documentId}
              onChange={(e) => update("documentId", e.target.value)}
            />
          )}
        </Field>

        <Field id="phone" label="Teléfono" optional error={errors.phone}>
          {(props) => (
            <input
              {...props}
              type="tel"
              inputMode="numeric"
              autoComplete="tel"
              value={values.phone}
              onChange={(e) => update("phone", e.target.value)}
            />
          )}
        </Field>
      </div>

      <Field id="email" label="Correo electrónico" error={errors.email}>
        {(props) => (
          <input
            {...props}
            type="email"
            autoComplete="email"
            value={values.email}
            onChange={(e) => update("email", e.target.value)}
          />
        )}
      </Field>

      <Field id="claimType" label="Tipo de reclamo" error={errors.claimType}>
        {(props) => (
          <select
            {...props}
            value={values.claimType}
            onChange={(e) =>
              update("claimType", e.target.value as CreateClaimInput["claimType"])
            }
          >
            {claimTypes.map((type) => (
              <option key={type} value={type}>
                {claimTypeLabels[type]}
              </option>
            ))}
          </select>
        )}
      </Field>

      <Field
        id="description"
        label="Descripción"
        error={errors.description}
        hint={`${values.description.trim().length}/2000 caracteres (mínimo 20)`}
      >
        {(props) => (
          <textarea
            {...props}
            rows={6}
            value={values.description}
            onChange={(e) => update("description", e.target.value)}
          />
        )}
      </Field>

      <button type="submit" className="btn btn-primary" disabled={submitting}>
        {submitting ? "Enviando..." : "Registrar reclamo"}
      </button>
    </form>
  );
}
