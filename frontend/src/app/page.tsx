import Link from "next/link";

export default function Home() {
  return (
    <>
      <h1>Punto de Reclamo</h1>
      <p className="lead">
        Registra tu reclamo y te daremos un folio para seguirlo. Atendemos cada caso en un
        plazo de 5 días hábiles.
      </p>

      <div className="grid-2">
        <Link href="/reclamos" className="card card-link">
          <h2>Registrar un reclamo</h2>
          <p>Cuéntanos qué ocurrió y recibe tu folio al instante.</p>
        </Link>
        <Link href="/seguimiento" className="card card-link">
          <h2>Consultar seguimiento</h2>
          <p>Con tu folio y correo puedes ver el estado y el historial.</p>
        </Link>
      </div>
    </>
  );
}
