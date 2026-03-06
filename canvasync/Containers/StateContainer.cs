using canvasync.Components.Pages;
using Microsoft.AspNetCore.Components.Forms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using canvasync.Library.Models;
using SkiaSharp;
using canvasync.Library.Dtos;
using PdfSharp.Drawing;
using PdfSharp.Snippets.Drawing;
using System.Drawing;

namespace canvasync.Containers;

public class StateContainer
{
    public List<Lecture> Lectures { get; set; } = new();

    public byte[] CreateOverlayPdf(DrawingsDto drawingsDto)
    {
        // 1. DTO나 페이지 리스트가 null이거나 비어있는지 확인
        if (drawingsDto?.PageDtos == null || !drawingsDto.PageDtos.Any())
        {
            // 그릴 페이지가 없으므로 빈 배열을 즉시 반환
            return Array.Empty<byte>();
        }

        using var ms = new MemoryStream();
        using (var document = SKDocument.CreatePdf(ms))
        {
            foreach (var pageDto in drawingsDto.PageDtos)
            {
                using (var canvas = document.BeginPage(pageDto.Width, pageDto.Height))
                {
                    canvas.Clear(SKColors.Transparent);

                    var factors = new List<Factor>();

                    foreach (var factorDto in pageDto.FactorDtos)
                    {
                        factors.Add(FactorDto.FactorDtoToFactor(factorDto));
                    }

                    foreach (var factor in factors)
                    {
                        factor.Draw(canvas);
                    }
                }
            }
        }

        return ms.ToArray();
    }

    public byte[] MergePdfs(byte[] basePdfBytes, byte[] overlayPdfBytes)
    {
        // using (MemoryStream overlayStream = new MemoryStream(overlayPdfBytes))
        // {
        //     return overlayStream.ToArray();    
        // }

        // Console.WriteLine($"overlayPdfBytes 길이 : {overlayPdfBytes.Length}");
        string tempOverlayFile = Path.GetTempFileName();
        try
        {
            // 1. 오버레이 PDF 바이트 배열을 임시 파일로 저장합니다.
            File.WriteAllBytes(tempOverlayFile, overlayPdfBytes);

            // 2. XPdfForm.FromFile을 사용하여 임시 파일로부터 XPdfForm 객체를 생성합니다.
            // XPdfForm은 재사용이 가능하므로, 루프 밖에서 한 번만 생성합니다.
            using (XPdfForm overlayForm = XPdfForm.FromFile(tempOverlayFile))
            {
                // 3. 배경 PDF를 MemoryStream으로 열어 수정 준비를 합니다.
                using (MemoryStream baseStream = new MemoryStream(basePdfBytes))
                using (PdfDocument basePdf = PdfReader.Open(baseStream, PdfDocumentOpenMode.Modify))
                {
                    // 4. 배경 PDF의 모든 페이지를 순회합니다.
                    for (int i = 0; i < basePdf.PageCount; i++)
                    {
                        // 오버레이할 페이지가 더 이상 없으면 중단합니다.
                        if (i >= overlayForm.PageCount)
                        {
                            break;
                        }
                        
                        PdfPage basePage = basePdf.Pages[i];

                        // 5. 배경 페이지 위에 그림을 그릴 XGraphics 객체를 생성합니다.
                        using (XGraphics gfx = XGraphics.FromPdfPage(basePage, XGraphicsPdfPageOptions.Append))
                        {
                            // 6. XPdfForm에서 특정 페이지를 지정하여 그립니다.
                            // 페이지 인덱스는 1부터 시작합니다. (overlayForm.PageNumber)
                            overlayForm.PageNumber = i + 1;

                            // 배경 페이지 전체에 꽉 차게 오버레이 페이지를 그립니다.
                            gfx.DrawImage(overlayForm, new XRect(0, 0, basePage.Width.Point, basePage.Height.Point));
                            var pen = new XPen(XColor.FromKnownColor(XKnownColor.Aqua));
                        }
                    }

                    // 7. 변경된 문서를 새로운 MemoryStream에 저장하고 byte[]로 반환합니다.
                    using (MemoryStream resultStream = new MemoryStream())
                    {
                        basePdf.Save(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }
        finally
        {
            // 8. 작업이 끝나면 반드시 임시 파일을 삭제합니다.
            if (File.Exists(tempOverlayFile))
            {
                File.Delete(tempOverlayFile);
            }
        }
    }

    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();
}