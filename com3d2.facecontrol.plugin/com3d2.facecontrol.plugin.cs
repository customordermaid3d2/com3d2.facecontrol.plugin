using System;
using System.IO;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;


namespace COM3D2.FaceControl.Plugin
{
    [PluginFilter("COM3D2x64"),
    PluginFilter("COM3D2VRx64"),
    PluginFilter("COM3D2OHx64"),
    PluginFilter("COM3D2OHVRx64"),
    PluginName("COM3D2 FaceControl"),
    PluginVersion("0.0.4.0")]
    public class FaceControl : PluginBase
    {
        private static readonly string PLUGIN_NAME = "FaceControl";
        private static readonly string PLUGIN_VERSION = "0.0.4.0";
        private static readonly int WINDOW_ID = 195;
        private static readonly int WINDOW_ID_POP = 196;
        private static readonly int WINDOW_ID_PROGRAM = 197;

        private static readonly float FACECHANGETTIME = 0.01666667f;

        private static readonly int YOTOGICHKFRAME = 5;

        private int iLastFrame = 0;

        private XMLManager xml;

        private bool bMouseOnWindow = false;

        private int iCurrentMaid = 0;
        private string sCurrentMaid = string.Empty;
        private int iGetMaidTimer = 0;
        private bool bGetMaid = false;

        private Rect rectWin = new Rect();
        private bool bGui = false;
        private Vector2 v2ScreenSize = Vector2.zero;
        private float fWinHeight = 0f;

        private Rect rectWinPop = new Rect();
        private bool bGuiPop = false;
        private bool bGuiPopClose = false;
        private float fWinPopHeight = 0f;
        private int iCurrentFace = 0;

        private Rect rectWinPop2 = new Rect();
        private bool bGuiPop2 = false;
        private bool bGuiPop2Close = false;
        private float fWinPop2Height = 0f;
        private int iCurrentMyFace = 0;
        private string[] sMyFaces;

        private Rect rectPopUpButton = new Rect();

        private Rect rectWinNameChange = new Rect();
        private bool bGuiNameChange = false;
        private bool bNameChangeTarget = true;
        private string sNameChangeTmp = string.Empty;
        private string sNameChangeMessage = string.Empty;
        private int iNameChangeMessageTimer = -1;

        private Rect rectWinProgram = new Rect();
        private bool bGuiProgram = false;
        private bool bGuiProgramPop = false;
        private int iCurrentAutoFaceSet = 0;
        private Vector2 v2ScrollPosL = Vector2.zero;
        private Vector2 v2ScrollPosR = Vector2.zero;
        private Texture2D texWhiteAlpha;
        private Texture2D texRedAlpha;
        private int iSelectAutoFace = 0;
        private bool bFaceListSortMode = false;
        private string[] sAutoFaces;

        private bool bYotogi = false;
        private YotogiPlayManager ypm;
        private FieldInfo fieldZettyo;
        private bool bInitYotogi = false;
        private int iYotogiTimer = 0;
        private int iZettyoCount = 0;
        private int iZettyoNowCount = 0;
        private int lastExcite = 0;

        private Yotogi.SkillDataPair skill_pair;
        private string sSkillName = string.Empty;
        private string sSkillCategory = string.Empty;

        private string keyShowPanel = "f9";

        private static readonly string[] sPoseFeras = new string[] { "fera", "_ir_", "sixnine" };
        private static readonly string[] sPoseFeraExcepts = new string[] { "taiki", "kiss", "sentan", "shaseigo", "shasei_kao" };
        private static readonly string[] sPoseZettyous = { "shasei_", "zeccyou_" };
        private static readonly string[] sPoseZettyougos = { "shaseigo", "zeccyougo" };
        private static readonly string[] sPoseKisses = { "_kiss_", "_momikiss_" };
        private static readonly string[] sPoseHousis = new string[] { "fera", "_ir_", "tekoki", "paizuri", "housi_" };

        private static readonly string[] sFaceNames = new string[]
            {
                "通常","微笑み","笑顔","にっこり",
                "優しさ","発情","ジト目","思案伏せ目",
                "ドヤ顔","引きつり笑顔","苦笑い","困った","疑問",
                "ぷんすか","むー","泣き","拗ね","照れ",
                "きょとん","びっくり","少し怒り","怒り",
                "照れ叫び","誘惑","接吻","居眠り安眠","まぶたギュ",
                "目を見開いて","恥ずかしい","ためいき","がっかり",
                "口開け","目口閉じ","ウインク照れ","にっこり照れ",
                "エロ期待","エロ緊張","エロ怯え",
                "エロ痛み我慢","絶頂射精後１",
                "興奮射精後１","通常射精後１","余韻弱",
                "エロメソ泣き","エロ絶頂","エロ放心","エロ舌責",
                "エロ舌責嫌悪","エロ舌責快楽",
                "エロ興奮０","エロ興奮３","エロ嫌悪１","エロ通常１","エロ通常３",
                "エロ好感１","エロ好感２","エロ好感３","エロ我慢１",
                "エロ我慢３","エロ痛み１","エロ痛み３","エロ羞恥１",
                "エロ羞恥３","エロ舐め嫌悪",
                "エロ舐め快楽","エロ舐め愛情","エロ舐め通常",
                "エロフェラ快楽","エロフェラ愛情","エロフェラ嫌悪","エロフェラ通常",
                "閉じ舐め嫌悪","閉じ舐め嫌悪２","閉じ舐め快楽","閉じ舐め愛情",
                "閉じ舐め通常","閉じフェラ快楽","閉じフェラ愛情",
                "閉じフェラ嫌悪","閉じフェラ通常",
            };
        private static readonly string[,] sKeyVals = new string[,]
            {
                { "eyeclose",   "目閉じ",      "0", "1" },
                { "eyeclose2",  "にっこり",     "0", "0" },
                { "eyeclose3",  "ジト目",      "0", "0" },
                { "eyebig",     "見開く",      "0", "0" },
                { "eyeclose5",  "ウィンク1",    "0", "0" },
                { "eyeclose6",  "ウィンク2",    "0", "0" },
                { "hitomis",    "瞳小",       "0", "0" },

                { "mayuv",      "眉キリッ",     "0", "1" },
                { "mayuw",      "眉困り",      "0", "0" },
                { "mayuha",     "眉ハの字",     "0", "0" },
                { "mayuup",     "眉上げ",      "0", "0" },
                { "mayuvhalf",  "眉傾き",      "0", "0" },

                { "mouthup",   "口角上げ",      "0", "1" },
                { "mouthdw",   "口角下げ",      "0", "0" },
                { "mouthuphalf",   "口角左上げ", "0", "0" },
                { "mouthhe",   "への字口",      "0", "0" },

                { "moutha",     "口あ",       "0", "1" },
                { "mouthc",     "口う",       "0", "0" },
                { "mouthi",     "口い",       "0", "0" },
                { "mouths",     "口笑顔",      "0", "0" },

                { "tangout",    "舌出し1",     "0", "1" },
                { "tangup",     "舌出し2",     "0", "0" },
                { "tangopen",   "舌根上げ",     "0", "0" },
                { "toothoff",   "歯オフ",      "0", "0" },

                { "hohos",      "頬1",           "1", "2" },
                { "hoho",       "頬2",           "1", "0" },
                { "hohol",      "頬3",           "1", "0" },

                { "tear1",      "涙1",           "1", "2" },
                { "tear2",      "涙2",           "1", "0" },
                { "tear3",      "涙3",           "1", "0" },

                { "yodare",     "よだれ",          "1", "2" },
                { "hoho2",      "赤面",           "1", "0" },
                { "shock",      "ショック",     "1", "0" },

                { "namida",     "涙",           "1",  "2" },
                { "hitomih",    "ハイライト",   "1", "0" },
                { "nosefook",    "鼻フック",        "1", "0" },
            };
        //"uru-uru","ウルウル",


        private enum AutoFaceMode
        {
            normal,
            housi,
            kiss,
            najirare,
            msei,
            maidOrgasm,
            afterMaidOrgasm,
            manOrgasm,
            afterManOrgasm
        }

        private class MaidBaseData
        {
            public bool enableMabataki { get; set; }
            public bool enableEye { get; set; }
            public bool eyeDontMove { get; set; }
            public bool enableBlend { get; set; }
            public bool disableMouseWhenFera { get; set; }
            public int faceChangeTimeRatio { get; set; }

            public MaidBaseData()
            {
                enableMabataki = true;
                enableEye = true;
                eyeDontMove = false;
                enableBlend = true;
                disableMouseWhenFera = true;
                faceChangeTimeRatio = 3;
            }
        }

        private class MaidFaceData
        {
            public Maid maid { get; set; }
            public bool enable { get; set; }
            public bool enableEye { get; set; }
            public bool enableBlend { get; set; }
            public bool disableMouseWhenFera { get; set; }
            public bool bInFera { get; set; }
            public bool bInKiss { get; set; }
            public bool bInZettyou { get; set; }
            public bool bInZettyougo { get; set; }
            public bool bInHousi { get; set; }
            public bool bInNajirare { get; set; }
            public bool bInMsei { get; set; }
            public bool bChangeZettyou { get; set; }
            public bool bChangeZettyougo { get; set; }
            public bool eyeDontMove { get; set; }
            public float eyeUp { get; set; }
            public Vector3[] eyePos { get; set; }
            public bool enableMabataki { get; set; }
            private int mabatakiTimer { get; set; }
            public int autoFaceInterval { get; set; }
            public int autoFacetimer { get; set; }
            public int faceChangeTimeRatio { get; set; }
            public string lastPose { get; set; }
            public int lastVoice { get; set; }
            public int lastExcite { get; set; }
            public int noAutoFaceInZettyoTimer { get; set; }
            public int iInZettyouTimer { get; set; }

            public AutoFaceSet autoFaceSet = new AutoFaceSet();
            
            public List<BaseData> data = new List<BaseData>();

            public List<BaseData> dataTmp = new List<BaseData>();
            public float eyeUpTmp { get; set; }
            public bool bDataTmpSet { get; set; }
            private float fFaceTime { get; set; }
            private int iDataTmpCounter { get; set; }

            public MaidFaceData(Maid m)
            {
                for (int i = 0; i < sKeyVals.GetLength(0); i++)
                {
                    data.Add(new BaseData(sKeyVals[i, 0]));
                    dataTmp.Add(new BaseData(sKeyVals[i, 0]));
                }
                enable = false;
                maid = m;
                enableEye = true;
                enableBlend = true;
                disableMouseWhenFera = true;
                eyeDontMove = false;
                eyePos = new Vector3[2] { Vector3.zero, Vector3.zero };
                enableMabataki = true;
                mabatakiTimer = 100;
                lastPose = string.Empty;
                faceChangeTimeRatio = 3;
            }

            public void DataTmpSet()
            {
                fFaceTime = 0f;
                bDataTmpSet = true;
            }
            public void Update()
            {
                if(bDataTmpSet)
                {
                    //fFaceTime += 0.01666667f;
                    fFaceTime += FACECHANGETTIME * (faceChangeTimeRatio / 3f);
                    float mul = UTY.COSS2(Mathf.Pow(fFaceTime, 0.4f), 4f);
                    for (int i = 0; i < dataTmp.Count; i++)
                    {
                        if (sKeyVals[i, 2] == "0")
                            data[i].val = data[i].val * (1f - mul) + dataTmp[i].val * mul;
                        //else
                        //    data[i].val = dataTmp[i].val;
                    }
                    eyeUp = eyeUp * (1f - mul) + eyeUpTmp * mul;
                    if (fFaceTime > 1f)
                    {
                        fFaceTime = 0f;
                        bDataTmpSet = false;
                    }
                }

                if (enableMabataki)
                    UpdateMabataki();
            }

            private void UpdateMabataki()
            {
                if (maid == null)
                    return;
                if (--mabatakiTimer < 0)
                {
                    maid.body0.Face.morph.EyeMabataki = 0f;
                    mabatakiTimer = UnityEngine.Random.Range(40, 400);
                }
                else if (mabatakiTimer < 14)
                {
                    maid.body0.Face.morph.EyeMabataki -= 0.0714f;
                }
                else if(mabatakiTimer < 22)
                {
                    maid.body0.Face.morph.EyeMabataki += 0.125f;
                }
            }

            public bool UpdateAutoFace(int iSub)
            {
                if (autoFaceSet.autofaces.Count == 0 || autoFaceInterval == 0)
                    return false;

                if ((autoFacetimer -= iSub) < 0)
                {
                    autoFacetimer = UnityEngine.Random.Range(300, autoFaceInterval + 300);
                    return true;
                }
                return false;
            }

            //public void SetAutoFaceTimer()
            //{
            //    autoFacetimer = UnityEngine.Random.Range(300, autoFaceInterval + 300);
            //}

            //public List<AutoFaceValue> GetAutoFaceValue(int iExcite, bool bOrgasm, bool bKiss, bool bManOrgasm, int zettyo)
            public List<AutoFaceValue> GetAutoFaceValue(int iExcite, int zettyo, AutoFaceMode afm)
            {
                List<AutoFaceValue> afvList = new List<AutoFaceValue>();
                bool b = false;
                foreach (AutoFaceData afd in autoFaceSet.autofaces)
                {
                    if (!afd.enable)
                        continue;

                    b = false;
                    switch(afm)
                    {
                        case AutoFaceMode.normal:
                            if (afd.normal)
                                b = true;
                            else
                            {
                                if (!afd.orgasm && !afd.afterOrgasm && !afd.manOrgasm && !afd.afterManOrgasm &&
                                    !afd.najirare && !afd.msei && !afd.housi && !afd.kiss)
                                {
                                    b = true;
                                }
                            }
                            break;
                        case AutoFaceMode.maidOrgasm:
                            if (afd.orgasm)
                                b = true;
                            break;
                        case AutoFaceMode.afterMaidOrgasm:
                            if (afd.afterOrgasm)
                                b = true;
                            break;
                        case AutoFaceMode.manOrgasm:
                            if (afd.manOrgasm)
                                b = true;
                            break;
                        case AutoFaceMode.afterManOrgasm:
                            if (afd.afterManOrgasm)
                                b = true;
                            break;
                        case AutoFaceMode.najirare:
                            if (afd.najirare)
                                b = true;
                            break;
                        case AutoFaceMode.msei:
                            if (afd.msei)
                                b = true;
                            break;
                        case AutoFaceMode.housi:
                            if (afd.housi)
                                b = true;
                            break;
                        case AutoFaceMode.kiss:
                            if (afd.kiss)
                                b = true;
                            break;
                    }

                    //if (afm == AutoFaceMode.maidOrgasm && !afd.orgasm)
                    //    continue;
                    //if (afm != AutoFaceMode.maidOrgasm && afd.orgasm)
                    //    continue;

                    //if (afm == AutoFaceMode.afterMaidOrgasm && !afd.afterOrgasm)
                    //    continue;
                    //if (afm != AutoFaceMode.afterMaidOrgasm && afd.afterOrgasm)
                    //    continue;

                    //if (afm == AutoFaceMode.manOrgasm && !afd.manOrgasm)
                    //    continue;
                    //if (afm != AutoFaceMode.manOrgasm && afd.manOrgasm)
                    //    continue;

                    //if (afm == AutoFaceMode.afterManOrgasm && !afd.afterManOrgasm)
                    //    continue;
                    //if (afm != AutoFaceMode.afterManOrgasm && afd.afterManOrgasm)
                    //    continue;

                    //if (afm == AutoFaceMode.najirare && !afd.najirare)
                    //    continue;
                    //if (afm != AutoFaceMode.najirare && afd.najirare)
                    //    continue;

                    //if (afm == AutoFaceMode.msei && !afd.msei)
                    //    continue;
                    //if (afm != AutoFaceMode.msei && afd.msei)
                    //    continue;

                    //if (afm == AutoFaceMode.housi && !afd.housi)
                    //    continue;
                    //if (afm != AutoFaceMode.housi && afd.housi)
                    //    continue;

                    //if (afm == AutoFaceMode.kiss && !afd.kiss)
                    //    continue;
                    //if (afm != AutoFaceMode.kiss && afd.kiss)
                    //    continue;

                    if (!b)
                        continue;
                    if (afd.zettyoLow > zettyo || zettyo > afd.zettyoHigh)
                        continue;
                    if (afd.exciteLow <= iExcite && iExcite <= afd.exciteHigh)
                    {
                        afvList.Add(new AutoFaceValue());
                        afvList.Last().morph = afd.morph;
                        afvList.Last().blend = afd.blend;
                        afvList.Last().hitomiy = afd.hitomiy;
                        afvList.Last().guidList = afd.guidList;
                    }
                }
                return afvList;
            }

