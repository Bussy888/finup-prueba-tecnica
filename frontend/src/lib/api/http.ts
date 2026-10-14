export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public fieldErrors: Record<string, string> = {},
  ) {
    super(message);
    this.name = "ApiError";
  }
}

interface ProblemBody {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

function fieldErrorsFrom(errors: ProblemBody["errors"]): Record<string, string> {
  const result: Record<string, string> = {};
  for (const [key, messages] of Object.entries(errors ?? {})) {
    const field = key.replace(/^\$\./, "");
    if (messages?.length) result[field] = messages[0];
  }
  return result;
}

export async function request<T>(
  baseUrl: string,
  path: string,
  init?: RequestInit,
): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${baseUrl}${path}`, {
      ...init,
      cache: "no-store",
      headers: { "Content-Type": "application/json", ...init?.headers },
    });
  } catch {
    throw new ApiError(
      "No se pudo conectar con el servidor. Intenta de nuevo en unos minutos.",
      0,
    );
  }

  if (response.ok) {
    return (await response.json()) as T;
  }

  const body: ProblemBody = await response.json().catch(() => ({}));

  const message =
    body.detail ??
    body.title ??
    (response.status >= 500
      ? "Ocurrió un error en el servidor."
      : "No se pudo completar la solicitud.");
  throw new ApiError(message, response.status, fieldErrorsFrom(body.errors));
}
