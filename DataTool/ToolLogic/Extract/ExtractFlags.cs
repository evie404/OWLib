using System;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace DataTool.ToolLogic.Extract {
    [Serializable]
    [UsedImplicitly]
    public class ExtractFlags : ICLIFlags {
        [CLIFlag("out-path", Category = "Extract", Help = "Output path", Positional = 2, IsRequired = true)]
        public string OutputPath { get; set; }

        [CLIFlag("convert-textures-type", Default = "tif", Category = "Extract", Help = "Texture output type", ValidValues = new[] { "dds", "tif", "png", "jpg", "hdr" })]
        public string ConvertTexturesType { get; set; }

        [CLIFlag("convert-lossless-textures", Category = "Extract", Help = "Output lossless textures (if converted)")]
        public bool ConvertTexturesLossless { get; set; }

        [CLIFlag("raw-textures", Category = "Extract", Help = "Do not convert textures")]
        public bool RawTextures { get; set; }

        [CLIFlag("raw-sound", Category = "Extract", Help = "Do not convert sounds")]
        public bool RawSound { get; set; }

        [CLIFlag("raw-models", Category = "Extract", Help = "Do not convert models")]
        public bool RawModels { get; set; }

        [CLIFlag("raw-animations", Category = "Extract", Help = "Do not convert animations")]
        public bool RawAnimations { get; set; }

        [CLIFlag("skip-textures", Category = "Extract", Help = "Skip texture extraction")]
        public bool SkipTextures { get; set; }

        [CLIFlag("skip-sound", Category = "Extract", Help = "Skip sound extraction")]
        public bool SkipSound { get; set; }

        [CLIFlag("skip-models", Category = "Extract", Help = "Skip model extraction")]
        public bool SkipModels { get; set; }

        [CLIFlag("skip-animations", Category = "Extract", Help = "Skip animation extraction")]
        public bool SkipAnimations { get; set; }

        [CLIFlag("extract-refpose", Category = "Extract", Help = "Extract skeleton refposes")]
        public bool ExtractRefpose { get; set; }

        [CLIFlag("extract-mstu", Category = "Extract", Help = "Extract model STU")]
        public bool ExtractModelStu { get; set; }

        [CLIFlag("raw", Category = "Extract", Help = "Skip all conversion")]
        public bool Raw { get; set; }

        [CLIFlag("lod", Default = (byte) 1, Category = "Extract", Help = "Force extracted model LOD")]
        public byte LOD { get; set; }

        [CLIFlag("scale-anims", Category = "Extract", Help = "set to true for Blender 2.79, false for Maya and when Blender SEAnim tools are updated for 2.8")]
        public bool ScaleAnims { get; set; }

        [CLIFlag("flatten", Category = "Extract", Help = "Flatten directory structure")]
        public bool FlattenDirectory { get; set; }

        [CLIFlag("force-dds-multisurface", Category = "Extract", Help = "Save multisurface textures as DDS")]
        public bool ForceDDSMultiSurface { get; set; }

        [CLIFlag("sheet-multisurface", Category = "Extract", Help = "Save multisurface textures as one large image, tiled across in the Y (vertical) direction")]
        public bool SheetMultiSurface { get; set; }

        [CLIFlag("extract-mips", Category = "Extract", Help = "Extract mip files")]
        public bool SaveMips { get; set; }

        [CLIFlag("subtitles-with-sounds", Category = "Extract", Help = "Extract subtitles alongside voicelines")]
        public bool SubtitlesWithSounds { get; set; }

        [CLIFlag("subtitles-as-sounds", Category = "Extract", Help = "Saves the sound files as the subtitle")]
        public bool SubtitlesAsSound { get; set; }

        [CLIFlag("voice-group-by-hero", Hidden = true)]
        public bool VoiceGroupByHero { get; set; }

        [CLIFlag("voice-group-by-type", Hidden = true)]
        public bool VoiceGroupByType { get; set; }

        [CLIFlag("skip-map-env-sound", Category = "Extract", Help = "Skip map Environment sound extraction")]
        public bool SkipMapEnvironmentSound { get; set; }

        [CLIFlag("skip-map-env-lut", Category = "Extract", Help = "Skip map Environment lut extraction")]
        public bool SkipMapEnvironmentLUT { get; set; }

        [CLIFlag("skip-map-env-blend", Category = "Extract", Help = "Skip map Environment blend cubemap extraction")]
        public bool SkipMapEnvironmentBlendCubemap { get; set; }

        [CLIFlag("skip-map-env-ground", Category = "Extract", Help = "Skip map Environment ground cubemap extraction")]
        public bool SkipMapEnvironmentGroundCubemap { get; set; }

        [CLIFlag("skip-map-env-sky", Category = "Extract", Help = "Skip map Environment sky cubemap extraction")]
        public bool SkipMapEnvironmentSkyCubemap { get; set; }

        [CLIFlag("skip-map-env-skybox", Category = "Extract", Help = "Skip map Environment skybox extraction")]
        public bool SkipMapEnvironmentSkybox { get; set; }

        [CLIFlag("skip-map-env-entity", Category = "Extract", Help = "Skip map Environment entity extraction")]
        public bool SkipMapEnvironmentEntity { get; set; }

        [CLIFlag("xml", Category = "Extract", Help = "Convert STUs to xml when extracted with ExtractDebugType", Hidden = true)]
        public bool ConvertToXML { get; set; }
    }
}