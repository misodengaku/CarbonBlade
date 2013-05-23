using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sambhala
{
    public class Salvation
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public List<Word> Words { get; set; }

        //public Salvation() { }

        public Salvation()
        {
            Key = "";
            Secret = "";
            Words = new List<Word>();
        }
    }

    public class Word
    {
        public string MatchWord { get; set; }
        public string PostWord { get; set; }
        public bool isRegExp { get; set; }

        public Word() { }

        public Word(string MatchWord, string PostWord, bool isRegExp = false)
        {
            this.MatchWord = MatchWord;
            this.PostWord = PostWord;
            this.isRegExp = isRegExp;
        }
    }
}
