using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Myriad.Library;

namespace Myriad.Formatter
{
    public class FigureFormatter
    {
        private readonly HTMLWriter writer;

        public FigureFormatter(HTMLWriter writer)
        {
            this.writer = writer;
        }
        public async Task GroupPictures(List<string> textofParagraphs)
        {
            List<ImageElement> elements = new List<ImageElement>();
            foreach (string paragraph in textofParagraphs)
            {
                if ((paragraph.Length > 2) && (paragraph.Substring(0, 2) == "[["))
                {
                    ImageElement imageElement = GetImageElement(paragraph);
                    if (imageElement != null) elements.Add(imageElement);
                }
            }
            int k = Ordinals.first;
            while (k < elements.Count)
            {
                if ((elements.Count - k) >= 4)
                {
                    await FigureElement(elements[k], elements[k + 1], elements[k + 2], elements[k + 3]);
                    k += 4;
                }
                else
                if ((elements.Count - k) == 3)
                {
                    await FigureElement(elements[k], elements[k + 1], elements[k + 2]);
                    k += 3;
                }
                else if ((elements.Count - k) == 2)
                {
                    await FigureElement(elements[k], elements[k + 1]);
                    k += 2;
                }
                else
                {
                    await writer.Append(FigureElement(elements[k]));
                    k++;
                }
            }
        }
        private static ImageElement GetImageElement(string p)
        {

            string filename = p.Replace("[[", "").Replace("]]", "");
            ImageElement result = new ImageElement(filename);
            return (result.Valid) ?
                  result :
                  null;
        }

        private async Task FigureElement(ImageElement imageElement1, ImageElement imageElement2)
        {
            string pattern = imageElement1.Pattern + imageElement2.Pattern;
            if ((pattern == "pl") || (pattern == "ss") || (pattern == "sl") || (pattern == "ll") || (pattern == "ls"))
            {
                await FigurePL(imageElement1, imageElement2);
                return;
            }
            if (pattern == "lp")
            {
                await FigureLP(imageElement1, imageElement2);
                return;
            }
            if ((pattern == "sp") || (pattern == "ps") || (pattern == "pp"))
            {
                await FigurePL(imageElement1, imageElement2);
                return;
            }
            await writer.Append(HTMLTags.StartFigure+HTMLTags.StartParagraph+"Error " + pattern + HTMLTags.EndParagraph+HTMLTags.EndFigure);
        }

