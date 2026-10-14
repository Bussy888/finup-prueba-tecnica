import { describe, expect, it } from "vitest";
import { adminUrl } from "./admin-url";

describe("adminUrl", () => {
  it("sin filtros apunta a /admin", () => {
    expect(adminUrl({})).toBe("/admin");
  });

  it("omite la página 1", () => {
    expect(adminUrl({ page: 1 })).toBe("/admin");
  });

  it("combina estado, página y folio", () => {
    expect(adminUrl({ status: "EnProceso", page: 3, folio: "REC-2026-000002" })).toBe(
      "/admin?status=EnProceso&page=3&folio=REC-2026-000002",
    );
  });
});
