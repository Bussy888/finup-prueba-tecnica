import { logoutAction } from "@/app/admin/actions";

export function LogoutButton() {
  return (
    <form action={logoutAction}>
      <button type="submit" className="btn btn-small">
        Cerrar sesión
      </button>
    </form>
  );
}
