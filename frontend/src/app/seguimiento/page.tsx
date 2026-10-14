import type { Metadata } from "next";
import { TrackingForm } from "@/components/public/TrackingForm";

export const metadata: Metadata = { title: "Seguimiento de reclamo" };

export default function TrackingPage() {
  return (
    <>
      <h1>Seguimiento de tu reclamo</h1>
      <p className="lead">
        Ingresa el folio que recibiste y el correo con el que registraste el reclamo.
      </p>
      <TrackingForm />
    </>
  );
}
