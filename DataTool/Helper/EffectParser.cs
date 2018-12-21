using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TankLib;
using TankLib.Chunks;

namespace DataTool.Helper {
    public class EffectParser {
        public teChunkedData ChunkedData;
        public ulong         GUID;

        public EffectParser(teChunkedData chunked, ulong guid) {
            ChunkedData = chunked;
            GUID        = guid;
        }

        public EffectInfo ProcessAll(Dictionary<ulong, ulong> replacements) {
            var ret = new EffectInfo { GUID = GUID };
            ret.SetupEffect();
            foreach (var chunk in GetChunks()) Process(ret, chunk, replacements);
            return ret;
        }

        public IEnumerable<KeyValuePair<ChunkPlaybackInfo, IChunk>> GetChunks() {
            PMCEInfo lastComponent = null;
            IChunk   lastChunk     = null;
            for (var i = 0; i < ChunkedData.Chunks.Length; i++) {
                if (ChunkedData?.Chunks[i]
                               ?.GetType() ==
                    typeof(teEffectChunkComponent)) {
                    if (!(ChunkedData.Chunks[i] is teEffectChunkComponent component)) continue;
                    lastComponent = new PMCEInfo {
                                                     Hardpoint = component.Header.Hardpoint,
                                                     StartTime = component.StartTime,
                                                     EndTime   = component.Duration
                                                 };
                    // if (effect.Hardpoints == null) continue;
                    // if (effect.Hardpoints.Length <= pmce.Data.Index) continue;
                    continue;
                }

                var playbackInfo = new ChunkPlaybackInfo(lastComponent, lastChunk);

                yield return new KeyValuePair<ChunkPlaybackInfo, IChunk>(playbackInfo, ChunkedData.Chunks[i]);

                lastComponent = null;
                lastChunk     = ChunkedData.Chunks[i];
            }
        }

        public static void AddDMCE(EffectInfo               effect,
                                   teEffectComponentModel   model,
                                   ChunkPlaybackInfo        playbackInfo,
                                   Dictionary<ulong, ulong> replacements) {
            var newInfo = new DMCEInfo {
                                           Model        = model.Header.Model,
                                           Material     = model.Header.ModelLook,
                                           Animation    = model.Header.Animation,
                                           PlaybackInfo = playbackInfo
                                       };
            if (replacements.ContainsKey(newInfo.Model)) newInfo.Model         = replacements[newInfo.Model];
            if (replacements.ContainsKey(newInfo.Material)) newInfo.Material   = replacements[newInfo.Material];
            if (replacements.ContainsKey(newInfo.Animation)) newInfo.Animation = replacements[newInfo.Animation];
            effect.DMCEs.Add(newInfo);
        }

        public static void AddCECE(EffectInfo                     effect,
                                   teEffectComponentEntityControl control,
                                   ChunkPlaybackInfo              playbackInfo,
                                   Dictionary<ulong, ulong>       replacements) {
            var newInfo = new CECEInfo {
                                           Animation    = control.Header.Animation,
                                           PlaybackInfo = playbackInfo,
                                           Action       = control.Header.Action,
                                           Identifier   = control.Header.Identifier
                                       };
            if (replacements.ContainsKey(newInfo.Animation)) newInfo.Animation   = replacements[newInfo.Animation];
            if (replacements.ContainsKey(newInfo.Identifier)) newInfo.Identifier = replacements[newInfo.Identifier];
            effect.CECEs.Add(newInfo);
        }

        public static void AddOSCE(EffectInfo               effect,
                                   teEffectComponentSound   osce,
                                   ChunkPlaybackInfo        playbackInfo,
                                   Dictionary<ulong, ulong> replacements) {
            var newInfo                                                = new OSCEInfo { PlaybackInfo = playbackInfo, Sound = osce.Header.Sound };
            if (replacements.ContainsKey(newInfo.Sound)) newInfo.Sound = replacements[newInfo.Sound];
            effect.OSCEs.Add(newInfo);
        }

        public static void AddFECE(EffectInfo               effect,
                                   ulong                    guid,
                                   EffectInfo               subEffect,
                                   ChunkPlaybackInfo        playbackInfo,
                                   Dictionary<ulong, ulong> replacements) {
            var newInfo = new FECEInfo {
                                           PlaybackInfo = playbackInfo,
                                           Effect       = subEffect,
                                           GUID         = guid
                                       };
            if (replacements.ContainsKey(newInfo.GUID)) newInfo.GUID = replacements[newInfo.GUID];
            effect.FECEs.Add(newInfo);
        }

