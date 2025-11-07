namespace Mix.Core.Entity;

public class BatchInfo
{
    public string Barcode { get; set; }

    public string DeviceCode { get; set; }

    public string ShiftName { get; set; }

    public string GroupName { get; set; }

    public string MaterialCode { get; set; }

    public string MaterialName { get; set; }

    public string BarcodeStart { get; set; }

    public string BarcodeEnd { get; set; }

    public string BarcodeRange => $"{BarcodeStart}→{BarcodeEnd}";

    public decimal StandardWeight { get; set; }

    public decimal FinalWeight { get; set; }

    public int IsPrinted { get; set; }

    public string ContainerNo { get; set; }
}
