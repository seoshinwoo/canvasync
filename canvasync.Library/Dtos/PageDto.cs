using canvasync.Library.Models;
using SkiaSharp;

namespace canvasync.Library.Dtos;

public class PageDto
{
    public string PDFId { get; set; } = string.Empty;
    public int PageIndex { get; set; } = 0;
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
    public List<FactorDto> FactorDtos { get; set; } = new();

    public static List<PageDto> PagesToPageDtos(List<Page> pages)
    {
        var pageDtos = new List<PageDto>();

        foreach (var page in pages)
        {
            var pageDto = new PageDto();

            pageDto.PDFId = page.PDFId;
            pageDto.PageIndex = page.PageIndex;
            pageDto.Width = page.Width;
            pageDto.Height = page.Height;

            foreach (var factor in page.Factors)
            {
                var factorDto = FactorDto.FactorToFactorDto(factor);
                pageDto.FactorDtos.Add(factorDto);
            }

            pageDtos.Add(pageDto);
        }
        
        return pageDtos;
    }
}