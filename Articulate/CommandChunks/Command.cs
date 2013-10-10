using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Diagnostics;

namespace Articulate
{
    class Command
    {
        public List<CommandChunk> CommandChunks { get; private set; }

        public Command(CommandChunk trigger)
        {
            CommandChunks = new List<CommandChunk>();
            CommandChunks.Add(trigger);
        }

        public Command(string semantic, string[] alternates, ushort[] keys)
        {
            // create command trigger
            CommandChunk trigger = new CommandChunk(semantic);
            trigger.Add(semantic, alternates, keys);

            CommandChunks = new List<CommandChunk>();
            CommandChunks.Add(trigger);
        }

        public Dictionary<string, ushort[]> GetKeyList()
        {
            Dictionary<string, ushort[]> keyList = new Dictionary<string, ushort[]>();

            foreach (CommandChunk chunk in CommandChunks)
            {
                Dictionary<string, ushort[]> chunkKeyList = chunk.GetKeyList();

                foreach(KeyValuePair<string, ushort[]> entry in chunkKeyList)
                {
                    keyList.Add(entry.Key, entry.Value);
                }
            }
            
            return keyList;
        }
    }
}
