"use client";

import { useActionState } from "react";
import { changeStatusAction } from "@/app/admin/actions";
import type { ActionState } from "@/app/admin/state";
import { Alert } from "@/components/Alert";
import { Field } from "@/components/Field";
import { SubmitButton } from "@/components/SubmitButton";
import { allowedTransitions, statusLabels } from "@/lib/claims";
import type { ClaimStatus } from "@/lib/types";

const initialState: ActionState = {};

interface ChangeStatusFormProps {
  folio: string;
  currentStatus: ClaimStatus;
}

export function ChangeStatusForm({ folio, currentStatus }: ChangeStatusFormProps) {
  const [state, formAction] = useActionState(changeStatusAction, initialState);
  const options = allowedTransitions(currentStatus);

  if (options.length === 0) {
    return <p className="muted">Este reclamo está cerrado y no admite más cambios.</p>;
  }

  return (
    <form action={formAction} className="form" noValidate>
      <h3>Cambiar estado</h3>
      {state.error && <Alert>{state.error}</Alert>}
      {state.success && <Alert kind="success">{state.success}</Alert>}

      <input type="hidden" name="folio" value={folio} />

      <Field id="status" label="Nuevo estado" error={state.fieldErrors?.status}>
        {(props) => (
          <select {...props} name="status" defaultValue={options[0]}>
            {options.map((status) => (
              <option key={status} value={status}>
                {statusLabels[status]}
              </option>
            ))}
          </select>
        )}
      </Field>

      <Field
        id="comment"
        label="Comentario"
        error={state.fieldErrors?.comment}
        hint="Obligatorio al resolver o rechazar."
      >
        {(props) => <textarea {...props} name="comment" rows={3} />}
      </Field>

      <SubmitButton pendingText="Guardando...">Guardar cambio</SubmitButton>
    </form>
  );
}
