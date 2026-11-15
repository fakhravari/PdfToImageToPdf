using PdfToImageToPdf;

class Program
{
    static void Main()
    {
        string rootFolder = @"D:\Repositories\GitHub\EducationNotCopy\JavaScript";

        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
        {
            Console.WriteLine("Usage: PdfToImageToPdf <folder-containing-pdfs>");
            Console.WriteLine("Or set the PDF_ROOT environment variable.");
            return;
        }

        new Aspose.Pdf.License().SetLicense(Path.Combine(AppContext.BaseDirectory, "Aspose.Total.NET.lic"));

        var converter = new PdfBatchConverter();
        converter.ConvertFolder(rootFolder);
    }
}
