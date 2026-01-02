import { ListMode } from '../enums/list-mode.enum';

export interface SyncMalRequest {
  listId: number;
  mode: ListMode;
  replaceExisting: boolean;
}

