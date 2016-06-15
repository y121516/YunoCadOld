using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Informatix.MGDS;
using static Informatix.MGDS.Cad;
using static Informatix.MGDS.AppError;

namespace MGDSNetDllTest
{
    public static class ApiExceptionExtension
    {
        public static AppError GetAppError(this ApiException ex)
        {
            // Enum.GetUnderlyingType(typeof(AppError)) == Int32
            var m = System.Text.RegularExpressions.Regex.Match(ex.Message, @"^\[(\d+)");
            return (AppError)int.Parse(m.Groups[1].Value);
        }
    }

    public class TemporaryDocument : IDisposable
    {
        public TemporaryDocument()
        {
            CreateFile();
        }
        public void Dispose()
        {
            CloseFile(Save.DoNotSave);
        }
    }

    [TestClass]
    public class CadUnitTest
    {
        static int SessionID;
        const int sessionTimeoutMs = 30 * 1000;
        const int conversationTimeoutMs = 5 * 1000;
        public Vector Origin { get; } = new Vector();

        [ClassInitialize]
        public static void Initialize(TestContext tc)
        {
            SessionID = StartMicroGDS(StartFileType.MAN, sessionTimeoutMs);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            using (var c = new Conversation())
            {
                c.Start(SessionID, conversationTimeoutMs);
                Exit(Save.DoNotSave, Save.DoNotSave);
            }
        }

        void ThrowsCadException(AppError expectedAppError, Action action)
        {
            try
            {
                action();
            }
            catch (CadException ex)
            {
                if (ex.ErrorOccurred(AppErrorType.MGDS, expectedAppError)) return;
                Assert.Fail("CadException 例外が投げられましたが {0} ではなく {1} でした。", expectedAppError, ex.GetAppError());
            }
            Assert.Fail("CadException 例外が投げられませんでした。");
        }

        void Converse(Action action)
        {
            using (var c = new Conversation())
            {
                c.Start(SessionID, conversationTimeoutMs);
                action();
            }
        }

        PriTriple GetPriFirstSelection()
        {
            var pt = new PriTriple[1];
            GetPriSelections(1, pt);
            return pt[0];
        }

        [TestMethod]
        public void AddMenuCommandTest()
        {
            ThrowsCadException(InvalidParameter, () => AddMenuCommand(null, null));
            ThrowsCadException(InvalidParameter, () => AddMenuCommand(null, ""));
            ThrowsCadException(NoConversation, () => AddMenuCommand("", null));
            ThrowsCadException(NoConversation, () => AddMenuCommand("", ""));

            Converse(() =>
            {
                ThrowsCadException(InvalidParameter, () => AddMenuCommand(null, null));
                ThrowsCadException(InvalidParameter, () => AddMenuCommand(null, ""));
                ThrowsCadException(MenuAddFail, () => AddMenuCommand("", null));
                ThrowsCadException(MenuAddFail, () => AddMenuCommand("", ""));
            });
        }

