import { BaseResponse } from '../models/responses/base-response.model';

export function isResponseSuccessful<T>(response: BaseResponse<T> | null | undefined): response is BaseResponse<T> {
  if (!response) {
    return false;
  }

  if (typeof response.success === 'boolean') {
    return response.success;
  }

  if (typeof response.errored === 'boolean') {
    return response.errored === false;
  }

  const returnValue = (response as any)?.returnValue;
  if (typeof returnValue === 'number') {
    return returnValue === 0;
  }

  return !!response.response;
}

