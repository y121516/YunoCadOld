using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatix.MGDS;

namespace YunoCad
{
    public static class Attribute
    {
        // reference, layer, object, primitive, window
        static void Delete(string mnemonicName)
        {
            Cad.AttDel(mnemonicName);
        }

        static void Delete(Mnemonic mnemonic)
        {
            Cad.AttDel(mnemonic.Name);
        }

        static void Set(string mnemonicName, string attributeValue)
        {
            Cad.AttVal(mnemonicName, attributeValue);
        }

        static void Set(Mnemonic mnemonic, string attributeValue)
        {
            Cad.AttVal(mnemonic.Name, attributeValue);
        }

        static string Get(string mnemonicName)
        {
            var attr = "";
            Cad.GetAttVal(mnemonicName, out attr);
            return attr;
        }

        static string Get(Mnemonic mnemonic)
        {
            var attr = "";
            Cad.GetAttVal(mnemonic.Name, out attr);
            return attr;
        }
    }

    public class Mnemonic
    {
        const AttributeType DefaultAttributeType = AttributeType.Text;
        const int DefaultMaxNumList = 1;
        const int DefaultMin = 0;
        const int DefaultMax = 65535;
        const string DefaultPrompt = "";
        const string DefaultWordList = "";

        public static void Define(
            string mnemonicName,
            AttributeType type = DefaultAttributeType,
            int maxNumList = DefaultMaxNumList,
            int min = DefaultMin,
            int max = DefaultMax,
            string prompt = DefaultPrompt,
            string wordList = DefaultWordList)
        {
            Cad.MnemDefLV(mnemonicName, type, maxNumList, min, max, prompt, wordList);
        }

        public static void Define(
            string mnemonicName,
            AttributeType type = DefaultAttributeType,
            int min = DefaultMin,
            int max = DefaultMax,
            string prompt = DefaultPrompt)
        {
            Define(mnemonicName, type, DefaultMaxNumList, min, max, prompt, DefaultWordList);
        }

        public static void Define(
            string mnemonicName,
            int min = DefaultMin,
            int max = DefaultMax,
            string prompt = DefaultPrompt)
        {
            Define(mnemonicName, DefaultAttributeType, DefaultMaxNumList, min, max, prompt, DefaultWordList);
        }


        public string Name { get; } = "";

        private AttributeType _Type;

        public AttributeType Type
        {
            get { return _Type; }
            private set { _Type = value; }
        }


        private int _MaxNumList;

        public int MaxNumList
        {
            get { return _MaxNumList; }
            private set { _MaxNumList = value; }
        }

        private int _Min;

        public int Min
        {
            get { return _Min; }
            private set { _Min = value; }
        }

        private int _Max;

        public int Max
        {
            get { return _Max; }
            private set { _Max = value; }
        }

        private string _Prompt = "";

        public string Prompt
        {
            get { return _Prompt; }
            private set { _Prompt = value; }
        }

        private string _WordList = "";

        public string WordList
        {
            get { return _WordList; }
            private set { _WordList = value; }
        }

        /// <summary>
        /// すでに定義されているニーモニックの情報を取得します。
        /// </summary>
        /// <param name="mnemonicName">ニーモニック名</param>
        public Mnemonic(string mnemonicName)
        {
            Cad.GetMnemDefLV(mnemonicName, out _Type, out _MaxNumList, out _Min, out _Max, out _Prompt, out _WordList);
        }

        /// <summary>
        /// 新たにニーモニックを定義します。
        /// すでに　<paramref name="mnemonicName"/>　ニーモニックが定義されている場合は設定しなおします。
        /// </summary>
        /// <param name="mnemonicName"></param>
        /// <param name="type"></param>
        /// <param name="maxNumList"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="prompt"></param>
        /// <param name="wordList"></param>
        public Mnemonic(
            string mnemonicName,
            AttributeType type = AttributeType.Text,
            int maxNumList = 1,
            int min = 0,
            int max = 65535,
            string prompt = "",
            string wordList = ""
            )
        {
            Cad.MnemDefLV(mnemonicName, type, maxNumList, min, max, prompt, wordList);
            Name = mnemonicName;
            Type = type;
            MaxNumList = maxNumList;
            Min = min;
            Max = max;
            Prompt = prompt;
            WordList = wordList;
        }

        static string[] Mnemonics()
        {
            var mnemonics = "";
            Cad.GetMnemonics(out mnemonics);
            return mnemonics.Split(',');
        }

        static int Count { get { return Cad.GetNumMnemDefs(); } }

        static void Delete(string mnemonicName)
        {
            Cad.MnemDefDel(mnemonicName);
        }

        void Delete()
        {
            Cad.MnemDefDel(Name);
        }

        static void Set(string mnemonicName, AttributeType attributeType, int maxNumList, int min, int max, string prompt, string wordlist)
        {
            Cad.MnemDefLV(mnemonicName, attributeType, maxNumList, min, max, prompt, wordlist);
        }

        void Set(AttributeType type, int maxNumList, int min, int max, string prompt, string wordList)
        {
            Cad.MnemDefLV(Name, type, maxNumList, min, max, prompt, wordList);
            Type = type;
            MaxNumList = maxNumList;
            Min = min;
            Max = max;
            Prompt = prompt;
            WordList = wordList;
        }
    }

    public class ReferenceLevelMnemonic
    {
        const char FirstLetter = 'R';

        public string Name { get; }

        public ReferenceLevelMnemonic(string mnemonicName)
        {
            Name = $"{FirstLetter}{mnemonicName}";
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class LayerLevelMnemonic { }
    public class ObjectLevelMnemonic { }
    public class WndLevelMnemonic { }

    public class CurrentObjectAttribute
    {
        public static CurrentObjectAttribute Instance { get; } = new CurrentObjectAttribute();

        CurrentObjectAttribute() { }

        public string this[ReferenceLevelMnemonic mnemonic]
        {
            get
            {
                var attr = "";
                Cad.GetAttVal(mnemonic.ToString(), out attr);
                return attr;
            }
            set
            {
                Cad.AttVal(mnemonic.ToString(), value);
            }
        }

        public string this[string mnemonic]
        {
            get
            {
                return this[new ReferenceLevelMnemonic(mnemonic)];
            }
            set
            {
                this[new ReferenceLevelMnemonic(mnemonic)] = value;
            }
        }
    }
}
