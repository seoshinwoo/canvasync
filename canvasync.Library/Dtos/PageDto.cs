using canvasync.Library.Models;
using SkiaSharp;

namespace canvasync.Library.Dtos;

public class PageDto
{
    public int PageIndex { get; set; } = 0;
    public string ImgData { get; set; } = string.Empty;
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
    public List<FactorDto> FactorDtos { get; set; } = new();

    public static List<PageDto> PagesToPageDtos(List<Page> pages)
    {
        var pageDtos = new List<PageDto>();

        foreach (var page in pages)
        {
            var pageDto = new PageDto();

            pageDto.PageIndex = page.PageIndex;
            pageDto.ImgData = page.ImgData;
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
    
    public static List<Page> PageDtosToPages(List<PageDto> pageDtos)
    {
        var pages = new List<Page>();

        foreach (var pageDto in pageDtos)
        {
            var page = new Page();

            page.PageIndex = pageDto.PageIndex;
            page.ImgData = pageDto.ImgData;
            page.Width = pageDto.Width;
            page.Height = pageDto.Height;

            foreach (var factorDto in pageDto.FactorDtos)
            {
                var factor = FactorDto.FactorDtoToFactor(factorDto);
                page.Factors.Add(factor);
            }

            pages.Add(page);
        }

        return pages;
    }
}