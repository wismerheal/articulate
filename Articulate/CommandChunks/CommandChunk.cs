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
    [Serializable]
    public class CommandChunkEntry
    {
        public string Semantic;
        public string[] Alternates;
        public ushort[] KeyList;

        public CommandChunkEntry()
        {
            Semantic = "";
            Alternates = null;
            KeyList = null;
        }
    }

    [Serializable]
    public class CommandChunk
    {
        #region Public Members

        /// <summary>
        /// The name of this CommandChunk
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Maximum number of semantic's appearing in this CommandChunk
        /// </summary>
        public int MaxRepeat { get; set; }

        /// <summary>
        /// Optional prefixes
        /// </summary>
        public string[] Prefixes { get; set; }

        /// <summary>
        /// Optional affixes
        /// </summary>
        public string[] Affixes { get; set; }

        /// <summary>
        /// A list of all of the CommandChunkEntries that make up this particular CommandChunk
        /// </summary>
        public List<CommandChunkEntry> Entries { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CommandChunk()
        {
            Entries = new List<CommandChunkEntry>();

            MaxRepeat = 1;
            Prefixes = null;
            Affixes = null;
        }

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
            CommandChunkEntry newEntry = new CommandChunkEntry();
            newEntry.Semantic = semantic;
            newEntry.Alternates = alternates;
            newEntry.KeyList = keys;
            Entries.Add(newEntry);
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

        #region Public Methods

        public SrgsItem GetItem()
        {
            // set the with the repeat parameters
            SrgsItem item = new SrgsItem(1, MaxRepeat);

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
                    item.Add(prefixes);
                }

                // add the required choice
                item.Add(choice);

                // add the optional affixes
                if (Affixes != null)
                {
                    SrgsOneOf affixesChoice = new SrgsOneOf(Affixes);
                    SrgsItem affixes = new SrgsItem(0, 1, affixesChoice);
                    item.Add(affixes);
                }
            }

            return item;
        }

        #endregion

        #region Private Methods

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
