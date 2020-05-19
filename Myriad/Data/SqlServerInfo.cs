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
        ReadImageSize, ReadFromAllWords, ReadRoots, ReadPhrases, ReadPhrase,
        ReadSynonymsFromID, ReadDefinitionIDs, ReadSynonyms,
        ReadSubtituteWords, ReadRelatedArticles, ReadDefinitionSearchesInVerse, ReadVerseCrossReferences,
        ReadVerseWords, ReadLinkedParagraphs, DefinitionSearchesInRange, ReadSearchPhrase,
        ReadCrossReferences, ReadRelatedArticleLinks, ReadLastWordIndex, ReadExistingRelatedIDs,
        ReadMatrixWords, ReadSentenceIndex, ReadSearchWordID, ReadDefinitionSearches,
        ReadDefinitionSearchID, ReadDefinitionSearchIDs, ReadSearchWords, ReadDefinitionSearchesInArticle,
        ReadOriginalWords, ReadOriginalWordCommentLink, ReadOriginalWordKeywords, ReadMaxCommentID,
        ReadMaxArticleID, ReadCorrectSpelling, ReadIDFromSynonym,

        CreateNavigationParagraph = 256, UpdateNavigationParagraph = 257, DeleteNavigationParagraph = 258,
        CreateArticleParagraph = 270, UpdateArticleParagraph = 271, DeleteArticleParagraph = 272,
        UpdateArticleTitle=274,
        CreateCommentParagraph = 280, UpdateCommentParagraph = 281, DeleteCommentParagraph = 282,
        CreateCrossReferences = 290, DeleteCrossReferences= 291,
        CreateRelatedArticleLinks = 300, DeleteRelatedArticleLinks = 301,
        CreateRelatedTags = 310, DeleteRelatedTags=311,
        CreatePhrase = 320,
        CreateMatrixWord = 330, UpdateMatrixWord=331, DeleteMatrixWord=332,
        CreateDefinitionSearch=340, DeleteDefinitionSearch = 342, AddParagraphIndexToDefinitionSearch = 343,
        DeleteDefinitionSearches=344,
        CreateCommentLink =350, DeleteCommentLink=352,
        CreateSynonym=360, UpdateSynonym=361, DeleteSynonyms=362,
        CreateTag=370
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
            {DataOperation.ReadLastWordIndex,
                "select Max(versewordindex) from keywords where keyid>=@key1 and keyid<=@key2" },
            { DataOperation.ReadFromAllWords,
                "select text from allwords where text=@key1" },
            { DataOperation.ReadRoots,
                "select Rtrim(root) from inflections where inflection=@key1" },
            { DataOperation.ReadPhrases,
                "select RTrim(phrase) from phrases where first=@key1 or first=@key2" },
            {DataOperation.ReadPhrase,
                "select _id from phrases where phrase=@key1" },
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
                "select start, last from definitionsearch where start>=@key1 and start<=@key2 and id=@key3" },
            {DataOperation.ReadSearchPhrase,
                    "select RTrim(text) from keywords where keyid>=@key1 and keyid<=@key2" },
            {DataOperation.ReadCrossReferences,
                "select start, last from crossreferences where commentid=@key1 and paragraphindex=@key2" },
            {DataOperation.CreateCrossReferences,
                "insert into crossreferences (commentid, paragraphindex, start, last) values (@key1, @key2, @key3, @key4)"},
            {DataOperation.DeleteCrossReferences,
                "delete from crossreferences where commentid=@key1 and paragraphindex=@key2 and start=@key3 and last=@key4" },
            {DataOperation.DeleteCommentParagraph,
                "delete from comments where id=@key1 and paragraphindex=@key2"},
            {DataOperation.DeleteArticleParagraph,
                "delete from glossary where id=@key1 and paragraphindex=@key2" },
            {DataOperation.CreateRelatedArticleLinks,
                "insert into RelatedArticles (articleid, paragraphindex, start, last) values (@key1, @key2, @key3, @key4)" },
            {DataOperation.DeleteRelatedArticleLinks,
                "delete from RelatedArticles where articleid=@key1 and paragraphindex=@key2 and start=@key3 and last=@key4" },
            {DataOperation.ReadRelatedArticleLinks,
                "select start, last from RelatedArticles where articleid=@key1 and paragraphindex=@key2" },
            {DataOperation.ReadExistingRelatedIDs,
                "select relatedid from RelatedTags where articleid=@key1 and paragraphindex=@key2" },
            {DataOperation.CreateRelatedTags,
                "insert into RelatedTags (articleid, paragraphindex, relatedid) values (@key1, @key2, @key3)" },
            {DataOperation.DeleteRelatedTags,
                "delete from RelatedTags where articleid=@key1 and paragraphindex=@key2 and relatedid=@key3" },
            {DataOperation.ReadMatrixWords,
                "select start,last,substitute,weight,RTrim(text) from searchwords where start=@key1 order by weight desc" },
            {DataOperation.CreatePhrase,
                "insert into phrases (first, phrase) values (@key1, @key2)" },
            {DataOperation.ReadSentenceIndex,
                "select sentenceID, sentencewordindex from keywords where keyid=@key1" },
            {DataOperation.CreateMatrixWord,
                "insert into searchwords (sentence, wordindex, text, weight, start, last, substitute) values (@key1, @key2, @key3, @key4, @key5, @key6, @key7)" },
            {DataOperation.UpdateMatrixWord,
                "update searchwords set weight=@key2, last=@key3, substitute=@key4 where _id=@key1" },
            {DataOperation.ReadSearchWordID,
                "select _id from searchwords where sentence=@key1 and wordindex=@key2 and text=@key3" },
            {DataOperation.DeleteMatrixWord,
                "delete from searchwords where sentence=@key1 and wordindex=@key2 and text=@key3" },
            {DataOperation.ReadDefinitionSearches,
                "select start, last, articleid from definitionsearch where start=@key1" },
            {DataOperation.ReadDefinitionSearchesInArticle,
                "select start, last, paragraphIndex from definitionsearch where id=@key1" },
            {DataOperation.ReadDefinitionSearchID,
                "select _id from definitionsearch where start=@key1 and text=@key2" },
            {DataOperation.ReadDefinitionSearchIDs,
                "select _id from definitionsearch where id=@key1 and start>=@key2 and last<=@key3" },
            {DataOperation.DeleteDefinitionSearch,
                "delete from definitionsearch where _id=@key1" },
            {DataOperation.DeleteDefinitionSearches,
                "delete from definitionsearch where id=@key1" },
            {DataOperation.CreateDefinitionSearch,
                "insert into definitionsearch (id, paragraphindex, sentence, wordindex, text, weight, start, last, substitute) values (@key1, @key2, @key3, @key4, @key5, @key6, @key7, @key8, @key9)" },
            {DataOperation.ReadSearchWords,
                "select RTrim(text), sentence, wordindex, weight, start, last, substitute from searchwords where text=@key1 and start>=@key2 and last<=@key3" },
            {DataOperation.AddParagraphIndexToDefinitionSearch,
                "update definitionsearch set paragraphindex=@key2 where id=@key1 and start=@key3 and last=@key4" },
            {DataOperation.ReadOriginalWords,
                "select RTrim(text), start, last from searchwords where start>=@key1 and last<=@key2 and weight=200" },
            {DataOperation.ReadOriginalWordCommentLink,
                "select id from commentlinks where start=@key1 and last=@key2" },
            {DataOperation.ReadOriginalWordKeywords,
                "select RTrim(text), iscapitalized from keywords where keyid>=@key1 and keyid<=@key2" },
            { DataOperation.ReadMaxCommentID,
                "select max(id) from comments" },
            { DataOperation.CreateCommentParagraph,
                "insert into comments (id, paragraphindex, text) values (@key1, @key2, @key3)" },
            { DataOperation.CreateArticleParagraph,
                "insert into glossary (id, paragraphindex, text) values (@key1, @key2, @key3)" },
            { DataOperation.CreateCommentLink,
                "insert into commentlinks (id, start, last, originalword) values (@key1, @key2, @key3, @key4)" },
            { DataOperation.DeleteCommentLink,
                "delete from commentlinks where id=@key1" },
            {DataOperation.UpdateArticleTitle,
                "update tags set title=@key2 where id=@key1" },
            { DataOperation.CreateSynonym,
                "insert into synonyms (id, synIndex, text) values (@key1, @key2, @key3)" },
            { DataOperation.UpdateSynonym,
                "update synonyms set text=@key3 where id=@key1 and synIndex=@key2" },
            { DataOperation.DeleteSynonyms,
                "delete from synonyms where id=@key1 and synIndex>=@key2" },
            { DataOperation.ReadMaxArticleID,
                "select max(id) from tags" },
            { DataOperation.CreateTag,
                "insert into tags (id, title) values (@key1, @key2)" },
            { DataOperation.ReadCorrectSpelling,
                "select RTrim(correct) from Misspelled where incorrect=@key1" },
            { DataOperation.ReadIDFromSynonym,
                "select id from synonyms where text=@key1 order by synIndex" }
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