        public static void AddNECE(EffectInfo               effect,
                                   teEffectComponentEntity  nece,
                                   ChunkPlaybackInfo        playbackInfo,
                                   Dictionary<ulong, ulong> replacements) {
            var newInfo = new NECEInfo {
                                           PlaybackInfo = playbackInfo,
                                           GUID         = nece.Header.Entity,
                                           Identifier   = nece.Header.Identifier
                                       };
            if (replacements.ContainsKey(newInfo.GUID)) newInfo.GUID             = replacements[newInfo.GUID];
            if (replacements.ContainsKey(newInfo.Identifier)) newInfo.Identifier = replacements[newInfo.Identifier];
            effect.NECEs.Add(newInfo);
        }

        /*public static void AddRPCE(EffectInfo effect, RPCE rpce, ChunkPlaybackInfo playbackInfo, Dictionary<ulong, ulong> replacements) {
            RPCEInfo newInfo = new RPCEInfo {PlaybackInfo = playbackInfo, Model = rpce.Data.Model};
            if (replacements.ContainsKey(newInfo.Model)) newInfo.Model = replacements[newInfo.Model];
            effect.RPCEs.Add(newInfo);
        }

        public static void AddSSCE(EffectInfo effectInfo, SSCE ssce, Type lastType, Dictionary<ulong, ulong> replacements) {
            ulong def = ssce.Data.TextureDefinition;
            ulong mat = ssce.Data.Material;
            if (replacements.ContainsKey(def)) def = replacements[def];
            if (replacements.ContainsKey(mat)) mat = replacements[mat];
            if (lastType == typeof(RPCE)) {
                RPCEInfo rpceInfo = effectInfo.RPCEs.Last();
                rpceInfo.TextureDefiniton = def;
                rpceInfo.Material = mat;
            }
        }*/

        public static void AddSVCE(EffectInfo                     effect,
                                   teEffectComponentVoiceStimulus svce,
                                   ChunkPlaybackInfo              playbackInfo,
                                   Dictionary<ulong, ulong>       replacements) {
            var newInfo                                                                = new SVCEInfo { PlaybackInfo = playbackInfo, VoiceStimulus = svce.Header.VoiceStimulus };
            if (replacements.ContainsKey(newInfo.VoiceStimulus)) newInfo.VoiceStimulus = replacements[newInfo.VoiceStimulus];
            effect.SVCEs.Add(newInfo);
        }

        public void Process(EffectInfo effectInfo, KeyValuePair<ChunkPlaybackInfo, IChunk> chunk, Dictionary<ulong, ulong> replacements) {
            // todo: STUVoiceStimulus has f3099f20/m_volume
            // probably more stuff too


            // hey have some notes about particles:
            // 000000003CEC.006 - 000000001D3D.08F = ana - guardian:
            //     one RPCE, 61 chunks
            //     seems to be at correct position with rpce at rot: x=90

            // 000000003796.006 - 000000001A31.08F = genji - warrior's salute:
            //     one RPCE, 64 chunks.

            // VCCE might be a texture/material transform
            // A B C D = R G B A
            // see 'extract-debug-vcce'


            if (effectInfo   == null) return;
            if (chunk.Value  == null) return;
            if (replacements == null) replacements = new Dictionary<ulong, ulong>();

            // if (chunk.Value.GetType() == typeof(TCFE)) {
            //     TCFE tcfe = chunk.Value as TCFE;
            //     if (tcfe == null) return; 
            //     effectInfo.EffectLength = tcfe.Data.EndTime1;
            // } else

            if (chunk.Value is teEffectComponentModel model) AddDMCE(effectInfo, model, chunk.Key, replacements);
            if (chunk.Value is teEffectComponentEntityControl control) AddCECE(effectInfo, control, chunk.Key, replacements);
            if (chunk.Value is teEffectComponentSound sound) {
                AddOSCE(effectInfo, sound, chunk.Key, replacements);
            } else if (chunk.Value is teEffectComponentEffect effectComponentEffect) {
                EffectInfo feceInfo                                  = null;
                ulong      effectGuid                                = effectComponentEffect.Header.Effect;
                if (replacements.ContainsKey(effectGuid)) effectGuid = replacements[effectGuid];
                using (var effectStream = IO.OpenFile(effectGuid)) {
                    if (effectStream != null) {
                        var subChunked = new teChunkedData(effectStream);
                        var sub        = new EffectParser(subChunked, effectGuid);
                        feceInfo = sub.ProcessAll(replacements);
                    }
                }

                AddFECE(effectInfo, effectGuid, feceInfo, chunk.Key, replacements);
            } else if (chunk.Value is teEffectComponentEntity entity) {
                AddNECE(effectInfo, entity, chunk.Key, replacements);
            } else if (chunk.Value is teEffectComponentVoiceStimulus voiceStimulus) {
                AddSVCE(effectInfo, voiceStimulus, chunk.Key, replacements);
            }

            // if (chunk.Value.GetType() == typeof(RPCE)) {
            //     RPCE rpce = chunk.Value as RPCE;
            //     if (rpce == null) return;
            //     AddRPCE(effectInfo, rpce, chunk.Key, replacements);
            // }
            // if (chunk.Value.GetType() == typeof(SSCE)) {
            //     SSCE ssce = chunk.Value as SSCE;
            //     if (ssce == null) return;
            //
            //     AddSSCE(effectInfo, ssce, chunk.Key.PreviousChunk?.GetType(), replacements);
            // }
        }

