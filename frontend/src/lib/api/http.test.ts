import { afterEach, describe, expect, it, vi } from "vitest";
import { ApiError, request } from "./http";

function mockFetch(response: Response | Error) {
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => {
      if (response instanceof Error) throw response;
      return response;
    }),
  );
}

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("request", () => {
  it("devuelve el JSON cuando la respuesta es correcta", async () => {
    mockFetch(Response.json({ folio: "REC-2026-000001" }, { status: 201 }));

    await expect(request("http://api", "/api/claims")).resolves.toEqual({
      folio: "REC-2026-000001",
    });
  });

  it("convierte los errores 422 en errores por campo", async () => {
    mockFetch(
      Response.json(
        {
          title: "Los datos enviados no son válidos.",
          errors: { email: ["Correo inválido"] },
        },
        { status: 422 },
      ),
    );

    const error = await request<ApiError>("http://api", "/x").catch((e) => e as ApiError);

    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(422);
    expect(error.fieldErrors).toEqual({ email: "Correo inválido" });
  });

  it("quita el prefijo $. de los errores de binding", async () => {
    mockFetch(
      Response.json({ errors: { "$.claimType": ["Valor inválido"] } }, { status: 400 }),
    );

    const error = await request<ApiError>("http://api", "/x").catch((e) => e as ApiError);

    expect(error.fieldErrors).toEqual({ claimType: "Valor inválido" });
  });

  it("usa el detalle del problema como mensaje", async () => {
    mockFetch(
      Response.json(
        { title: "Conflicto", detail: "No se puede pasar de A a B." },
        { status: 409 },
      ),
    );

    const error = await request<ApiError>("http://api", "/x").catch((e) => e as ApiError);

    expect(error.message).toBe("No se puede pasar de A a B.");
  });

  it("maneja respuestas de error que no son JSON", async () => {
    mockFetch(new Response("boom", { status: 500 }));

    const error = await request<ApiError>("http://api", "/x").catch((e) => e as ApiError);

    expect(error.status).toBe(500);
    expect(error.message).toMatch(/servidor/);
  });

  it("avisa cuando no hay conexión", async () => {
    mockFetch(new TypeError("fetch failed"));

    const error = await request<ApiError>("http://api", "/x").catch((e) => e as ApiError);

    expect(error.status).toBe(0);
  });
});
