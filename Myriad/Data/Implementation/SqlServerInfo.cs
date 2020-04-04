using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Myriad.Library;

namespace Myriad.Data.Implementation
{
    public class SqlServerInfo
    {
        const string keyID = "@id";
        const string keyTitle = "@title";
        const string keyName = "@name";
        const string keyStart = "@start";
        const string keyLast = "@last";
        const string keyWidth = "@width";
        const string keyHeight = "@height";
        const string keyIndex = "@index";
        const string keyText = "@text";
        internal static Dictionary<(DataOperation, int), string> parameterNames = new Dictionary<(DataOperation, int), string>()
        {
            { (DataOperation.ReadNavigationPage, Ordinals.first), keyName },
            { (DataOperation.ReadNavigationParagraph, Ordinals.first), keyID },
            { (DataOperation.ReadNavigationParagraph, Ordinals.second), keyIndex },
            { (DataOperation.ReadNavigationID, Ordinals.first), keyName },
            { (DataOperation.UpdateNavigationParagraph, Ordinals.first), keyID },
            { (DataOperation.UpdateNavigationParagraph, Ordinals.second), keyIndex },
            { (DataOperation.UpdateNavigationParagraph, Ordinals.third), keyText },
            { (DataOperation.ReadArticleTitle, Ordinals.first), keyID },
            { (DataOperation.ReadArticleID, Ordinals.first), keyTitle },
            { (DataOperation.ReadArticle, Ordinals.first), keyID },
            { (DataOperation.ReadArticleParagraph, Ordinals.first), keyID },
            { (DataOperation.ReadArticleParagraph, Ordinals.second), keyIndex },
            { (DataOperation.UpdateArticleParagraph, Ordinals.first), keyID },
            { (DataOperation.UpdateArticleParagraph, Ordinals.second), keyIndex },
            { (DataOperation.UpdateArticleParagraph, Ordinals.third), keyText },
            { (DataOperation.ReadCommentIDs, Ordinals.first), keyLast },
            { (DataOperation.ReadCommentIDs, Ordinals.second), keyStart },
            { (DataOperation.ReadNextCommentRange, Ordinals.first), keyStart },
            { (DataOperation.ReadPrecedingCommentRange, Ordinals.first), keyStart },
            { (DataOperation.ReadComment, Ordinals.first), keyID },
            { (DataOperation.ReadCommentParagraph, Ordinals.first), keyID },
            { (DataOperation.ReadCommentParagraph, Ordinals.second), keyIndex },
            { (DataOperation.UpdateCommentParagraph, Ordinals.first), keyID },
            { (DataOperation.UpdateCommentParagraph, Ordinals.second), keyIndex },
            { (DataOperation.UpdateCommentParagraph, Ordinals.third), keyText },
            { (DataOperation.ReadCommentLinks, Ordinals.first), keyID },
            { (DataOperation.ReadKeywords, Ordinals.first), keyStart },
            { (DataOperation.ReadKeywords, Ordinals.second), keyLast }

        };

        internal static Dictionary<(DataOperation, int), System.Data.SqlDbType> parameterTypes = new Dictionary<(DataOperation, int), System.Data.SqlDbType>()
        {

        };

