using System;
using System.Collections.Generic;
using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    /// <summary>
    /// Find を呼び出すことのできるインターフェイス。
    /// </summary>
    public interface IDocumentFindable
    {
        /// <summary>
        /// Find メソッドは 内部で Informatix.MGDS.Cad.DocFind を呼び出します。
        /// </summary>
        /// <returns></returns>
        ActiveDocument Find();
    }

    /// <summary>
    /// ドキュメントの名前を表すクラス。
    /// </summary>
    public class DocumentName : IDocumentFindable
    {
        string Name { get; }

        internal DocumentName(string docName) { Name = docName; }

        public override string ToString() => Name;

        public ActiveDocument Find()
        {
            MC.DocFind(true, Name);
            return ActiveDocument.Instance;
        }
    }

    /// <summary>
    /// ドキュメントの ID を表すクラス。
    /// </summary>
    public class DocumentID : IDocumentFindable
    {
        string ID { get; }

        internal DocumentID(string docID) { ID = docID; }

        public override string ToString() => ID;

        public ActiveDocument Find()
        {
            MC.DocFind(false, ID);
            return ActiveDocument.Instance;
        }
    }

    /// <summary>
    /// Cad.DocFirst、Cad.DocNext による検索ループ中にできる操作をまとめたクラス。
    /// プロパティを含みません。
    /// </summary>
    public class ScanDocumentBase
    {
        internal ScanDocumentBase() { }

        public virtual DocumentID GetID()
        {
            var docID = "";
            MC.DocGetID(out docID);
            return new DocumentID(docID);
        }

        public virtual DocumentName GetName()
        {
            var docName = "";
            MC.DocGetName(out docName);
            return new DocumentName(docName);
        }
    }

    /// <summary>
    /// Cad.DocFirst、Cad.DocNext による検索ループ中でのみできる操作をまとめたクラス。
    /// ID プロパティを持ちます。
    /// </summary>
    public class ScanDocument : ScanDocumentBase
    {
        public DocumentID ID { get; }

        internal ScanDocument(string docID)
        {
            ID = new DocumentID(docID);
        }

        /// <summary>
        /// Cad.DocFirst、Cad.DocNext による検索ループでスキャン中のドキュメントをアクティブにします。
        /// </summary>
        /// <returns></returns>
        public ScanActiveDocument Activate()
        {
            var docName = "";
            MC.DocActivate(out docName);
            return new ScanActiveDocument(ID, docName);
        }
    }

    /// <summary>
    /// アクティブなドキュメントにできる操作をまとめたクラス。
    /// プロパティを含みません。
    /// </summary>
    public class ActiveDocument : ScanDocumentBase
    {
        static public ActiveDocument Instance { get; } = new ActiveDocument();

        internal ActiveDocument() { }

        public System.Windows.Forms.DialogResult Close(M.Save drawingSave = M.Save.Prompt)
        {
            return MC.DocClose(drawingSave);
        }

        public M.DocViewType ViewType => MC.DocGetViewType();

        public System.Windows.Forms.DialogResult Save()
        {
            return MC.DocSave();
        }

        public virtual CurrentDocument Resynch()
        {
            MC.DocResynch();
            return CurrentDocument.Instance;
        }

        public DocumentView.DocumentViews Views { get; } = new DocumentView.DocumentViews();
    }

    /// <summary>
    /// Cad.DocFirst、Cad.DocNextによる検索ループ中で、
    /// かつアクティブなドキュメントにできる操作をまとめたクラス。
    /// ID プロパティと Name プロパティを持ちます。
    /// </summary>
    public class ScanActiveDocument : ActiveDocument
    {
        public DocumentID ID { get; }
        public DocumentName Name { get; }

        internal ScanActiveDocument(DocumentID docID, string docName)
        {
            ID = docID;
            Name = new DocumentName(docName);
        }

        public new ScanCurrentDocument Resynch()
        {
            MC.DocResynch();
            return new ScanCurrentDocument(ID, Name);
        }
    }

    /// <summary>
    /// Cad.DocResynch() 後などMicroGDS API がカレントにしているドキュメントにできる操作をまとめたクラス。
    /// プロパティを含みません。
    /// </summary>
    public class CurrentDocument : ActiveDocument
    {
        public static new CurrentDocument Instance { get; } = new CurrentDocument();
        public Selection Selection { get; } = Selection.Instance;
        public Aliases Aliases { get; } = Aliases.Instance;
        public SetWnds SetWnds { get; } = SetWnds.Instance;
        public Layers Layers { get; } = Layers.Instance;
        public SetEdit SetEdit { get; } = SetEdit.Instance;

        internal CurrentDocument() { }

        public override DocumentID GetID()
        {
            var docID = "";
            MC.DocGetCurID(out docID);
            return new DocumentID(docID);
        }

        public override DocumentName GetName()
        {
            var docName = "";
            MC.DocGetCurName(out docName);
            return new DocumentName(docName);
        }

        DocumentID CurID
        {
            get
            {
                var docID = "";
                MC.DocGetCurID(out docID);
                return new DocumentID(docID);
            }
        }

        string CurName
        {
            get
            {
                var docName = "";
                MC.DocGetCurName(out docName);
                return docName;
            }
        }



        public int Colour
        {
            get { return MC.GetSetColour(); }
            set { MC.SetColour(value); }
        }

        public Tuple<int, int, int, int, int> ColourEx
        {
            get
            {
                int colourIndex, red, green, blue, alpha;
                MC.GetSetColourEx(out colourIndex, out red, out green, out blue, out alpha);
                return Tuple.Create(colourIndex, red, green, blue, alpha);
            }
            set
            {
                MC.SetColourEx(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
            }
        }

        // 通常は SetWnd が単位を持つが
        // SetWnd が一つもない場合、ドキュメントも単位を持てる
        public Tuple<string, int> Units
        {
            get
            {
                var units = "";
                var decimalPlace = MC.GetSetUnits(out units);
                return Tuple.Create(units, decimalPlace);
            }
            set { MC.SetUnits(value.Item1, value.Item2); }
        }

        /// <summary>
        /// カーソルのリソースが含まれているファイル名（絶対パス）。
        /// 空の文字列を指定すると、ユーザー定義のカーソルはすべて削除されます。
        /// </summary>
        public string CursorFullPath
        {
            set { MC.SetCursorFromFile(value); }
        }

        const string StylePathSeparator = ";";
        public string[] StylePaths
        {
            get
            {
                string paths;
                MC.GetStylePath(out paths);
                return string.IsNullOrEmpty(paths?.Trim().Trim(StylePathSeparator[0]))
                    ? new string[0]
                    : paths.Split(StylePathSeparator[0]);
            }
            set { MC.StylePath(string.Join(StylePathSeparator, value)); }
        }
    }

    /// <summary>
    /// Cad.DocFirst、Cad.DocNextによる検索ループ中で、
    /// かつアクティブかつカレントのドキュメントにできる操作をまとめたクラス。
    /// ID プロパティと Name プロパティを持ちます。
    /// </summary>
    public class ScanCurrentDocument : CurrentDocument
    {
        public DocumentID ID { get; }
        public DocumentName Name { get; }

        internal ScanCurrentDocument(DocumentID docID, DocumentName docName)
        {
            ID = docID;
            Name = docName;
        }
    }


    public class Documents
    {
        public static Documents Instance { get; } = new Documents();

        Documents() { }

        public CurrentDocument CreateNew()
        {
            MC.CreateFile();
            return CurrentDocument.Instance; // 作成したファイル(ドキュメント)がカレントドキュメントとなる
        }

        public int Count
        {
            get
            {
                var docID = "";
                int n = 0;
                if (MC.DocFirst(out docID))
                {
                    do
                    {
                        ++n;
                    } while (MC.DocNext(out docID));
                }
                return n;
            }
        }

        public IEnumerable<ScanDocument> Scan()
        {
            var docID = "";
            if (MC.DocFirst(out docID))
            {
                do
                {
                    yield return new ScanDocument(docID);
                } while (MC.DocNext(out docID));
            }
        }
    }

    class CurrentSetWnd
    {

    }


    /// <summary>
    /// MicroGDS では Document に含まれる
    /// ウィンドウ定義をドキュメントビューとして表示する
    /// </summary>
    public class DocumentView
    {
        int ID { get; }

        DocumentView(int viewID)
        {
            ID = viewID;
        }

        public Object.Objects Objects { get; } = new Object.Objects();

        public class DocumentViews
        {
            public IEnumerable<DocumentView> Scan()
            {
                // ViewTypeのチェックが必要かも。ビューが無い場合
                if (MC.DocGetViewType() != M.DocViewType.Drawing) yield break;

                int viewID;
                if (MC.DocViewFirst(out viewID))
                {
                    do
                    {
                        yield return new DocumentView(viewID);
                    } while (MC.DocViewNext(out viewID));
                }
            }
        }
    }
}
