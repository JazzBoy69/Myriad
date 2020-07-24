using Feliciana.Data;
using Feliciana.HTML;
using Feliciana.Library;
using Feliciana.ResponseWriter;
using Myriad.Data;
using Myriad.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Formatter
{
    public class Timeline
    {
        internal static async Task Write(HTMLWriter writer, int chronoID)
        {
            var currentEvent = ReadEvent(chronoID);
            var precedingEvent = ReadPrecedingEvent(currentEvent);
            var precedingMajorEvents = ReadPrecedingMajorEvents(currentEvent);
            var nextEvent = ReadNextEvent(currentEvent);
            var nextMajorEvents = ReadNextMajorEvents(currentEvent);
            if ((precedingMajorEvents.Count>1) && ((precedingMajorEvents[precedingMajorEvents.Count - 1].ID == currentEvent.ID) || 
                (precedingMajorEvents[precedingMajorEvents.Count - 1].ID == precedingEvent.ID)))
            {
                precedingMajorEvents.RemoveAt(precedingMajorEvents.Count - 1);
            }
            if (nextMajorEvents.Count > Number.nothing)
            {
                if ((nextMajorEvents[Ordinals.first].ID == currentEvent.ID) ||
                    (nextMajorEvents[Ordinals.first].ID == nextEvent.ID))
                {
                    nextMajorEvents.RemoveAt(Ordinals.first);
                }
                if (precedingMajorEvents.Count > 6)
                    precedingMajorEvents.RemoveAt(Ordinals.first);
            }
            await StartTimeline(writer);
            await WritePrecedingMajorEvents(writer, precedingMajorEvents);
            await WritePrecedingEvent(writer, precedingEvent, precedingMajorEvents.Count+1);
            await WriteCurrentEvent(writer, currentEvent, precedingMajorEvents.Count + 2);
            await WriteNextEvent(writer, nextEvent, precedingMajorEvents.Count + 3);
            await WriteNextMajorEvents(writer, nextMajorEvents, precedingMajorEvents.Count + 4);
            await EndTimeline(writer);
        }

        private static async Task WriteNextEvent(HTMLWriter writer, Event nextEvent, int column)
        {
            if (nextEvent == null) return;
            await writer.Append(HTMLTags.StartImg);
            await writer.Append(nextEvent.PictureFile);
            await writer.Append(HTMLTags.CloseQuote);
            await writer.Append(nextEvent.Offset);
            await writer.Append(HTMLTags.Class +
                HTMLClasses.normalEvent +
                HTMLClasses.right +
                HTMLClasses.column);
            await writer.Append(column);
            await writer.Append(HTMLTags.CloseQuote + HTMLTags.EndSingleTag);
        }

        private static async Task EndTimeline(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.EndDiv);
        }

        private static async Task WriteNextMajorEvents(HTMLWriter writer, List<Event> nextMajorEvents, int column)
        {
            int index = Ordinals.first;
            while ((index < nextMajorEvents.Count) && (column + index < 11))
            {
                await writer.Append(HTMLTags.StartImg);
                await writer.Append(nextMajorEvents[index].PictureFile);
                await writer.Append(HTMLTags.CloseQuote);
                await writer.Append(nextMajorEvents[index].Offset);
                await writer.Append(HTMLTags.Class +
                    HTMLClasses.majorEvent +
                    HTMLClasses.right +
                    HTMLClasses.column);
                await writer.Append(column+index);
                await writer.Append(HTMLTags.CloseQuote + HTMLTags.EndSingleTag);
                index++;
            }
        }

        private static async Task WriteCurrentEvent(HTMLWriter writer, Event currentEvent, int column)
        {
            await writer.Append(HTMLTags.StartImg);
            await writer.Append(currentEvent.PictureFile);
            await writer.Append(HTMLTags.CloseQuote);
            await writer.Append(currentEvent.Offset);
            await writer.Append(HTMLTags.Class +
                HTMLClasses.currentEvent +
                HTMLClasses.left +
                HTMLClasses.column);
            await writer.Append(column);
            await writer.Append(HTMLTags.CloseQuote + HTMLTags.EndSingleTag);
        }

        private static async Task WritePrecedingEvent(HTMLWriter writer, Event precedingEvent, int column)
        {
            if (precedingEvent == null) return;
            await writer.Append(HTMLTags.StartImg);
            await writer.Append(precedingEvent.PictureFile);
            await writer.Append(HTMLTags.CloseQuote);
            await writer.Append(precedingEvent.Offset);
            await writer.Append(HTMLTags.Class+
                HTMLClasses.normalEvent +
                HTMLClasses.left +
                HTMLClasses.column);
            await writer.Append(column);
            await writer.Append(HTMLTags.CloseQuote + HTMLTags.EndSingleTag);
        }

        private static async Task WritePrecedingMajorEvents(HTMLWriter writer, List<Event> precedingMajorEvents)
        {
            for (int index = Ordinals.first; index < precedingMajorEvents.Count; index++)
            {
                await writer.Append(HTMLTags.StartImg);
                await writer.Append(precedingMajorEvents[index].PictureFile);
                await writer.Append(HTMLTags.CloseQuote);
                await writer.Append(precedingMajorEvents[index].Offset);
                await writer.Append(HTMLTags.Class +
                    HTMLClasses.majorEvent +
                    HTMLClasses.left +
                    HTMLClasses.column);
                await writer.Append(index+1);
                await writer.Append(HTMLTags.CloseQuote + HTMLTags.EndSingleTag);
            }
        }

        private static async Task StartTimeline(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithClass +
                HTMLClasses.timeline + Symbol.space+
                HTMLClasses.hidden+
                HTMLTags.CloseQuoteEndTag);
        }

        private static List<Event> ReadNextMajorEvents(Event currentEvent)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadNextMajorEvents), currentEvent.Index);
            var results = reader.GetClassData<Event>();
            reader.Close();
            return results;
        }

        private static Event ReadNextEvent(Event currentEvent)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadNextEvent), currentEvent.Index);
            var results = reader.GetClassDatum<Event>();
            reader.Close();
            return results;
        }

        private static List<Event> ReadPrecedingMajorEvents(Event currentEvent)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadPrecedingMajorEvents), currentEvent.Index);
            var results = reader.GetClassData<Event>();
            reader.Close();
            return results;
        }

        private static Event ReadPrecedingEvent(Event currentEvent)
        {
            var reader = new DataReaderProvider<int, string>(SqlServerInfo.GetCommand(DataOperation.ReadPrecedingEvent), 
                currentEvent.Index, currentEvent.Picture);
            var results = reader.GetClassDatum<Event>();
            reader.Close();
            return results;
        }

        private static Event ReadEvent(int chronoID)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadEvent), chronoID);
            var result = reader.GetClassDatum<Event>();
            reader.Close();
            return result;
        }

    }
}
