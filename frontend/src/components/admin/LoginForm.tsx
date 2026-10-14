"use client";

import { useActionState } from "react";
import { loginAction } from "@/app/admin/actions";
import type { ActionState } from "@/app/admin/state";
import { Alert } from "@/components/Alert";
import { Field } from "@/components/Field";
import { SubmitButton } from "@/components/SubmitButton";

const initialState: ActionState = {};

export function LoginForm({ notice }: { notice?: string }) {
  const [state, formAction] = useActionState(loginAction, initialState);

  return (
    <form action={formAction} className="card form narrow" noValidate>
      <h2>Acceso del personal</h2>
      {notice && <Alert kind="info">{notice}</Alert>}
      {state.error && <Alert>{state.error}</Alert>}

      <Field id="apiKey" label="Clave de acceso" error={state.fieldErrors?.apiKey}>
        {(props) => <input {...props} name="apiKey" type="password" autoComplete="off" />}
      </Field>

      <SubmitButton pendingText="Verificando...">Ingresar</SubmitButton>
    </form>
  );
}
