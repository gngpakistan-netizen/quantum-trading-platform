import { corsHeaders } from './router';

export function handleCors(request: Request): Response {
  return new Response(null, {
    status: 204,
    headers: corsHeaders(),
  });
}
