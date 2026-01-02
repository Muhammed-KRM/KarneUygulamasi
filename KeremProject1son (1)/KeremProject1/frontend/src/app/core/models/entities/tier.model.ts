import { RankedItem } from './ranked-item.model';

export interface Tier {
  id: number;
  title: string;
  color: string;
  order: number;
  items: RankedItem[];
}

