using System.Runtime.Serialization;
using DataTool.Helper;
using TankLib;
using TankLib.STU.Types;
using TankLib.STU.Types.Enums;
using static DataTool.Helper.IO;

namespace DataTool.DataModels {
    [DataContract]
    public class Loadout {
        [DataMember]
        public LoadoutCategory Category;

        [DataMember]
        public string Description;

        [DataMember]
        public teResourceGUID MovieGUID;

        [DataMember]
        public string Name;

        public Loadout(ulong key) {
            var loadout = STUHelper.GetInstance<STULoadout>(key);
            if (loadout == null) return;
            Init(loadout);
        }

        public Loadout(STULoadout loadout) { Init(loadout); }

        private void Init(STULoadout loadout) {
            MovieGUID = loadout.m_infoMovie;

            Category = loadout.m_category;

            Name        = GetString(loadout.m_name);
            Description = GetString(loadout.m_description);
        }
    }
}
