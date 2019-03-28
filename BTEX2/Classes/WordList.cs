using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BitCWallet.MnemonicEx1;

namespace BitCWallet
{
    class Wordlist
    {
        public static IWordlistSource WordlistSource
        {
            get;
            set;
        }

        private static string[] _words;
        public string Name { get; }
        public char Space { get; }
        public Wordlist(string[] words, char space, string name)
        {
            _words = words.Select(w => NormalizeString(w)).ToArray();
            Space = space;
            Name = name;
        }

        public Wordlist()
        {
            WordlistSource = new HardcodedWordlistSource();
        }

        public string GetSentence(int[] indices)
        {
            return string.Join(Space.ToString(), GetWords(indices));

        }
        private static Wordlist _Japanese;
        public static Wordlist Japanese
        {
            get
            {
                if (_Japanese == null)
                    _Japanese = LoadWordList(Language.Japanese).Result;
                return _Japanese;
            }
        }
        private static Wordlist _ChineseSimplified;
        public static Wordlist ChineseSimplified
        {
            get
            {
                if (_ChineseSimplified == null)
                    _ChineseSimplified = LoadWordList(Language.ChineseSimplified).Result;
                return _ChineseSimplified;
            }
        }
        private static Wordlist _Spanish;
        public static Wordlist Spanish
        {
            get
            {
                if (_Spanish == null)
                    _Spanish = LoadWordList(Language.Spanish).Result;
                return _Spanish;
            }
        }
        public static string English
        {
            get
            {
                return GetLanguageFileName(Language.English);
            }
        }
        private static Wordlist _French;
        public static Wordlist French
        {
            get
            {
                if (_French == null)
                    _French = LoadWordList(Language.French).Result;
                return _French;
            }
        }
        public int[] ToIndices(string[] words)
        {
            var indices = new int[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                int idx = -1;

                if (!WordExists(words[i], out idx))
                {
                    throw new FormatException("Word " + words[i] + " is not in the wordlist for this language, cannot continue to rebuild entropy from wordlist");
                }
                indices[i] = idx;
            }
            return indices;
        }
        public static string GetWordAtIndex(int index)
        {
            if (_words == null) return string.Empty;
            return _words[index];
        }
        public string[] GetWords(string[] sentence)
        {
            return ToIndices(sentence).Select(i => GetWordAtIndex(i)).ToArray();
        }
        public static string[] GetWords(int[] indices)
        {
            return
                indices
                .Select(i => GetWordAtIndex(i))
                .ToArray();
        }
       
        public static Task<Wordlist> LoadWordList(Language language)
        {
            GetLanguageFileName(language);
            return LoadWordList(language);
        }

        public static BitArray ToBits(int[] values)
        {
            if (values.Any(v => v >= 2048))
                return null;
            
            BitArray result = new BitArray(values.Length * 11);
            int i = 0;
            foreach (var val in values)
            {
                for (int p = 0; p < 11; p++)
                {
                    var v = (val & (1 << (10 - p))) != 0;
                    result.Set(i, v);
                    i++;
                }
            }
            return result;
        }
        public static Task<Wordlist> AutoDetectAsync(string[] sentence)
        {
            return LoadWordList(AutoDetectLanguage(sentence));
        }
        public static Wordlist AutoDetect(string[] sentence)
        {
            return LoadWordList(AutoDetectLanguage(sentence)).Result;
        }
        internal static string GetLanguageFileName(Language language)
        {
            string name = null;
            switch (language)
            {
                case Language.ChineseTraditional:
                    name = "chinese_traditional";
                    break;
                case Language.ChineseSimplified:
                    name = "chinese_simplified";
                    break;
                case Language.English:
                    name = "english";
                    break;
                case Language.Japanese:
                    name = "japanese";
                    break;
                case Language.Spanish:
                    name = "spanish";
                    break;
                case Language.French:
                    name = "french";
                    break;
                case Language.PortugueseBrazil:
                    name = "portuguese_brazil";
                    break;
                default:
                    throw new NotSupportedException(language.ToString());
            }
            return name;
        }
     
        public bool WordExists(string word, out int index)
        {
            word = NormalizeString(word);
            if (_words.Contains(word))
            {
                index = Array.IndexOf(_words, word);
                return true;
            }
            index = -1;
            return false;
        }

        public static Language AutoDetectLanguage(string[] words)
        {
            List<int> languageCount = new List<int>(new int[] { 0, 0, 0, 0, 0, 0, 0 });
            int index;
            foreach (string s in words)
            { 
                if (Japanese.WordExists(s, out index))
                {
                    languageCount[1]++;
                }
                if (Spanish.WordExists(s, out index))
                {
                    //spanish is at 2
                    languageCount[2]++;
                }
                if (ChineseSimplified.WordExists(s, out index))
                {
                    languageCount[3]++;
                }
                if (French.WordExists(s, out index))
                {
                    languageCount[5]++;
                }
            }
            if (languageCount.Max() == 0)
            {
                return Language.Unknown;
            }
            if (languageCount.IndexOf(languageCount.Max()) == 0)
            {
                return Language.English;
            }
            else if (languageCount.IndexOf(languageCount.Max()) == 1)
            {
                return Language.Japanese;
            }
            else if (languageCount.IndexOf(languageCount.Max()) == 2)
            {
                return Language.Spanish;
            }
            else if (languageCount.IndexOf(languageCount.Max()) == 3)
            {
                if (languageCount[4] > 0)
                {
                    return Language.ChineseTraditional;
                }
                return Language.ChineseSimplified;
            }
            else if (languageCount.IndexOf(languageCount.Max()) == 4)
            {
                return Language.ChineseTraditional;
            }
            else if (languageCount.IndexOf(languageCount.Max()) == 5)
            {
                return Language.French;
            }
            else if (languageCount.IndexOf(languageCount.Max()) == 6)
            {
                return Language.PortugueseBrazil;
            }
            return Language.Unknown;
        }

        public static int[] ToIntegers(BitArray bits)
        {
            return
                bits.OfType<bool>().Select((v, i) => new
                {
                    Group = i / 11,
                    Value = v ? 1 << (10 - (i % 11)) : 0
                })
                .GroupBy(_ => _.Group, _ => _.Value)
                .Select(g => g.Sum())
                .ToArray();
        }
    }
}