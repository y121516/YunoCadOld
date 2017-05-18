using M = Informatix.MGDS;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class MnemonicInfo
    {
        const M.AttributeType DefaultAttributeType = M.AttributeType.Text;
        const int DefaultMaxNumList = 1;
        const int DefaultMin = 0;
        const int DefaultMax = 65535;
        const string DefaultPrompt = "";
        const string DefaultWordList = "";

        public M.AttributeType AttributeType { get; set; }
        public int MaxNumList { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public string Prompt { get; set; }
        public string WordList { get; set; }

        public MnemonicInfo(
            M.AttributeType attributeType = DefaultAttributeType,
            int maxNumList = DefaultMaxNumList,
            int min = DefaultMin,
            int max = DefaultMax,
            string prompt = DefaultPrompt,
            string wordList = DefaultWordList)
        {
            AttributeType = attributeType;
            MaxNumList = maxNumList;
            Min = min;
            Max = max;
            Prompt = prompt;
            WordList = wordList;
        }
    }

    public class Mnemonics
    {
        internal Mnemonics Instance { get; } = new Mnemonics();
        Mnemonics() { }

        public MnemonicInfo this[string mnemonicName]
        {
            get
            {
                MC.GetMnemDefLV(mnemonicName, out M.AttributeType type, out int maxNumList, out int min, out int max, out string prompt, out string wordList);
                return new MnemonicInfo(type, maxNumList, min, max, prompt, wordList);
            }
            set => MC.MnemDefLV(mnemonicName, value.AttributeType, value.MaxNumList, value.Min, value.Max, value.Prompt, value.WordList);
        }

        public void Delete(string mnemonicName)
            => MC.AttDel(mnemonicName);

        public string[] List
        {
            get
            {
                MC.GetMnemonics(out string mnemonics);
                return mnemonics.Split(',');
            }

        }

        public int Count { get => MC.GetNumMnemDefs(); }
    }
}
