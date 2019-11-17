namespace MaterialDrying
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    public class XlsxExporter
    {
        private readonly IDictionary<int, string> _dict = new Dictionary<int, string>
                                                        {
                                                            [0] = "A",
                                                            [1] = "B",
                                                            [2] = "C",
                                                            [3] = "D",
                                                            [4] = "E",
                                                            [5] = "F",
                                                            [6] = "G",
                                                            [7] = "H",
                                                            [8] = "I",
                                                            [9] = "J",
                                                            [10] = "K",
                                                            [11] = "L",
                                                            [12] = "M",
                                                            [13] = "N",
                                                            [14] = "O",
                                                            [15] = "P",
                                                            [16] = "Q",
                                                            [17] = "R",
                                                            [18] = "S",
                                                            [19] = "T",
                                                            [20] = "U",
                                                            [21] = "V",
                                                            [22] = "W",
                                                            [23] = "X",
                                                            [24] = "Y",
                                                            [25] = "Z",
                                                        };
        
        public void Export(string fileName, IEnumerable<ExportData> exportData)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName + ".xlsx");

            using (var spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                var workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                // Add Sheets to the Workbook.
                var sheets = spreadsheetDocument.WorkbookPart
                                                .Workbook
                                                .AppendChild(new Sheets());

                // Append a new worksheet and associate it with the workbook.
                var sheet = new Sheet
                              {
                                  Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                                  SheetId = 1,
                                  Name = nameof(MaterialDrying)
                              };
                sheets.Append(sheet);

                var propertyInfos = InsertHeader(exportData, sheetData);
                InsertData(exportData, sheetData, propertyInfos);

                workbookpart.Workbook.Save();
                spreadsheetDocument.Close();
            }
        }

        private void InsertData(IEnumerable<ExportData> exportData,
                                SheetData sheetData,
                                PropertyInfo[] propertyInfos)
        {
            var rows = exportData.Select(data =>
                                         {
                                             var rowIndex = data.Step + 2;
                                             
                                             var row = new Row { RowIndex = rowIndex };

                                             var cells = propertyInfos.Select((p, i) => new Cell
                                                                            {
                                                                                CellReference = $"{GetColumnName(i)}{rowIndex}",
                                                                                CellValue = new CellValue(p.GetValue(data).ToString()),
                                                                                DataType = CellValues.Number
                                                                            });

                                             row.Append(cells);
                                             
                                             return row;
                                         });

            sheetData.Append(rows);
        }

        private PropertyInfo[] InsertHeader(IEnumerable<ExportData> exportData, SheetData sheetData)
        {
            var headerRow = new Row { RowIndex = 1 };

            var properties = exportData.GetType()
                                       .GetGenericArguments()[0]
                                       .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                       .Where(z => z.Name != nameof(ExportData.Step))
                                       .ToArray();

            var headerCells = properties.Select((prop, i) => new Cell
                                                             {
                                                                 CellReference = $"{GetColumnName(i)}{1}",
                                                                 CellValue = new CellValue(prop.Name),
                                                                 DataType = CellValues.String
                                                             });

            headerRow.Append(headerCells);
            sheetData.Append(headerRow);

            return properties;
        }

        private StringValue GetColumnName(int i)
        {
            if (!_dict.TryGetValue(i, out var ch))
            {
                Console.WriteLine("Too much columns (max = 26)");
                throw new IndexOutOfRangeException(i.ToString());
            }

            return ch;
        }
    }
}