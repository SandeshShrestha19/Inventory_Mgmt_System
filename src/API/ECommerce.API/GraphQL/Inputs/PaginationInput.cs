public record PaginationInput(
  Guid? CursorId = null,
  int PageSize = 5
);