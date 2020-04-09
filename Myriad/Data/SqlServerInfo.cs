using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Feliciana.Library;
using Feliciana.Data;

namespace Myriad.Data
{
    public enum DataOperation
    {
        ReadNavigationPage, ReadNavigationParagraph, ReadNavigationID,
        ReadNavigationTitle,
        ReadArticleTitle, ReadArticleID, ReadArticle, ReadArticleParagraph,
        ReadCommentIDs, ReadCommentLinks, ReadComment, ReadCommentParagraph, ReadNextCommentRange,
        ReadPrecedingCommentRange,
        ReadKeywords, ReadImageSize,

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
             { DataOperation.ReadArticleTitle,
                 "select title from tags where id=@key1"},
             { DataOperation.ReadArticleID,
                 "select id from tags where title=@key1"},
             { DataOperation.ReadArticleParagraph,
                 "select text from glossary where id=@key1 and paragraphindex=@key2"},
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
             { DataOperation.UpdateCommentParagraph,
                 "update comments set text=@key3 where id=@key1 and paragraphindex=@key2"},
             { DataOperation.ReadCommentLinks,
                 "select start, last from commentlinks where id=@key1" },
             { DataOperation.ReadKeywords,
                 "select keyid, RTrim(leadingsymbols), RTrim(text), RTrim(trailingsymbols)+' ', iscapitalized, poetic, sentence*256+sentencewordindex from keywords"+
                 " where keyid>=@key1 and keyid<=@key2" }
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
    }
}
