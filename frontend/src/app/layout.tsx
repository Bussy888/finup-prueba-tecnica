import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

export const metadata: Metadata = {
  title: "Punto de Reclamo",
  description: "Registra un reclamo y consulta su estado.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="es">
      <body>
        <header className="site-header">
          <div className="container header-inner">
            <Link href="/" className="brand">
              Punto de Reclamo
            </Link>
            <nav aria-label="Principal">
              <Link href="/reclamos">Registrar reclamo</Link>
              <Link href="/seguimiento">Seguimiento</Link>
              <Link href="/admin">Administración</Link>
            </nav>
          </div>
        </header>
        <main className="container">{children}</main>
      </body>
    </html>
  );
}