        internal static Dictionary<DataOperation, string> Selectors = new Dictionary<DataOperation, string>()
        {
            { DataOperation.ReadNavigationPage,
                "select text from navigationparagraphs where name="+
                parameterNames[(DataOperation.ReadNavigationPage, Ordinals.first)]
                 +" order by paragraphindex" },
            { DataOperation.ReadNavigationParagraph,
                "select text from navigationparagraphs where articleid="+
                parameterNames[(DataOperation.ReadNavigationParagraph, Ordinals.first)]+
                " and paragraphindex="+
                parameterNames[(DataOperation.ReadNavigationParagraph, Ordinals.second)] },
            { DataOperation.UpdateNavigationParagraph,
                "update navigationparagraphs set text="+
                parameterNames[(DataOperation.UpdateNavigationParagraph, Ordinals.third)]+
                " where articleid="+
                parameterNames[(DataOperation.UpdateNavigationParagraph, Ordinals.first)]+
                " and paragraphindex="+
                parameterNames[(DataOperation.UpdateNavigationParagraph, Ordinals.second)] },
             { DataOperation.ReadNavigationID,
                "select _id from navigation where name="+
                parameterNames[(DataOperation.ReadNavigationPage, Ordinals.first)]},
            { DataOperation.ReadArticleTitle,
                "select title from tags where id="+
                parameterNames[(DataOperation.ReadArticleTitle, Ordinals.first)]},
            { DataOperation.ReadArticleID,
                "select id from tags where title="+
                parameterNames[(DataOperation.ReadArticleID, Ordinals.first)]},
            { DataOperation.ReadArticle,
                "select text from glossary where id="+
                parameterNames[(DataOperation.ReadArticle, Ordinals.first)]},
            { DataOperation.ReadArticleParagraph,
                "select text from glossary where id="+
                parameterNames[(DataOperation.ReadArticleParagraph, Ordinals.first)]+
                " and paragraphindex="+
                parameterNames[(DataOperation.ReadArticleParagraph, Ordinals.second)]},
            { DataOperation.UpdateArticleParagraph,
                "update glossary set text="+
                parameterNames[(DataOperation.UpdateArticleParagraph, Ordinals.third)]+
                " where id="+
                parameterNames[(DataOperation.UpdateArticleParagraph, Ordinals.first)]+
                " and paragraphindex="+
                parameterNames[(DataOperation.UpdateArticleParagraph, Ordinals.second)]},
            { DataOperation.ReadCommentIDs,
                "select id from commentlinks where originalword = 0 and last>= "+
                parameterNames[(DataOperation.ReadCommentIDs, Ordinals.first)] +
                " and start<="+parameterNames[(DataOperation.ReadCommentIDs, Ordinals.second)] },
            {DataOperation.ReadNextCommentRange,
                "select start, last from commentlinks where start >" +
                parameterNames[(DataOperation.ReadNextCommentRange, Ordinals.first)]
                + " and originalword=0 order by start" },
            {DataOperation.ReadPrecedingCommentRange,
                "select start, last from commentlinks where last <" +
                parameterNames[(DataOperation.ReadPrecedingCommentRange, Ordinals.first)]
                + " and originalword=0 order by last desc" },
            { DataOperation.ReadComment,
                "select RTrim(text) from comments where id="+
                parameterNames[(DataOperation.ReadComment, Ordinals.first)] },
            { DataOperation.ReadCommentParagraph,
                "select RTrim(text) from comments where id="+
                parameterNames[(DataOperation.ReadCommentParagraph, Ordinals.first)]+
                " and paragraphindex="+
                parameterNames[(DataOperation.ReadCommentParagraph, Ordinals.second)]},
            { DataOperation.UpdateCommentParagraph,
                "update comments set text="+
                parameterNames[(DataOperation.UpdateCommentParagraph, Ordinals.third)]+
                " where id="+
                parameterNames[(DataOperation.UpdateCommentParagraph, Ordinals.first)]+
                " and paragraphindex="+
                parameterNames[(DataOperation.UpdateCommentParagraph, Ordinals.second)]},
            { DataOperation.ReadCommentLinks,
                "select start, last from commentlinks where id="+
                parameterNames[(DataOperation.ReadCommentLinks, Ordinals.first)] },
            { DataOperation.ReadKeywords,
                "select keyid, RTrim(leadingsymbols), RTrim(text), RTrim(trailingsymbols)+' ', iscapitalized, poetic, sentence*256+sentencewordindex from keywords"+
                " where keyid>="
                + parameterNames[(DataOperation.ReadKeywords, Ordinals.first)] +
                " and keyid<=" + parameterNames[(DataOperation.ReadKeywords, Ordinals.second)] }
        };
        internal static SqlConnection Connection()
        {
            return new SqlConnection(ConnectionString);
        }
        static string ConnectionString = "Server=.\\SQLExpress;Initial Catalog=Myriad;Trusted_Connection=Yes;";
    }
}
