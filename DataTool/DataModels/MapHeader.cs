using System.Runtime.Serialization;
using TankLib;
using TankLib.STU.Types;
using TankLib.STU.Types.Enums;
using static DataTool.Helper.IO;

namespace DataTool.DataModels {
    [DataContract]
    public class MapHeader {
        [DataMember]
        public teResourceGUID[] GameModes;

        [DataMember]
        public teResourceGUID MapGUID;

        [DataMember]
        public Enum_A0F51DCC MapType;

        [DataMember]
        public string Name;

        [DataMember]
        public Enum_668FA6B6 State;

        [DataMember]
        public string VariantName;

        public MapHeader(STUMapHeader mapHeader) {
            Name        = GetString(mapHeader.m_displayName);
            VariantName = GetString(mapHeader.m_1C706502);
            MapGUID     = mapHeader.m_map;
            State       = mapHeader.m_A125818B;
            MapType     = mapHeader.m_mapType;

            GameModes = Helper.JSON.FixArray(mapHeader.m_D608E9F3);
        }

        public string GetName() { return VariantName ?? Name ?? "Title Screen"; }

        public string GetUniqueName() {
            var name = GetName();
            return $"{name}:{teResourceGUID.Index(MapGUID):X}";
        }
    }
}
