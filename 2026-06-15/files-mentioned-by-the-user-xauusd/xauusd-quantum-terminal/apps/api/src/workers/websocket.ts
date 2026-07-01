import type { Env } from '../index';

export async function wsHandler(request: Request, env: Env): Promise<Response> {
  const upgrade = request.headers.get('Upgrade');
  if (upgrade !== 'websocket') {
    return new Response('Expected WebSocket upgrade', { status: 426 });
  }
  const [client, server] = Object.values(new WebSocketPair());
  server.accept();
  server.send(JSON.stringify({ type: 'CONNECTED', symbol: 'XAUUSD', timestamp: new Date().toISOString() }));
  const interval = setInterval(() => {
    try {
      const price = 2348.65 + (Math.random() - 0.5) * 0.4;
      server.send(JSON.stringify({
        type: 'TICK',
        symbol: 'XAUUSD',
        price: parseFloat(price.toFixed(3)),
        timestamp: new Date().toISOString(),
      }));
    } catch { clearInterval(interval); }
  }, 1200);
  server.addEventListener('close', () => clearInterval(interval));
  return new Response(null, { status: 101, webSocket: client });
}