        public class ChunkPlaybackTimeInfo {
            public float EndTime;
            public float StartTime;
        }

        public class ChunkPlaybackInfo {
            public ulong                 Hardpoint;
            public IChunk                PreviousChunk;
            public ChunkPlaybackTimeInfo TimeInfo;

            public ChunkPlaybackInfo(PMCEInfo component, IChunk previousChunk) {
                if (component != null) {
                    TimeInfo  = new ChunkPlaybackTimeInfo { StartTime = component.StartTime, EndTime = component.EndTime };
                    Hardpoint = component.Hardpoint;
                } else {
                    Hardpoint = 0;
                }

                PreviousChunk = previousChunk;
            }
        }

        public class EffectChunkInfo {
            public ChunkPlaybackInfo PlaybackInfo;
        }

        public class DMCEInfo : EffectChunkInfo {
            public ulong Animation;
            public ulong Material;
            public ulong Model;
        }

        public class CECEInfo : EffectChunkInfo {
            public teEffectComponentEntityControl.Action Action;
            public ulong                                 Animation;
            public ulong                                 Identifier;
        }

        public class OSCEInfo : EffectChunkInfo {
            public ulong Sound;
        }

        public class FECEInfo : EffectChunkInfo {
            public EffectInfo Effect;
            public ulong      GUID;
        }

        public class NECEInfo : EffectChunkInfo {
            public ulong GUID;
            public ulong Identifier;
        }

        public class RPCEInfo : EffectChunkInfo {
            public ulong Material;
            public ulong Model;
            public ulong TextureDefiniton;
        }

        public class SVCEInfo : EffectChunkInfo {
            public ulong VoiceStimulus;
        }

        public class PMCEInfo : EffectChunkInfo {
            public float EndTime;
            public ulong Hardpoint;
            public float StartTime;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public class EffectInfo {
            public List<CECEInfo> CECEs;
            public List<DMCEInfo> DMCEs;

            public float          EffectLength; // seconds
            public List<FECEInfo> FECEs;
            public ulong          GUID;
            public List<NECEInfo> NECEs;
            public List<OSCEInfo> OSCEs;
            public List<RPCEInfo> RPCEs;
            public List<SVCEInfo> SVCEs;

            public ulong VoiceSet; // 05F for VoiceStimuli

            // todo: many more chunks
            // todo: OSCE / 02C is controlled by the bnk?

            public void SetupEffect() {
                DMCEs = new List<DMCEInfo>();
                CECEs = new List<CECEInfo>();
                OSCEs = new List<OSCEInfo>();
                NECEs = new List<NECEInfo>();
                FECEs = new List<FECEInfo>();
                RPCEs = new List<RPCEInfo>();
                SVCEs = new List<SVCEInfo>();
            }
        }
    }
}
