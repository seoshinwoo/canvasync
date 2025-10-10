namespace canvasync.Library.Models;

public class FactorData
{
    public int PageIndex { get; set; }
    public int FactorIndex { get; set; }
    public Factor Factor { get; set; } = new();
}