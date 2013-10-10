using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Articulate
{
    /// <summary>
    /// Each entry into the command chunk is a semantic choice
    /// </summary>
    public struct CommandChunkEntry
    {
        public string Semantic;
        public string[] Alternates;
        public ushort[] KeyList;
    }

    [Serializable]
    class CommandChunk
    {
        #region Public Members

        /// <summary>
        /// The SrgsItem that contains the command chunk information
        /// </summary>
        [XmlIgnore]
        public SrgsItem Item
        {
            get
            {
                if (_item == null)
                {
                    Generate();
                }
                return _item;
            }
            private set
            {
                _item = value;
            }
        }

        [XmlIgnore]
        [NonSerialized]
        private SrgsItem _item;

        /// <summary>
        /// The name of this CommandChunk
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Maximum number of semantic's appearing in this CommandChunk
        /// </summary>
        public int MaxRepeat
        {
            get
            {
                return _maxRepeat;
            }
            set
            {
                _maxRepeat = value;
                Generate();
            }
        }

        [XmlIgnore]
        [NonSerialized]
        private int _maxRepeat;

        /// <summary>
        /// Optional prefixes
        /// </summary>
        public string[] Prefixes
        {
            get
            {
                return _prefixes;
            }
            set
            {
                _prefixes = value;
                Generate();
            }
        }

        [XmlIgnore]
        [NonSerialized]
        private string[] _prefixes;

        /// <summary>
        /// Optional affixes
        /// </summary>
        public string[] Affixes
        {
            get
            {
                return _affixes;
            }
            set
            {
                _affixes = value;
                Generate();
            }
        }

        [XmlIgnore]
        [NonSerialized]
        private string[] _affixes;

        #endregion

        #region Private Members

        /// <summary>
        /// A list of all of the CommandChunkEntries that make up this particular CommandChunk
        /// </summary>
        private List<CommandChunkEntry> Entries { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="name">The name of this CommandChunk</param>
        public CommandChunk(string name)
        {
            Entries = new List<CommandChunkEntry>();
            Name = name;
            MaxRepeat = 1;
            Prefixes = null;
            Affixes = null;
        }

        #endregion

        #region Public Methods

        public void Add(string semantic, string[] alternates, ushort[] keys)
        {
            // fill in the new entry and add it in
            CommandChunkEntry newEntry;
            newEntry.Semantic = semantic;
            newEntry.Alternates = alternates;
            newEntry.KeyList = keys;
            Entries.Add(newEntry);

            Generate();
        }

        public Dictionary<string, ushort[]> GetKeyList()
        {
            Dictionary<string, ushort[]> keyList = new Dictionary<string, ushort[]>();

            // add each entry to the choice object
            foreach (CommandChunkEntry entry in Entries)
            {
                keyList.Add(entry.Semantic, entry.KeyList);
            }

            return keyList;
        }

        #endregion

        #region Private Methods

        private void Generate()
        {
            // set the with the repeat parameters
            Item = new SrgsItem(1, MaxRepeat);

            if (Entries.Count > 0)
            {
                SrgsOneOf choice = new SrgsOneOf();

                // add each entry to the choice object
                foreach (CommandChunkEntry entry in Entries)
                {
                    choice.Add(GetNewNode(entry.Alternates, entry.Semantic));
                }

                // add the optional prefixes
                if (Prefixes != null)
                {
                    SrgsOneOf prefixesChoice = new SrgsOneOf(Prefixes);
                    SrgsItem prefixes = new SrgsItem(0, 1, prefixesChoice);
                    Item.Add(prefixes);
                }

                // add the required choice
                Item.Add(choice);

                // add the optional affixes
                if (Affixes != null)
                {
                    SrgsOneOf affixesChoice = new SrgsOneOf(Affixes);
                    SrgsItem affixes = new SrgsItem(0, 1, affixesChoice);
                    Item.Add(affixes);
                }
            }
        }

        /// <summary>
        /// Gets a new SrgsItem with the spoken word alternates and a semantic assoicated
        /// </summary>
        /// <param name="alternates">Array of strings that are all alternate spoken word triggers</param>
        /// <param name="semantic">Semanitic assoicated</param>
        /// <returns>A new SrgsItem</returns>
        private SrgsItem GetNewNode(string[] alternates, string semantic)
        {
            return new SrgsItem(new SrgsOneOf(alternates), new SrgsSemanticInterpretationTag("out += \"{" + semantic + "}\";"));
        }

        #endregion
    }
}