        private async Task FigurePL(ImageElement imageElement1, ImageElement imageElement2)
        {
            AdjustProportions(imageElement1, imageElement2);
            await writer.Append(HTMLTags.StartFigureWithClass +
                HTMLClasses.landscape +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(imageElement1.LeftSiblingString);
            await writer.Append(imageElement2.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }

        private static void AdjustProportions(ImageElement imageElement1, ImageElement imageElement2)
        {
            double heightratio = imageElement1.Height / imageElement2.Height;
            double newWidth = imageElement1.Width / heightratio;
            double totalWidth = newWidth + imageElement2.Width;
            imageElement1.WidthPercentage = newWidth / totalWidth;
            imageElement2.WidthPercentage = (1 - imageElement1.WidthPercentage) * .99;
            imageElement1.WidthPercentage *= .99;
        }

        private async Task FigureLP(ImageElement imageElement1, ImageElement imageElement2)
        {
            AdjustProportions(imageElement2, imageElement1);
            await writer.Append(HTMLTags.StartFigureWithClass +
                HTMLClasses.landscape +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(imageElement1.LeftSiblingString);
            await writer.Append(imageElement2.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }

        private static string FigureElement(ImageElement imageElement)
        {
            if (imageElement == null) return "";
            return "<figure class=" + imageElement.Class + ">" + imageElement.OnlyChildString + "</figure>";
        }

        private async Task FigureElement(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3, ImageElement imageElement4)
        {
            string pattern = imageElement1.Pattern + imageElement2.Pattern + imageElement3.Pattern +
                imageElement4.Pattern;
            /*      if ((pattern == "ppl") || (pattern == "pps")) return FigurePPL(imageElement1, imageElement2, imageElement3);
                  if ((pattern == "plp") || (pattern == "psp")) return FigurePPL(imageElement1, imageElement3, imageElement2);
                  if ((pattern == "lpp") || (pattern == "spp")) return FigureLPP(imageElement1, imageElement2, imageElement3);
                  if (pattern == "lls") return FigureLLS(imageElement1, imageElement2, imageElement3);
                  if (pattern == "sps") return FigureSPS(imageElement1, imageElement2, imageElement3);
                  if (pattern == "lll") return FigureLLL(imageElement1, imageElement2, imageElement3); */
            if ((pattern == "spss") || (pattern == "psss"))
            {
                await FigureSPSS(imageElement1, imageElement2, imageElement3, imageElement4);
                return;
            }
            if ((pattern == "psps") || (pattern == "plsp"))
            {
                await FigurePSPS(imageElement1, imageElement2, imageElement3, imageElement4);
                return;
            }
            if (pattern == "llll")
            {
                await FigureLLLL(imageElement1, imageElement2, imageElement3, imageElement4);
                return;
            }
            await writer.Append(HTMLTags.StartFigure + HTMLTags.StartParagraph +
                "Error pattern" + HTMLTags.EndParagraph + HTMLTags.EndFigure);
        }

        private async Task FigureLLLL(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3, ImageElement imageElement4)
        {
            AdjustProportions(imageElement1, imageElement2);
            AdjustProportions(imageElement3, imageElement4);
            await writer.Append(HTMLTags.StartFigureWithClass +
                HTMLClasses.landscape +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(imageElement1.LeftSiblingString);
            await writer.Append(imageElement2.RightSiblingString);
            await writer.Append(imageElement3.LeftSiblingString);
            await writer.Append(imageElement4.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }

        private async Task FigurePSPS(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3, ImageElement imageElement4)
        {
            AdjustProportions(imageElement1, imageElement2);
            AdjustProportions(imageElement3, imageElement4);
            await writer.Append(HTMLTags.StartFigureWithClass +
                HTMLClasses.portrait +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(imageElement1.LeftSiblingString);
            await writer.Append(imageElement2.RightSiblingString);
            await writer.Append(imageElement3.LeftSiblingString);
            await writer.Append(imageElement4.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }

        private async Task FigureSPSS(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3, ImageElement imageElement4)
        {
            AdjustProportions(imageElement1, imageElement2);
            await writer.Append(HTMLTags.StartFigureWithClass +
                HTMLClasses.portrait +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(imageElement1.LeftSiblingString);
            await writer.Append(imageElement2.RightSiblingString);
            await writer.Append(imageElement3.SiblingString);
            await writer.Append(imageElement4.SiblingString);
            await writer.Append("</figure>");
        }

        private async Task FigureElement(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
        {
            string pattern = imageElement1.Pattern + imageElement2.Pattern + imageElement3.Pattern;
            if ((pattern == "spl") || (pattern == "ppl") || (pattern == "pps") || (pattern == "pls"))
            {
                await FigurePPL(imageElement1, imageElement2, imageElement3);
                return;
            }
            if ((pattern == "plp") || (pattern == "psp") || (pattern == "sls") || (pattern == "sll"))
            {
                await FigurePPL(imageElement1, imageElement3, imageElement2);
                return;
            }
            if ((pattern == "lpp") || (pattern == "spp"))
            {
                await FigureLPP(imageElement1, imageElement2, imageElement3);
                return;
            }
            if (pattern == "lls")
            {
                await FigureLLS(imageElement1, imageElement2, imageElement3);
                return;
            }
            if (pattern == "sps")
            {
                await FigureSPS(imageElement1, imageElement2, imageElement3);
                return;
            }
            if (pattern == "lll")
            {
                await FigureLLL(imageElement1, imageElement2, imageElement3);
                return;
            }
            await writer.Append(HTMLTags.StartFigure+HTMLTags.StartParagraph+
                "Error pattern"+HTMLTags.EndParagraph+HTMLTags.EndFigure);
        }

        private async Task FigureSPS(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
        {
            AdjustProportions(imageElement3, imageElement2);
            await writer.Append(HTMLTags.StartFigureWithClass +
                HTMLClasses.portrait +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(imageElement1.SiblingString);
            await writer.Append(imageElement2.LeftSiblingString);
            await writer.Append(imageElement3.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }
        private async Task FigureLLS(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
        {
            AdjustProportions(imageElement3, imageElement2);
            await writer.Append(HTMLTags.StartFigureWithClass +
                HTMLClasses.landscape +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(imageElement1.SiblingString);
            await writer.Append(imageElement2.LeftSiblingString);
            await writer.Append(imageElement3.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }

        private async Task FigureLLL(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
        {
            AdjustProportions(imageElement3, imageElement2);
            await writer.Append(HTMLTags.StartFigure);
            await writer.Append(imageElement1.SiblingString);
            await writer.Append(imageElement2.LeftSiblingString);
            await writer.Append(imageElement3.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }

        private async Task FigureLPP(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
        {
            if (imageElement2.Height > imageElement3.Height)
            {
                AdjustProportions(imageElement3, imageElement2);
            }
            else
            {
                AdjustProportions(imageElement2, imageElement3);
            }
            await writer.Append(HTMLTags.StartFigure);
            await writer.Append(imageElement1.SiblingString);
            await writer.Append(imageElement2.LeftSiblingString);
            await writer.Append(imageElement3.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }

        private async Task FigurePPL(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
        {
            AdjustProportions(imageElement1, imageElement2);
            await writer.Append(HTMLTags.StartFigureWithClass +
                HTMLClasses.portrait +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(imageElement1.LeftSiblingString);
            await writer.Append(imageElement2.RightSiblingString);
            await writer.Append(imageElement3.SiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }


    }
}
