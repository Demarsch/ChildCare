﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Wpf.Services;
using Core.Wpf.Misc;

namespace Core.Reports
{
    public class DocXReportGenerator : IReportGenerator
    {
        const string tempfolderprefix = "DocXReport";
        const string reportfileextention = "docx";
        private IFileService fileService;

        #region IReportGenerator implementation

        public string Template { get; set; }
        public string Title { get; set; }
        public bool Editable { get; set; }

        public DocXReportGenerator(IFileService fileService)
        {
            this.fileService = fileService;
            Data = new ReportData();
        }

        public ReportData Data { get; set; }

        public void Dispose()
        {
            if (Data == null)
                return;

            Data.Dispose();
            Data = null;
        }
        
        public string Save()
        {
            if (Template == null)
            {
                throw new NullReferenceException("Не указан шаблон для отчета");
            }

            string content = string.Empty;
            try
            {
                content = FillTemplate(Template, Data);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при заполнении шаблона отчета данными", ex);
            }

            string filename = string.Empty;
            try
            {
                filename = fileService.PrepareTempFileName(tempfolderprefix, Title, reportfileextention);

                using (Stream doc = fileService.CreateStreamForFile(filename, false))
                {
                    using (WordprocessingDocument package = WordprocessingDocument.Create(doc, WordprocessingDocumentType.Document))
                    {
                        MainDocumentPart mainPart = package.AddMainDocumentPart(); 
                        using (Stream st = mainPart.GetStream())
                        {
                            var buf = (new System.Text.UTF8Encoding()).GetBytes(content);
                            st.Write(buf, 0, buf.Length);
                        }

                        if (!Editable)
                        {
                            if (mainPart.DocumentSettingsPart == null) mainPart.AddNewPart<DocumentSettingsPart>();
                            if (mainPart.DocumentSettingsPart.Settings == null) mainPart.DocumentSettingsPart.Settings = new Settings();
                            mainPart.DocumentSettingsPart.Settings.RemoveAllChildren<DocumentProtection>();
                            mainPart.DocumentSettingsPart.Settings.AppendChild(new DocumentProtection() { Edit = DocumentProtectionValues.ReadOnly, Enforcement = new OnOffValue(true) });
                            mainPart.DocumentSettingsPart.Settings.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при создании файла отчета", ex);
            }

            return filename;
        }

        public string Show()
        {
            try
            {
                var filename = Save();
                fileService.RunFile(filename);
                return filename;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при открытии созданного файла отчета", ex);
            }
        }

        public string Print()
        {
            try
            {
                var filename = Save();
                fileService.RunFile(filename, FileServiceActions.Print);
                return filename;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при открытии созданного файла отчета", ex);
            }
        }

        public void LoadTemplateFromFile(string fileName)
        {
            try
            {
                using (Stream st = fileService.CreateStreamForFile(fileName))
                {
                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(st, false))
                    {
                        using (Stream stm = wordDoc.MainDocumentPart.GetStream())
                        {
                            using (StreamReader sr = new StreamReader(stm))
                            {
                                Template = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при загрузке шаблона отчета из файла", ex);
            }
        }

        #endregion

        #region template working

        const string parabegin = "<w:p ";
        const string parabeginfull = "<w:p>";
        const string paraend = "</w:p>";
        const int paraendlen = 6;
        
        const string rowbegin = "<w:tr ";
        const string rowbeginfull = "<w:tr>";
        const string rowend = "</w:tr>";
        const int rowendlen = 7;

        const string textbegin = "<w:t ";
        const string textbeginfull = "<w:t>";
        const string textend = "</w:t>";

        const char tagbegin = '<';
        const char tagend = '>';

        static string FillTemplate(string template, ReportData data)
        {
            string temp = template;

            // sections
            foreach (var sectionKey in data.Sections.GetUsedNames().OrderByDescending(x => x.Length))
            {
                for (int mark = FindFirstKey(temp, sectionKey, 0); mark >= 0; mark = FindFirstKey(temp, sectionKey, 0))
                {
                    int inpara = Math.Max(temp.LastIndexOf(parabegin, mark), temp.LastIndexOf(parabeginfull, mark));
                    if (inpara < 0) continue;

                    int outpara = temp.IndexOf(paraend, mark);
                    if (outpara < 0) continue;

                    int mark1 = FindFirstKey(temp, sectionKey, outpara);
                    if (mark1 < 0) continue;

                    int inpara1 = Math.Max(temp.LastIndexOf(parabegin, mark1), temp.LastIndexOf(parabeginfull, mark1));
                    if (inpara1 < 0) continue;

                    int outpara1 = temp.IndexOf(paraend, mark1);
                    if (outpara1 < 0) continue;

                    string subtemp = temp.Substring(outpara + paraendlen, inpara1 - outpara - paraendlen);
                    string sections = string.Empty;
                    foreach (var sectionIndex in data.Sections[sectionKey].GetUsedIndexes().OrderBy(x => x))
                        sections += FillTemplate(subtemp, data[sectionKey, sectionIndex]);

                    temp = temp.Substring(0, inpara) + sections + temp.Substring(outpara1 + paraendlen);
                }
            }

            // tables

            foreach (var tableKey in data.Tables.GetUsedNames().OrderByDescending(x => x.Length))
            {
                for (int mark = FindFirstKey(temp, tableKey, 0); mark >= 0; mark = FindFirstKey(temp, tableKey, 0))
                {
                    int inrow = Math.Max(temp.LastIndexOf(rowbegin, mark), temp.LastIndexOf(rowbeginfull, mark));
                    if (inrow < 0) continue;

                    int outrow = temp.IndexOf(rowend, mark);
                    if (outrow < 0) continue;

                    string rowtemp = temp.Substring(inrow, outrow - inrow + rowendlen);
                    string rows = string.Empty;
                    var rowindexes = data.Tables[tableKey].GetUsedRowIndexes();
                    if (rowindexes.Any())
                    {
                        int rowcount = rowindexes.Max() + 1;
                        int tablekeylen = tableKey.Length;
                        for (int rowIndex = 0; rowIndex < rowcount; rowIndex++)
                        {
                            string row = rowtemp;
                            for (int colIndex = 0, cell = FindFirstKey(row, tableKey, 0); cell >= 0; colIndex++, cell = FindFirstKey(row, tableKey, 0))
                            {
                                if (data.Tables[tableKey].IsCellUsed(rowIndex, colIndex))
                                    row = row.Substring(0, cell) + GetValueText(data[tableKey, colIndex, rowIndex]) + row.Substring(cell + tablekeylen);
                                else 
                                    row = row.Substring(0, cell) + row.Substring(cell + tablekeylen);
                            }
                            rows += row;
                        }
                    }
                    temp = temp.Substring(0, inrow) + rows + temp.Substring(outrow + rowendlen);
                }
            }

            // fields

            foreach (var fieldKey in data.GetUsedFields().OrderByDescending(x => x.Length))
            {
                for (int begin = 0, mark = FindFirstKey(temp, fieldKey, 0); mark >= 0; mark = FindFirstKey(temp, fieldKey, begin))
                {
                    string text = GetValueText(data[fieldKey]);
                    temp = temp.Substring(0, mark) + text + temp.Substring(mark + fieldKey.Length);
                    begin = mark + text.Length;
                }
            }

            return temp;
        }

        static int FindFirstKey(string template, string key, int start)
        {
            int mark = template.IndexOf(key, start, StringComparison.CurrentCultureIgnoreCase);
            if (mark < 0) return -1;

            int intext = Math.Max(template.LastIndexOf(textbeginfull, mark), template.LastIndexOf(textbegin, mark));
            if (intext < 0) return -1;

            int wrong = template.IndexOf(tagbegin, intext + 1, mark - intext);
            if (wrong >= 0) return -1;

            intext = template.IndexOf(textend, mark);
            if (intext < 0) return -1;

            wrong = template.IndexOf(tagend, mark, intext - mark - 1);
            if (wrong >= 0) return -1;

            return mark;
        }

        static string GetValueText(object value)
        {
            if (value == null)
                return string.Empty;

            return value.ToString()
                .Replace(" <b>", "###b")
                .Replace("<b> ", "###b")
                .Replace("<b>", "##b")

                .Replace(" <u>", "###u")
                .Replace("<u> ", "###u")
                .Replace("<u>", "##u")

                .Replace(" </b>", "###/b")
                .Replace("</b> ", "###/b")
                .Replace("</b>", "##/b")

                .Replace(" </u>", "###/u")
                .Replace("</u> ", "###/u")
                .Replace("</u>", "##/u")

                .Replace("<br>", Environment.NewLine)

                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\t", "&#09;")
                .Replace("\"", "&quot;")

                .Replace("###b", "</w:t><w:t xml:space=\"preserve\"> </w:t><w:rPr><w:b w:val=\"true\"/></w:rPr><w:t>")
                .Replace("##b", "</w:t><w:rPr><w:b w:val=\"true\"/></w:rPr><w:t>")

                .Replace("###u", "</w:t><w:t xml:space=\"preserve\"> </w:t><w:rPr><w:u w:val=\"single\"/></w:rPr><w:t>")
                .Replace("##u", "</w:t><w:rPr><w:u w:val=\"single\"/></w:rPr><w:t>")

                .Replace("###/b", "</w:t><w:rPr><w:b w:val=\"false\"/></w:rPr><w:t xml:space=\"preserve\"> </w:t><w:t>")
                .Replace("##/b", "</w:t><w:rPr><w:b w:val=\"false\"/></w:rPr><w:t>")

                .Replace("###/u", "</w:t><w:rPr><w:u w:val=\"none\"/></w:rPr><w:t xml:space=\"preserve\"> </w:t><w:t>")
                .Replace("##/u", "</w:t><w:rPr><w:u w:val=\"none\"/></w:rPr><w:t>")

                .Replace(Environment.NewLine, "</w:t><w:br/><w:t>")
                .Replace("\n", "</w:t><w:br/><w:t>")
                .Replace("\r", "</w:t><w:br/><w:t>");
        }

        #endregion
    }
}
