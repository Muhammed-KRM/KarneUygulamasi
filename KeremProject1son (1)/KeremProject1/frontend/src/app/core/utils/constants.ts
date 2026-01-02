// API Endpoints
export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register'
  },
  MAL: {
    GET_AUTH_URL: '/mal/get-auth-url',
    CALLBACK: '/mal/callback',
    GET_MY_LIST: '/mal/get-my-list'
  },
  LIST: {
    CREATE: '/list/create',
    SAVE: '/list/save',
    GET: '/list',
    GET_ALL: '/list/all',
    DELETE: '/list',
    UPDATE_TITLE: '/list',
    CONVERT_TO_FUSION: '/list/convert-to-fusion',
    CONVERT_TO_RANKED: '/list/convert-to-ranked'
  },
  ITEM: {
    ADD: '/list/item/add',
    REMOVE: '/list/item/remove'
  },
  TIER: {
    ADD: '/list/tier/add',
    UPDATE: '/list/tier/update',
    REMOVE: '/list/tier/remove'
  },
  GENERATE: {
    BY_SCORE: '/generate/by-score',
    BY_YEAR: '/generate/by-year',
    BY_GENRE: '/generate/by-genre',
    GENRES: '/generate/genres'
  },
  SEARCH: {
    ANIME: '/search/anime'
  },
  SHARE: {
    SET_VISIBILITY: '/share/set-visibility',
    GENERATE_LINK: '/share/generate-link',
    PUBLIC: '/share/public',
    DELETE_LINK: '/share/link'
  },
  SOCIAL: {
    LIKE: '/social/like',
    FOLLOW: '/social/follow',
    PROFILE: '/social/profile',
    NOTIFICATIONS: '/social/notifications',
    TEMPLATE: '/social/template'
  },
  COMMENT: {
    ADD: '/comment/add',
    LIST: '/comment/list',
    UPDATE: '/comment/update',
    DELETE: '/comment'
  },
  USER: {
    UPLOAD_IMAGE: '/user/upload-image',
    GET: '/user',
    GET_ME: '/user/me',
    GET_ALL: '/user/all',
    SEARCH: '/user/search',
    UPDATE: '/user/update',
    CHANGE_PASSWORD: '/user/change-password',
    PROFILE: '/user/profile',
    DELETE: '/user'
  },
  FILE: {
    DOWNLOAD: '/file/download',
    INFO: '/file/info',
    CLEAN_TEMP: '/file/clean-temp'
  },
  STATISTICS: {
    USER: '/statistics/user',
    ME: '/statistics/me'
  },
  ACTIVITY: {
    USER: '/activity/user',
    ME: '/activity/me'
  },
  RECOMMENDATION: {
    ANIME: '/recommendation/anime',
    TRENDING: '/recommendation/trending'
  },
  COPY: {
    LIST: '/copy/list'
  },
  EXPORT: {
    IMAGE: '/export/image',
    EMBED: '/export/embed'
  },
  SYNC: {
    MAL: '/sync/mal'
  },
  DRAGDROP: {
    MOVE_ITEM: '/dragdrop/move-item',
    REORDER_ITEMS: '/dragdrop/reorder-items'
  }
};

// Default Tier Colors
export const DEFAULT_TIER_COLORS = {
  S: '#ff7f7f',
  A: '#ffbf7f',
  B: '#ffff7f',
  C: '#bfff7f',
  D: '#7fffff',
  F: '#bf7fff'
};

// Default Tier Names
export const DEFAULT_TIER_NAMES = ['S', 'A', 'B', 'C', 'D', 'F'];

// Pagination
export const DEFAULT_PAGE_SIZE = 20;
export const MAX_PAGE_SIZE = 100;

// Validation
export const VALIDATION = {
  USERNAME_MIN_LENGTH: 3,
  USERNAME_MAX_LENGTH: 50,
  PASSWORD_MIN_LENGTH: 6,
  LIST_TITLE_MAX_LENGTH: 100,
  TIER_TITLE_MAX_LENGTH: 50,
  COMMENT_MAX_LENGTH: 500
};

// File Upload
export const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
export const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];

