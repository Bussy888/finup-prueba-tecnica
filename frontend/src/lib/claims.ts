import type { ClaimStatus, ClaimType } from "./types";

export const claimTypeLabels: Record<ClaimType, string> = {
  Producto: "Producto",
  Atencion: "Atención",
  Cobros: "Cobros",
  Otro: "Otro",
};

export const statusLabels: Record<ClaimStatus, string> = {
  Recibido: "Recibido",
  EnProceso: "En proceso",
  Resuelto: "Resuelto",
  Rechazado: "Rechazado",
};

const transitions: Record<ClaimStatus, ClaimStatus[]> = {
  Recibido: ["EnProceso", "Rechazado"],
  EnProceso: ["Resuelto", "Rechazado"],
  Resuelto: [],
  Rechazado: [],
};

export function allowedTransitions(from: ClaimStatus): ClaimStatus[] {
  return transitions[from];
}

export function requiresComment(to: ClaimStatus): boolean {
  return to === "Resuelto" || to === "Rechazado";
}

export function isClosed(status: ClaimStatus): boolean {
  return status === "Resuelto" || status === "Rechazado";
}

export function isClaimStatus(value: string | undefined): value is ClaimStatus {
  return value !== undefined && value in statusLabels;
}
