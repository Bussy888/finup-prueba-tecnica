import type { CreateClaimInput } from "./types";

export type FieldErrors = Partial<Record<string, string>>;

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const phonePattern = /^\d{7,15}$/;

export function validateClaim(values: CreateClaimInput): FieldErrors {
  const errors: FieldErrors = {};

  const name = values.fullName.trim();
  if (name.length < 3 || name.length > 100) {
    errors.fullName = "El nombre debe tener entre 3 y 100 caracteres.";
  }

  const document = values.documentId.trim();
  if (!document) {
    errors.documentId = "El documento de identidad (CI) es obligatorio.";
  } else if (document.length > 30) {
    errors.documentId = "El documento no puede superar los 30 caracteres.";
  }

  if (!emailPattern.test(values.email.trim())) {
    errors.email = "Ingresa un correo electrónico válido.";
  }

  const phone = values.phone.trim();
  if (phone && !phonePattern.test(phone)) {
    errors.phone = "El teléfono debe tener entre 7 y 15 dígitos.";
  }

  const description = values.description.trim();
  if (description.length < 20 || description.length > 2000) {
    errors.description = "La descripción debe tener entre 20 y 2000 caracteres.";
  }

  return errors;
}

export function validateTracking(folio: string, email: string): FieldErrors {
  const errors: FieldErrors = {};
  if (!folio.trim()) errors.folio = "Ingresa el folio de tu reclamo.";
  if (!emailPattern.test(email.trim()))
    errors.email = "Ingresa un correo electrónico válido.";
  return errors;
}
