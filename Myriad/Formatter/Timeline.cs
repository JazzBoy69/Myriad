﻿using Feliciana.Data;
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
            int currentIndex = await DataRepository.EventIndex(chronoID);
            var currentEvent = await DataRepository.Event(currentIndex);
            var precedingEvent = await DataRepository.Event(currentIndex - 1);
            var precedingMajorEvents = await DataRepository.MajorEventsBefore(currentIndex);
            var nextEvent = await DataRepository.Event(currentIndex + 1);
            var nextMajorEvents = await DataRepository.MajorEventsAfter(currentIndex);
            int maxNumPrecedingEvents = 7 - Math.Min(2, nextMajorEvents.Count);
            if ((precedingMajorEvents.Count>1) && ((precedingMajorEvents[precedingMajorEvents.Count - 1].Picture == currentEvent.Picture) || 
                (precedingMajorEvents[precedingMajorEvents.Count - 1].Picture == precedingEvent.Picture)))
            {
                precedingMajorEvents.RemoveAt(precedingMajorEvents.Count - 1);
            }
            if (nextMajorEvents.Count > Number.nothing)
            {
                if ((nextMajorEvents[Ordinals.first].Chapter == currentEvent.Chapter) ||
                    (nextMajorEvents[Ordinals.first].Chapter == nextEvent.Chapter))
                {
                    nextMajorEvents.RemoveAt(Ordinals.first);
                }
            }
            while (precedingMajorEvents.Count > maxNumPrecedingEvents)
                precedingMajorEvents.RemoveAt(Ordinals.first);
            await StartTimeline(writer);
            await WritePrecedingMajorEvents(writer, precedingMajorEvents);
            await WritePrecedingEvent(writer, precedingEvent, precedingMajorEvents.Count+1);
            await WriteCurrentEvent(writer, currentEvent, precedingMajorEvents.Count + 2);
            await WriteNextEvent(writer, nextEvent, precedingMajorEvents.Count + 3);
            await WriteNextMajorEvents(writer, nextMajorEvents, precedingMajorEvents.Count + 4);
            await EndTimeline(writer);
        }

        private static async Task WriteNextEvent(HTMLWriter writer, TimelineEvent nextEvent, int column)
        {
            if (nextEvent == null) return;
            await writer.Append(HTMLTags.StartImg);
            await writer.Append(nextEvent.Picture);
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

        private static async Task WriteNextMajorEvents(HTMLWriter writer, List<TimelineEvent> nextMajorEvents, int column)
        {
            int index = Ordinals.first;
            while ((index < nextMajorEvents.Count) && (column + index < 11))
            {
                await writer.Append(HTMLTags.StartImg);
                await writer.Append(nextMajorEvents[index].Picture);
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

        private static async Task WriteCurrentEvent(HTMLWriter writer, TimelineEvent currentEvent, int column)
        {
            await writer.Append(HTMLTags.StartImg);
            await writer.Append(currentEvent.Picture);
            await writer.Append(HTMLTags.CloseQuote);
            await writer.Append(currentEvent.Offset);
            await writer.Append(HTMLTags.Class +
                HTMLClasses.currentEvent +
                HTMLClasses.left +
                HTMLClasses.column);
            await writer.Append(column);
            await writer.Append(HTMLTags.CloseQuote + HTMLTags.EndSingleTag);
        }

        private static async Task WritePrecedingEvent(HTMLWriter writer, TimelineEvent precedingEvent, int column)
        {
            if (precedingEvent == null) return;
            await writer.Append(HTMLTags.StartImg);
            await writer.Append(precedingEvent.Picture);
            await writer.Append(HTMLTags.CloseQuote);
            await writer.Append(precedingEvent.Offset);
            await writer.Append(HTMLTags.Class+
                HTMLClasses.normalEvent +
                HTMLClasses.left +
                HTMLClasses.column);
            await writer.Append(column);
            await writer.Append(HTMLTags.CloseQuote + HTMLTags.EndSingleTag);
        }

        private static async Task WritePrecedingMajorEvents(HTMLWriter writer, List<TimelineEvent> precedingMajorEvents)
        {
            for (int index = Ordinals.first; index < precedingMajorEvents.Count; index++)
            {
                await writer.Append(HTMLTags.StartImg);
                await writer.Append(precedingMajorEvents[index].Picture);
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
                HTMLTags.CloseQuoteEndTag);
        }
    }
}
