import type { Metadata } from "next";
import { ClaimForm } from "@/components/public/ClaimForm";

export const metadata: Metadata = { title: "Registrar reclamo" };

export default function ClaimsPage() {
  return (
    <>
      <h1>Registrar un reclamo</h1>
      <p className="lead">Todos los campos son obligatorios salvo el teléfono.</p>
      <ClaimForm />
    </>
  );
}
