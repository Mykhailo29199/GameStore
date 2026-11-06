export interface Genre {
  id: string;
  name: string;
  parent?: string | null;
}

export interface Platform {
  id: string;
  type: string;
}

export interface Publisher {
  id: string;
  companyName: string;
  homePage?: string | null;
  description?: string | null;
}
