export interface AdminQuery {
  status?: string;
  page?: number;
  folio?: string;
}

export function adminUrl(query: AdminQuery): string {
  const params = new URLSearchParams();
  if (query.status) params.set("status", query.status);
  if (query.page && query.page > 1) params.set("page", String(query.page));
  if (query.folio) params.set("folio", query.folio);

  const qs = params.toString();
  return qs ? `/admin?${qs}` : "/admin";
}
