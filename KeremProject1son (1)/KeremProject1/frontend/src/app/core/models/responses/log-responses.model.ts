export interface LogDto {
  id: number;
  tableName: string;
  oldValue?: string;
  newValue?: string;
  action: 'C' | 'U' | 'D';
  actionName: string;
  oldModUser?: number;
  oldModUsername?: string;
  oldModTime?: string;
  modUser: number;
  modUsername: string;
  modTime: string;
}

export interface LogListResponse {
  logs: LogDto[];
  totalCount: number;
  page: number;
  limit: number;
  totalPages: number;
}