        [TestMethod]
        public void AliasDefinitionTest()
        {
            ThrowsCadException(InvalidParameter, () => AliasDefinition(AliasName.Raster, null, @"C:\", false));
            ThrowsCadException(InvalidParameter, () => AliasDefinition(AliasName.Raster, "RasterTestAlias", null, false));
            ThrowsCadException(NoConversation, () => AliasDefinition(0, "RasterTestAlias", @"C:\", false));
            ThrowsCadException(NoConversation, () => AliasDefinition((AliasName)6, "RasterTestAlias", @"C:\", false));
            ThrowsCadException(NoConversation, () => AliasDefinition((AliasName)8, "RasterTestAlias", @"C:\", false));
            ThrowsCadException(NoConversation, () => AliasDefinition(AliasName.Layer, "", "", false));

            Converse(() =>
            {
                ThrowsCadException(InvalidParameter, () => AliasDefinition(AliasName.Raster, null, @"C:\", false));
                ThrowsCadException(InvalidParameter, () => AliasDefinition(AliasName.Raster, "RasterTestAlias", null, false));
                ThrowsCadException(RequiresDocument, () => AliasDefinition(0, "RasterTestAlias", @"C:\", false));
                ThrowsCadException(RequiresDocument, () => AliasDefinition((AliasName)6, "RasterTestAlias", @"C:\", false));
                ThrowsCadException(RequiresDocument, () => AliasDefinition((AliasName)8, "RasterTestAlias", @"C:\", false));
                ThrowsCadException(RequiresDocument, () => AliasDefinition(AliasName.Raster, "RasterTestAlias", @"C:\", false));

                using (var td = new TemporaryDocument())
                {
                    ThrowsCadException(InvalidParameter, () => AliasDefinition(AliasName.Raster, null, @"C:\", false));
                    ThrowsCadException(InvalidParameter, () => AliasDefinition(AliasName.Raster, "RasterTestAlias", null, false));
                    ThrowsCadException(InvalidParameter, () => AliasDefinition(0, "RasterTestAlias", @"C:\", false));
                    ThrowsCadException(InvalidParameter, () => AliasDefinition((AliasName)6, "RasterTestAlias", @"C:\", false));
                    ThrowsCadException(InvalidParameter, () => AliasDefinition((AliasName)8, "RasterTestAlias", @"C:\", false));

                    {
                        AliasDefinition(AliasName.Raster, "", @"C:\test", false);
                        var path = ""; bool expandable;
                        GetAliasDefinition(AliasName.Raster, "", out path, out expandable);
                        Assert.AreEqual(@"C:\test", path);
                        Assert.AreEqual(false, expandable);
                    }
                    {
                        AliasDefinition(AliasName.Raster, "RasterTestAlias", "", true);
                        var path = ""; bool expandable;
                        GetAliasDefinition(AliasName.Raster, "RasterTestAlias", out path, out expandable);
                        Assert.AreEqual("", path);
                        Assert.AreEqual(true, expandable);
                    }
                }
            });
        }

        [TestMethod]
        public void AlignSelectionTest()
        {
            ThrowsCadException(NoConversation, () => AlignSelection());

            Converse(() =>
            {
                ThrowsCadException(RequiresDocument, () => AlignSelection());
                using (var td = new TemporaryDocument())
                {
                    ThrowsCadException(RequiresSelection, () => AlignSelection());
                    // OK
                    Assert.AreEqual(0, GetNumSelPrim());
                    CreateText("AlignSelectionTest", Origin);
                    AlignSelection();
                    Assert.AreEqual(1, GetNumSelPrim());
                }
            });
        }

        [TestMethod]
        public void ArcToTest()
        {
            var x1 = new Vector(1, 0, 0);
            var x2 = new Vector(2, 0, 0);

            ThrowsCadException(NoConversation, () => ArcTo(Origin, Origin));

            Converse(() =>
            {
                ThrowsCadException(RequiresDocument, () => ArcTo(Origin, Origin));
                using (var td = new TemporaryDocument())
                {
                    // 3点のいずれかが同じあるいは非常に近い場合、PointsTooClose になる
                    MoveTo(Origin); ThrowsCadException(PointsTooClose, () => ArcTo(Origin, Origin));
                    MoveTo(Origin); ThrowsCadException(PointsTooClose, () => ArcTo(Origin, x1));
                    MoveTo(Origin); ThrowsCadException(PointsTooClose, () => ArcTo(x1, Origin));
                    MoveTo(Origin); ThrowsCadException(PointsTooClose, () => ArcTo(x1, x1));

                    // 3点が直線上にある場合、ImpossibleConstruct になる
                    MoveTo(Origin); ThrowsCadException(ImpossibleConstruct, () => ArcTo(x1, x2));
                    MoveTo(Origin); ThrowsCadException(ImpossibleConstruct, () => ArcTo(x2, x1));

                    //ThrowsCadException(GraphicsMoveWarn, () => { /* どうすれば例外を投げるのか不明 */});
                    //MoveTo(Origin); Assert.AreEqual(1, ArcTo(/* どうすれば 1 を返すのか不明 */));

                    // OK
                    MoveTo(Origin);
                    Assert.AreEqual(2, ArcTo(new Vector(1, 1, 0), x1));
                }
            });
        }

        [TestMethod]
        public void ArrayPathSelTest()
        {
            ThrowsCadException(NoConversation, () => ArrayPathSel(Origin, new PriTriple(), Origin, Origin, 0, 0, false));
            Converse(() =>
            {
                ThrowsCadException(RequiresDocument, () => ArrayPathSel(Origin, new PriTriple(), Origin, Origin, 0, 0, false));
                using (var td = new TemporaryDocument())
                {
                    ThrowsCadException(RequiresSelection, () => ArrayPathSel(Origin, new PriTriple(), Origin, Origin, 0, 0, false));
                    CreateText("TEST", Origin); var text = GetPriFirstSelection();
                    MoveTo(Origin); LineTo(new Vector(100, 100, 0)); LineTo(new Vector(200, 110, 0)); var line = GetPriFirstSelection();
                    CreateCircle(100, 0, 10);
                    ThrowsCadException(InvalidParameter, () => ArrayPathSel(Origin, new PriTriple(), Origin, Origin, 0, 0, false));
                    ThrowsCadException(InvalidPriLink, () => ArrayPathSel(Origin, text, Origin, Origin, 0, 0, false));
                    ThrowsCadException(ArrayPathFailed, () => ArrayPathSel(Origin, line, Origin, Origin, 0, 0, false));

                    // OK
                    ArrayPathSel(Origin, line, Origin, new Vector(200, 110, 0), 9, 0, false);

                    // コピーが選択状態になる
                }
            });
        }

        [TestMethod]
        public void ArrayPolarSelTest()
        {
            ThrowsCadException(NoConversation, () => ArrayPolarSel(Origin, Origin, 0, 0, 0, false, false));
            Converse(() =>
            {
                ThrowsCadException(RequiresDocument, () => ArrayPolarSel(Origin, Origin, 0, 0, 0, false, false));
                using (var td = new TemporaryDocument())
                {
                    ThrowsCadException(RequiresSelection, () => ArrayPolarSel(Origin, Origin, 0, 0, 0, false, false));

                    // OK
                    // centre は選択図形との相対位置。
                    // whole == false の時、number は選択された図形を含めない。コピーする回数を指定する。
                    // while == true の時、number は選択された図形を含める。コピーする回数-1を指定する。
                    // start, endAngle はラジアン単位。
                    // whole == false の時、start, endAngle を用いる(MicroGDS V11 .NET APIヘルプは間違っている)。
                    //                     endAngle の直前までコピーされる。
                    // while == true の時、start, endAngle を無視する。コピーを完全な円周上に配置する。
                    CreateCircle(100, 0, 10);
                    var radianStart = 0 * (Math.PI / 180);
                    var radianEnd = 360 * (Math.PI / 180);
                    ArrayPolarSel(Origin, new Vector(100, 0, 0), 30, radianStart, radianEnd, false, false);

                    CreateCircle(100, 0, 10);
                    ArrayPolarSel(Origin, new Vector(-100, 0, 0), 30, 0, 0, false, true);

                    // コピーが選択状態になる
                }
            });
        }

        void ArrayRectSelTestSub(Tuple<int, int, Vector, int>[] parms)
        {
            foreach (var item in parms)
            {
                CreateCircle(0, 0, 1);
                // numberx, numbery に不正と思われる値を渡しても例外は投げられない。コピーされず、選択もされない。
                // コピーされる場合、元々選択されていた図形は選択されず、コピーされた図形のみ選択される。
                // numberx * numbery - 1 回分が選択状態になる。
                ArrayRectSel(item.Item1, item.Item2, item.Item3);
                Assert.AreEqual(item.Item4, GetNumSelPrim());
                SelectAll(); DeleteSelection();
            }
        }

        [TestMethod]
        public void ArrayRectSelTest()
        {
            ThrowsCadException(NoConversation, () => ArrayRectSel(0, 0, Origin));
            Converse(() =>
            {
                ThrowsCadException(RequiresDocument, () => ArrayRectSel(0, 0, Origin));
                using (var td = new TemporaryDocument())
                {
                    ThrowsCadException(RequiresSelection, () => ArrayRectSel(0, 0, Origin));
                    ArrayRectSelTestSub(new[] {
                        Tuple.Create(-1, -1, new Vector(10, 10, 0), 0),
                        Tuple.Create(-1, 0, new Vector(10, 10, 0), 0),
                        Tuple.Create(-1, 1, new Vector(10, 10, 0), 0),
                        Tuple.Create(-1, 2, new Vector(10, 10, 0), 0),
                        Tuple.Create(0, -1, new Vector(10, 10, 0), 0),
                        Tuple.Create(0, 0, new Vector(10, 10, 0), 0),
                        Tuple.Create(0, 1, new Vector(10, 10, 0), 0),
                        Tuple.Create(0, 2, new Vector(10, 10, 0), 0),
                        Tuple.Create(1, -1, new Vector(10, 10, 0), 0),
                        Tuple.Create(1, 0, new Vector(10, 10, 0), 0),
                        Tuple.Create(1, 1, new Vector(10, 10, 0), 0),
                        Tuple.Create(1, 2, new Vector(10, 10, 0), 1),
                        Tuple.Create(2, -1, new Vector(10, 10, 0), 0),
                        Tuple.Create(2, 0, new Vector(10, 10, 0), 0),
                        Tuple.Create(2, 1, new Vector(10, 10, 0), 1),
                        Tuple.Create(2, 2, new Vector(10, 10, 0), 3),
                        Tuple.Create(2, 3, new Vector(-10, -10, 0), 5),
                        Tuple.Create(3, 2, new Vector(0, 0, 0), 5),
                        Tuple.Create(3, 3, new Vector(10, 10, 0), 8),
                    });
                }
            });
        }

        [TestMethod]
        public void AttDelTest()
        {
            ThrowsCadException(NoConversation, () => AttDel(""));
            Converse(() =>
            {
                ThrowsCadException(RequiresDocument, () => AttDel(""));
                using (var td = new TemporaryDocument())
                {
                    MnemDefLV("Wa", AttributeType.Text, 1, 1, 9, "prompt", "");
                    MnemDefLV("La", AttributeType.Text, 1, 1, 9, "prompt", "");
                    MnemDefLV("Ra", AttributeType.Text, 1, 1, 9, "prompt", "");
                    MnemDefLV("Oa", AttributeType.Text, 1, 1, 9, "prompt", "");
                    MnemDefLV("Pa", AttributeType.Text, 1, 1, 9, "prompt", "");

                    ThrowsCadException(InvalidParameter, () => AttDel(null));
                    ThrowsCadException(InvalidMnemName, () => AttDel(""));
                    ThrowsCadException(InvalidMnemName, () => AttDel("W"));
                    ThrowsCadException(InvalidMnemName, () => AttDel("L"));
                    ThrowsCadException(InvalidMnemName, () => AttDel("R"));
                    ThrowsCadException(InvalidMnemName, () => AttDel("O"));
                    ThrowsCadException(InvalidMnemName, () => AttDel("P"));

                    ThrowsCadException(NothingDeleted, () => AttDel("Wa"));
                    ThrowsCadException(NoCurLay, () => AttDel("La"));
                    ThrowsCadException(NoCurObj, () => AttDel("Ra"));
                    ThrowsCadException(NoCurObj, () => AttDel("Oa"));
                    ThrowsCadException(NoCurObj, () => AttDel("Pa"));

                    AttVal("Wa", "window");
                    AttDel("Wa");

                    CreateLayer("TestLayer", null);
                    ThrowsCadException(NothingDeleted, () => AttDel("La"));
                    ThrowsCadException(NoCurObj, () => AttDel("Ra"));
                    ThrowsCadException(NoCurObj, () => AttDel("Oa"));
                    ThrowsCadException(NoCurObj, () => AttDel("Pa"));

                    AttVal("Wa", "window");
                    AttDel("Wa");
                    AttVal("La", "layer");
                    AttDel("La");

                    CreateObject("TestObject", Origin);
                    ThrowsCadException(NothingDeleted, () => AttDel("Ra"));
                    ThrowsCadException(NothingDeleted, () => AttDel("Oa"));
                    ThrowsCadException(NoCurPri, () => AttDel("Pa"));

                    AttVal("Wa", "window");
                    AttDel("Wa");
                    AttVal("La", "layer");
                    AttDel("La");
                    AttVal("Oa", "object");
                    AttDel("Oa");
                    AttVal("Ra", "reference");
                    AttDel("Ra");

                    CreateText("TestPrimitive", Origin);
                    var p = GetPriFirstSelection();
                    CurPrimitive(p.llink, p.vlink, p.plink);
                    ThrowsCadException(NothingDeleted, () => AttDel("Pa"));

                    AttVal("Wa", "window");
                    AttDel("Wa");
                    AttVal("La", "layer");
                    AttDel("La");
                    AttVal("Oa", "object");
                    AttDel("Oa");
                    AttVal("Ra", "reference");
                    AttDel("Ra");
                    AttVal("Pa", "primitive");
                    AttDel("Pa");

                    AttVal("La", "layer");
                    CurPhaseNum(1);
                    CurPhase("STATE", "INVISIBLE");
                    AttDel("La"); // LayNotEditable を投げない
                }
            });
        }

        [TestMethod]
        public void AttValTest()
        {
            ThrowsCadException(NoConversation, () => AttVal("", ""));
            Converse(() =>
            {
                ThrowsCadException(RequiresDocument, () => AttVal("", ""));
                using (var td = new TemporaryDocument())
                {
                    ThrowsCadException(InvalidParameter, () => AttVal(null, null));
                    ThrowsCadException(InvalidParameter, () => AttVal("", null));
                    ThrowsCadException(InvalidParameter, () => AttVal(null, ""));
                    ThrowsCadException(InvalidMnemName, () => AttVal("W", ""));
                    ThrowsCadException(InvalidMnemName, () => AttVal("L", ""));
                    ThrowsCadException(InvalidMnemName, () => AttVal("O", ""));
                    ThrowsCadException(InvalidMnemName, () => AttVal("R", ""));
                    ThrowsCadException(InvalidMnemName, () => AttVal("P", ""));

                    // ニーモニック定義を設定しなくてもエラーにならず自動的に無制限テキストが定義される。
                    AttVal("Wa", "test");
                    ThrowsCadException(NoCurLay, () => AttVal("La", ""));
                    ThrowsCadException(NoCurObj, () => AttVal("Oa", ""));
                    ThrowsCadException(NoCurObj, () => AttVal("Ra", ""));
                    ThrowsCadException(NoCurObj, () => AttVal("Pa", ""));

                    CreateLayer("TestLayer", null);
                    AttVal("La", "layerTest");
                    ThrowsCadException(NoCurObj, () => AttVal("Ra", ""));
                    ThrowsCadException(NoCurObj, () => AttVal("Oa", ""));
                    ThrowsCadException(NoCurObj, () => AttVal("Pa", ""));

                    CreateObject("TestObject", Origin);
                    AttVal("Ra", "referenceTest");
                    AttVal("Oa", "objectTest");
                    ThrowsCadException(NoCurPri, () => AttVal("Pa", ""));

                    CreateText("TestPrimitive", Origin);
                    var p = GetPriFirstSelection();
                    CurPrimitive(p.llink, p.vlink, p.plink);
                    AttVal("Pa", "primitiveTest");

                    MnemDefLV("Wa", AttributeType.Text, 1, 1, 9, "prompt", "");
                    MnemDefLV("La", AttributeType.Text, 1, 1, 9, "prompt", "");
                    MnemDefLV("Ra", AttributeType.Text, 1, 1, 9, "prompt", "");
                    MnemDefLV("Oa", AttributeType.Text, 1, 1, 9, "prompt", "");
                    MnemDefLV("Pa", AttributeType.Text, 1, 1, 9, "prompt", "");

                    ThrowsCadException(SetAttDataFail, () => AttVal("Wa", ""));
                    ThrowsCadException(SetAttDataFail, () => AttVal("La", ""));
                    ThrowsCadException(SetAttDataFail, () => AttVal("Oa", ""));
                    ThrowsCadException(SetAttDataFail, () => AttVal("Ra", ""));
                    ThrowsCadException(SetAttDataFail, () => AttVal("Pa", ""));
                }
            });
        }

        [TestMethod]
        public void AxesResynchTest()
        {
            // ウィンドウ定義毎にの設定座標軸を持てる。
            // 通信時にどの設定座標軸を用いるか?
            // AxesResynch 呼び出しで、
            // 通信時に現在アクティブなウィンドウ定義の設定座標軸を用いることができる
            ThrowsCadException(NoConversation, () => AxesResynch());
            Converse(() =>
            {
                ThrowsCadException(RequiresDocument, () => AxesResynch());
                CreateFile();
                CreateLayer("default", null);
                var axes = new Axes();
                axes.origin = new Vector(1000, 0, 0);
                axes.xAxis.x = 1;
                axes.yAxis.y = 1;
                axes.zAxis.z = 1;
                axes.axesScale = 1;
                axes.handedness = AxesHand.Right;
                SetAxes(ref axes, AxesRelative.ToNormalAxes);

                CloneSetWnd(false);
                axes.origin = new Vector(0, 1000, 0);
                SetAxes(ref axes, AxesRelative.ToNormalAxes);
            });
            Converse(() =>
            {
                Func<Axes> f = () =>
                {
                    var axes = new Axes();
                    GetSetAxes(out axes);
                    return axes;
                };
                Echo(f().ToString());

                var wndName = "";
                if (WndScanStart("**", out wndName))
                {
                    do
                    {
                        OpenWnd(wndName, false);
                        Echo(f().ToString());
                    } while (WndNext(out wndName));
                }
                if (WndScanStart("**", out wndName))
                {
                    do
                    {
                        OpenWnd(wndName, false);
                        AxesResynch();
                        Echo(f().ToString());
                    } while (WndNext(out wndName));
                }
                CloseFile(Save.DoNotSave);
            });
            // 一つのドキュメントで複数のウィンドウを作る
            // それぞれのウィンドウで座標軸を設定する

            // 通信を切って、再通信
            // 現在の設定座標軸を得る
            // 別のウィンドウの座標軸を得る
            // AxesResynchを呼び出して、座標軸を得る
            // 通信毎
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void BulgeToTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void BurnInCurInstanceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CanRedoTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CanUndoTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ClearUndoTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CloneCurLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CloneSetWndTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CloseFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CloseSetWndOnlyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CloseViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CloseWindowTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ClumpCurFaceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ClumpNextFaceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ClumpScanFaceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ClumpSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ConvNormToSetTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ConvSetToNormTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CopySelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CopySelectionNoOLETest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateAssemblyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateCircleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateCuboidTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateCylinderTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateFastDrawPhaseTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateFormattedTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateLinkedLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateMANFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateObjectTest()
        {
            // CreateObject を呼び出すと、カレントのレイヤとカレントのオブジェクトが設定されます。
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void CreatePhaseTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreatePhotoFromCurProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreatePhotoLVTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreatePolygonsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateProjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateProjectFromTemplateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateRasterTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateRasterRectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateRasterResTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateSphereTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateTempLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateTextExTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateWindowTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateWndTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateWndFromCurProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CreateWorkspaceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurLayFromSetTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurLayLabelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurLayLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurLayNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjFlashTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjFromSetTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjHookTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjIsVirtualTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjLightTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjMoveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurObjRotateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseColourTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseCStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseIncTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseLabelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseLStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseNumTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhasePenTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseReNumTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPhaseStatusTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriBreakTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriFormattedTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriGlueTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriIntersectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriJoinTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriLinestyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPrimitiveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriMoveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriPolylineTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriPolylineTest2() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriRotateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriSmoothTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriSmoothLineTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriTextAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriTextPropertyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriTrimTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPriWrapTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurPrjLabelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionBlackAndWhiteTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionDefaultTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionDetailTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionExpandViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionFromCurPhotoTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionFromSetWndTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionFromSetWndViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionGraffitiTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionLOSTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionPerspAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionRenderStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionRotationTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionSectionCubeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionSectionNoneTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionSectionPlaneTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionSectionStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionStandardDirectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionTypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionViewAxesXYTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionViewBoxTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CurProjectionZoomExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CutSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void CutSelectionNoOLETest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DatabaseLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeleteAliasDefinitionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeleteDatabaseLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeleteLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeleteObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeletePhaseTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeletePrimitiveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeleteSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeleteStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeleteWndNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DeselectAllTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DisownLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DisownSetWndLayersTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocActivateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocActivatePrjTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocCloseTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocFindTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocFirstTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocGetCurIDTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocGetCurNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocGetIDTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocGetNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocGetViewTypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocResynchTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocSaveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocViewFirstTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DocViewNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DragSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DrawExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void DrawLastViewTest() { Assert.Fail("Not implemented"); }

        [TestMethod]
        public void EchoTest()
        {
            ThrowsCadException(NoConversation, () => Echo(""));
            Converse(() =>
            {
                ThrowsCadException(InvalidParameter, () => Echo(null));
                // OK
                Echo(@"(⋈◍＞◡＜◍)。✧♡");
            });
        }

        [TestMethod]
        public void EllipseTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void EnableMenuCommandTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExitTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExpandViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExpandViewIsActiveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExportTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExportViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtCopyObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtInstanceObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtrudeSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtrudeSelByTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtrudeSelToPlaneTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtSplatInstanceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtSplatObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtTransformInstanceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ExtTransformObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void FaceNextLoopTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void FaceScanLoopTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void FragmentSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetAliasDefinitionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetArgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetArgDragTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetAttValTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCheckMenuCommandTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetClumpCurFaceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetClumpFaceMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetClumpMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetClumpNPTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetClumpNumFacesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetClumpPtTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetClumpPtsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurInstanceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurLayLabelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurLayLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurLayNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurLayTypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObj3DExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjHookTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjLenTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjLightTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjScaleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjSurfAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjTypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurObjVolumeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseColourTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseCStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseIncTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseLabelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseLStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseNumTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhasePenTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPhaseStatusTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPri3DExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriBulgeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriExpandedTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriFormattedTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriLenTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriLinestyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriNPTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriPhasesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriPtTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriPtsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriSmoothTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriSurfAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriTextAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriTextPropertyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriTypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriVolumeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPriWrapTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurPrjLabelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionBlackAndWhiteTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionDetailTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionExpandViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionGraffitiTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionLOSTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionPerspAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionRenderStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionRotationTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionSectionCubeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionSectionPlaneTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionSectionStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionSectionTypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionStandardDirectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionTypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetCurProjectionZoomExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetDatabaseLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetEntityMapNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetExtentOriginTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetExtentSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetFaceLoopNPTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetFaceLoopPtsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetFaceLoopVerticesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetFaceNumLoopsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetFreeMenuEventTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetFreeToolbarEventTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetInfoBarButtonTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetJustPtsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetLayerOfChildTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetLayerOfParentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetMaterialSubsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetMDISizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetMnemDefLVTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetMnemonicsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetNumMnemDefsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetNumSelObjTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetNumSelPrimTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetNumSnapHitsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetNumStylesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectCountTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectCountAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectCountPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectCountPrimPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectCountVolumeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectLinksTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectLinksAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectLinksPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectLinksPrimPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjectLinksVolumeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetObjSelectionsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetOptWindowStatusTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPhotoCornersTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPhotoExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPhotoEyeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPhotoLookTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPhotoQualityTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPhotoSourceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPlotPaperSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPlotSettingsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPolylinePtsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefBackgroundColTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefBackupTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefBIFFormatTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefBIFRetainItemsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefBlankManFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefBmpDibEditorTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefCheckOnOpenTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefColourTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefColourExTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefCVFFilesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDefLayNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDefLightTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDefObjNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimAngleCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimAngleLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimArcLenCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimArcLenLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimChainCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimChainLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimDatumCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimDatumLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimDistCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimDistLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimFixedLeadersTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimObjScaleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimOvershootTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimRadiusCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimRadiusLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimRunCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimRunLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimThousandsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDimUndershootTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDWFCompressTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDXFBinaryTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDXFExplodeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDXFLineTypeFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDXFPlacesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDXFTextStyleFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDXFVersionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefDynamicPointerPosTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefEpixEditorTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefFirstTemplateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefFloatingPromptBarTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefGreekingTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefGuidesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefGuideStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefHitRadiusTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefImagesDirTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefJpegEditorTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefLayAssistApplicTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefLayAssistCfgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefLayNamesUniqueTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefMeasureDialoguesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefMenuCfgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefNextTemplateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefNudgeMoveByTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefNudgeMoveModeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefNudgeMovePercentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefNudgeSwingTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefNudgeZoomTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefObjAssistApplicTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefObjAssistCfgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefOpenAsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefPLOHatchSpacingTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefPLOPensTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefPLOSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefProjectDefaultTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefRasterCacheTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefRasterOpt1Test() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefRasterOpt2Test() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefRenderConversionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefRenderImageSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefRightBtnTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefSelectColTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefSetNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefShowSnapErrorsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefShowTextFormulasTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefShowZeroInchesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefSnapCycleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefSnapHiddenLineTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefSnapsAsWordsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefStarAttribTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefTemplatesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefTextDirTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefTiffEditorTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewC2D3DTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewC3dHorizTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewC3dVertTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCChgCentreTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCExpandedTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCNewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewControlTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCOrientTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCPreviousTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCToBackTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCZoomBarTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCZoomExtentsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCZoomInTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCZoomOutTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCZoomPopupTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCZoomRectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrefViewCZoomSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPriCountTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPriLinksTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPriLinksTest2() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrincipalWndNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPrinterSetupTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetPriSelectionsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetProfileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetRenderFileSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetRenderOrientationTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetRenderQualityTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetScanPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSelectModeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSelPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSelPolyNPTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSessionCountTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSessionIDsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetAliasTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetAxesAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetAxesOrgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetAxesScaleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetAxesShowTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetCharStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetColourTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetColourExTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetDataTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetEditColourTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetEditLineStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetEditMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetEditObjTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetEditTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetFacetTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetGridTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetGridExTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetJustTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetLayLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetLightTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetLineStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetObjLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetUnitsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndDefaultViewNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndDimensionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndEyeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndHandleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndHideTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndLabelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndLookTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndPaperTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndPosTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndPreviousViewsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndQualityTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndShowIntersectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSetWndShowSmoothTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSnapHitTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSnapPosTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetStyleMapNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetStylePathTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetSystemTypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetTitleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetViewNudgeModeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetViewPaperSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetWarningsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetXMLAttrstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetXMLCharstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetXMLCurProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetXMLDoctypeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetXMLLightstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetXMLLinestyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetXMLMatstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetXMLSchemaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void GetZoomAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void HatchSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ImportTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ImportFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ImportThingTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void InfoBarButtonTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void InsertMenuCommandTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsCurLayOwnedTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsCurLayTempTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsCurObjSelectedTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsCurPriMirroredTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsCurPrimReadonlyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsCurPriSelectedTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsEntityTableLoadedTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsObjectIncludedTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void IsStyleTableLoadedTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void KillInteractiveCmdTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LayerLinkFromPathTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LayerNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LayerPathFromLinkTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LayerScanStartTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LinesFromCurPhotoTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LinesFromCurTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LineToTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LoadEntityTableTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LoadFormatFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LoadMenuTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LoadStyleTableTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void LocateHitTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void MaterialSubsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void MnemDefDelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void MnemDefLVTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void MoveToTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void NameSetWndTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectGetTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectListTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectScanTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectScanAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectScanLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectScanPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectScanPrimPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ObjectScanVolumeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OpenTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OpenBifInLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OpenFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OpenMANFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OpenProjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OpenWndTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OpenWndFromCurProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OpenWndNamedViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OptWindowStatusTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void OwnLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PasteTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PasteSpecialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PlotOverridesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PlotResetToDefaultTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PlotSetupTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PlotViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PlotViewExTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PlotWndTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PointInPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PointInPrimPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PolylineTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PolylineTest2() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefAddTemplateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefBackgroundColTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefBackupTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefBIFFormatTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefBIFRetainItemsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefBlankManFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefBmpDibEditorTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefCheckOnOpenTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefColourTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefColourExTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefCopyProjectColoursTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefCVFFilesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDefLayNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDefLightTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDefObjNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimAngleCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimAngleLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimArcLenCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimArcLenLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimChainCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimChainLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimDatumCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimDatumLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimDistCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimDistLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimFixedLeadersTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimObjScaleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimOvershootTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimRadiusCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimRadiusLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimRunCstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimRunLstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimThousandsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDimUndershootTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDWFCompressTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDXFBinaryTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDXFExplodeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDXFLineTypeFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDXFPlacesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDXFTextStyleFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDXFVersionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefDynamicPointerPosTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefEpixEditorTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefFloatingPromptBarTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefGreekingTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefGuidesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefGuideStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefHitRadiusTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefImagesDirTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefJpegEditorTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefLayAssistApplicTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefLayAssistCfgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefLayNamesUniqueTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefMeasureDialoguesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefMenuCfgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefNudgeMoveByTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefNudgeMoveModeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefNudgeMovePercentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefNudgeSwingTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefNudgeZoomTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefObjAssistApplicTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefObjAssistCfgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefOpenAsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefPLOHatchSpacingTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefPLOPensTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefPLOSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefProjectDefaultTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefRasterCacheTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefRasterOpt1Test() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefRasterOpt2Test() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefRemoveTemplateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefRenderConversionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefRenderImageSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefRightBtnTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefSelectColTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefSetNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefShowSnapErrorsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefShowTextFormulasTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefShowZeroInchesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefSnapCycleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefSnapHiddenLineTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefSnapsAsWordsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefStarAttribTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefTemplatesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefTextDirTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefTiffEditorTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewC2D3DTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewC3dHorizTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewC3dVertTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCChgCentreTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCExpandedTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCNewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewControlTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCOrientTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCPreviousTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCToBackTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCZoomBarTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCZoomExtentsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCZoomInTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCZoomOutTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCZoomPopupTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCZoomRectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrefViewCZoomSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrimitiveGetTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrimNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrimScanTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrimScanAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrimScanPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrimScanPrimPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrimScanVolumeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrincipalWndNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrintTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void PrinterSetupTest() { Assert.Fail("Not implemented"); }

        [TestMethod]
        public void PromptTest()
        {
            ThrowsCadException(NoConversation, () => Prompt(""));
            Converse(() =>
            {
                ThrowsCadException(InvalidParameter, () => Prompt(null));
                // OK
                Prompt(@"(⋈◍＞◡＜◍)。✧♡");
            });
        }

        [TestMethod]
        public void QuadFacesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RecallProfileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RecallViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RedoTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RefreshInstancesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RegenTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RemoveHolesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RemoveMenuCommandTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderFileSizeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderOrientationTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderQualityTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderRefreshTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderToFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderUpdateAllTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderUpdateEnvTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderUpdateGeometryTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderUpdateLightsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderUpdateViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenderViewSaveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RenSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ResetAliasTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ResetLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ResetObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ResetPrimTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RestoreMenuStateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ReverseSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RevolveSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RevolveSelAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RevolveSelPosTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RubberBandArcTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RubberBandCircleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RubberBandLineTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RubberBandNPTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RubberBandRectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RubberBandRectangleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void RubberFenceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveAsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveAsBmpTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveAsEpixTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveAsWRLTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SavePreferencesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveProfileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveSetWndLayersTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveSetWndOnlyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SaveViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ScheduleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ScreenUpdateModeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SelectAddTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SelectAllTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SelectionGetTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SelectObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SelectPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SelectPrimTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SelectPrimPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SelectRemoveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SendToBackTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetAliasTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetAxesAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetAxesOrgTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetAxesScaleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetAxesShowTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetCharStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetCheckMenuCommandTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetClumpFaceMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetClumpMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetClumpPtTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetClumpPtsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetColourTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetColourExTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetCurInstanceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetCursorFromFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetEditColourTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetEditLineStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetEditMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetEditObjTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetEditTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetFaceMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetFacetTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetGridTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetGridExTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetJustTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetLayerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetLightTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetLineStyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetMousePosTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetPhotoCornersTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetPhotoExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetPhotoEyeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetPhotoFromCurProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetPhotoLookTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetPhotoQualityTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetPhotoSourceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetPrimitiveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetProfileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetScanPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetSelPolyTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetSnapBitmapTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetSnapPosTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetSolidMaterialTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetUnitsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetViewNudgeModeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndAngleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndDefaultViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndDeleteNamedViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndDimensionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndExtentTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndEyeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndFromCurProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndHandleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndHideTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndLabelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndLookTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndNameTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndPaperTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndPosTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndPreviousViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndQualityTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndRenameViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndRestoreAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndRestoreNamedViewTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndSaveAxesTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndShowIntersectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndShowSmoothTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndViewFromCurProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndViewNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetWndViewScanStartTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetXMLAttrstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetXMLCharstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetXMLCurProjectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetXMLLightstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetXMLLinestyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetXMLMatstyleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SetXMLSchemaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SliceSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SnipSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SolidAddTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SolidAddWorkpieceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SolidCheckClashTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SolidIntersectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SolidIntersectSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SolidSubtractTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SolidSubtractSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SolidSubtractWorkpieceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void StartMicroGDSTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void StylePathTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void StyleScanTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void StyleScanNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void SweepSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarCustomCreateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarDeleteTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarEnableItemTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarGetCheckButtonTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarGetEditItemTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarGetItemCountTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarGetItemInfoTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarGetListCountTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarGetListCurSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarInsertButtonTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarInsertButtonFromFileTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarInsertButtonFromResourceTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarInsertControlTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarInsertFlyoutTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarInsertStatusTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarIsVisibleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarItemNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarItemScanTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarRemoveItemTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarScanTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarSetCheckButtonTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarSetItemTextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarSetListCurSelTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TBarVisibleTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TraceAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TransformCurObjectTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TransformCurPrimitiveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TransformSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void TriangulateTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void UnclumpSelectionTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void UndoTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void UndoDisableTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void UndoEnableTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void UndoSetMarkerTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void UnloadEntityTableTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void UnloadStyleTableTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void UpdateViewThumbnailTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void VertexAddTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void VertexDeleteTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void VertexMoveTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void WindowArrangeTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void WindowRedrawTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void WndNextTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void WndScanStartTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ZoomAreaTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ZoomExtentsTest() { Assert.Fail("Not implemented"); }
        [TestMethod]
        public void ZoomSelectionTest() { Assert.Fail("Not implemented"); }
    }
}
