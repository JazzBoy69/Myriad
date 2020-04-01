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
        internal static Dictionary<(DataOperation, int), string> parameterNames = new Dictionary<(DataOperation, int), string>()
        {
            { (DataOperation.ReadNavigationPage, Ordinals.first), keyName },
            { (DataOperation.ReadNavigationParagraph, Ordinals.first), keyID },
            { (DataOperation.ReadNavigationParagraph, Ordinals.second), keyIndex },
            { (DataOperation.ReadNavigationID, Ordinals.first), keyName },
            { (DataOperation.ReadArticleTitle, Ordinals.first), keyID },
            { (DataOperation.ReadArticleID, Ordinals.first), keyTitle },
            { (DataOperation.ReadArticle, Ordinals.first), keyID },
            { (DataOperation.ReadArticleParagraph, Ordinals.first), keyID },
            { (DataOperation.ReadArticleParagraph, Ordinals.second), keyIndex },
            { (DataOperation.ReadCommentIDs, Ordinals.first), keyLast },
            { (DataOperation.ReadCommentIDs, Ordinals.second), keyStart },
            { (DataOperation.ReadComment, Ordinals.first), keyID },
            { (DataOperation.ReadCommentParagraph, Ordinals.first), keyID },
            { (DataOperation.ReadCommentParagraph, Ordinals.second), keyIndex },
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
            { DataOperation.ReadCommentIDs,
                "select id from commentlinks where originalword = 0 and last>= "+
                parameterNames[(DataOperation.ReadCommentIDs, Ordinals.first)] +
                " and start<="+parameterNames[(DataOperation.ReadCommentIDs, Ordinals.second)] },
            { DataOperation.ReadComment,
                "select RTrim(text) from comments where id="+
                parameterNames[(DataOperation.ReadComment, Ordinals.first)] },
             { DataOperation.ReadCommentParagraph,
                "select RTrim(text) from comments where id="+
                parameterNames[(DataOperation.ReadCommentParagraph, Ordinals.first)]+
                " and paragraphindex="+
                parameterNames[(DataOperation.ReadCommentParagraph, Ordinals.second)]},
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
