using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TankLib.Math;

namespace TankLib {
    /// <summary>Tank Animation, type 006</summary>
    public class teAnimation {
        public BoneAnimation[] BoneAnimations;

        /// <summary>Bone IDs</summary>
        public int[] BoneList;

        /// <summary>Header data</summary>
        public AnimHeader Header;

        /// <summary>Bone info tables</summary>
        public InfoTable[] InfoTables;

        /// <summary>Number of info tables</summary>
        public int InfoTableSize;

        /// <summary>Load animation from a stream</summary>
        public teAnimation(Stream stream, bool keepOpen = false) {
            using (var reader = new BinaryReader(stream, Encoding.Default, keepOpen)) {
                Header = reader.Read<AnimHeader>();

                InfoTableSize = (int) (Header.FPS * Header.Duration) + 1;

                reader.BaseStream.Position = Header.BoneListOffset;
                BoneList                   = reader.ReadArray<int>(Header.BoneCount);

                reader.BaseStream.Position = Header.InfoTableOffset;
                InfoTables                 = new InfoTable[Header.BoneCount];
                BoneAnimations             = new BoneAnimation[Header.BoneCount];

                for (var boneIndex = 0; boneIndex < Header.BoneCount; boneIndex++) {
                    var streamPos     = reader.BaseStream.Position;
                    var infoTable     = reader.Read<InfoTable>();
                    var afterTablePos = reader.BaseStream.Position;
                    InfoTables[boneIndex] = infoTable;

                    var scaleIndicesPos    = (long) infoTable.ScaleIndicesOffset    * 4 + streamPos;
                    var positionIndicesPos = (long) infoTable.PositionIndicesOffset * 4 + streamPos;
                    var rotationIndicesPos = (long) infoTable.RotationIndicesOffset * 4 + streamPos;
                    var scaleDataPos       = (long) infoTable.ScaleDataOffset       * 4 + streamPos;
                    var positionDataPos    = (long) infoTable.PositionDataOffset    * 4 + streamPos;
                    var rotationDataPos    = (long) infoTable.RotationDataOffset    * 4 + streamPos;

                    reader.BaseStream.Position = scaleIndicesPos;
                    var scaleIndices = ReadIndices(reader, infoTable.ScaleCount);

                    reader.BaseStream.Position = positionIndicesPos;
                    var positionIndices = ReadIndices(reader, infoTable.PositionCount);

                    reader.BaseStream.Position = rotationIndicesPos;
                    var rotationIndices = ReadIndices(reader, infoTable.RotationCount);

                    var boneAnimation = new BoneAnimation();
                    BoneAnimations[boneIndex] = boneAnimation;

                    reader.BaseStream.Position = scaleDataPos;
                    for (var j = 0; j < infoTable.ScaleCount; j++) {
                        var frame = System.Math.Abs(scaleIndices[j]) % InfoTableSize;
                        boneAnimation.Scales[frame] = ReadScale(reader);
                    }

                    reader.BaseStream.Position = positionDataPos;
                    for (var j = 0; j < infoTable.PositionCount; j++) {
                        var frame = System.Math.Abs(positionIndices[j]) % InfoTableSize;
                        boneAnimation.Positions[frame] = ReadPosition(reader);
                    }

                    reader.BaseStream.Position = rotationDataPos;
                    for (var j = 0; j < infoTable.RotationCount; j++) {
                        var frame = System.Math.Abs(rotationIndices[j]) % InfoTableSize;
                        boneAnimation.Rotations[frame] = ReadRotation(reader);
                    }

                    reader.BaseStream.Position = afterTablePos;
                }
            }
        }

        /// <summary>
        ///     Read rotation value
        /// </summary>
        /// <param name="reader">Source reader</param>
        /// <returns>Rotation value</returns>
        private static teQuat ReadRotation(BinaryReader reader) {
            var x = reader.ReadUInt16();
            var y = reader.ReadUInt16();
            var z = reader.ReadUInt16();

            return UnpackRotation(x, y, z);
        }

        /// <summary>
        ///     Read position value
        /// </summary>
        /// <param name="reader">Source reader</param>
        /// <returns>Position value</returns>
        private static teVec3 ReadPosition(BinaryReader reader) {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();

            return new teVec3(x, y, z);
        }

        /// <summary>
        ///     Read scale value
        /// </summary>
        /// <param name="reader">Source reader</param>
        /// <returns>Scale value</returns>
        private static teVec3 ReadScale(BinaryReader reader) {
            var x = reader.ReadUInt16();
            var y = reader.ReadUInt16();
            var z = reader.ReadUInt16();

            return UnpackScale(x, y, z);
        }

