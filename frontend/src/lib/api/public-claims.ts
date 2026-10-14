import { browserApiUrl } from "./config";
import { request } from "./http";
import type { ClaimTracking, CreateClaimInput, CreateClaimResult } from "../types";

export function createClaim(input: CreateClaimInput) {
  return request<CreateClaimResult>(browserApiUrl, "/api/claims", {
    method: "POST",
    body: JSON.stringify({
      ...input,
      fullName: input.fullName.trim(),
      documentId: input.documentId.trim(),
      email: input.email.trim(),
      phone: input.phone.trim() || null,
      description: input.description.trim(),
    }),
  });
}

export function trackClaim(folio: string, email: string) {
  const query = new URLSearchParams({ email: email.trim() });
  return request<ClaimTracking>(
    browserApiUrl,
    `/api/claims/${encodeURIComponent(folio.trim())}?${query}`,
  );
}
