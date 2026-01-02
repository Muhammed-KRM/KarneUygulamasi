import { ListMode } from '../models/enums/list-mode.enum';

const listModeApiMap: Record<ListMode, number> = {
  [ListMode.Ranked]: 0,
  [ListMode.Tiered]: 1,
  [ListMode.Fusion]: 2
};

export function mapListModeToApi(mode: ListMode): number {
  return listModeApiMap[mode] ?? listModeApiMap[ListMode.Ranked];
}

export function mapListModeFromApi(value: number | string | null | undefined): ListMode {
  if (value === null || value === undefined) {
    return ListMode.Ranked;
  }

  if (typeof value === 'number') {
    const entry = Object.entries(listModeApiMap).find(([, apiValue]) => apiValue === value);
    if (entry) {
      return entry[0] as ListMode;
    }
    return ListMode.Ranked;
  }

  const normalized = value.toString().toLowerCase();
  const match = Object.keys(listModeApiMap).find(
    (mode) => mode.toLowerCase() === normalized
  ) as ListMode | undefined;

  return match ?? ListMode.Ranked;
}

export function normalizeListMode<T extends { mode?: any }>(item: T): T {
  if (!item) {
    return item;
  }

  if ('mode' in item) {
    (item as any).mode = mapListModeFromApi((item as any).mode);
  }

  return item;
}