        /// <summary>
        ///     Unpack rotation value
        /// </summary>
        /// <param name="a">Packed A component</param>
        /// <param name="b">Packed B component</param>
        /// <param name="c">Packed C component</param>
        /// <returns>Unpacked rotation value</returns>
        private static teQuat UnpackRotation(ushort a, ushort b, ushort c) {
            var q = new teQuat();

            var axis1 = a >> 15;
            var axis2 = b >> 15;
            var axis  = (axis2 << 1) | axis1;

            a = (ushort) (a & 0x7FFF);
            b = (ushort) (b & 0x7FFF);

            var x = 1.41421 * (a                        - 0x4000) / 0x8000;
            var y = 1.41421 * (b                        - 0x4000) / 0x8000;
            var z = 1.41421 * (c                        - 0x8000) / 0x10000;
            var w = System.Math.Pow(1.0 - x * x - y * y - z * z, 0.5);

            // Console.Out.WriteLine("Unpack Values: X: {0}, Y: {1}, Z: {2}, W: {3}, Axis: {4}", x, y, z, w, axis);

            if (axis == 0)
                q = new teQuat(w, x, y, z);
            else if (axis == 1)
                q = new teQuat(x, w, y, z);
            else if (axis == 2)
                q = new teQuat(x, y, w, z);
            else if (axis == 3)
                q = new teQuat(x, y, z, w);
            else
                Console.Out.WriteLine($"Unknown Axis detected! Axis: {axis}");

            return q;
        }

        /// <summary>
        ///     Unpack scale value
        /// </summary>
        /// <param name="a">Packed X component</param>
        /// <param name="b">Packed Y component</param>
        /// <param name="c">Packed Z component</param>
        /// <returns>Unpacked scale value</returns>
        private static teVec3 UnpackScale(ushort a, ushort b, ushort c) {
            var x = a / 1024d;
            var y = b / 1024d;
            var z = c / 1024d;

            var xDelta = (1 - x) / 1024d;
            var yDelta = (1 - y) / 1024d;
            var zDelta = (1 - z) / 1024d;

            return new teVec3(xDelta + 1, yDelta + 1, zDelta + 1);
        }

        /// <summary>
        ///     Read frame indices
        /// </summary>
        /// <param name="reader">Source BinaryReader</param>
        /// <param name="count">Number of indices</param>
        /// <returns>Frame indices</returns>
        private int[] ReadIndices(BinaryReader reader, ushort count) {
            var ret = new int[count];
            for (var i = 0; i < count; i++)
                if (InfoTableSize <= 255)
                    ret[i] = reader.ReadByte();
                else
                    ret[i] = reader.ReadInt16();
            return ret;
        }

        /// <summary>Animation header</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct AnimHeader {
            /// <summary>
            ///     Animation "priority". Is 160 for many animations. No idea what it is used for
            /// </summary>
            public uint Priority;

            /// <summary>Duration, in seconds</summary>
            public float Duration;

            /// <summary>Frames Per Second</summary>
            public float FPS;

            /// <summary>Number of bones animated</summary>
            public ushort BoneCount;

            /// <summary>Unknown flags</summary>
            public ushort Flags;

            private ulong Unknown;

            /// <summary>teAnimationEffect(?) reference</summary>
            /// <remarks>File type 08F</remarks>
            public teResourceGUID Effect;

            private ulong Padding;

            /// <summary>Offset to bone list</summary>
            public long BoneListOffset;

            /// <summary>Offset to info table</summary>
            public long InfoTableOffset;

            /// <summary>Offset to end of animation</summary>
            public long Size;

            /// <summary>Offset to end of file</summary>
            public long Eof;


            /// <summary>Unknown value that is always 0. Maybe padding</summary>
            public long Zero;
        }

        public struct InfoTable {
            /// <summary>Number of scales</summary>
            public ushort ScaleCount;

            /// <summary>Number of positions</summary>
            public ushort PositionCount;

            /// <summary>Number of rotations</summary>
            public ushort RotationCount;

            /// <summary>Unknown flags</summary>
            public ushort Flags;

            /// <summary>Offset to scale indices</summary>
            public int ScaleIndicesOffset;

            /// <summary>Offset to position indices</summary>
            public int PositionIndicesOffset;

            /// <summary>Offset to rotation indices</summary>
            public int RotationIndicesOffset;

            /// <summary>Offset to scale data</summary>
            public int ScaleDataOffset;

            /// <summary>Offset to postion data</summary>
            public int PositionDataOffset;

            /// <summary>Offset to rotation data</summary>
            public int RotationDataOffset;
        }

        public class BoneAnimation {
            public Dictionary<int, teVec3> Positions;
            public Dictionary<int, teQuat> Rotations;
            public Dictionary<int, teVec3> Scales;

            public BoneAnimation() {
                Positions = new Dictionary<int, teVec3>();
                Scales    = new Dictionary<int, teVec3>();
                Rotations = new Dictionary<int, teQuat>();
            }

            // key = frame number; value = abs value this frame
            // then slerp between or whatever
        }
    }
}
