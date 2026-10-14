"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { changeClaimStatus, login } from "@/lib/api/admin-claims";
import { ApiError } from "@/lib/api/http";
import { isClaimStatus, requiresComment, statusLabels } from "@/lib/claims";
import { clearAdminToken, getAdminToken, setAdminToken } from "@/lib/session";
import type { ActionState } from "./state";

export async function loginAction(
  _prev: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const apiKey = String(formData.get("apiKey") ?? "").trim();
  if (!apiKey) {
    return { fieldErrors: { apiKey: "Ingresa la clave de acceso." } };
  }

  try {
    const { token, expiresAt } = await login(apiKey);
    await setAdminToken(token, expiresAt);
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      return { fieldErrors: { apiKey: "La clave de acceso no es correcta." } };
    }
    return {
      error: error instanceof ApiError ? error.message : "No se pudo iniciar sesión.",
    };
  }

  redirect("/admin");
}

export async function logoutAction() {
  await clearAdminToken();
  redirect("/admin");
}

export async function changeStatusAction(
  _prev: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const token = await getAdminToken();
  if (!token) return { error: "Tu sesión venció. Vuelve a ingresar la clave." };

  const folio = String(formData.get("folio") ?? "");
  const status = String(formData.get("status") ?? "");
  const comment = String(formData.get("comment") ?? "");

  if (!isClaimStatus(status)) {
    return { fieldErrors: { status: "Elige el nuevo estado." } };
  }
  if (requiresComment(status) && !comment.trim()) {
    return {
      fieldErrors: {
        comment: `Para dejar el reclamo como "${statusLabels[status]}" escribe un comentario.`,
      },
    };
  }

  try {
    await changeClaimStatus(token, folio, status, comment);
  } catch (error) {
    if (error instanceof ApiError) {
      if (error.status === 401)
        return { error: "Tu sesión venció. Vuelve a ingresar la clave." };
      return { error: error.message, fieldErrors: error.fieldErrors };
    }
    return { error: "Ocurrió un error inesperado." };
  }

  revalidatePath("/admin");
  return { success: `El reclamo pasó a "${statusLabels[status]}".` };
}
