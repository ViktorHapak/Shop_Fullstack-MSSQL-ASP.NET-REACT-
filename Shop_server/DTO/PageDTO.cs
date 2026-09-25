using System.ComponentModel.DataAnnotations;

namespace Shop_server.DTO
{
    public class PageDTO<T>
    {
            public List<T> Items { get; init; } = new List<T>();

            public int TotalItems { get; init; }

            [Range(1, 100,
            ErrorMessage = "Az oldalméret 1 és 100 között lehet!")]
            public int Size { get; init; }

            public int TotalPages =>
                Size == 0
                    ? 0
                    : (int)Math.Ceiling(
                        TotalItems / (double)Size);

            [Range(0, int.MaxValue,
            ErrorMessage = "Az oldalszám nem lehet negatív.")]
            public int Page { get; init; }

            public bool HasPreviousPage => Page > 1;

            public bool HasNextPage => Page < TotalPages;
    }
}
