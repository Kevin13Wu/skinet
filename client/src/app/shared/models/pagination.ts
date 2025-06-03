export type Pagination<T> = {
  pageIndex: number;
  pageSize: number;
  data: T[];
  count: number;
};
