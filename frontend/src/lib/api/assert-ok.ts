export function assertOk<TData, TError>(
  result: { data?: TData; error?: TError; response: Response },
  message: string,
): TData {
  if (result.error !== undefined) {
    throw new Error(message, {
      cause: { status: result.response.status, error: result.error },
    });
  }

  return result.data as TData;
}
