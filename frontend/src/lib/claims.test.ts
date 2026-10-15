import { describe, expect, it } from "vitest";
import { allowedTransitions, isClaimStatus, isClosed, requiresComment } from "./claims";

describe("allowedTransitions", () => {
  it("Recibido puede pasar a EnProceso o Rechazado", () => {
    expect(allowedTransitions("Recibido")).toEqual(["EnProceso", "Rechazado"]);
  });

  it("EnProceso puede pasar a Resuelto o Rechazado", () => {
    expect(allowedTransitions("EnProceso")).toEqual(["Resuelto", "Rechazado"]);
  });

  it("los estados cerrados no tienen salida", () => {
    expect(allowedTransitions("Resuelto")).toEqual([]);
    expect(allowedTransitions("Rechazado")).toEqual([]);
  });
});

describe("requiresComment / isClosed", () => {
  it("resolver y rechazar exigen comentario", () => {
    expect(requiresComment("Resuelto")).toBe(true);
    expect(requiresComment("Rechazado")).toBe(true);
    expect(requiresComment("EnProceso")).toBe(false);
  });

  it("identifica estados cerrados", () => {
    expect(isClosed("Resuelto")).toBe(true);
    expect(isClosed("Recibido")).toBe(false);
  });
});

describe("isClaimStatus", () => {
  it("valida el valor del filtro de la URL", () => {
    expect(isClaimStatus("EnProceso")).toBe(true);
    expect(isClaimStatus("otro")).toBe(false);
    expect(isClaimStatus(undefined)).toBe(false);
  });
});
