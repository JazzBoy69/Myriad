using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Feliciana.Library;
using Feliciana.Data;
using System.Text;

namespace Myriad.Data
{
    public enum DataOperation
    {
        ReadNavigationPage, ReadNavigationParagraph, ReadNavigationID, ReadParentNavigationName,
        ReadNavigationTitle, ReadNextNavigationName, ReadPrecedingNavigationName,
        ReadArticleTitle, ReadArticleID, ReadArticle, ReadArticleParagraph,
        ReadCommentIDs, ReadCommentLinks, ReadComment, ReadCommentParagraph, ReadNextCommentRange,
        ReadPrecedingCommentRange, ReadCommentTitle, ReadRelatedParagraphIndex,
        ReadKeywords, ReadWordIndex, ReadKeywordSentence,
        ReadImageSize, ReadFromAllWords, ReadRoots, ReadPhrases,
        ReadSynonymsFromID, ReadDefinitionIDs, ReadSynonyms,
        ReadSubtituteWords, ReadRelatedArticles, ReadDefinitionSearchesInVerse, ReadVerseCrossReferences,
        ReadVerseWords, ReadLinkedParagraphs, DefinitionSearchesInRange,

        CreateNavigationParagraph = 256, UpdateNavigationParagraph = 257, DeleteNavigationParagraph = 258,
        CreateArticleParagraph = 270, UpdateArticleParagraph = 271, DeleteArticleParagraph = 272,
        CreateCommentParagraph = 280, UpdateCommentParagraph = 281, DeleteCommentParagraph = 282

    }
    public class SqlServerInfo
    {
        internal static List<string> parameterNames = new List<string>()
        {
            { "@key1" }, {"@key2" }, {"@key3" }, {"@key4" }, {"@key5" }, {"@key6" }

        };

