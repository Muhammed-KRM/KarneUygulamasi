export interface BaseResponse<T = any> {
  success?: boolean;
  message?: string;
  errorCode?: number;
  response?: T;
  userId?: number;
  errored?: boolean;
  errorMessage?: string;
  returnValue?: number;
}

