import "server-only";
import { serverApiUrl } from "./config";
import { request } from "./http";
import type { ClaimDetail, ClaimStatus, ClaimSummary, Paged } from "../types";

interface LoginResult {
  token: string;
  expiresAt: string;
}

function authHeaders(token: string) {
  return { Authorization: `Bearer ${token}` };
}

export function login(apiKey: string) {
  return request<LoginResult>(serverApiUrl, "/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ apiKey }),
  });
}

export function listClaims(
  token: string,
  params: { status?: ClaimStatus; page: number; pageSize: number },
) {
  const query = new URLSearchParams({
    page: String(params.page),
    pageSize: String(params.pageSize),
  });
  if (params.status) query.set("status", params.status);

  return request<Paged<ClaimSummary>>(serverApiUrl, `/api/admin/claims?${query}`, {
    headers: authHeaders(token),
  });
}

export function getClaim(token: string, folio: string) {
  return request<ClaimDetail>(
    serverApiUrl,
    `/api/admin/claims/${encodeURIComponent(folio)}`,
    {
      headers: authHeaders(token),
    },
  );
}

export function changeClaimStatus(
  token: string,
  folio: string,
  status: ClaimStatus,
  comment: string,
) {
  return request<ClaimDetail>(
    serverApiUrl,
    `/api/admin/claims/${encodeURIComponent(folio)}/status`,
    {
      method: "PATCH",
      headers: authHeaders(token),
      body: JSON.stringify({ status, comment: comment.trim() || null }),
    },
  );
}
