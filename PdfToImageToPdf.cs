using Aspose.Pdf;
using Aspose.Pdf.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfToImageToPdf
{
    public class PdfBatchConverter
    {
        // ⬇️ تغییرات اینجا اعمال شد ⬇️
        private const int DpiResolution = 400; // افزایش از 300 به 400 (یا 600)
        private const int JpegQuality = 100;   // افزایش از 95 به 100 (حداکثر کیفیت)
        // ⬆️ ---------------------- ⬆️

        public void ConvertFolder(string rootFolder)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
            {
                Console.WriteLine("❌ پوشه معتبر نیست.");
                return;
            }

            rootFolder = Path.GetFullPath(rootFolder);
            Console.WriteLine($"=== تبدیل دسته‌ای PDF → تصویر → PDF (جایگزینی روی فایل اصلی) ===");
            Console.WriteLine($"کیفیت: {DpiResolution} DPI | JPEG: {JpegQuality}%");
            Console.WriteLine($"پوشه ریشه: {rootFolder}\n");

            List<string> pdfFiles;
            try
            {
                pdfFiles = Directory.EnumerateFiles(rootFolder, "*.pdf", SearchOption.AllDirectories)
                                     .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                     .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در پیمایش پوشه: {ex.Message}");
                return;
            }

            if (pdfFiles.Count == 0)
            {
                Console.WriteLine("هیچ فایل PDF پیدا نشد.");
                return;
            }

            Console.WriteLine($"📄 {pdfFiles.Count} فایل PDF پیدا شد.\n");

            int success = 0, failed = 0;
            var errors = new List<(string file, string error)>();

            for (int i = 0; i < pdfFiles.Count; i++)
            {
                string inputPdf = pdfFiles[i];
                string relative = Path.GetRelativePath(rootFolder, inputPdf);
                Console.WriteLine($"[{i + 1}/{pdfFiles.Count}] {relative}");

                try
                {
                    ProcessAndReplacePdf(inputPdf);
                    Console.WriteLine("   ✅ موفق (جایگزین شد)");
                    success++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ خطا: {ex.Message}");
                    errors.Add((relative, ex.Message));
                    failed++;
                }
            }

            Console.WriteLine("\n========== خلاصه ==========");
            Console.WriteLine($"✅ موفق:      {success}");
            Console.WriteLine($"❌ ناموفق:    {failed}");

            if (errors.Count > 0)
            {
                Console.WriteLine("\nفایل‌های ناموفق:");
                foreach (var (file, err) in errors)
                    Console.WriteLine($"  - {file}\n      {err}");
            }
        }

        private void ProcessAndReplacePdf(string inputPdfPath)
        {
            string tempImageFolder = Path.Combine(Path.GetTempPath(), "PdfImages_" + Guid.NewGuid().ToString("N"));
            string tempPdfPath = Path.Combine(Path.GetTempPath(), "TempPdf_" + Guid.NewGuid().ToString("N") + ".pdf");

            Directory.CreateDirectory(tempImageFolder);

            try
            {
                int pageCount;
                using (Document pdfDocument = new Document(inputPdfPath))
                {
                    pageCount = pdfDocument.Pages.Count;
                    var resolution = new Resolution(DpiResolution);
                    var jpegDevice = new JpegDevice(resolution, JpegQuality);

                    for (int i = 1; i <= pageCount; i++)
                    {
                        string imagePath = Path.Combine(tempImageFolder, $"page_{i:D4}.jpg");
                        using var imageStream = new FileStream(imagePath, FileMode.Create);
                        jpegDevice.Process(pdfDocument.Pages[i], imageStream);
                    }
                }

                using (Document newPdf = new Document())
                {
                    var imageFiles = Directory.GetFiles(tempImageFolder, "*.jpg").OrderBy(f => f).ToArray();

                    foreach (string imageFile in imageFiles)
                    {
                        Page page = newPdf.Pages.Add();
                        using var fs = new FileStream(imageFile, FileMode.Open, FileAccess.Read);
                        page.AddImage(fs, new Rectangle(0, 0, page.PageInfo.Width, page.PageInfo.Height));
                    }

                    newPdf.Save(tempPdfPath);
                }

                File.Copy(tempPdfPath, inputPdfPath, overwrite: true);
            }
            finally
            {
                if (Directory.Exists(tempImageFolder))
                {
                    try { Directory.Delete(tempImageFolder, true); } catch { }
                }
                if (File.Exists(tempPdfPath))
                {
                    try { File.Delete(tempPdfPath); } catch { }
                }
            }
        }
    }
}