            public class AutoFaceValue
            {
                public List<string> guidList = new List<string>();
                public bool morph { get; set; }
                public bool blend { get; set; }
                public bool hitomiy { get; set; }
            }
        }

        public class AutoFaceData
        {
            public bool enable { get; set; }
            public int exciteLow { get; set; }
            public int exciteHigh { get; set; }
            public bool morph { get; set; }
            public bool blend { get; set; }
            public bool hitomiy { get; set; }
            public bool normal { get; set; }
            public bool orgasm { get; set; }
            public bool afterOrgasm { get; set; }
            public bool manOrgasm { get; set; }
            public bool afterManOrgasm { get; set; }
            public bool kiss { get; set; }
            public bool housi { get; set; }
            public bool najirare { get; set; }
            public bool msei { get; set; }
            public int zettyoLow { get; set; }
            public int zettyoHigh { get; set; }
            public List<string> guidList = new List<string>();

            public AutoFaceData()
            {
                enable = true;
                morph = true;
                blend = true;
                hitomiy = false;
            }
        }

        public class FaceData
        {
            public List<BaseData> data = new List<BaseData>();
            public string name { get; set; }
            public string guid { get; set; }
            public float eyeUp { get; set; }

            public FaceData()
            {
                Init(null, null, 0f);
            }

            public FaceData(string s)
            {
                Init(s, null, 0f);
            }

            public FaceData(string s, List<BaseData> d)
            {
                Init(s, d, 0f);
            }

            public FaceData(string s, List<BaseData> d, float fEyeUp)
            {
                Init(s, d, fEyeUp);
            }

            private void Init(string s, List<BaseData> d, float fEyeUp)
            {
                guid = Guid.NewGuid().ToString("N");
                if (s == null)
                    name = string.Empty;
                else
                {
                    name = s;
                    if (d == null)
                    {
                        for (int i = 0; i < sKeyVals.GetLength(0); i++)
                        {
                            data.Add(new BaseData(sKeyVals[i, 0]));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < sKeyVals.GetLength(0); i++)
                        {
                            data.Add(new BaseData(sKeyVals[i, 0], d[i].val));
                        }
                    }
                }
                eyeUp = fEyeUp;
            }

            public void SetFaceData(List<BaseData> d, float fEyeUp)
            {
                for (int i = 0; i < sKeyVals.GetLength(0); i++)
                {
                    data[i].val = d[i].val;
                }
                eyeUp = fEyeUp;
            }
        }

        public class BaseData
        {
            public string key { get; set; }
            public float val { get; set; }

            public BaseData()
            {
                key = string.Empty;
                val = 0f;
            }
            public BaseData(string k)
            {
                key = k;
                val = 0f;
            }
            public BaseData(string k, float v)
            {
                key = k;
                val = v;
            }
        }

        public class AutoFaceSet
        {
            public string name { get; set; }
            public string guid { get; set; }
            public bool noAutoFaceInZettyo { get; set; }
            public int zettyointerval { get; set; }
            public List<AutoFaceData> autofaces = new List<AutoFaceData>();

            public AutoFaceSet()
            {
                name = string.Empty;
                guid = Guid.NewGuid().ToString("N");
            }
            public AutoFaceSet(string n, List<AutoFaceData> a)
            {
                name = n == null ? string.Empty : n;
                if (a != null)
                    autofaces = a;
                guid = Guid.NewGuid().ToString("N");
            }
            public AutoFaceSet(string n, List<AutoFaceData> a, int z, string g)
            {
                name = n == null ? string.Empty : n;
                if (a != null)
                    autofaces = a;
                zettyointerval = z;
                if (string.IsNullOrEmpty(g))
                    guid = Guid.NewGuid().ToString("N");
                else
                    guid = g;
            }
        }
        
        private List<FaceData> fdList = new List<FaceData>();
        private List<AutoFaceSet> afList = new List<AutoFaceSet>();
        private List<MaidFaceData> mfdList = new List<MaidFaceData>();
        private MaidBaseData mbd = new MaidBaseData();

        public void Awake()
        {
            UnityEngine.GameObject.DontDestroyOnLoad(this);
        }

        public void Start()
        {
            {
                texWhiteAlpha = new Texture2D(2, 2);
                Color32[] colorWhiteAlpha = new Color32[4];
                for (int i = 0; i < colorWhiteAlpha.Length; i++)
                {
                    colorWhiteAlpha[i] = new Color32(255, 255, 255, 128);
                }
                texWhiteAlpha.SetPixels32(colorWhiteAlpha);
                texWhiteAlpha.Apply();
            }
            {
                texRedAlpha = new Texture2D(2, 2);
                Color32[] colorRedAlpha = new Color32[4];
                for (int i = 0; i < colorRedAlpha.Length; i++)
                {
                    colorRedAlpha[i] = new Color32(192, 64, 64, 64);
                }
                texRedAlpha.SetPixels32(colorRedAlpha);
                texRedAlpha.Apply();
            }
            xml = new XMLManager();
            LoadXML(true);
        }

        public void OnLevelWasLoaded(int level)
        {
            bGui = false;
            bMouseOnWindow = false;

            MaidDataClear();

            bYotogi = false;
            bInitYotogi = false;
            ypm = null;
            fieldZettyo = null;
            sSkillName = string.Empty;
            sSkillCategory = string.Empty;

            iYotogiTimer = 0;
            //iInZettyoTimer = 0;
            iZettyoCount = 0;
            iZettyoNowCount = 0;
            //bInZettyo = false;

            bGuiProgram = false;
            sAutoFaces = null;
            sMyFaces = null;
            bFaceListSortMode = false;
            iCurrentFace = 0;
            iCurrentMyFace = 0;
            iCurrentAutoFaceSet = 0;
            iSelectAutoFace = 0;
            GameMain.Instance.MainCamera.SetControl(true);

            if (level == 10 || level == 14)
                bYotogi = true;

            //iSceneLevel = level;
        }
        
        public void OnGUI()
        {
            if (bGui)
            {
                GUIStyle winStyle = new GUIStyle("box");
                winStyle.fontSize = GetPix(12);
                winStyle.alignment = TextAnchor.UpperRight;
                if (rectWin.width < 1)
                {
                    rectWin.Set(Screen.width - winStyle.fontSize * 20, 0, winStyle.fontSize * 20, winStyle.fontSize * 50);
                }

                if (fWinHeight != rectWin.height)
                {
                    rectWin.Set(rectWin.x, rectWin.y, rectWin.width, fWinHeight);
                }

                if (v2ScreenSize != new Vector2(Screen.width, Screen.height))
                {
                    rectWin.Set(rectWin.x, rectWin.y, winStyle.fontSize * 20, fWinHeight);
                    if(rectWinProgram.width > 1)
                        rectWinProgram.Set(rectWinProgram.x, rectWinProgram.y, winStyle.fontSize * 61, winStyle.fontSize * 55);
                    v2ScreenSize = new Vector2(Screen.width, Screen.height);
                }
                if (rectWin.x < 0 - rectWin.width * 0.9f)
                {
                    rectWin.x = 0;
                }
                else if (rectWin.x > v2ScreenSize.x - rectWin.width * 0.1f)
                {
                    rectWin.x = v2ScreenSize.x - rectWin.width;
                }
                else if (rectWin.y < 0 - rectWin.height * 0.9f)
                {
                    rectWin.y = 0;
                }
                else if (rectWin.y > v2ScreenSize.y - rectWin.height * 0.1f)
                {
                    rectWin.y = v2ScreenSize.y - rectWin.height;
                }
                rectWin = GUI.Window(WINDOW_ID, rectWin, GuiFunc, PLUGIN_NAME + PLUGIN_VERSION, winStyle);

                if (GetAnyMouseButtonUp())
                {
                    if (bGuiPopClose)
                    {
                        Vector2 v2Tmp = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                        if (!rectPopUpButton.Contains(v2Tmp))
                            bGuiPopClose = false;
                    }
                    if (!bGuiPop2)
                    {
                        Vector2 v2Tmp = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                        if (!rectPopUpButton.Contains(v2Tmp))
                            bGuiPop2Close = false;
                    }
                }
                if (bGuiPop)
                {
                    rectWinPop = GUI.Window(WINDOW_ID_POP, rectWinPop, GuiFuncPop, string.Empty, winStyle);
                    if (GetAnyMouseButtonDown())
                    {
                        Vector2 v2Tmp = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                        if (!rectWinPop.Contains(v2Tmp))
                        {
                            bGuiPop = false;
                            if (rectPopUpButton.Contains(v2Tmp))
                                bGuiPopClose = true;
                        }
                    }
                }
                if (bGuiPop2 || bGuiProgramPop)
                {
                    if(bGuiPop2)
                        rectWinPop2 = GUI.Window(WINDOW_ID_POP, rectWinPop2, GuiFuncPop2, string.Empty, winStyle);
                    else
                        rectWinPop2 = GUI.Window(WINDOW_ID_POP, rectWinPop2, GuiFuncPopPg, string.Empty, winStyle);
                    if (GetAnyMouseButtonDown())
                    {
                        Vector2 v2Tmp = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                        if (!rectWinPop2.Contains(v2Tmp))
                        {
                            bGuiPop2 = false;
                            bGuiProgramPop = false;
                            if(rectPopUpButton.Contains(v2Tmp))
                                bGuiPop2Close = true;
                        }
                    }
                }
                if(bGuiNameChange)
                {
                    if(rectWinNameChange.width < 1)
                    {
                        float fWidth = winStyle.fontSize * 10;
                        float fHeight = winStyle.fontSize * 10;
                        rectWinNameChange.Set((Screen.width / 2) - (fWidth / 2), (Screen.height / 2) - (fHeight / 2), fWidth, fHeight);
                    }
                    rectWinNameChange = GUI.Window(WINDOW_ID_POP, rectWinNameChange, GuiFuncNameChange, string.Empty, winStyle);
                    if (GetAnyMouseButtonDown() && !rectWinNameChange.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                        bGuiNameChange = false;
                }

                if (bGuiProgram)
                {
                    if (rectWinProgram.width < 1)
                    {
                        float fWidth = winStyle.fontSize * 61;
                        float fHeight = winStyle.fontSize * 55;
                        rectWinProgram.Set((Screen.width / 2) - (fWidth / 2), (Screen.height / 2) - (fHeight / 2), fWidth, fHeight);
                    }
                    //if (fWinHeight != rectWinProgram.height)
                    //{
                    //    rectWinProgram.Set(rectWinProgram.x, rectWinProgram.y, rectWinProgram.width, fWinHeight);
                    //}

                    if (rectWinProgram.x < 0 - rectWinProgram.width * 0.9f)
                    {
                        rectWinProgram.x = 0;
                    }
                    else if (rectWinProgram.x > v2ScreenSize.x - rectWinProgram.width * 0.1f)
                    {
                        rectWinProgram.x = v2ScreenSize.x - rectWinProgram.width;
                    }
                    else if (rectWinProgram.y < 0 - rectWinProgram.height * 0.9f)
                    {
                        rectWinProgram.y = 0;
                    }
                    else if (rectWinProgram.y > v2ScreenSize.y - rectWinProgram.height * 0.1f)
                    {
                        rectWinProgram.y = v2ScreenSize.y - rectWinProgram.height;
                    }
                    if (rectWinProgram.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                    {
                        GameMain.Instance.MainCamera.SetControl(false);
                        bMouseOnWindow = true;
                    }
                    rectWinProgram = GUI.Window(WINDOW_ID_PROGRAM, rectWinProgram, GuiFuncProgram, PLUGIN_NAME + PLUGIN_VERSION, winStyle);
                }
            }
            else 
            {
                if(bMouseOnWindow)
                    GameMain.Instance.MainCamera.SetControl(true);
                bMouseOnWindow = false;
            }
        }
        
        private void GuiFunc(int winId)
        {
            GUIStyle gsLabel = new GUIStyle("label");
            gsLabel.fontSize = GetPix(12);
            gsLabel.alignment = TextAnchor.UpperLeft;

            int fFontSize = gsLabel.fontSize;

            GUIStyle gsLabelR = new GUIStyle("label");
            gsLabelR.fontSize = fFontSize;
            gsLabelR.alignment = TextAnchor.UpperRight;

            GUIStyle gsToggle = new GUIStyle("toggle");
            gsToggle.fontSize = fFontSize;
            gsToggle.alignment = TextAnchor.MiddleLeft;

            GUIStyle gsButton = new GUIStyle("button");
            gsButton.fontSize = fFontSize;
            gsButton.alignment = TextAnchor.MiddleCenter;

            GUIStyle gsButtonL = new GUIStyle("button");
            gsButtonL.fontSize = fFontSize;
            gsButtonL.alignment = TextAnchor.MiddleLeft;

            float fItemHeight = fFontSize * 1.5f;
            float fMargin = fFontSize * 0.5f;
            float fRightPos = rectWin.width / 2 + (fFontSize / 2);
            float fTmp;

            Rect rectInner = new Rect(fFontSize / 2, fFontSize + fMargin, rectWin.width - fFontSize, rectWin.height - fFontSize);
            Rect rectItem = new Rect(0f, 0f, fItemHeight, fItemHeight);

            if (GUI.Button(rectItem, "×", gsButton))
            {
                bGui = false;
            }

            rectItem.Set(rectInner.x, rectItem.y + rectItem.height + fMargin, fFontSize * 2, fItemHeight);
            if (GUI.Button(rectItem, "<", gsButton))
            {
                --iCurrentMaid;
                GetMaid();
                SetCurrentAutoFaceSet();
            }
            rectItem.x += rectItem.width;
            if (GUI.Button(rectItem, ">", gsButton))
            {
                ++iCurrentMaid;
                GetMaid();
                SetCurrentAutoFaceSet();
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 7, rectItem.height);
            GUI.Label(rectItem, sCurrentMaid, gsLabel);

            if (mfdList.Count == 0)
            {
                fWinHeight = rectItem.y + rectItem.height + fMargin;
                GUI.DragWindow();
                return;
            }

            bool bTmp;
            rectItem.Set(rectInner.x, rectItem.y + rectItem.height + fMargin, fFontSize * 4, rectItem.height);
            bTmp = GUI.Toggle(rectItem, mfdList[iCurrentMaid].enable, "有効", gsToggle);
            if(bTmp != mfdList[iCurrentMaid].enable)
            {
                SetCurrentAutoFaceSet();
                CreateMyFaceNameArray();
                mfdList[iCurrentMaid].enable = bTmp;
                if(bTmp)
                {
                    mfdList[iCurrentMaid].maid.boMabataki = false;
                    mfdList[iCurrentMaid].maid.body0.Face.morph.EyeMabataki = 0f;
                    mfdList[iCurrentMaid].eyePos[0] = mfdList[iCurrentMaid].maid.body0.trsEyeL.localPosition;
                    mfdList[iCurrentMaid].eyePos[1] = mfdList[iCurrentMaid].maid.body0.trsEyeR.localPosition;
                }
                else
                {
                    mfdList[iCurrentMaid].maid.boMabataki = true;
                    mfdList[iCurrentMaid].maid.body0.trsEyeL.localPosition = mfdList[iCurrentMaid].eyePos[0];
                    mfdList[iCurrentMaid].maid.body0.trsEyeR.localPosition = mfdList[iCurrentMaid].eyePos[1];
                }
            }

            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 5, rectItem.height);
            mfdList[iCurrentMaid].enableMabataki = GUI.Toggle(rectItem, mfdList[iCurrentMaid].enableMabataki, "まばたき", gsToggle);

            rectItem.Set(fRightPos, rectItem.y, fFontSize * 4.5f, rectItem.height);
            if (GUI.Button(rectItem, "保存", gsButton))
            {
                SaveXML();
                LoadMaidBaseData();
            }

            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, rectItem.width, rectItem.height);
            if (GUI.Button(rectItem, "復元", gsButton))
            {
                LoadXML(false);
                LoadAutoFaceGuid();
                SetCurrentAutoFaceSet();
            }



            rectItem.Set(rectInner.x, rectItem.y + rectItem.height + fMargin, fFontSize * 2, rectItem.height);
            if (GUI.Button(rectItem, "<", gsButton))
            {
                if (--iCurrentAutoFaceSet < 0)
                    if (afList.Count == 0)
                        iCurrentAutoFaceSet = 0;
                    else
                        iCurrentAutoFaceSet = afList.Count - 1;
                SetMfdAutoFace();
                if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                    iSelectAutoFace = 0;
            }
            rectItem.x += rectItem.width;
            if (GUI.Button(rectItem, ">", gsButton))
            {
                if (++iCurrentAutoFaceSet >= afList.Count)
                    iCurrentAutoFaceSet = 0;
                SetMfdAutoFace();
                if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                    iSelectAutoFace = 0;
            }

            rectItem.Set(rectInner.x, rectItem.y + rectItem.height, fFontSize * 9, rectItem.height);
            if (GUI.Button(rectItem, afList.Count == 0 ? "- - -" : afList[iCurrentAutoFaceSet].name, gsButton))
            {
                if (bGuiPop2Close)
                {
                    bGuiPop2Close = false;
                    SetMfdAutoFace();
                    if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                        iSelectAutoFace = 0;
                }
                else
                {
                    CreateAutoFaceNameArray();
                    fWinPop2Height = fItemHeight * sAutoFaces.Length;
                    rectWinPop2.Set(rectWin.x + rectItem.x, rectWin.y + rectItem.y + rectItem.height, rectItem.width, fWinPop2Height);
                    bGuiProgramPop = true;
                    rectPopUpButton = new Rect(rectWin.x + rectItem.x, rectWin.y + rectItem.y, rectItem.width, rectItem.height);
                }
            }



            rectItem.Set(fRightPos, rectItem.y - rectItem.height, fFontSize * 9, rectItem.height);
            mfdList[iCurrentMaid].disableMouseWhenFera = GUI.Toggle(rectItem, mfdList[iCurrentMaid].disableMouseWhenFera, "フェラ時口無効", gsToggle);

            rectItem.Set(fRightPos, rectItem.y + rectItem.height, fFontSize * 8, rectItem.height);
            mfdList[iCurrentMaid].enableBlend = GUI.Toggle(rectItem, mfdList[iCurrentMaid].enableBlend, "ブレンド読込", gsToggle);




            //rectItem.Set(rectInner.x + rectItem.width, rectItem.y + rectItem.height, fFontSize * 3, rectItem.height);
            //GUI.Label(rectItem, mfdList[iCurrentMaid].autoFacetimer.ToString(), gsLabelR);

            rectItem.Set(rectInner.x, rectItem.y + rectItem.height + fMargin, fFontSize * 4, rectItem.height);
            if (GUI.Button(rectItem, "PG", gsButton))
            {
                SetCurrentAutoFaceSet();
                CreateMyFaceNameArray();
                bGuiProgram = !bGuiProgram;
            }

            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 5, rectItem.height);
            GUI.Label(rectItem, mfdList[iCurrentMaid].autoFacetimer.ToString() + "/" + mfdList[iCurrentMaid].autoFaceInterval.ToString(), gsLabelR);

