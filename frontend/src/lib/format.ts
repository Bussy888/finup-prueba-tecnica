const TIME_ZONE = "America/La_Paz";

const dateTime = new Intl.DateTimeFormat("es-BO", {
  dateStyle: "medium",
  timeStyle: "short",
  timeZone: TIME_ZONE,
});

const dateOnly = new Intl.DateTimeFormat("es-BO", {
  dateStyle: "long",
  timeZone: TIME_ZONE,
});

export function formatDateTime(iso: string): string {
  return dateTime.format(new Date(iso));
}

export function formatDate(iso: string): string {
  return dateOnly.format(new Date(iso));
}
