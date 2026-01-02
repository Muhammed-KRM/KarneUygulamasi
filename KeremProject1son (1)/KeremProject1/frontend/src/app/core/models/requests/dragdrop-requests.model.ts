export interface MoveItemRequest {
  itemId: number;
  targetTierId: number;
  newRankInTier: number;
}

export interface ReorderItemsRequest {
  tierId: number;
  items: Array<{
    itemId: number;
    rankInTier: number;
  }>;
}

