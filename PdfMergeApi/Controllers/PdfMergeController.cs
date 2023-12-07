

using Microsoft.AspNetCore.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfMergeApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PdfMergeController : ControllerBase
{

    private readonly ILogger<PdfMergeController> _logger;

    public PdfMergeController(ILogger<PdfMergeController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult MergePdf([FromBody] Contract contract)
    {
        if (contract == null)
            return BadRequest("Send at least two base64 strings to match.");

        if (contract.Documents.Length == 1)
            Ok(contract.Documents);

        try
        {
            List<byte[]> pdfBytesList = new List<byte[]>();

            foreach (string base64String in contract.Documents)
                pdfBytesList.Add(Convert.FromBase64String(base64String));

            PdfDocument mergedDocument = new PdfDocument();

            foreach (byte[] pdfBytes in pdfBytesList)
            {
                PdfDocument doc = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.Import);
                foreach (PdfPage page in doc.Pages)
                {
                    PdfPage newPage = mergedDocument.AddPage(page);
                }
            }

            byte[] mergedBytes;
            using (MemoryStream stream = new MemoryStream())
            {
                mergedDocument.Save(stream, false);
                mergedBytes = stream.ToArray();
            }

            string combinedBase64 = Convert.ToBase64String(mergedBytes);

            return Ok(combinedBase64);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to combine and convert documents: " + ex.Message);
        }
    }

    public class Contract
    {
        public string LaunchId { get; set; }
        public string[] Documents { get; set; }
    }
}
