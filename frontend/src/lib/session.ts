import "server-only";
import { cookies } from "next/headers";

const COOKIE_NAME = "admin_token";

export async function getAdminToken(): Promise<string | undefined> {
  return (await cookies()).get(COOKIE_NAME)?.value;
}

export async function setAdminToken(token: string, expiresAt: string) {
  (await cookies()).set(COOKIE_NAME, token, {
    httpOnly: true,
    sameSite: "lax",
    secure:
      process.env.NODE_ENV === "production" && process.env.COOKIE_SECURE !== "false",
    path: "/",
    expires: new Date(expiresAt),
  });
}

export async function clearAdminToken() {
  (await cookies()).delete(COOKIE_NAME);
}
