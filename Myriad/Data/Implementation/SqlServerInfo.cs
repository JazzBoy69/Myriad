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
        internal static Dictionary<(DataOperation, int), string> parameterNames = new Dictionary<(DataOperation, int), string>()
        {
            { (DataOperation.ReadNavigationPage, Ordinals.first), keyName },
            { (DataOperation.ReadArticleTitle, Ordinals.first), keyID },
            { (DataOperation.ReadArticleID, Ordinals.first), keyTitle },
            { (DataOperation.ReadArticle, Ordinals.first), keyID },
            { (DataOperation.ReadCommentIDs, Ordinals.first), keyLast },
            { (DataOperation.ReadCommentIDs, Ordinals.second), keyStart },
            { (DataOperation.ReadCommentParagraphs, Ordinals.first), keyID },
            { (DataOperation.ReadCommentLinks, Ordinals.first), keyID },
            { (DataOperation.ReadKeywords, Ordinals.first), keyStart },
            { (DataOperation.ReadKeywords, Ordinals.second), keyLast },
            { (DataOperation.ReadImageSize, Ordinals.first), keyName },
            { (DataOperation.CreateImageSize, Ordinals.first), keyName },
            { (DataOperation.CreateImageSize, Ordinals.second), keyWidth },
            { (DataOperation.CreateImageSize, Ordinals.third), keyHeight },
            { (DataOperation.DeleteImageSize, Ordinals.first), keyName }
        };

        internal static Dictionary<(DataOperation, int), System.Data.SqlDbType> parameterTypes = new Dictionary<(DataOperation, int), System.Data.SqlDbType>()
        {
            { (DataOperation.CreateImageSize, Ordinals.first), SqlDbType.NVarChar },
            { (DataOperation.CreateImageSize, Ordinals.second), SqlDbType.Float },
            { (DataOperation.CreateImageSize, Ordinals.third), SqlDbType.Float },
            { (DataOperation.DeleteImageSize, Ordinals.first), SqlDbType.NVarChar }
        };

        internal static Dictionary<DataOperation, string> Selectors = new Dictionary<DataOperation, string>()
        {
            { DataOperation.ReadNavigationPage,
                "select text from navigationparagraphs where name="+
                parameterNames[(DataOperation.ReadNavigationPage, Ordinals.first)]
                 +" order by paragraphindex" },
            { DataOperation.ReadArticleTitle,
                "select title from tags where id="+
                parameterNames[(DataOperation.ReadArticleTitle, Ordinals.first)]},
            { DataOperation.ReadArticleID,
                "select id from tags where title="+
                parameterNames[(DataOperation.ReadArticleID, Ordinals.first)]},
            { DataOperation.ReadArticle,
                "select text from glossary where id="+
                parameterNames[(DataOperation.ReadArticle, Ordinals.first)]},
            { DataOperation.ReadCommentIDs,
                "select id from commentlinks where last>= "+
                parameterNames[(DataOperation.ReadCommentIDs, Ordinals.first)] +
                " and start<="+parameterNames[(DataOperation.ReadCommentIDs, Ordinals.second)] },
            { DataOperation.ReadCommentParagraphs,
                "select RTrim(text) from comments where id="+
                parameterNames[(DataOperation.ReadCommentParagraphs, Ordinals.first)] },
            { DataOperation.ReadCommentLinks,
                "select start, last from commentlinks where id="+
                parameterNames[(DataOperation.ReadCommentLinks, Ordinals.first)] },
            { DataOperation.ReadKeywords,
                "select keyid, RTrim(leadingsymbols), RTrim(text), RTrim(trailingsymbols)+' ', iscapitalized, poetic, sentence*256+sentencewordindex from keywords"+
                " where keyid>="
                + parameterNames[(DataOperation.ReadKeywords, Ordinals.first)] +
                " and keyid<=" + parameterNames[(DataOperation.ReadKeywords, Ordinals.second)] },
            { DataOperation.ReadImageSize,
                "select width, height from ImageSizes where name="+
                parameterNames[(DataOperation.ReadImageSize, Ordinals.first)] },

            {DataOperation.CreateImageSize,
                "insert into ImageSizes (name, width, height) values ("+
                parameterNames[(DataOperation.CreateImageSize, Ordinals.first)]+","+
                parameterNames[(DataOperation.CreateImageSize, Ordinals.second)]+","+
                parameterNames[(DataOperation.CreateImageSize, Ordinals.third)]+")" },
            {DataOperation.DeleteImageSize,
                "delete from ImageSizes where name="+
                parameterNames[(DataOperation.DeleteImageSize, Ordinals.first)]}
        };
        internal static SqlConnection Connection()
        {
            return new SqlConnection(ConnectionString);
        }
        static string ConnectionString = "Server=.\\SQLExpress;Initial Catalog=Myriad;Trusted_Connection=Yes;";
    }
}
