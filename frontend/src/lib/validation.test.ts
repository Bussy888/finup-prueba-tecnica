import { describe, expect, it } from "vitest";
import type { CreateClaimInput } from "./types";
import { validateClaim, validateTracking } from "./validation";

const valid: CreateClaimInput = {
  fullName: "María Fernández Rojas",
  documentId: "1234567 LP",
  email: "maria@example.com",
  phone: "71234567",
  claimType: "Cobros",
  description: "Se me cobró una comisión que no corresponde según mi contrato.",
};

describe("validateClaim", () => {
  it("acepta un reclamo válido", () => {
    expect(validateClaim(valid)).toEqual({});
  });

  it("el teléfono es opcional", () => {
    expect(validateClaim({ ...valid, phone: "" })).toEqual({});
  });

  it("rechaza nombres cortos", () => {
    expect(validateClaim({ ...valid, fullName: "Al" }).fullName).toBeDefined();
  });

  it("exige el CI", () => {
    expect(validateClaim({ ...valid, documentId: "  " }).documentId).toBeDefined();
  });

  it.each(["maria", "maria@", "maria@example"])("rechaza el email %s", (email) => {
    expect(validateClaim({ ...valid, email }).email).toBeDefined();
  });

  it.each(["123456", "1234567890123456", "7123abc9"])(
    "rechaza el teléfono %s",
    (phone) => {
      expect(validateClaim({ ...valid, phone }).phone).toBeDefined();
    },
  );

  it("rechaza descripciones de menos de 20 caracteres", () => {
    expect(
      validateClaim({ ...valid, description: "muy corta" }).description,
    ).toBeDefined();
  });

  it("rechaza descripciones de más de 2000 caracteres", () => {
    expect(
      validateClaim({ ...valid, description: "a".repeat(2001) }).description,
    ).toBeDefined();
  });
});

describe("validateTracking", () => {
  it("exige folio y email válido", () => {
    expect(validateTracking("", "x")).toEqual({
      folio: expect.any(String),
      email: expect.any(String),
    });
  });

  it("acepta datos correctos", () => {
    expect(validateTracking("REC-2026-000001", "ana@example.com")).toEqual({});
  });
});
