export type ClaimType = "Producto" | "Atencion" | "Cobros" | "Otro";
export type ClaimStatus = "Recibido" | "EnProceso" | "Resuelto" | "Rechazado";

export const claimTypes: ClaimType[] = ["Producto", "Atencion", "Cobros", "Otro"];
export const claimStatuses: ClaimStatus[] = [
  "Recibido",
  "EnProceso",
  "Resuelto",
  "Rechazado",
];

export interface CreateClaimInput {
  fullName: string;
  documentId: string;
  email: string;
  phone: string;
  claimType: ClaimType;
  description: string;
}

export interface CreateClaimResult {
  folio: string;
  status: ClaimStatus;
  createdAt: string;
  dueDate: string;
}

export interface StatusHistoryItem {
  fromStatus: ClaimStatus | null;
  toStatus: ClaimStatus;
  comment: string | null;
  changedAt: string;
}

export interface ClaimTracking {
  folio: string;
  claimType: ClaimType;
  status: ClaimStatus;
  createdAt: string;
  dueDate: string;
  resolvedAt: string | null;
  isOverdue: boolean;
  history: StatusHistoryItem[];
}

export interface ClaimSummary {
  folio: string;
  claimType: ClaimType;
  fullName: string;
  status: ClaimStatus;
  createdAt: string;
  dueDate: string;
  isOverdue: boolean;
}

export interface ClaimDetail extends ClaimTracking {
  fullName: string;
  documentId: string;
  email: string;
  phone: string | null;
  description: string;
}

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
