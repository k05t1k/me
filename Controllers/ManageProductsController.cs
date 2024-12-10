using Azure.Core;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[Authorize(Roles = "Manager,Admin")]
public class ManageProductsController : Controller
{
    public ActionResult Index()
    {
        int totalQuantity = int.TryParse(Request.Cookies["TotalQuantity"], out totalQuantity) ? totalQuantity : 0;
        decimal totalAmount = decimal.TryParse(Request.Cookies["TotalAmount"], out totalAmount) ? totalAmount : 0m;

        var productSales = Request.Cookies["ProductSales"];
        var productSalesDict = string.IsNullOrEmpty(productSales) ? new Dictionary<string, int>() : JsonConvert.DeserializeObject<Dictionary<string, int>>(productSales);

        ViewBag.TotalQuantity = totalQuantity;
        ViewBag.TotalAmount = totalAmount;
        ViewBag.ProductSales = productSalesDict;

        return View();
    }

    [HttpPost]
    public ActionResult ExportToPdf()
    {
        int totalQuantity = int.TryParse(Request.Cookies["TotalQuantity"], out totalQuantity) ? totalQuantity : 0;
        decimal totalAmount = decimal.TryParse(Request.Cookies["TotalAmount"], out totalAmount) ? totalAmount : 0m;

        var productSales = Request.Cookies["ProductSales"];
        var productSalesDict = string.IsNullOrEmpty(productSales) ? new Dictionary<string, int>() : JsonConvert.DeserializeObject<Dictionary<string, int>>(productSales);

        using (MemoryStream stream = new MemoryStream())
        {
            PdfWriter writer = new PdfWriter(stream);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc);

            document.Add(new Paragraph($"Total Quantity: {totalQuantity}"));
            document.Add(new Paragraph($"Total Amount: {totalAmount}"));
            document.Add(new Paragraph("Product Sales:"));
            foreach (var item in productSalesDict)
            {
                document.Add(new Paragraph($"{item.Key}: {item.Value}"));
            }
            document.Close();

            byte[] bytes = stream.ToArray();
            stream.Close();

            return File(bytes, "application/pdf", "ProductData.pdf");
        }
    }
}