            rectItem.Set(rectInner.x, rectItem.y + rectItem.height, rectWin.width / 2 - fFontSize, rectItem.height);
            mfdList[iCurrentMaid].autoFaceInterval = (int)GUI.HorizontalSlider(rectItem, mfdList[iCurrentMaid].autoFaceInterval, 0f, 1000f);


            rectItem.Set(fRightPos, rectItem.y - rectItem.height, fFontSize * 7, rectItem.height);
            GUI.Label(rectItem, "表情変化速度", gsLabel);

            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
            if(GUI.Button(rectItem, "|", gsButton))
            {
                mfdList[iCurrentMaid].faceChangeTimeRatio = 3;
            }

            rectItem.Set(fRightPos, rectItem.y + rectItem.height, fFontSize * 9, rectItem.height);
            mfdList[iCurrentMaid].faceChangeTimeRatio = (int)GUI.HorizontalSlider(rectItem, mfdList[iCurrentMaid].faceChangeTimeRatio, 1, 5);


            rectItem.Set(rectInner.x, rectItem.y + rectItem.height + fMargin, fFontSize * 2, rectItem.height);
            if(GUI.Button(rectItem, "<", gsButton))
            {
                if (--iCurrentFace < 0)
                    iCurrentFace = sFaceNames.Length - 1;
                SetFace();
            }
            rectItem.x += rectItem.width;
            if (GUI.Button(rectItem, ">", gsButton))
            {
                if (++iCurrentFace >= sFaceNames.Length)
                    iCurrentFace = 0;
                SetFace();
            }
            rectItem.Set(rectInner.x, rectItem.y + rectItem.height, fFontSize * 9, rectItem.height);
            if (GUI.Button(rectItem, sFaceNames[iCurrentFace], gsButton))
            {
                if(bGuiPopClose)
                {
                    SetFace(); 
                    bGuiPopClose = false;
                }
                else
                {
                    fWinPopHeight = fItemHeight * sFaceNames.Length / 2;
                    rectWinPop.Set(rectWin.x + rectItem.x, rectWin.y + rectItem.y + rectItem.height, rectItem.width * 2, fWinPopHeight);
                    bGuiPop = true;
                    rectPopUpButton = new Rect(rectWin.x + rectItem.x, rectWin.y + rectItem.y, rectItem.width, rectItem.height);
                }
            }

            rectItem.Set(fRightPos, rectItem.y - rectItem.height, fFontSize * 2, rectItem.height);
            if (GUI.Button(rectItem, "<", gsButton))
            {
                if (--iCurrentMyFace < 0)
                {
                    if (fdList.Count == 0)
                        iCurrentMyFace = 0;
                    else
                        iCurrentMyFace = fdList.Count - 1;
                }
                if (fdList.Count != 0)
                {
                    SetFace(fdList[iCurrentMyFace].data, mfdList[iCurrentMaid], new bool[3] { true, true, true}, fdList[iCurrentMyFace].eyeUp);
                }
            }
            rectItem.x += rectItem.width;
            if (GUI.Button(rectItem, ">", gsButton))
            {
                if (++iCurrentMyFace >= fdList.Count)
                    iCurrentMyFace = 0;
                if (fdList.Count != 0)
                {
                    SetFace(fdList[iCurrentMyFace].data, mfdList[iCurrentMaid], new bool[3] { true, true, true }, fdList[iCurrentMyFace].eyeUp);
                }
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 5, rectItem.height);
            if (GUI.Button(rectItem, "名前変更", gsButton))
            {
                if (fdList.Count > 0)
                {
                    CreateMyFaceNameArray();
                    sNameChangeTmp = fdList[iCurrentMyFace].name;
                    bGuiNameChange = true;
                    bNameChangeTarget = true;
                }
            }

            rectItem.Set(fRightPos, rectItem.y + rectItem.height, fFontSize * 9, rectItem.height);
            if (GUI.Button(rectItem, fdList.Count == 0 ? "- - -" : fdList[iCurrentMyFace].name, fdList.Count != 0 && fdList[iCurrentMyFace].name.Length > 8 ? gsButtonL : gsButton))
            {
                if (bGuiPop2Close)
                {
                    if (fdList.Count != 0)
                    {
                        SetFace(fdList[iCurrentMyFace].data, mfdList[iCurrentMaid], new bool[3] { true, true, true }, fdList[iCurrentMyFace].eyeUp);
                    }
                    bGuiPop2Close = false;
                }
                else
                {
                    CreateMyFaceNameArray();
                    fWinPop2Height = fItemHeight * sMyFaces.Length;
                    rectWinPop2.Set(rectWin.x + rectItem.x, rectWin.y + rectItem.y + rectItem.height, rectItem.width, fWinPop2Height);
                    bGuiPop2 = true;
                    rectPopUpButton = new Rect(rectWin.x + rectItem.x, rectWin.y + rectItem.y, rectItem.width, rectItem.height);
                }
            }
            rectItem.Set(rectItem.x, rectItem.y + rectItem.height, fFontSize * 3, rectItem.height);
            if (GUI.Button(rectItem, "追加", gsButton))
            {
                AddFaceList(mfdList[iCurrentMaid].data, mfdList[iCurrentMaid].eyeUp);
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, rectItem.width, rectItem.height);
            if (GUI.Button(rectItem, "上書", gsButton))
            {
                ChangeFaceList(mfdList[iCurrentMaid].data, mfdList[iCurrentMaid].eyeUp);
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, rectItem.width, rectItem.height);
            if (GUI.Button(rectItem, "削除", gsButton))
            {
                RemoveFaceList();
            }



            rectItem.Set(rectInner.x, rectItem.y + rectItem.height, fFontSize * 3, rectItem.height);
            bTmp = GUI.Toggle(rectItem, mfdList[iCurrentMaid].enableEye, "瞳Y", gsToggle);
            if (mfdList[iCurrentMaid].enableEye != bTmp)
            {
                mfdList[iCurrentMaid].enableEye = bTmp;
                if (bTmp)
                {
                    mfdList[iCurrentMaid].eyePos[0] = mfdList[iCurrentMaid].maid.body0.trsEyeL.localPosition;
                    mfdList[iCurrentMaid].eyePos[1] = mfdList[iCurrentMaid].maid.body0.trsEyeR.localPosition;
                }
                else
                {
                    mfdList[iCurrentMaid].maid.body0.trsEyeL.localPosition = mfdList[iCurrentMaid].eyePos[0];
                    mfdList[iCurrentMaid].maid.body0.trsEyeR.localPosition = mfdList[iCurrentMaid].eyePos[1];
                }
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 4, rectItem.height);
            mfdList[iCurrentMaid].eyeDontMove = GUI.Toggle(rectItem, mfdList[iCurrentMaid].eyeDontMove, "固定", gsToggle);

            rectItem.Set(rectItem.x - fFontSize * 3, rectItem.y + rectItem.height, rectWin.width / 2 - fFontSize, rectItem.height);
            fTmp = GUI.HorizontalSlider(rectItem, mfdList[iCurrentMaid].eyeUp, 0f, 1f);
            if (!mfdList[iCurrentMaid].bDataTmpSet && fTmp != mfdList[iCurrentMaid].eyeUp)
            {
                mfdList[iCurrentMaid].eyeUp = fTmp;
                mfdList[iCurrentMaid].eyeUpTmp = fTmp;
            }

            rectItem.Set(fRightPos, rectItem.y - fMargin, fFontSize * 6, rectItem.height);
            if (GUI.Button(rectItem, "絶頂数:" + iZettyoNowCount.ToString(), gsButton))
            {
                iZettyoNowCount = 0;
            }

            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 1.5f, rectItem.height);
            if (GUI.Button(rectItem, "<", gsButton))
            {
                if (--iZettyoNowCount < 0)
                    iZettyoNowCount = 0;
            }
            rectItem.x += rectItem.width;
            if (GUI.Button(rectItem, ">", gsButton))
            {
                if (++iZettyoNowCount > 10)
                    iZettyoNowCount = 10;
            }

            rectItem = new Rect(0f, rectItem.y, rectWin.width / 2 - fFontSize, fItemHeight);
            bool bLeft = false;
            bool bToggle = false;
            float fMargin2;
            

            for (int i = 0; i < sKeyVals.GetLength(0); i++)
            {
                if(sKeyVals[i, 3] == "2")
                {
                    bToggle = true;
                    bLeft = true;
                    rectItem.width = rectInner.width / 3;
                    fMargin2 = 0f;
                }
                else if (sKeyVals[i, 3] == "1")
                {
                    fMargin2 = fMargin;
                    bLeft = true;
                }
                else
                {
                    fMargin2 = 0f;
                }

                if (!bToggle)
                {
                    if (bLeft)
                        rectItem.Set(rectInner.x, rectItem.y + rectItem.height + fMargin2, rectItem.width, rectItem.height);
                    else
                        rectItem.Set(fRightPos, rectItem.y - rectItem.height, rectItem.width, rectItem.height);

                    GUI.Label(rectItem, sKeyVals[i, 1], gsLabel);

                    rectItem.Set(rectItem.x, rectItem.y + rectItem.height, rectItem.width, rectItem.height);
                    fTmp = GUI.HorizontalSlider(rectItem, mfdList[iCurrentMaid].data[i].val, 0f, 1f);
                    if(!mfdList[iCurrentMaid].bDataTmpSet && fTmp != mfdList[iCurrentMaid].dataTmp[i].val)
                    {
                        mfdList[iCurrentMaid].data[i].val = fTmp;
                        mfdList[iCurrentMaid].dataTmp[i].val = fTmp;
                    }

                    bLeft = !bLeft;
                }
                else
                {
                    if (bLeft)
                    {
                        rectItem.Set(rectInner.x, rectItem.y + rectItem.height, rectItem.width, rectItem.height);
                        bLeft = false;
                    }
                    else
                        rectItem.Set(rectItem.x + rectItem.width, rectItem.y, rectItem.width, rectItem.height);

                    bTmp = mfdList[iCurrentMaid].data[i].val >= 1f ? true : false;
                    bool bTmp2 = bTmp;
                    bTmp = GUI.Toggle(rectItem, bTmp, sKeyVals[i, 1], gsToggle);
                    if(!mfdList[iCurrentMaid].bDataTmpSet && bTmp != bTmp2)
                    {
                        mfdList[iCurrentMaid].data[i].val = bTmp ? 1f : 0f;
                        mfdList[iCurrentMaid].dataTmp[i].val = mfdList[iCurrentMaid].data[i].val;
                    }
                }
            }

