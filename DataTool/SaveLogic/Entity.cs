using System.IO;
using System.Linq;
using TankLib;
using TankLib.ExportFormats;

namespace DataTool.SaveLogic {
    public static class Entity {
        public class OverwatchEntity : IExportFormat {
            public const       ushort                        VersionMajor = 1;
            public const       ushort                        VersionMinor = 1;
            protected readonly FindLogic.Combo.EntityInfoNew Entity;

            protected readonly FindLogic.Combo.ComboInfo Info;

            public OverwatchEntity(FindLogic.Combo.EntityInfoNew entity, FindLogic.Combo.ComboInfo info) {
                Info   = info;
                Entity = entity;
            }

            public string Extension => "owentity";

            public void Write(Stream stream) {
                using (var writer = new BinaryWriter(stream)) {
                    writer.Write(Extension); // type identifier
                    writer.Write(VersionMajor);
                    writer.Write(VersionMinor);

                    writer.Write(Entity.GetNameIndex());
                    if (Entity.Model != 0) {
                        var modelInfo = Info.Models[Entity.Model];
                        writer.Write(modelInfo.GetName());
                    } else {
                        writer.Write("null");
                    }

                    if (Entity.RootEffect != 0) {
                        var effectInfo = Info.Effects[Entity.RootEffect];
                        writer.Write(effectInfo.GetName());
                    } else {
                        writer.Write("null");
                    }

                    writer.Write(teResourceGUID.Index(Entity.GUID));
                    writer.Write(teResourceGUID.Index(Entity.Model));
                    writer.Write(teResourceGUID.Index(Entity.RootEffect));

                    if (Entity.Children == null) {
                        writer.Write(0);
                        return;
                    }

                    writer.Write(Entity.Children.Count(x => x.GUID != 0));
                    foreach (var childEntityReference in Entity.Children.Where(x => x.GUID != 0)) {
                        var childEntityInfo = Info.Entities[childEntityReference.GUID];

                        writer.Write(childEntityInfo.GetName());
                        writer.Write(childEntityReference.Hardpoint);
                        writer.Write(childEntityReference.Variable);
                        writer.Write(teResourceGUID.Index(childEntityReference.Hardpoint));
                        writer.Write(teResourceGUID.Index(childEntityReference.Variable));
                        if (childEntityReference.Hardpoint != 0)
                            writer.Write(OverwatchModel.IdToString("hardpoint", teResourceGUID.Index(childEntityReference.Hardpoint)));
                        else
                            writer.Write("null"); // erm, k
                    }
                }
            }
        }
    }
}
