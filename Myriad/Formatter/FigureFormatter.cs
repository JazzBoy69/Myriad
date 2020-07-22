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
                    await WriteFigureElement(elements[k], elements[k + 1], elements[k + 2], elements[k + 3]);
                    k += 4;
                }
                else
                if ((elements.Count - k) == 3)
                {
                    await WriteFigureElement(elements[k], elements[k + 1], elements[k + 2]);
                    k += 3;
                }
                else if ((elements.Count - k) == 2)
                {
                    await WriteFigureElement(elements[k], elements[k + 1]);
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
            int index = p.IndexOf("]]");
            string filename = p.Substring(Ordinals.third, index - 2);
            ImageElement result = new ImageElement(filename);
            return (result.Valid) ?
                  result :
                  null;
        }

        private async Task WriteFigureElement(ImageElement imageElement1, ImageElement imageElement2)
        {
            string pattern = imageElement1.Pattern + imageElement2.Pattern;
            if ((pattern == "pl") || (pattern == "ss") || (pattern == "sl") || (pattern == "ll") || (pattern == "ls"))
            {
                await WriteFigurePL(imageElement1, imageElement2);
                return;
            }
            if (pattern == "lp")
            {
                await WriteFigureLP(imageElement1, imageElement2);
                return;
            }
            if ((pattern == "sp") || (pattern == "ps") || (pattern == "pp"))
            {
                await WriteFigurePL(imageElement1, imageElement2);
                return;
            }
            await writer.Append(HTMLTags.StartFigure+HTMLTags.StartParagraph+"Error " + pattern + HTMLTags.EndParagraph+HTMLTags.EndFigure);
        }

        private async Task WriteFigurePL(ImageElement imageElement1, ImageElement imageElement2)
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

        private async Task WriteFigureLP(ImageElement imageElement1, ImageElement imageElement2)
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

        private async Task WriteFigureElement(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3, ImageElement imageElement4)
        {
            string pattern = imageElement1.Pattern + imageElement2.Pattern + imageElement3.Pattern +
                imageElement4.Pattern;
            if ((pattern == "spss") || (pattern == "psss"))
            {
                await WriteFigureSPSS(imageElement1, imageElement2, imageElement3, imageElement4);
                return;
            }
            if ((pattern == "psps") || (pattern == "plsp"))
            {
                await WriteFigurePSPS(imageElement1, imageElement2, imageElement3, imageElement4);
                return;
            }
            if (pattern == "llll")
            {
                await WriteFigureLLLL(imageElement1, imageElement2, imageElement3, imageElement4);
                return;
            }
            await writer.Append(HTMLTags.StartFigure + HTMLTags.StartParagraph +
                "Error pattern" + HTMLTags.EndParagraph + HTMLTags.EndFigure);
        }

        private async Task WriteFigureLLLL(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3, ImageElement imageElement4)
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

        private async Task WriteFigurePSPS(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3, ImageElement imageElement4)
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

        private async Task WriteFigureSPSS(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3, ImageElement imageElement4)
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

        private async Task WriteFigureElement(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
        {
            string pattern = imageElement1.Pattern + imageElement2.Pattern + imageElement3.Pattern;
            if ((pattern == "spl") || (pattern == "ppl") || (pattern == "pps") || (pattern == "pls") || (pattern == "pll"))
            {
                await WriteFigurePPL(imageElement1, imageElement2, imageElement3);
                return;
            }
            if ((pattern == "plp") || (pattern == "psp") || (pattern == "sls") || (pattern == "sll"))
            {
                await WriteFigurePPL(imageElement1, imageElement3, imageElement2);
                return;
            }
            if ((pattern == "lpp") || (pattern == "spp"))
            {
                await WriteFigureLPP(imageElement1, imageElement2, imageElement3);
                return;
            }
            if (pattern == "lls")
            {
                await WriteFigureLLS(imageElement1, imageElement2, imageElement3);
                return;
            }
            if (pattern == "sps")
            {
                await WriteFigureSPS(imageElement1, imageElement2, imageElement3);
                return;
            }
            if (pattern == "lll")
            {
                await WriteFigureLLL(imageElement1, imageElement2, imageElement3);
                return;
            }
            await writer.Append(HTMLTags.StartFigure+HTMLTags.StartParagraph+
                "Error pattern"+HTMLTags.EndParagraph+HTMLTags.EndFigure);
        }

        private async Task WriteFigureSPS(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
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
        private async Task WriteFigureLLS(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
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

        private async Task WriteFigureLLL(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
        {
            AdjustProportions(imageElement3, imageElement2);
            await writer.Append(HTMLTags.StartFigure);
            await writer.Append(imageElement1.SiblingString);
            await writer.Append(imageElement2.LeftSiblingString);
            await writer.Append(imageElement3.RightSiblingString);
            await writer.Append(HTMLTags.EndFigure);
        }

        private async Task WriteFigureLPP(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
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

        private async Task WriteFigurePPL(ImageElement imageElement1, ImageElement imageElement2, ImageElement imageElement3)
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
