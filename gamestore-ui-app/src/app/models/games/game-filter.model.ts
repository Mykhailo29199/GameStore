export interface GameDTO {
  name: string;
  key?: string;
  description?: string;
  price: number;
  unitInStock: number;
  discount: number;
}

export interface GameWithIdDTO extends GameDTO {
  id: string;
}

export interface SaveGameRequest {
  game: GameDTO & { id?: string };
  genres: string[];
  platforms: string[];
  publisher: string;
}

export interface GameFilterDTO {
  genres: string[];
  platforms: string[];
  publishers: string[];
  minPrice: number | null;
  maxPrice: number | null;
  datePublishing: string;
  name: string;
  sort: string;
  page: number;
  pageCount: string;
}

export interface FilteredGamesResponseDTO {
  games: GameWithIdDTO[];
  totalPages: number;
  currentPage: number;
}

export interface GameFilterOptions {
  sorting: string[];
  publishDates: string[];
  pageSizes: string[];
}

export const DEFAULT_PAGE_SIZE = '20';

export function createDefaultFilter(): GameFilterDTO {
  return {
    genres: [],
    platforms: [],
    publishers: [],
    minPrice: null,
    maxPrice: null,
    datePublishing: '',
    name: '',
    sort: '',
    page: 1,
    pageCount: DEFAULT_PAGE_SIZE,
  };
}
