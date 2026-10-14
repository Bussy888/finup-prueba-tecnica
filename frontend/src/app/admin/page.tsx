import type { Metadata } from "next";
import { Alert } from "@/components/Alert";
import { ClaimDetailPanel } from "@/components/admin/ClaimDetailPanel";
import { ClaimsTable } from "@/components/admin/ClaimsTable";
import { LoginForm } from "@/components/admin/LoginForm";
import { LogoutButton } from "@/components/admin/LogoutButton";
import { Pagination } from "@/components/admin/Pagination";
import { StatusFilter } from "@/components/admin/StatusFilter";
import { getClaim, listClaims } from "@/lib/api/admin-claims";
import { ApiError } from "@/lib/api/http";
import { isClaimStatus } from "@/lib/claims";
import { getAdminToken } from "@/lib/session";
import type { ClaimDetail } from "@/lib/types";

export const metadata: Metadata = { title: "Administración" };

const PAGE_SIZE = 10;

export default async function AdminPage({ searchParams }: PageProps<"/admin">) {
  const token = await getAdminToken();
  if (!token) return <AdminLogin />;

  const params = await searchParams;
  const rawStatus = typeof params.status === "string" ? params.status : undefined;
  const status = isClaimStatus(rawStatus) ? rawStatus : undefined;
  const page = Math.max(Number(params.page) || 1, 1);
  const folio = typeof params.folio === "string" ? params.folio : undefined;
  const query = { status, page, folio };

  let data;
  try {
    const [claims, selected] = await Promise.all([
      listClaims(token, { status, page, pageSize: PAGE_SIZE }),
      folio ? loadDetail(token, folio) : Promise.resolve(null),
    ]);
    data = { claims, selected };
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      return <AdminLogin notice="Tu sesión venció. Ingresa la clave de nuevo." />;
    }
    return (
      <>
        <h1>Reclamos</h1>
        <Alert>
          {error instanceof ApiError
            ? error.message
            : "No se pudieron cargar los reclamos."}
        </Alert>
      </>
    );
  }

  const { claims, selected } = data;

  return (
    <>
      <div className="page-header">
        <h1>Reclamos</h1>
        <LogoutButton />
      </div>

      <StatusFilter current={status} />

      <div className={folio ? "admin-layout with-detail" : "admin-layout"}>
        <section>
          <ClaimsTable claims={claims.items} query={query} />
          <Pagination
            page={claims.page}
            totalPages={claims.totalPages}
            totalCount={claims.totalCount}
            query={query}
          />
        </section>

        {folio && (
          <div>
            {selected ? (
              <ClaimDetailPanel claim={selected} query={query} />
            ) : (
              <Alert>No se encontró el reclamo {folio}.</Alert>
            )}
          </div>
        )}
      </div>
    </>
  );
}

async function loadDetail(token: string, folio: string): Promise<ClaimDetail | null> {
  try {
    return await getClaim(token, folio);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
}

function AdminLogin({ notice }: { notice?: string }) {
  return (
    <>
      <h1>Administración</h1>
      <LoginForm notice={notice} />
    </>
  );
}