            fWinHeight = rectItem.y + rectItem.height + fMargin;
            GUI.DragWindow();
        }
        
        private void GuiFuncPop(int winId)
        {
            GUIStyle gsSelectionGrid = new GUIStyle();
            gsSelectionGrid.fontSize = GetPix(12);

            GUIStyleState gssBlack = new GUIStyleState();
            gssBlack.textColor = Color.white;
            gssBlack.background = Texture2D.blackTexture;
            GUIStyleState gssWhite = new GUIStyleState();
            gssWhite.textColor = Color.black;
            gssWhite.background = Texture2D.whiteTexture;

            gsSelectionGrid.normal = gssBlack;
            gsSelectionGrid.hover = gssWhite;

            Rect rectItem = new Rect(0f, 0f, rectWinPop.width, fWinPopHeight);

            int iTmp = -1;
            iTmp = GUI.SelectionGrid(rectItem, -1, sFaceNames, 2, gsSelectionGrid);
            if(iTmp >= 0)
            {
                iCurrentFace = iTmp;
                SetFace();
                bGuiPop = false;
            }
        }

        private void GuiFuncPop2(int winId)
        {
            GUIStyle gsSelectionGrid = new GUIStyle();
            gsSelectionGrid.fontSize = GetPix(12);

            GUIStyleState gssBlack = new GUIStyleState();
            gssBlack.textColor = Color.white;
            gssBlack.background = Texture2D.blackTexture;
            GUIStyleState gssWhite = new GUIStyleState();
            gssWhite.textColor = Color.black;
            gssWhite.background = Texture2D.whiteTexture;

            gsSelectionGrid.normal = gssBlack;
            gsSelectionGrid.hover = gssWhite;

            Rect rectItem = new Rect(0f, 0f, rectWinPop2.width, fWinPop2Height);

            int iTmp = -1;
            iTmp = GUI.SelectionGrid(rectItem, -1, sMyFaces, 1, gsSelectionGrid);
            if (iTmp >= 0)
            {
                iCurrentMyFace = iTmp;
                if (fdList.Count != 0)
                {
                    SetFace(fdList[iCurrentMyFace].data, mfdList[iCurrentMaid], new bool[3] { true, true, true }, fdList[iCurrentMyFace].eyeUp);
                    //if (mfdList[iCurrentMaid].eyeChara)
                    //    SetFace(fdList[iCurrentMyFace].data);
                    //else
                    //    SetFace(fdList[iCurrentMyFace].data, fdList[iCurrentMyFace].eyeUp);
                }
                bGuiPop2 = false;
            }
        }

        private void GuiFuncNameChange(int winId)
        {
            GUIStyle gsLabel = new GUIStyle("label");
            gsLabel.fontSize = GetPix(12);
            gsLabel.alignment = TextAnchor.UpperLeft;

            int fFontSize = gsLabel.fontSize;

            GUIStyle gsText = new GUIStyle("textfield");
            gsText.fontSize = fFontSize;
            gsText.alignment = TextAnchor.UpperLeft;

            GUIStyle gsButton = new GUIStyle("button");
            gsButton.fontSize = fFontSize;
            gsButton.alignment = TextAnchor.MiddleCenter;

            float fItemHeight = fFontSize * 1.5f;
            float fMargin = fFontSize * 0.5f;

            Rect rectInner = new Rect(fFontSize / 2, fFontSize + fMargin, rectWin.width - fFontSize, rectWin.height - fFontSize);
            Rect rectItem = new Rect(0f, 0f, fItemHeight, fItemHeight);

            if (GUI.Button(rectItem, "×", gsButton))
            {
                bGuiNameChange = false;
                iNameChangeMessageTimer = -1;
            }

            bool bTmp = false;
            if (bNameChangeTarget)
                if (fdList.Count == 0)
                    bTmp = true;
            else
                if (afList.Count == 0)
                    bTmp = true;
            if (bTmp)
            {
                rectItem.Set(rectInner.x, rectItem.y + rectItem.height, fFontSize * 10, fItemHeight);
                GUI.Label(rectItem, "リストが空です", gsLabel);

                //GUI.DragWindow();
                return;
            }


            rectItem.Set(rectInner.x, rectItem.y + rectItem.height, fFontSize * 5, fItemHeight);
            GUI.Label(rectItem, "名称変更", gsLabel);

            rectItem.Set(rectInner.x, rectItem.y + rectItem.height, fFontSize * 9, fItemHeight);
            GUI.Label(rectItem, bNameChangeTarget ? fdList[iCurrentMyFace].name : afList[iCurrentAutoFaceSet].name, gsLabel);

            rectItem.Set(rectInner.x, rectItem.y + rectItem.height, fFontSize * 9, fItemHeight);
            sNameChangeTmp = GUI.TextField(rectItem, sNameChangeTmp, gsText);

            rectItem.Set(rectInner.x, rectItem.y + rectItem.height + fMargin, fFontSize * 4, fItemHeight);
            if (GUI.Button(rectItem, "変更", gsButton))
            {
                if (sNameChangeTmp == string.Empty)
                {
                    sNameChangeMessage = "名前が空です";
                    iNameChangeMessageTimer = 120;
                }
                else if(sNameChangeTmp != (bNameChangeTarget ? fdList[iCurrentMyFace].name : afList[iCurrentAutoFaceSet].name) && sMyFaces.Contains(sNameChangeTmp))
                {
                    sNameChangeMessage = "既にあります";
                    iNameChangeMessageTimer = 120;
                }
                else
                {
                    if(bNameChangeTarget)
                        fdList[iCurrentMyFace].name = sNameChangeTmp;
                    else
                        afList[iCurrentAutoFaceSet].name = sNameChangeTmp;
                    bGuiNameChange = false;
                    iNameChangeMessageTimer = -1;
                }
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, rectItem.width, fItemHeight);
            if (GUI.Button(rectItem, "取消", gsButton))
            {
                bGuiNameChange = false;
                iNameChangeMessageTimer = -1;
            }

            if (iNameChangeMessageTimer > 0)
            {
                rectItem.Set(rectInner.x, rectItem.y + rectItem.height, fFontSize * 10, fItemHeight);
                GUI.Label(rectItem, sNameChangeMessage, gsLabel);
            }

            GUI.DragWindow();
        }

        private void GuiFuncProgram(int winId)
        {
            GUIStyle gsLabel = new GUIStyle("label");
            gsLabel.fontSize = GetPix(12);
            gsLabel.alignment = TextAnchor.UpperLeft;

            int fFontSize = gsLabel.fontSize;

            GUIStyle gsLabelR = new GUIStyle("label");
            gsLabelR.fontSize = fFontSize;
            gsLabelR.alignment = TextAnchor.UpperRight;
            
            GUIStyle gsToggle = new GUIStyle("toggle");
            gsToggle.fontSize = fFontSize;
            gsToggle.alignment = TextAnchor.UpperLeft;

            GUIStyle gsButton = new GUIStyle("button");
            gsButton.fontSize = fFontSize;
            gsButton.alignment = TextAnchor.MiddleCenter;

            GUIStyle gsText = new GUIStyle("textfield");
            gsText.fontSize = fFontSize;
            gsText.alignment = TextAnchor.UpperRight;

            GUIStyle gsGroup = new GUIStyle();

            GUIStyleState gssWhite = new GUIStyleState();
            gssWhite.textColor = Color.black;
            gssWhite.background = texWhiteAlpha;
            gsGroup.normal = gssWhite;

            GUIStyle gsGroup2 = new GUIStyle();

            GUIStyleState gssRed = new GUIStyleState();
            gssRed.textColor = Color.black;
            gssRed.background = texRedAlpha;
            gsGroup2.normal = gssRed;

            float fItemHeight = fFontSize * 1.5f;
            float fMargin = fFontSize * 0.5f;

            Rect rectInner = new Rect(fFontSize / 2, fFontSize + fMargin, rectWinProgram.width - fFontSize, rectWinProgram.height - fFontSize - fMargin);
            Rect rectItem = new Rect(0f, 0f, fItemHeight, fItemHeight);

            if (GUI.Button(rectItem, "×", gsButton))
            {
                bGuiProgram = false;
            }

            rectItem.Set(rectInner.x, rectInner.y + fMargin, fFontSize * 2, fItemHeight);
            if (GUI.Button(rectItem ,"<", gsButton))
            {
                if (--iCurrentAutoFaceSet < 0)
                    if (afList.Count == 0)
                        iCurrentAutoFaceSet = 0;
                    else
                        iCurrentAutoFaceSet = afList.Count - 1;
                SetMfdAutoFace();
                if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                    iSelectAutoFace = 0;

            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 9, rectItem.height);
            if (GUI.Button(rectItem, afList.Count == 0 ? "- - -" : afList[iCurrentAutoFaceSet].name, gsButton))
            {
                if (bGuiPop2Close)
                {
                    bGuiPop2Close = false;
                    SetMfdAutoFace();
                    if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                        iSelectAutoFace = 0;

                }
                else
                {
                    CreateAutoFaceNameArray();
                    fWinPop2Height = fItemHeight * sAutoFaces.Length;
                    rectWinPop2.Set(rectWinProgram.x + rectItem.x, rectWinProgram.y + rectItem.y + rectItem.height, rectItem.width, fWinPop2Height);
                    bGuiProgramPop = true;
                    rectPopUpButton = new Rect(rectWinProgram.x + rectItem.x, rectWinProgram.y + rectItem.y, rectItem.width, rectItem.height);
                }
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
            if (GUI.Button(rectItem, ">", gsButton))
            {
                if (++iCurrentAutoFaceSet >= afList.Count)
                    iCurrentAutoFaceSet = 0;
                SetMfdAutoFace();
                if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                    iSelectAutoFace = 0;
            }
            rectItem.Set(rectItem.x + rectItem.width + fFontSize, rectItem.y, fFontSize * 5, rectItem.height);
            if (GUI.Button(rectItem, "名前変更", gsButton))
            {
                if (afList.Count > 0)
                {
                    CreateAutoFaceNameArray();
                    sNameChangeTmp = afList[iCurrentAutoFaceSet].name;
                    bGuiNameChange = true;
                    bNameChangeTarget = false;
                }
            }

            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 3, rectItem.height);
            if (GUI.Button(rectItem, "追加", gsButton))
            {
                afList.Add(new AutoFaceSet(afList.Count.ToString(), new List<AutoFaceData>()));
                iCurrentAutoFaceSet = afList.Count - 1;
                SetMfdAutoFace();
                if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                    iSelectAutoFace = 0;
            }

            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 3, rectItem.height);
            if (GUI.Button(rectItem, "削除", gsButton))
            {
                if (iCurrentAutoFaceSet < 0 || afList.Count <= iCurrentAutoFaceSet)
                    return;
                afList.RemoveAt(iCurrentAutoFaceSet);
                if (--iCurrentAutoFaceSet < 0)
                    iCurrentAutoFaceSet = 0;
                if (afList.Count == 0)
                    return;
                SetMfdAutoFace();
                if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                    iSelectAutoFace = 0;
            }
            rectItem.Set(rectItem.x + rectItem.width+ fFontSize, rectItem.y, fFontSize * 20, rectItem.height);
            GUI.Label(rectItem, "保存はメインウィンドウで行ってください", gsLabel);

            rectItem.x = rectInner.width - fFontSize * 9.5f;
            rectItem.width = fFontSize * 10;
            if(GUI.Button(rectItem, "マイ表情セット並替", gsButton))
            {
                bFaceListSortMode = !bFaceListSortMode;
            }

            int iITemNum = afList.Count == 0 ? 0 : afList[iCurrentAutoFaceSet].autofaces.Count();
            Rect rectScroll = new Rect(rectInner.x, rectItem.y + rectItem.height + fMargin, fFontSize * 47, rectInner.height - fItemHeight * 5);
            Rect rectScrollInnerL = new Rect(0f, 0f, rectScroll.width - fItemHeight, (fItemHeight * 5) * iITemNum);
            Rect rectGroup = new Rect(0f, 0f, rectScrollInnerL.width, fItemHeight * 5);

            Vector2 v2Mouse = new Vector2(Input.mousePosition.x - rectWinProgram.x - rectScroll.x, Screen.height - Input.mousePosition.y - rectWinProgram.y - rectScroll.y + v2ScrollPosL.y);
            v2ScrollPosL = GUI.BeginScrollView(rectScroll, v2ScrollPosL, rectScrollInnerL);
            for(int i = 0; i < iITemNum; i++)
            {
                if (i == iSelectAutoFace)
                    GUI.BeginGroup(rectGroup, gsGroup);
                else
                    GUI.BeginGroup(rectGroup);

                rectItem.Set(fFontSize / 2, fItemHeight / 2, fFontSize * 4, fItemHeight);
                if (GUI.Button(rectItem, "選択", gsButton))
                {
                    iSelectAutoFace = i;
                }

                rectItem.Set(rectItem.x, rectItem.y + rectItem.height, fFontSize * 4, rectItem.height);
                afList[iCurrentAutoFaceSet].autofaces[i].enable = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].enable, "有効", gsToggle);

                rectItem.Set(rectItem.x, rectItem.y + rectItem.height, fFontSize * 2, rectItem.height);
                if(GUI.Button(rectItem, "△", gsButton))
                {
                    SortList(afList[iCurrentAutoFaceSet].autofaces, i, true);
                    if ((iSelectAutoFace = i - 1) < 0)
                        iSelectAutoFace = 0;
                }
                rectItem.x += rectItem.width;
                if (GUI.Button(rectItem, "▽", gsButton))
                {
                    SortList(afList[iCurrentAutoFaceSet].autofaces, i, false);
                    if ((iSelectAutoFace = i + 1) >= afList[iCurrentAutoFaceSet].autofaces.Count)
                        iSelectAutoFace = afList[iCurrentAutoFaceSet].autofaces.Count - 1;
                }

                GUI.enabled = afList[iCurrentAutoFaceSet].autofaces[i].enable;

                rectItem.Set(rectItem.x + rectItem.width + fFontSize, rectItem.y - rectItem.height * 2, fFontSize * 3, rectItem.height);
                GUI.Label(rectItem, "興奮値：", gsLabel);

                string sTmp = afList[iCurrentAutoFaceSet].autofaces[i].exciteLow.ToString();
                string sTmp2 = sTmp;
                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 3, rectItem.height);
                sTmp = GUI.TextField(rectItem, sTmp, gsText);
                if(sTmp != sTmp2)
                {
                    int iTmp;
                    iTmp = int.TryParse(sTmp, out iTmp) ? iTmp : afList[iCurrentAutoFaceSet].autofaces[i].exciteLow;
                    if (iTmp < -100)
                        iTmp = -100;
                    else if (iTmp > 300)
                        iTmp = 300;
                    afList[iCurrentAutoFaceSet].autofaces[i].exciteLow = iTmp;
                }

                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
                if (GUI.Button(rectItem, "<", gsButton))
                {
                    if (--afList[iCurrentAutoFaceSet].autofaces[i].exciteLow < -100)
                        afList[iCurrentAutoFaceSet].autofaces[i].exciteLow = -100;
                }

                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
                if (GUI.Button(rectItem, ">", gsButton))
                {
                    if (++afList[iCurrentAutoFaceSet].autofaces[i].exciteLow > 300)
                        afList[iCurrentAutoFaceSet].autofaces[i].exciteLow = 300;
                }

                rectItem.Set(rectItem.x - fFontSize * 5, rectItem.y + rectItem.height, fFontSize * 7, rectItem.height);
                afList[iCurrentAutoFaceSet].autofaces[i].exciteLow = (int)GUI.HorizontalSlider(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].exciteLow, -100f, 300f);

                if (afList[iCurrentAutoFaceSet].autofaces[i].exciteLow > afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh)
                    afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh = afList[iCurrentAutoFaceSet].autofaces[i].exciteLow;
                
                //

                rectItem.Set(rectItem.x - fFontSize * 2, rectItem.y + rectItem.height, fFontSize * 2, rectItem.height);
                GUI.Label(rectItem, "から", gsLabel);

                sTmp = afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh.ToString();
                sTmp2 = sTmp;
                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 3, rectItem.height);
                sTmp = GUI.TextField(rectItem, sTmp, gsText);
                if (sTmp != sTmp2)
                {
                    int iTmp;
                    iTmp = int.TryParse(sTmp, out iTmp) ? iTmp : afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh;
                    if (iTmp < -100)
                        iTmp = -100;
                    else if (iTmp > 300)
                        iTmp = 300;
                    afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh = iTmp;
                }

                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
                if (GUI.Button(rectItem, "<", gsButton))
                {
                    if (--afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh < -100)
                        afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh = -100;
                }

                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
                if (GUI.Button(rectItem, ">", gsButton))
                {
                    if (++afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh > 300)
                        afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh = 300;
                }

                rectItem.Set(rectItem.x - fFontSize * 5, rectItem.y + rectItem.height, fFontSize * 7, rectItem.height);
                afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh = (int)GUI.HorizontalSlider(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh, -100f, 300f);

                if (afList[iCurrentAutoFaceSet].autofaces[i].exciteLow > afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh)
                    afList[iCurrentAutoFaceSet].autofaces[i].exciteLow = afList[iCurrentAutoFaceSet].autofaces[i].exciteHigh;


                rectItem.Set(rectItem.x + rectItem.width + fFontSize, fItemHeight / 2, fFontSize * 3, rectItem.height);
                GUI.Label(rectItem, "絶頂数", gsLabel);

                sTmp = afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow.ToString();
                sTmp2 = sTmp;
                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 3, rectItem.height);
                sTmp = GUI.TextField(rectItem, sTmp, gsText);
                if (sTmp != sTmp2)
                {
                    int iTmp;
                    iTmp = int.TryParse(sTmp, out iTmp) ? iTmp : afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow;
                    if (iTmp < 0)
                        iTmp = 0;
                    else if (iTmp > 10)
                        iTmp = 10;
                    afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow = iTmp;
                }

                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
                if (GUI.Button(rectItem, "<", gsButton))
                {
                    if (--afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow < 0)
                        afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow = 0;
                }

                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
                if (GUI.Button(rectItem, ">", gsButton))
                {
                    if (++afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow > 10)
                        afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow = 10;
                }

                rectItem.Set(rectItem.x - fFontSize * 5, rectItem.y + rectItem.height, fFontSize * 7, rectItem.height);
                afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow = (int)GUI.HorizontalSlider(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow, -0f, 10f);

                if (afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow > afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh)
                    afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh = afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow;
                
                //

                rectItem.Set(rectItem.x - fFontSize * 2, rectItem.y + rectItem.height, fFontSize * 2, rectItem.height);
                GUI.Label(rectItem, "から", gsLabel);

                sTmp = afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh.ToString();
                sTmp2 = sTmp;
                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 3, rectItem.height);
                sTmp = GUI.TextField(rectItem, sTmp, gsText);
                if (sTmp != sTmp2)
                {
                    int iTmp;
                    iTmp = int.TryParse(sTmp, out iTmp) ? iTmp : afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh;
                    if (iTmp < 0)
                        iTmp = 0;
                    else if (iTmp > 10)
                        iTmp = 10;
                    afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh = iTmp;
                }

                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
                if (GUI.Button(rectItem, "<", gsButton))
                {
                    if (--afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh < 0)
                        afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh = 0;
                }

                rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 2, rectItem.height);
                if (GUI.Button(rectItem, ">", gsButton))
                {
                    if (++afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh > 10)
                        afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh = 10;
                }

                rectItem.Set(rectItem.x - fFontSize * 5, rectItem.y + rectItem.height, fFontSize * 7, rectItem.height);
                afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh = (int)GUI.HorizontalSlider(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh, 0f, 10f);

                if (afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow > afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh)
                    afList[iCurrentAutoFaceSet].autofaces[i].zettyoLow = afList[iCurrentAutoFaceSet].autofaces[i].zettyoHigh;


                rectItem.Set(rectItem.x + rectItem.width + fFontSize, fItemHeight / 2, fFontSize * 5, rectItem.height);
                afList[iCurrentAutoFaceSet].autofaces[i].orgasm = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].orgasm, "絶頂時", gsToggle);

                rectItem.y += rectItem.height;
                afList[iCurrentAutoFaceSet].autofaces[i].afterOrgasm = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].afterOrgasm, "絶頂後", gsToggle);

                rectItem.y += rectItem.height;
                afList[iCurrentAutoFaceSet].autofaces[i].kiss = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].kiss, "キス時", gsToggle);

                rectItem.y += rectItem.height;
                afList[iCurrentAutoFaceSet].autofaces[i].msei = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].msei, "M性時", gsToggle);

                rectItem.Set(rectItem.x + rectItem.width, fItemHeight / 2, fFontSize * 6, rectItem.height);
                afList[iCurrentAutoFaceSet].autofaces[i].manOrgasm = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].manOrgasm, "男絶頂時", gsToggle);

                rectItem.y += rectItem.height;
                afList[iCurrentAutoFaceSet].autofaces[i].afterManOrgasm = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].afterManOrgasm, "男絶頂後", gsToggle);

                rectItem.y += rectItem.height;
                afList[iCurrentAutoFaceSet].autofaces[i].housi = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].housi, "奉仕時", gsToggle);

                rectItem.y += rectItem.height;
                afList[iCurrentAutoFaceSet].autofaces[i].najirare = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].najirare, "詰られ時", gsToggle);

                rectItem.Set(rectItem.x + rectItem.width, fItemHeight / 2, fFontSize * 5, rectItem.height);
                afList[iCurrentAutoFaceSet].autofaces[i].normal = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].autofaces[i].normal, "通常時");


                rectItem.Set(rectItem.x, rectItem.y + rectItem.height, fFontSize * 6, fItemHeight * 3);
                GUI.BeginGroup(rectItem, gsGroup2);
                {
                    Rect rectItem2 = new Rect(0f, 0f, fFontSize * 5, fItemHeight);
                    afList[iCurrentAutoFaceSet].autofaces[i].morph = GUI.Toggle(rectItem2, afList[iCurrentAutoFaceSet].autofaces[i].morph, "モーフ", gsToggle);
                    rectItem2.Set(rectItem2.x, rectItem2.y + rectItem2.height, fFontSize * 6, rectItem2.height);
                    afList[iCurrentAutoFaceSet].autofaces[i].blend = GUI.Toggle(rectItem2, afList[iCurrentAutoFaceSet].autofaces[i].blend, "ブレンド", gsToggle);
                    rectItem2.Set(rectItem2.x, rectItem2.y + rectItem2.height, fFontSize * 4, rectItem2.height);
                    afList[iCurrentAutoFaceSet].autofaces[i].hitomiy = GUI.Toggle(rectItem2, afList[iCurrentAutoFaceSet].autofaces[i].hitomiy, "瞳Y", gsToggle);
                }
                GUI.EndGroup();

                
                
                GUI.enabled = true;
                GUI.EndGroup();
                rectGroup.y += rectGroup.height;
            }
            GUI.EndScrollView();
            if (bAddProgram)
            {
                bAddProgram = false;
                v2ScrollPosL.y = rectScrollInnerL.height - fItemHeight * 5;
                iSelectAutoFace = afList[iCurrentAutoFaceSet].autofaces.Count - 1;
            }

            rectScroll.Set(rectScroll.x + rectScroll.width, rectScroll.y, rectInner.width - rectScroll.width, rectScroll.height);
            Rect rectScrollInnerR = new Rect(0f, 0f, rectScroll.width - fFontSize * 2, fItemHeight * fdList.Count);

            bool[] bChecks = new bool[fdList.Count];
            if (bFaceListSortMode)
                rectItem.Set(fFontSize / 2, 0f, fFontSize * 1.5f, fItemHeight);
            else
                rectItem.Set(fFontSize / 2, 0f, fFontSize * 11, fItemHeight);
            v2ScrollPosR = GUI.BeginScrollView(rectScroll, v2ScrollPosR, rectScrollInnerR);
            if (afList.Count != 0 && afList[iCurrentAutoFaceSet].autofaces.Count != 0)
            {
                if (bFaceListSortMode)
                {
                    for (int i = 0; i < fdList.Count; i++)
                    {
                        rectItem.width = fFontSize * 1.5f;
                        rectItem.x = fFontSize / 2;
                        if (GUI.Button(rectItem, "△", gsButton))
                        {
                            SortList(fdList, i, true);
                        }
                        rectItem.x += rectItem.width;
                        if (GUI.Button(rectItem, "▽", gsButton))
                        {
                            SortList(fdList, i, false);
                        }
                        rectItem.x += rectItem.width;
                        rectItem.width = fFontSize * 8;
                        GUI.Label(rectItem, fdList[i].name, gsLabel);

                        rectItem.y += rectItem.height;
                    }
                }
                else
                {
                    for (int i = 0; i < fdList.Count; i++)
                    {
                        bool bTmp = afList[iCurrentAutoFaceSet].autofaces[iSelectAutoFace].guidList.Contains(fdList[i].guid);
                        bool bTmp2 = bTmp;
                        bTmp = GUI.Toggle(rectItem, bTmp, fdList[i].name, gsToggle);
                        if (bTmp != bTmp2)
                        {
                            if (bTmp)
                                afList[iCurrentAutoFaceSet].autofaces[iSelectAutoFace].guidList.Add(fdList[i].guid);
                            else
                                afList[iCurrentAutoFaceSet].autofaces[iSelectAutoFace].guidList.Remove(fdList[i].guid);
                        }
                        rectItem.y += rectItem.height;
                    }
                }
            }
            GUI.EndScrollView();

            rectItem.Set(rectInner.x, rectScroll.y + rectScroll.height + fMargin, fFontSize * 3, fItemHeight);
            if(GUI.Button(rectItem, "追加", gsButton))
            {
                if (afList.Count == 0)
                    afList.Add(new AutoFaceSet("0", new List<AutoFaceData>()));
                afList[iCurrentAutoFaceSet].autofaces.Add(new AutoFaceData());
                bAddProgram = true;
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 9, rectItem.height);
            if (GUI.Button(rectItem, "選択行の下に追加", gsButton))
            {
                if (afList.Count == 0)
                    afList.Add(new AutoFaceSet("0", new List<AutoFaceData>()));
                if(afList[iCurrentAutoFaceSet].autofaces.Count == 0)
                {
                    afList[iCurrentAutoFaceSet].autofaces.Add(new AutoFaceData());
                    iSelectAutoFace = 0;
                }
                else
                {
                    afList[iCurrentAutoFaceSet].autofaces.Insert(iSelectAutoFace + 1, new AutoFaceData());
                    ++iSelectAutoFace;
                }
            }
            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 3, rectItem.height);
            if (GUI.Button(rectItem, "削除", gsButton))
            {
                if (afList[iCurrentAutoFaceSet].autofaces.Count <= iSelectAutoFace)
                    iSelectAutoFace = 0;
                else
                {
                    afList[iCurrentAutoFaceSet].autofaces.RemoveAt(iSelectAutoFace);
                    if (--iSelectAutoFace < 0)
                        iSelectAutoFace = 0;
                }
            }

            rectItem.x += rectItem.width + fFontSize;
            if(GUI.Button(rectItem, "△", gsButton))
            {
                SortList(afList[iCurrentAutoFaceSet].autofaces, iSelectAutoFace, true);
                if (--iSelectAutoFace < 0)
                    iSelectAutoFace = 0;
            }


            rectItem.x += rectItem.width;
            if (GUI.Button(rectItem, "▽", gsButton))
            {
                SortList(afList[iCurrentAutoFaceSet].autofaces, iSelectAutoFace, false);
                if (++iSelectAutoFace >= afList[iCurrentAutoFaceSet].autofaces.Count)
                    iSelectAutoFace = afList[iCurrentAutoFaceSet].autofaces.Count - 1;
            }

            rectItem.Set(rectScroll.x - fFontSize * 17, rectItem.y, fFontSize * 9, fItemHeight * 2);
            if (afList.Count == 0)
            {
                GUI.enabled = false;
                GUI.Toggle(rectItem, false, "絶頂後体位変更\nまで表情固定", gsToggle);
            }
            else
                afList[iCurrentAutoFaceSet].noAutoFaceInZettyo = GUI.Toggle(rectItem, afList[iCurrentAutoFaceSet].noAutoFaceInZettyo, "絶頂後体位変更\nまで表情固定", gsToggle);

            GUI.enabled = afList.Count == 0 ? false : !afList[iCurrentAutoFaceSet].noAutoFaceInZettyo;

            rectItem.Set(rectScroll.x - fFontSize * 7, rectItem.y, fFontSize * 7, fItemHeight * 2);
            GUI.Label(rectItem, "絶頂後表情固定\nインターバル", gsLabel);

            rectItem.Set(rectItem.x + rectItem.width, rectItem.y, fFontSize * 7, fItemHeight);
            GUI.Label(rectItem, afList.Count == 0 ? "0" : afList[iCurrentAutoFaceSet].zettyointerval.ToString(), gsLabelR);

            rectItem.Set(rectItem.x, rectItem.y + rectItem.height, rectItem.width, rectItem.height);
            if(afList.Count == 0)
                GUI.HorizontalSlider(rectItem, 0f, 0f, 1200f);
            else
                afList[iCurrentAutoFaceSet].zettyointerval = (int)GUI.HorizontalSlider(rectItem, afList[iCurrentAutoFaceSet].zettyointerval, 0f, 1200f);

            GUI.enabled = true;

            GUI.DragWindow();
        }
        private bool bAddProgram = false;

        private void GuiFuncPopPg(int winId)
        {
            GUIStyle gsSelectionGrid = new GUIStyle();
            gsSelectionGrid.fontSize = GetPix(12);

            GUIStyleState gssBlack = new GUIStyleState();
            gssBlack.textColor = Color.white;
            gssBlack.background = Texture2D.blackTexture;
            GUIStyleState gssWhite = new GUIStyleState();
            gssWhite.textColor = Color.black;
            gssWhite.background = Texture2D.whiteTexture;

            gsSelectionGrid.normal = gssBlack;
            gsSelectionGrid.hover = gssWhite;

            Rect rectItem = new Rect(0f, 0f, rectWinPop2.width, fWinPop2Height);

            int iTmp = -1;
            iTmp = GUI.SelectionGrid(rectItem, -1, sAutoFaces, 1, gsSelectionGrid);
            if (iTmp >= 0)
            {
                iCurrentAutoFaceSet = iTmp;
                bGuiProgramPop = false;
                SetMfdAutoFace();
            }
        }

        public void Update()
        {
            if(Input.GetKeyDown(keyShowPanel))
            {
                bGui = !bGui;
            }

            if(bGetMaid)
            {
                if(++iGetMaidTimer > 30)
                {
                    GetMaid();
                    iGetMaidTimer = 0;
                    if (bGetMaid)
                        return;
                }
                return;
            }

            if (iNameChangeMessageTimer > 0)
                iNameChangeMessageTimer--;

            if (bYotogi && !bInitYotogi)
                bInitYotogi = InitYotogi();

            if(bInitYotogi && --iYotogiTimer < 0)
            {
                iYotogiTimer = YOTOGICHKFRAME;
                skill_pair = GetFieldValue<YotogiPlayManager, Yotogi.SkillDataPair>(ypm, "skill_pair_");
                if (skill_pair.base_data == null)
                {
                    sSkillName = string.Empty;
                    sSkillCategory = string.Empty;
                }
                else
                {
                    sSkillName = skill_pair.base_data.name;
                    sSkillCategory = skill_pair.base_data.category.ToString();
                }

                int iTmp = (int)fieldZettyo.GetValue(ypm);
                if (iZettyoCount != iTmp)
                {
                    iZettyoCount = iTmp;
                    iZettyoNowCount++;
                }
            }

            foreach (MaidFaceData mfd in mfdList)
            {
                if(mfd.maid == null || !mfd.maid.Visible || mfd.maid.body0.Face == null)
                {
                    bGetMaid = true;
                    return;
                }
                
                SetFaceMorph(mfd);
            }
        }

        private bool GetAnyMouseButtonDown()
        {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        }

        private bool GetAnyMouseButtonUp()
        {
            return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2);
        }

        private void LoadXML(bool b)
        {
            xml.LoadXML();
            if (b)
            {
                string sTmp = xml.GetString("//config/keyshowpanel");
                keyShowPanel = string.IsNullOrEmpty(sTmp) ? keyShowPanel : sTmp;
            }
            {
                List<Dictionary<string, string>> data = xml.GetAttr("//faces");
                iCurrentMyFace = 0;
                fdList = new List<FaceData>();
                float fTmp = 0f;
                for (int i = 0; i < data.Count; i++)
                {
                    FaceData fd = new FaceData();
                    foreach (KeyValuePair<string, string> kvp in data[i])
                    {
                        if (kvp.Key == "name")
                        {
                            fd.name = kvp.Value;
                            continue;
                        }
                        if (kvp.Key == "guid")
                        {
                            fd.guid = kvp.Value;
                            continue;
                        }
                        if (kvp.Key == "eyeup")
                        {
                            float.TryParse(kvp.Value, out fTmp);
                            fd.eyeUp = fTmp;
                            continue;
                        }
                        float.TryParse(kvp.Value, out fTmp);
                        fd.data.Add(new BaseData(kvp.Key, fTmp));
                    }
                    fdList.Add(fd);
                }
            }

            {
                List<Dictionary<string, string>> data = xml.GetAttr("//autofaces", "autoface");
                iCurrentAutoFaceSet = 0;
                iSelectAutoFace = 0;
                bool bTmp;
                afList = new List<AutoFaceSet>();
                int iTmp = 0;

                for (int i = 0; i < data.Count; i++)
                {
                    AutoFaceSet af = new AutoFaceSet();
                    foreach (KeyValuePair<string, string> kvp in data[i])
                    {
                        if (kvp.Key == "name")
                        {
                            af.name = kvp.Value;
                            continue;
                        }
                        if (kvp.Key == "guid")
                        {
                            af.guid = kvp.Value;
                            continue;
                        }
                        if (kvp.Key == "noautoface")
                        {
                            bool.TryParse(kvp.Value, out bTmp);
                            af.noAutoFaceInZettyo = bTmp;
                            continue;
                        }
                        if (kvp.Key == "zettyointerval")
                        {
                            int.TryParse(kvp.Value, out iTmp);
                            af.zettyointerval = iTmp;
                            continue;
                        }
                    }

                    List<Dictionary<string, string>> data2 = xml.GetAttr("//autofaces/autoface", "item", i);

                    List<AutoFaceData> afdList = new List<AutoFaceData>();

                    for (int j = 0; j < data2.Count; j++)
                    {
                        AutoFaceData afd = new AutoFaceData();
                        foreach (KeyValuePair<string, string> kvp in data2[j])
                        {
                            if (kvp.Key == "enabled")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.enable = bTmp;
                                continue;
                            }
                            if (kvp.Key == "excitelow")
                            {
                                int.TryParse(kvp.Value, out iTmp);
                                afd.exciteLow = iTmp;
                                continue;
                            }
                            if (kvp.Key == "excitehigh")
                            {
                                int.TryParse(kvp.Value, out iTmp);
                                afd.exciteHigh = iTmp;
                                continue;
                            }
                            if (kvp.Key == "morph")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.morph = bTmp;
                                continue;
                            }
                            if (kvp.Key == "blend")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.blend = bTmp;
                                continue;
                            }
                            if (kvp.Key == "hitomiy")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.hitomiy = bTmp;
                                continue;
                            }
                            if (kvp.Key == "normal")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.normal = bTmp;
                                continue;
                            }
                            if (kvp.Key == "orgasm")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.orgasm = bTmp;
                                continue;
                            }
                            if (kvp.Key == "afterorgasm")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.afterOrgasm = bTmp;
                                continue;
                            }
                            if (kvp.Key == "manorgasm")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.manOrgasm = bTmp;
                                continue;
                            }
                            if (kvp.Key == "aftermanorgasm")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.afterManOrgasm = bTmp;
                                continue;
                            }
                            if (kvp.Key == "housi")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.housi = bTmp;
                                continue;
                            }
                            if (kvp.Key == "kiss")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.kiss = bTmp;
                                continue;
                            }
                            if (kvp.Key == "najirare")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.najirare = bTmp;
                                continue;
                            }
                            if (kvp.Key == "msei")
                            {
                                bool.TryParse(kvp.Value, out bTmp);
                                afd.msei = bTmp;
                                continue;
                            }
                            if (kvp.Key == "zettyolow")
                            {
                                int.TryParse(kvp.Value, out iTmp);
                                afd.zettyoLow = iTmp;
                                continue;
                            }
                            if (kvp.Key == "zettyohigh")
                            {
                                int.TryParse(kvp.Value, out iTmp);
                                afd.zettyoHigh = iTmp;
                                continue;
                            }
                        }
                        List<Dictionary<string, string>> data3 = xml.GetAttr("//autofaces/autoface/item", "guid", new int[2] { i, j });
                        for (int k = 0; k < data3.Count; k++)
                        {
                            foreach (KeyValuePair<string, string> kvp3 in data3[k])
                            {
                                afd.guidList.Add(kvp3.Value);
                            }
                        }
                        afdList.Add(afd);

                    }
                    af.autofaces = afdList;
                    afList.Add(af);
                }
            }

            LoadMaidBaseData();
        }

        private void LoadMaidBaseData()
        {
            bool bTmp;
            int iTmp;

            string sTmp = xml.GetString("//generalsetting/enablemabataki");
            mbd.enableMabataki = bool.TryParse(sTmp, out bTmp) ? bTmp : true;

            sTmp = xml.GetString("//generalsetting/enableeye");
            mbd.enableEye = bool.TryParse(sTmp, out bTmp) ? bTmp : true;

            sTmp = xml.GetString("//generalsetting/eyedontmove");
            mbd.eyeDontMove = bool.TryParse(sTmp, out bTmp) ? bTmp : false;

            sTmp = xml.GetString("//generalsetting/enableblend");
            mbd.enableBlend = bool.TryParse(sTmp, out bTmp) ? bTmp : true;

            sTmp = xml.GetString("//generalsetting/disablemousewhenfera");
            mbd.disableMouseWhenFera = bool.TryParse(sTmp, out bTmp) ? bTmp : true;

            sTmp = xml.GetString("//generalsetting/facechangetimeratio");
            mbd.faceChangeTimeRatio = int.TryParse(sTmp, out iTmp) ? iTmp : 3;
            if (mbd.faceChangeTimeRatio < 1 || mbd.faceChangeTimeRatio > 5)
                mbd.faceChangeTimeRatio = 3;
        }

        private void SetMaidBaseData(MaidFaceData mfd)
        {
            mfd.enableMabataki = mbd.enableMabataki;
            mfd.enableEye = mbd.enableEye;
            mfd.eyeDontMove = mbd.eyeDontMove;
            mfd.enableBlend = mbd.enableBlend;
            mfd.disableMouseWhenFera = mbd.disableMouseWhenFera;
            mfd.faceChangeTimeRatio = mbd.faceChangeTimeRatio;
        }

        private void LoadAutoFaceGuid()
        {
            List<Dictionary<string, string>> data = xml.GetAttr("//maids", "maid");
            bool bTmp = false;
            for (int i = 0; i < mfdList.Count; i++)
            {
                foreach (Dictionary<string, string> dict in data)
                {
                    bTmp = false;
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {

                        if (kvp.Key == "maidguid" && kvp.Value == mfdList[i].maid.status.guid)
                        {
                            bTmp = true;
                            break;
                        }

                    }
                    if (bTmp)
                    {
                        foreach (KeyValuePair<string, string> kvp in dict)
                        {
                            if (kvp.Key == "autofaceguid")
                            {
                                mfdList[i].autoFaceSet.guid = kvp.Value;
                                break;
                            }
                        }
                        break;
                    }
                }
                SetMfdAutoFace(i, mfdList[i].autoFaceSet.guid);
            }
        }

        private string GetAutoFaceGuid(string guid)
        {
            //xml.LoadXML();
            bool bTmp = false;
            string sRet = string.Empty;
            List<Dictionary<string, string>> data = xml.GetAttr("//maids", "maid");
            foreach (Dictionary<string, string> dict in data)
            {
                bTmp = false;
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    if (kvp.Key == "maidguid" && kvp.Value == guid)
                    {
                        bTmp = true;
                        break;
                    }
                }
                if (bTmp)
                {
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        if (kvp.Key == "autofaceguid")
                        {
                            sRet = kvp.Value;
                            break;
                        }
                    }
                    break;
                }
            }
            return sRet;
        }

        private void SaveAutoFaceGuid(int iMaid)
        {
            if (mfdList.Count == 0 || mfdList.Count <= iMaid)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            MaidStatus.Status maidStatus = mfdList[iMaid].maid.status;
            data.Add("name", maidStatus.lastName + " " + maidStatus.firstName);
            data.Add("maidguid", maidStatus.guid);
            data.Add("autofaceguid", mfdList[iMaid].autoFaceSet.guid);
            xml.SetVal("//maids", "maid", data, "maidguid");
        }

        private void SaveXML()
        {
            {
                for (int i = 0; i < mfdList.Count; i++)
                {
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    MaidStatus.Status maidStatus = mfdList[i].maid.status;
                    data.Add("name", maidStatus.lastName + " " + maidStatus.firstName);
                    data.Add("maidguid", maidStatus.guid);
                    data.Add("autofaceguid", mfdList[i].autoFaceSet.guid);
                    xml.SetVal("//maids", "maid", data, "maidguid");
                }
            }
            {
                List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
                for (int i = 0; i < fdList.Count; i++)
                {
                    data.Add(new Dictionary<string, string>());
                    data.Last().Add("name", fdList[i].name);
                    data.Last().Add("guid", fdList[i].guid);
                    data.Last().Add("eyeup", fdList[i].eyeUp.ToString());
                    for (int j = 0; j < fdList[i].data.Count; j++)
                    {
                        data.Last().Add(fdList[i].data[j].key, fdList[i].data[j].val.ToString());
                    }
                }
                xml.SetVal("//faces", "item", data);
            }
            if (afList.Count > 0)
            {
                List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
                xml.SetVal("//autofaces", "autoface", data);
                for (int i = 0; i < afList.Count; i++)
                {
                    data = new List<Dictionary<string, string>>();
                    data.Add(new Dictionary<string, string>());
                    data.Last().Add("name", afList[i].name);
                    data.Last().Add("guid", afList[i].guid);
                    data.Last().Add("noautoface", afList[i].noAutoFaceInZettyo.ToString());
                    data.Last().Add("zettyointerval", afList[i].zettyointerval.ToString());
                    xml.SetVal("//autofaces", "autoface", data, true);

                    for (int j = 0; j < afList[i].autofaces.Count; j++)
                    {
                        data = new List<Dictionary<string, string>>();
                        data.Add(new Dictionary<string, string>());
                        data.Last().Add("enable", afList[i].autofaces[j].enable.ToString());
                        data.Last().Add("excitelow", afList[i].autofaces[j].exciteLow.ToString());
                        data.Last().Add("excitehigh", afList[i].autofaces[j].exciteHigh.ToString());
                        data.Last().Add("morph", afList[i].autofaces[j].morph.ToString());
                        data.Last().Add("blend", afList[i].autofaces[j].blend.ToString());
                        data.Last().Add("hitomiy", afList[i].autofaces[j].hitomiy.ToString());
                        data.Last().Add("normal", afList[i].autofaces[j].normal.ToString());
                        data.Last().Add("orgasm", afList[i].autofaces[j].orgasm.ToString());
                        data.Last().Add("afterorgasm", afList[i].autofaces[j].afterOrgasm.ToString());
                        data.Last().Add("manorgasm", afList[i].autofaces[j].manOrgasm.ToString());
                        data.Last().Add("aftermanorgasm", afList[i].autofaces[j].afterManOrgasm.ToString());
                        data.Last().Add("housi", afList[i].autofaces[j].housi.ToString());
                        data.Last().Add("kiss", afList[i].autofaces[j].kiss.ToString());
                        data.Last().Add("najirare", afList[i].autofaces[j].najirare.ToString());
                        data.Last().Add("msei", afList[i].autofaces[j].msei.ToString());
                        data.Last().Add("zettyolow", afList[i].autofaces[j].zettyoLow.ToString());
                        data.Last().Add("zettyohigh", afList[i].autofaces[j].zettyoHigh.ToString());

                        xml.SetVal("//autofaces/autoface", "item", data, true);

                        data = new List<Dictionary<string, string>>();
                        for (int k = 0; k < afList[i].autofaces[j].guidList.Count; k++)
                        {
                            data.Add(new Dictionary<string, string>());
                            data.Last().Add("val", afList[i].autofaces[j].guidList[k]);
                        }
                        xml.SetVal("//autofaces/autoface/item", "guid", data, true);
                    }
                }
            }
            else
            {
                List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
                xml.SetVal("//autofaces", "autoface", data);
            }

            if (mfdList.Count > 0)
            {
                xml.SetVal("//generalsetting", "enablemabataki", mfdList[0].enableMabataki.ToString());
                xml.SetVal("//generalsetting", "enableeye", mfdList[0].enableEye.ToString());
                xml.SetVal("//generalsetting", "eyedontmove", mfdList[0].eyeDontMove.ToString());
                xml.SetVal("//generalsetting", "enableblend", mfdList[0].enableBlend.ToString());
                xml.SetVal("//generalsetting", "disablemousewhenfera", mfdList[0].disableMouseWhenFera.ToString());
                xml.SetVal("//generalsetting", "facechangetimeratio", mfdList[0].faceChangeTimeRatio.ToString());
            }
            xml.SaveXML();
        }

        //

        private void SetMfdAutoFace()
        {
            if (afList.Count == 0 || afList.Count <= iCurrentAutoFaceSet)
                return;
            if (mfdList.Count == 0 || mfdList.Count <= iCurrentMaid)
                return;
            
            mfdList[iCurrentMaid].autoFaceSet = afList[iCurrentAutoFaceSet];
            SaveAutoFaceGuid(iCurrentMaid);
        }

        private void SetMfdAutoFace(int iMaid, string guid)
        {
            if (afList.Count == 0)
                return;
            if (mfdList.Count == 0 || mfdList.Count <= iMaid)
                return;

            bool bTmp = false;
            for(int i = 0; i < afList.Count; i++)
            {
                if (afList[i].guid == guid)
                {
                    mfdList[iMaid].autoFaceSet = afList[i];
                    bTmp = true;
                    //mfdList[iMaid].autoFace = afList[i].autofaces;
                    break;
                }
            }
            if(!bTmp)
                mfdList[iMaid].autoFaceSet = afList[0];
        }

        //コマンドクリックをフックするとAddYotogiSliderと衝突するため
        //原始的な方法で取得
        private bool InitYotogi()
        {
            ypm = FindObjectOfType(typeof(YotogiPlayManager)) as YotogiPlayManager;
            if (ypm == null)
                return false;

            fieldZettyo = GetField<YotogiPlayManager>(ypm, "zettyo_count");
            if (fieldZettyo == null)
                return false;

            iYotogiTimer = YOTOGICHKFRAME;
            return true;
        }

        private void SetCurrentAutoFaceSet()
        {
            if (mfdList.Count == 0)
                return;
            for(int i = 0; i < afList .Count; i++)
            {
                if (afList[i].guid == mfdList[iCurrentMaid].autoFaceSet.guid)
                {
                    iCurrentAutoFaceSet = i;
                    return;
                }

            }
            iCurrentAutoFaceSet = 0;
        }

        private void CreateMyFaceNameArray()
        {
            if (fdList.Count == 0)
                sMyFaces = new string[0];

            List<string> _sFaceList = new List<string>();
            for (int i = 0; i < fdList.Count; i++)
            {
                _sFaceList.Add(fdList[i].name);
            }
            sMyFaces = _sFaceList.ToArray();
        }

        private void CreateAutoFaceNameArray()
        {
            if (afList.Count == 0)
                sAutoFaces = new string[0];

            List<string> _sAutoFaceList = new List<string>();
            for (int i = 0; i < afList.Count; i++)
            {
                _sAutoFaceList.Add(afList[i].name);
            }
            sAutoFaces = _sAutoFaceList.ToArray();
        }

        private void AddFaceList(List<BaseData> data, float fEyeUp)
        {
            fdList.Add(new FaceData(fdList.Count.ToString(), data, fEyeUp));
            iCurrentMyFace = fdList.Count - 1;
        }

        private void ChangeFaceList(List<BaseData> data, float fEyeUp)
        {
            if (fdList.Count == 0)
                return;
            fdList[iCurrentMyFace].SetFaceData(data, fEyeUp);
        }

        private void RemoveFaceList()
        {
            if (iCurrentMyFace < 0 || fdList.Count <= iCurrentMyFace)
                return;
            fdList.RemoveAt(iCurrentMyFace);
            if (--iCurrentMyFace < 0)
                iCurrentMyFace = 0;
        }

        private void SetFace()
        {
            if (mfdList[iCurrentMaid].disableMouseWhenFera && mfdList[iCurrentMaid].bInFera)
            {
                mfdList[iCurrentMaid].maid.body0.Face.morph.MulBlendValues(sFaceNames[iCurrentFace], 1);
                GetFaceVal(mfdList[iCurrentMaid], false);
            }
            else
            {
                mfdList[iCurrentMaid].maid.body0.Face.morph.MulBlendValues(sFaceNames[iCurrentFace], 1);
                GetFaceVal(mfdList[iCurrentMaid]);
            }
        }

        private void SetFace(List<BaseData> data, MaidFaceData mfd, bool[] b, float fEyeUp)
        {
            if (b.Length != 3)
                return;

            for (int i = 0; i < data.Count; i++)
            {
                if ((b[0] || b[2]) && sKeyVals[i, 2] == "0")
                {
                    if (b[0])
                    {
                        if (mfd.disableMouseWhenFera && mfd.bInFera)
                        {
                            if (!sKeyVals[i, 0].Contains("mouth") && !sKeyVals[i, 0].Contains("tang"))
                                mfd.dataTmp[i].val = data[i].val;
                        }
                        else
                        {
                            mfd.dataTmp[i].val = data[i].val;
                        }
                    }
                    else
                    {
                        if (sKeyVals[i, 0].Contains("hitomis"))
                            mfd.dataTmp[i].val = data[i].val;
                    }
                }
                else if (b[1] && mfd.enableBlend && sKeyVals[i, 2] == "1")
                {
                    mfd.data[i].val = data[i].val;
                }
            }

            if (b[2] && !mfd.eyeDontMove)
            {
                mfd.eyeUpTmp = fEyeUp;
            }
            mfd.DataTmpSet();

            //hitomis
            //for (int i = 0; i < data.Count; i++)
            //{
            //    if (b[0] && sKeyVals[i, 2] == "0")
            //    {
            //        if(mfd.disableMouseWhenFera && mfd.bOnFera)
            //        {
            //            if (!sKeyVals[i, 0].Contains("mouth") && !sKeyVals[i, 0].Contains("tang"))
            //            {
            //                    mfd.data[i].val = data[i].val;
            //            }
            //        }
            //        else
            //        {
            //            mfd.data[i].val = data[i].val;
            //        }
            //    }
            //    else if (b[1] && mfd.enableBlend && sKeyVals[i, 2] == "1")
            //    {
            //        mfd.data[i].val = data[i].val;
            //    }
            //}
            //if (b[2] && !mfd.eyeChara)
            //    mfd.eyeUp = fEyeUp;
        }

        private void AutoFace(MaidFaceData mfd)
        {
            AutoFace(mfd, AutoFaceMode.normal);
        }
        private void AutoFace(MaidFaceData mfd, AutoFaceMode afm)
        {
            List<MaidFaceData.AutoFaceValue> listAfvTmp = mfd.GetAutoFaceValue(lastExcite, Math.Min(10, iZettyoNowCount), afm);
            List<string> listMorph = new List<string>();
            List<string> listBlend = new List<string>();
            List<string> listHitomiY = new List<string>();
            Dictionary<string, int> dictFdGuid = new Dictionary<string, int>();

            for (int i = 0; i < fdList.Count; i++)
            {
                dictFdGuid.Add(fdList[i].guid, i);
            }

            for (int i = 0; i < listAfvTmp.Count; i++)
            {
                for (int j = 0; j < listAfvTmp[i].guidList.Count; j++)
                {
                    if (!dictFdGuid.ContainsKey(listAfvTmp[i].guidList[j]))
                        continue;

                    if (listAfvTmp[i].morph)
                    {
                        listMorph.Add(listAfvTmp[i].guidList[j]);
                    }
                    if (listAfvTmp[i].blend)
                    {
                        listBlend.Add(listAfvTmp[i].guidList[j]);
                    }
                    if (listAfvTmp[i].hitomiy)
                    {
                        listHitomiY.Add(listAfvTmp[i].guidList[j]);
                    }
                }
            }
            if(listMorph.Count > 0)
            {
                int iRandom = UnityEngine.Random.Range(0, listMorph.Count);
                bool[] b = new bool[3] { true, false, false};
                iCurrentMyFace = dictFdGuid[listMorph[iRandom]];
                SetFace(fdList[iCurrentMyFace].data, mfd, b, 0f);
            }
            if (listBlend.Count > 0)
            {
                int iRandom = UnityEngine.Random.Range(0, listBlend.Count);
                bool[] b = new bool[3] { false, true, false };
                SetFace(fdList[dictFdGuid[listBlend[iRandom]]].data, mfd, b, 0f);
            }
            if (listHitomiY.Count > 0)
            {
                int iRandom = UnityEngine.Random.Range(0, listHitomiY.Count);
                bool[] b = new bool[3] { false, false, true };
                SetFace(fdList[dictFdGuid[listHitomiY[iRandom]]].data, mfd, b, fdList[dictFdGuid[listHitomiY[iRandom]]].eyeUp);
            }
        }

        private void GetFaceVal(MaidFaceData mfd)
        {
            if (mfd.maid.boFaceAnime)
            {
                return;
            }
            TMorph morph = mfd.maid.body0.Face.morph;

            foreach (BaseData bd in mfd.data)
            {
                if (morph.Contains(bd.key))
                {
                    if (bd.key == "hitomih")
                        continue;
                    bd.val = morph.GetBlendValues((int)morph.hash[bd.key]);
                }
            }
        }

        private void GetFaceVal(MaidFaceData mfd, bool bMouth)
        {
            if(mfd.maid.boFaceAnime)
            {
                return;
            }
            TMorph morph = mfd.maid.body0.Face.morph;

            if (bMouth)
            {
                foreach (BaseData bd in mfd.dataTmp)
                {
                    if (morph.Contains(bd.key))
                    {
                        if (bd.key.Contains("mouth") || bd.key.Contains("tang"))
                            bd.val = morph.GetBlendValues((int)morph.hash[bd.key]);
                    }
                }
            }
            else
            {
                foreach (BaseData bd in mfd.dataTmp)
                {
                    if (morph.Contains(bd.key))
                    {
                        if (!bd.key.Contains("mouth") && !bd.key.Contains("tang"))
                            bd.val = morph.GetBlendValues((int)morph.hash[bd.key]);
                    }
                }
            }
        }

        private bool ChkVoice(MaidFaceData mfd)
        {
            if (mfd.maid.AudioMan.audiosource.clip == null)
            {
                if (mfd.lastVoice != 0)
                {
                    mfd.lastVoice = 0;
                    return true;
                }
            }
            else if (mfd.lastVoice != mfd.maid.AudioMan.audiosource.clip.GetInstanceID())
            {
                mfd.lastVoice = mfd.maid.AudioMan.audiosource.clip.GetInstanceID();
                return true;
            }
            return false;
        }

        private bool ContainsWordClips(string sWord, string[] sWordClips)
        {
            foreach (string s in sWordClips)
            {
                if (sWord.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ContainsWordClips(string sWord, string[] sWordClips, string[] sWordExpects)
        {
            foreach (string s in sWordClips)
            {
                if (sWord.Contains(s))
                {
                    foreach (string s2 in sWordExpects)
                    {
                        if (sWord.Contains(s2))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private void ChkMaidState()
        {
            foreach (MaidFaceData mfd in mfdList)
            {
                if (mfd.lastPose != mfd.maid.body0.LastAnimeFN)
                {
                    mfd.lastPose = mfd.maid.body0.LastAnimeFN;
#if DEBUG
                    Console.WriteLine("体位:" + mfd.lastPose);
#endif
                    mfd.bInFera = ContainsWordClips(mfd.lastPose, sPoseFeras, sPoseFeraExcepts);
                    mfd.bInKiss = ContainsWordClips(mfd.lastPose, sPoseKisses, sPoseFeras);
                    mfd.bInHousi = ContainsWordClips(mfd.lastPose, sPoseHousis);

                    bool bTmp = mfd.bInZettyou;
                    bool bTmp2 = mfd.bInZettyougo;

                    mfd.bInZettyou = ContainsWordClips(mfd.lastPose, sPoseZettyous);
                    if (mfd.bInZettyou && mfd.lastPose.Contains("taiki"))
                    {
                        mfd.bInZettyou = false;
                        mfd.bInZettyougo = true;
                    }
                    else
                    {
                        mfd.bInZettyougo = ContainsWordClips(mfd.lastPose, sPoseZettyougos);
                    }

                    if (mfd.bInZettyou != bTmp)
                        mfd.bChangeZettyou = true;
                    else
                        mfd.bChangeZettyou = false;

                    if (mfd.bInZettyougo != bTmp2)
                        mfd.bChangeZettyougo = true;
                    else
                        mfd.bChangeZettyougo = false;

                    mfd.bInNajirare = sSkillCategory == "詰られ" ? true : false;
                    if (!mfd.bInNajirare)
                        mfd.bInNajirare = sSkillName.Contains("足コキ");
                    mfd.bInMsei = sSkillCategory == "M性" ? true : false;
                }
                mfd.autoFacetimer = 0;
            }
        }

        private void SetFaceMorph(MaidFaceData mfd)
        {
            if (!mfd.enable)
                return;

            if (bInitYotogi)
            {
                if (mfd.autoFaceInterval > 0 && iYotogiTimer == YOTOGICHKFRAME)
                {
                    if (ChkVoice(mfd) && iLastFrame != UnityEngine.Time.frameCount)
                    {
                        iLastFrame = UnityEngine.Time.frameCount;
                        ChkMaidState();
                        //mfd.bInNajirare = ContainsWordClips(sSkillName, sSkillNajirares);
                        //mfd.bInMsei = sSkillCategory == "M性" ? true : false; 
                        lastExcite = mfd.maid.status.currentExcite;

                        //foreach (MaidFaceData m in mfdList)
                        //{
                        //    m.autoFacetimer = 0;
                        //}
                    }
                    if (mfd.bChangeZettyou)
                    {
                        mfd.bChangeZettyou = false;
                        if (mfd.bInZettyou)
                        {
                            if (mfd.bInHousi || mfd.bInNajirare)
                                AutoFace(mfd, AutoFaceMode.manOrgasm);
                            else
                                AutoFace(mfd, AutoFaceMode.maidOrgasm);
                            mfd.noAutoFaceInZettyoTimer = mfd.autoFaceSet.zettyointerval + YOTOGICHKFRAME;
                            mfd.iInZettyouTimer = 600;
                        }
                    }
                    if (mfd.bChangeZettyougo)
                    {
                        mfd.bChangeZettyougo = false;
                        if (mfd.bInZettyougo)
                        {
                            if (mfd.bInHousi || mfd.bInNajirare)
                                AutoFace(mfd, AutoFaceMode.afterManOrgasm);
                            else
                                AutoFace(mfd, AutoFaceMode.afterMaidOrgasm);
                            //mfd.noAutoFaceInZettyoTimer = mfd.autoFaceSet.zettyointerval + YOTOGICHKFRAME;
                            mfd.iInZettyouTimer = 0;
                        }
                    }

                    if (mfd.bInZettyou || mfd.bInZettyougo)
                    {
                        if (!mfd.autoFaceSet.noAutoFaceInZettyo && (mfd.noAutoFaceInZettyoTimer -= YOTOGICHKFRAME) < 0)
                        {
                            mfd.bInZettyou = false;
                            mfd.bInZettyougo = false;
                        }
                        if(mfd.bInZettyou && (mfd.iInZettyouTimer - YOTOGICHKFRAME) < 0)
                        {
                            mfd.bInZettyou = false;
                            mfd.bInZettyougo = true;
                            mfd.bChangeZettyougo = true;
                        }
                    }
                    else
                    {
                        if (mfd.UpdateAutoFace(YOTOGICHKFRAME))
                        {
                            if (mfd.bInKiss)
                                AutoFace(mfd, AutoFaceMode.kiss);
                            else if(mfd.bInNajirare)
                                AutoFace(mfd, AutoFaceMode.najirare);
                            else if (mfd.bInMsei)
                                AutoFace(mfd, AutoFaceMode.msei);
                            else if (mfd.bInHousi)
                                AutoFace(mfd, AutoFaceMode.housi);
                            else
                                AutoFace(mfd, AutoFaceMode.normal);
                        }
                    }
                }
            }
            else
            {
                if (mfd.UpdateAutoFace(1))
                {
                    if (mfd.bInNajirare)
                        AutoFace(mfd, AutoFaceMode.najirare);
                    else if (mfd.bInMsei)
                        AutoFace(mfd, AutoFaceMode.msei);
                    else if (mfd.bInHousi)
                        AutoFace(mfd, AutoFaceMode.housi);
                    else if (mfd.bInKiss)
                        AutoFace(mfd, AutoFaceMode.kiss);
                    else
                        AutoFace(mfd, AutoFaceMode.normal);
                }
            }
            mfd.maid.boMabataki = false;
            mfd.maid.boFaceAnime = false;
            //SetFieldValue<Maid>(mfd.maid, "FaceName", sFaceNames[iCurrentFace]);
            string sFace = GetFieldValue<Maid, string>(mfd.maid, "FaceName2");
            if(!string.IsNullOrEmpty(sFace))
            {
                if (mfd.disableMouseWhenFera && mfd.bInFera)
                {
                    mfd.maid.body0.Face.morph.MulBlendValues(sFace, 1);
                    GetFaceVal(mfd, true);
                }
                SetFieldValue<Maid>(mfd.maid, "FaceName2", string.Empty);
            }

            mfd.Update();

            TMorph morph = mfd.maid.body0.Face.morph;
            foreach (BaseData bd in mfd.data)
            {
                if (morph.Contains(bd.key))
                {
                    if (bd.key == "nosefook")
                        mfd.maid.boNoseFook = bd.val > 0f ? true : false;
                    else if (bd.key == "hitomih")
                        morph.SetBlendValues((int)morph.hash[bd.key], bd.val * 3);
                    else
                        morph.SetBlendValues((int)morph.hash[bd.key], bd.val);
                }
            }

            morph.LipSync1 = Mathf.Max(morph.LipSync1, morph.GetBlendValues((int)morph.hash["moutha"]));
            morph.LipSync2 = Mathf.Max(morph.LipSync2, morph.GetBlendValues((int)morph.hash["mouthi"]));
            morph.LipSync3 = Mathf.Max(morph.LipSync3, morph.GetBlendValues((int)morph.hash["mouthc"]));

            morph.LipSync3 = Math.Min((1f - morph.LipSync1), morph.LipSync3);

            if (mfd.enableEye)
            {
                mfd.maid.body0.trsEyeL.localPosition = mfd.eyePos[0] + new Vector3(-mfd.eyeUp * 0.005f, mfd.eyeUp * 0.02f, 0f);
                mfd.maid.body0.trsEyeR.localPosition = mfd.eyePos[1] - new Vector3(mfd.eyeUp * 0.005f, mfd.eyeUp * 0.02f, 0f);
            }

            morph.FixBlendValues_Face();
        }

        private void GetMaid()
        {
            CharacterMgr cm = GameMain.Instance.CharacterMgr;
            List<Maid> _maidList = new List<Maid>();

            for (int i = 0; i < cm.GetMaidCount(); i++)
            {
                Maid maid = cm.GetMaid(i);
                if (maid == null)
                    continue;
                if (maid.boAllProcPropBUSY || !maid.Visible)
                    continue;
                if (maid.AudioMan == null)
                    continue;
                _maidList.Add(maid);
            }

            if (_maidList.Count == 0)
            {
                MaidDataClear();
                return;
            }

            if (mfdList.Count > _maidList.Count)
            {
                mfdList.RemoveRange(_maidList.Count, mfdList.Count - _maidList.Count);
            }

            for (int i = 0; i < _maidList.Count; i++)
            {
                if(mfdList.Count > i)
                {
                    mfdList[i].maid = _maidList[i];
                    SetMfdAutoFace(i, GetAutoFaceGuid(_maidList[i].status.guid));
                    
                }
                else
                {
                    mfdList.Add(new MaidFaceData(_maidList[i]));
                    SetMfdAutoFace(mfdList.Count - 1, GetAutoFaceGuid(_maidList[i].status.guid));
                    SetMaidBaseData(mfdList.Last());
                }
            }

            if (iCurrentMaid < 0)
            {
                iCurrentMaid = mfdList.Count - 1;
            }
            else if (mfdList.Count <= iCurrentMaid)
            {
                iCurrentMaid = 0;
            }

            sCurrentMaid = _maidList[iCurrentMaid].status.lastName + " " + _maidList[iCurrentMaid].status.firstName;
            bGetMaid = false;
            return;
        }

        private void MaidDataClear()
        {
            mfdList.Clear();
            iCurrentMaid = 0;
            sCurrentMaid = string.Empty;
            bGetMaid = true;
        }

        private void SortList<T>(List<T> list, int iNum, bool bUp)
        {
            if (list.Count <= 1)
                return;
            if (bUp)
            {
                if (iNum == 0)
                    return;

                T listTmp = list[iNum - 1];
                list[iNum - 1] = list[iNum];
                list[iNum] = listTmp;
            }
            else
            {
                if (iNum >= list.Count - 1)
                    return;

                T listTmp = list[iNum + 1];
                list[iNum + 1] = list[iNum];
                list[iNum] = listTmp;
            }
        }

        internal static FieldInfo GetField<T>(T inst, string name)
        {
            if (inst == null) return null;

            FieldInfo field = GetFieldInfo<T>(name);
            if (field == null) return null;

            return field;
        }

        //internal static TResult GetMethodDelegate<T, TResult>(T inst, string name) where T : class where TResult : class
        //{
        //    return Delegate.CreateDelegate(typeof(TResult), inst, name) as TResult;
        //}

        internal static FieldInfo GetFieldInfo<T>(string name)
        {
            BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            return typeof(T).GetField(name, bf);
        }

        internal static TResult GetFieldValue<T, TResult>(T inst, string name)
        {
            if (inst == null) return default(TResult);

            FieldInfo field = GetFieldInfo<T>(name);
            if (field == null) return default(TResult);

            return (TResult)field.GetValue(inst);
        }

        internal static void SetFieldValue<T>(object inst, string name, object val)
        {
            FieldInfo field = GetFieldInfo<T>(name);
            if (field != null)
            {
                field.SetValue(inst, val);
            }
        }

        private int GetPix(int i)
        {
            float f = 1f + (Screen.width / 1280f - 1f) * 0.6f;
            return (int)(f * i);
        }

        private class XMLManager
        {
            private string sXmlFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Config\FaceControl.xml";
            private XmlDocument xmldoc = new XmlDocument();
            private bool bModify = false;

            public XMLManager()
            {
                Init();
            }

            private void Init()
            {
                if (!File.Exists(sXmlFileName))
                {
                    xmldoc = new XmlDocument();
                    XmlDeclaration declaration = xmldoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xmldoc.AppendChild(declaration);

                    XmlElement root = xmldoc.CreateElement("root");
                    XmlElement maids = xmldoc.CreateElement("maids");
                    XmlElement face = xmldoc.CreateElement("faces");
                    XmlElement pg = xmldoc.CreateElement("autofaces");
                    XmlElement config = xmldoc.CreateElement("config");

                    XmlElement keyshowpanel = xmldoc.CreateElement("keyshowpanel");
                    keyshowpanel.SetAttribute("val", "f9");

                    config.AppendChild(keyshowpanel);

                    root.AppendChild(maids);
                    root.AppendChild(face);
                    root.AppendChild(pg);
                    root.AppendChild(config);
                    xmldoc.AppendChild(root);

                    xmldoc.Save(sXmlFileName);
                }
                xmldoc.Load(sXmlFileName);
            }

            public bool SetVal(string sParentNode, string sNode, Dictionary<string, string> dictAttr, string sChkAttr)
            {
                if (xmldoc == null)
                    return false;

                XmlNode parentNode = xmldoc.SelectSingleNode(sParentNode);
                if (parentNode == null)
                    return false;

                if(!dictAttr.ContainsKey(sChkAttr))
                    return false;

                string sChkAttrVal = dictAttr[sChkAttr];

                XmlElement nodeTarget = null;
                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if(node.Attributes[sChkAttr] != null)
                    {
                        if (node.Attributes[sChkAttr].Value == sChkAttrVal)
                        {
                            nodeTarget = (XmlElement)node;
                            break;
                        }
                    }
                }
                if(nodeTarget == null)
                    nodeTarget = xmldoc.CreateElement(sNode);
                else
                    nodeTarget.RemoveAll();

                foreach (KeyValuePair<string, string> kvp in dictAttr)
                {
                    nodeTarget.SetAttribute(kvp.Key, kvp.Value);                   
                }
                parentNode.AppendChild(nodeTarget);

                bModify = true;

                return true;
            }

            public bool SetVal(string sParentNode, string sNode, List<Dictionary<string, string>> listAttr)
            {
                return SetVal(sParentNode, sNode, listAttr, false, true);
            }
            public bool SetVal(string sParentNode, string sNode, List<Dictionary<string, string>> listAttr, bool bNew)
            {
                return SetVal(sParentNode, sNode, listAttr, bNew, true);
            }
            public bool SetVal(string sParentNode, string sNode, List<Dictionary<string, string>> listAttr, bool bNew, bool bRemoveAttr)
            {
                if (xmldoc == null)
                    return false;

                XmlNodeList parentNode = xmldoc.SelectNodes(sParentNode);
                if (parentNode.Count == 0)
                    return false;

                if (!bNew)
                {
                    if (bRemoveAttr)
                        parentNode[parentNode.Count - 1].RemoveAll();
                    else
                        parentNode[parentNode.Count - 1].InnerXml = string.Empty;
                }
                foreach (Dictionary<string, string> item in listAttr)
                {
                    XmlElement nodeItem = xmldoc.CreateElement(sNode);
                    foreach (KeyValuePair<string, string> kvp in item)
                    {
                        nodeItem.SetAttribute(kvp.Key, kvp.Value);
                    }
                    parentNode[parentNode.Count - 1].AppendChild(nodeItem);
                }
                bModify = true;

                return true;
            }

            public bool SetVal(string sParentNode, string sNode, string sVal)
            {
                if (xmldoc == null || string.IsNullOrEmpty(sNode))
                    return false;

                XmlNode parent = xmldoc.SelectSingleNode(sParentNode);
                if (parent == null)
                {
                    string[] sNodeArr = sParentNode.Split('/');
                    XmlNode parentNew = xmldoc.SelectSingleNode("root");
                    string sNodeTmp = "/";
                    for (int i = 0; i < sNodeArr.Length; i++)
                    {
                        if (sNodeArr[i] == string.Empty)
                            continue;
                        sNodeTmp += "/" + sNodeArr[i];
                        XmlNode n = xmldoc.SelectSingleNode(sNodeTmp);
                        if (n == null)
                        {
                            n = xmldoc.CreateElement(sNodeArr[i]);
                            parentNew.AppendChild(n);
                        }
                        parentNew = n;
                    }
                    parent = parentNew;
                    bModify = true;
                }

                XmlNode node = parent.SelectSingleNode(sNode);
                if (node == null)
                {
                    XmlElement newNode = xmldoc.CreateElement(sNode);
                    newNode.SetAttribute("val", sVal);
                    parent.AppendChild(newNode);

                    bModify = true;
                    return true;
                }

                if (node.Attributes["val"] != null)
                {
                    if (node.Attributes["val"].Value != sVal)
                    {
                        node.Attributes["val"].Value = sVal;
                        bModify = true;
                    }
                }
                return true;
            }

            public List<Dictionary<string, string>> GetAttr(string sNode)
            {
                return GetAttr(sNode, "item", new int[0]);
            }
            public List<Dictionary<string, string>> GetAttr(string sNode, string sChildNodeName)
            {
                return GetAttr(sNode, sChildNodeName, new int[0]);
            }

            public List<Dictionary<string, string>> GetAttr(string sNode, string sChildNodeName, int iNodeNum)
            {
                return GetAttr(sNode, sChildNodeName, new int[1] { iNodeNum });
            }
            public List<Dictionary<string, string>> GetAttr(string sNode, string sChildNodeName, int[] iNodeNums)
            {
                List<Dictionary<string, string>> retList = new List<Dictionary<string, string>>();

                if (xmldoc == null)
                    return retList;

                XmlNode nodeParent = xmldoc.SelectSingleNode(sNode);
                int iSelNode = 0;
                if (iNodeNums.Length > 0)
                {
                    for(int i = 0; i < iNodeNums.Length; i++)
                    {
                        nodeParent = nodeParent.ParentNode;
                    }
                    for (int i = 0; i < iNodeNums.Length; i++)
                    {
                        nodeParent = nodeParent.ChildNodes[iNodeNums[i]];
                    }
                    iSelNode = iNodeNums[iNodeNums.Length - 1];
                }

                //XmlNodeList node = xmldoc.SelectNodes(sNode);
                if (nodeParent == null)
                    return retList;

                XmlNodeList itemList = nodeParent.SelectNodes(sChildNodeName);
                if (itemList.Count == 0)
                    return retList;

                for (int i = 0; i < itemList.Count; i++)
                {
                    retList.Add(new Dictionary<string, string>());
                    for (int j = 0; j < itemList[i].Attributes.Count; j++)
                    {
                        retList.Last().Add(itemList[i].Attributes.Item(j).Name, itemList[i].Attributes.Item(j).Value);
                    }
                }
                return retList;
            }

            public bool GetBool(string sNode)
            {
                string sVal = GetVal(sNode);
                if (string.IsNullOrEmpty(sVal))
                {
                    return false;
                }
                return bool.Parse(sVal);
            }

            public float GetFloat(string sNode)
            {
                string sVal = GetVal(sNode);
                if (string.IsNullOrEmpty(sVal))
                {
                    return 0f;
                }
                return float.Parse(sVal);
            }

            public int GetInt(string sNode)
            {
                string sVal = GetVal(sNode);
                if (string.IsNullOrEmpty(sVal))
                {
                    return 0;
                }
                return int.Parse(sVal);
            }

            public string GetString(string sNode)
            {
                return GetVal(sNode);
            }

            private string GetVal(string sNode)
            {
                if (xmldoc == null)
                    return string.Empty;

                XmlNode node = xmldoc.SelectSingleNode(sNode);
                if (node == null)
                    return string.Empty;

                if (node.Attributes["val"] != null)
                {
                    return node.Attributes["val"].Value;
                }
                return string.Empty;
            }

            public void LoadXML()
            {
                Init();
            }

            public void SaveXML()
            {
                if (bModify)
                {
                    xmldoc.Save(sXmlFileName);
                    bModify = false;
                }
                return;
            }
        }

        //private static readonly string[] sFaceNames = new string[]
        //{
        //    "通常","オリジナル","微笑み","笑顔","にっこり",
        //    "優しさ","発情","ジト目","閉じ目","思案伏せ目",
        //    "ドヤ顔","引きつり笑顔","苦笑い","困った","疑問",
        //    "ぷんすか","むー","泣き","拗ね","照れ",
        //    "悲しみ２","きょとん","びっくり","少し怒り","怒り",
        //    "照れ叫び","誘惑","接吻","居眠り安眠","まぶたギュ",
        //    "目を見開いて","痛みで目を見開いて","恥ずかしい","ためいき","がっかり",
        //    "口開け","目口閉じ","ウインク照れ","にっこり照れ","ダンス目つむり",
        //    "ダンスあくび","ダンスびっくり","ダンス微笑み","ダンス目あけ","ダンス目とじ",
        //    "ダンス誘惑","ダンス困り顔","ダンスウインク","ダンス真剣","ダンス憂い",
        //    "ダンスジト目","ダンスキス","エロ期待","エロ緊張","エロ怯え",
        //    "エロ痛み我慢","エロ痛み我慢２","エロ痛み我慢３","絶頂射精後１","絶頂射精後２",
        //    "興奮射精後１","興奮射精後２","通常射精後１","通常射精後２","余韻弱",
        //    "追加よだれ","エロメソ泣き","エロ絶頂","エロ放心","エロ舌責",
        //    "エロ舌責嫌悪","エロ舌責快楽","エロ興奮０","エロ興奮１","エロ興奮２",
        //    "エロ興奮３","エロ嫌悪１","エロ通常１","エロ通常２","エロ通常３",
        //    "エロ好感１","エロ好感２","エロ好感３","エロ我慢１","エロ我慢２",
        //    "エロ我慢３","エロ痛み１","エロ痛み２","エロ痛み３","エロ羞恥１",
        //    "エロ羞恥２","エロ羞恥３","あーん","エロ舐め嫌悪","エロ舐め嫌悪２",
        //    "エロ舐め快楽","エロ舐め快楽２","エロ舐め愛情","エロ舐め愛情２","エロ舐め通常",
        //    "エロ舐め通常２","エロフェラ快楽","エロフェラ愛情","エロフェラ嫌悪","エロフェラ通常",
        //    "閉じ舐め嫌悪","閉じ舐め嫌悪２","閉じ舐め快楽","閉じ舐め快楽２","閉じ舐め愛情",
        //    "閉じ舐め愛情２","閉じ舐め通常","閉じ舐め通常２","閉じフェラ快楽","閉じフェラ愛情",
        //    "閉じフェラ嫌悪","閉じフェラ通常",
        //};

    }
}