        internal static Dictionary<DataOperation, string> Commands = new Dictionary<DataOperation, string>()
        {
            { DataOperation.ReadNavigationPage,
                 "select text from navigationparagraphs where name=@key1 order by paragraphindex" },
            { DataOperation.ReadNavigationParagraph,
                 "select text from navigationparagraphs where articleid=@key1 and paragraphindex=@key2" },
            { DataOperation.UpdateNavigationParagraph,
                 "update navigationparagraphs set text=@key3 where articleid=@key1 and paragraphindex=@key2" },
            { DataOperation.ReadNavigationID,
                 "select _id from navigation where name=@key1"},
            { DataOperation.ReadNavigationTitle,
                 "select heading from navigation where name=@key1"},
            { DataOperation.ReadParentNavigationName,
                "select name from navigationparagraphs where text=@key1" },
            { DataOperation.ReadNextNavigationName,
                "select text from navigationparagraphs join (select name, paragraphindex from navigationparagraphs where text=@key1) as s on navigationparagraphs.name = s.name and navigationparagraphs.paragraphindex=s.paragraphindex+1" },
            { DataOperation.ReadPrecedingNavigationName,
                "select text from navigationparagraphs join (select name, paragraphindex from navigationparagraphs where text=@key1) as s on navigationparagraphs.name = s.name and navigationparagraphs.paragraphindex=s.paragraphindex-1" },
            { DataOperation.ReadArticleTitle,
                 "select RTrim(title) from tags where id=@key1"},
            { DataOperation.ReadArticleID,
                 "select id from tags where title=@key1"},
            { DataOperation.ReadArticleParagraph,
                 "select text from glossary where id=@key1 and paragraphindex=@key2"},
            { DataOperation.ReadArticle,
                "select text from glossary where id=@key1 order by paragraphindex" },
            { DataOperation.UpdateArticleParagraph,
                 "update glossary set text=@key3 where id=@key1 and paragraphindex=@key2"},
            { DataOperation.ReadCommentIDs,
                 "select id from commentlinks where originalword = 0 and last>=@key1 and start<=@key2 order by start"},
            {DataOperation.ReadNextCommentRange,
                 "select start, last from commentlinks where start>@key1 and originalword=0 order by start" },
            {DataOperation.ReadPrecedingCommentRange,
                 "select start, last from commentlinks where last<@key1 and originalword=0 order by last desc" },
            { DataOperation.ReadComment,
                 "select RTrim(text) from comments where id=@key1" },
            { DataOperation.ReadCommentParagraph,
                 "select RTrim(text) from comments where id=@key1 and paragraphindex=@key2"},
            { DataOperation.ReadCommentTitle,
                "select RTrim(text) from comments where id=@key1 and paragraphindex=0" },
            { DataOperation.UpdateCommentParagraph,
                 "update comments set text=@key3 where id=@key1 and paragraphindex=@key2"},
            { DataOperation.ReadCommentLinks,
                 "select start, last from commentlinks where id=@key1" },
            { DataOperation.ReadKeywords,
                 "select keyid, RTrim(leadingsymbols), RTrim(text), RTrim(trailingsymbols)+' ', iscapitalized, poetic, sentence*256+sentencewordindex from keywords"+
                 " where keyid>=@key1 and keyid<=@key2" },
            { DataOperation.ReadWordIndex,
                "select versewordindex from keywords where keyid>=@key2 and keyid<=@key3 and text=@key1" },
            {DataOperation.ReadKeywordSentence,
                "select keyid, RTrim(leadingsymbols), RTrim(text), RTrim(trailingsymbols)+' ', iscapitalized, poetic, sentence*256+sentencewordindex from keywords"+
                " where sentenceID=@key1 order by keyid" },
            { DataOperation.ReadFromAllWords,
                "select text from allwords where text=@key1" },
            { DataOperation.ReadRoots,
                "select Rtrim(root) from inflections where inflection=@key1" },
            { DataOperation.ReadPhrases,
                "select RTrim(phrase) from phrases where first=@key1 or first=@key2" },
            { DataOperation.ReadRelatedParagraphIndex,
                "select paragraphindex from RelatedTags where articleID=@key1 and relatedid=@key2" },
            {DataOperation.ReadSynonymsFromID,
                "select RTrim(text) from synonyms where id=@key1 order by synIndex" },
            {DataOperation.ReadDefinitionIDs,
                "select id from synonyms where text=@key1" },
            {DataOperation.ReadSynonyms,
                "select RTrim(text) from synonyms where id=@key1 and text!=@key2 order by synIndex" },
            {DataOperation.ReadSubtituteWords,
                "select RTrim(text), last from searchwords where substitute=1 and start=@key1" },
            {DataOperation.ReadRelatedArticles,
                "select start, last, articleid, paragraphindex from RelatedArticles where last>=@key1 and start<=@key2 order by articleid, paragraphindex, last-start" },
            {DataOperation.ReadDefinitionSearchesInVerse,
                "select start, last, definitionsearch.id, synonyms.synIndex from definitionsearch join synonyms on synonyms.id=definitionsearch.id where start>=@key1 and start<=@key2 order by start, (last-start) desc, weight desc, synonyms.synIndex" },
            {DataOperation.ReadVerseCrossReferences,
                "select start, last, commentid, paragraphindex from crossreferences where start<=@key2 and last>=@key1 order by commentid, paragraphindex, last-start" },
            {DataOperation.ReadVerseWords,
                "select start, last, RTrim(text), weight, substitute from searchwords where start >= @key1" +
                " and start<= @key2 order by start asc, last-start desc, weight desc" },
            {DataOperation.ReadLinkedParagraphs,
                "select start, last, id, 0 from commentlinks where start>= @key1 and start<= @key2" },
            {DataOperation.DefinitionSearchesInRange,
                "select start, last from definitionsearch where start>=@key1 and start<=@key2 and id=@key3" }
        };

        public static DataCommand GetCommand(DataOperation operation)
        {
            return new DataCommand(Commands[operation], Connection());
        }
        private static SqlConnection Connection()
        {
            return new SqlConnection(ConnectionString);
        }
        static readonly string ConnectionString = "Server=.\\SQLExpress;Initial Catalog=Myriad;Trusted_Connection=Yes;";

        internal static DataCommand CreateCommandFromQuery(string query)
        {
            return new DataCommand(query, Connection()); 
        }
    }
}
