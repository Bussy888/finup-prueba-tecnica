"use client";

import { useFormStatus } from "react-dom";

interface SubmitButtonProps {
  children: string;
  pendingText?: string;
}

export function SubmitButton({
  children,
  pendingText = "Enviando...",
}: SubmitButtonProps) {
  const { pending } = useFormStatus();

  return (
    <button type="submit" className="btn btn-primary" disabled={pending}>
      {pending ? pendingText : children}
    </button>
  );
}
