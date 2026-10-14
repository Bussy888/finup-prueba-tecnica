const publicApiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5282";

export const browserApiUrl = publicApiUrl;
export const serverApiUrl = process.env.API_URL ?? publicApiUrl;
