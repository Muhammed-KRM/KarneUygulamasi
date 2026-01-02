export function buildRequestPayload<T>(payload: T): { request: T } {
  return { request: payload };
}

