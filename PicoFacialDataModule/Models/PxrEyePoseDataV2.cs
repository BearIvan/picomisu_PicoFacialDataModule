using System.Runtime.InteropServices;
using System.Text;

namespace PicoFacialDataModule.Models
{
    [Flags]
    public enum EyePoseStatus : uint
    {
        GAZE_POINT_VALID = (1 << 0),
        GAZE_VECTOR_VALID = (1 << 1),
        EYE_OPENNESS_VALID = (1 << 2),
        EYE_PUPIL_DILATION_VALID = (1 << 3),
        EYE_POSITION_GUIDE_VALID = (1 << 4),
        EYE_PUPIL_POSITION_VALID = (1 << 5),
        EYE_CONVERGENCE_DISTANCE_VALID = (1 << 6),
        EYE_GAZE_POINT_VALID = (1 << 7),
        EYE_GAZE_VECTOR_VALID = (1 << 8),
        PUPIL_DISTANCE_VALID = (1 << 9),
        CONVERGENCE_DISTANCE_VALID = (1 << 10),
        PUPIL_DIAMETER_VALID = (1 << 11),
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PxrEyePoseDataV2
    {
        public uint Timestamp;

        public EyePoseStatus LeftEyePoseStatus;
        public EyePoseStatus RightEyePoseStatus;
        public EyePoseStatus CombinedEyePoseStatus;

        public float LeftEyeGazePointX;
        public float LeftEyeGazePointY;
        public float LeftEyeGazePointZ;

        public float RightEyeGazePointX;
        public float RightEyeGazePointY;
        public float RightEyeGazePointZ;

        public float CombinedEyeGazePointX;
        public float CombinedEyeGazePointY;
        public float CombinedEyeGazePointZ;

        public float LeftEyeGazeVectorX;
        public float LeftEyeGazeVectorY;
        public float LeftEyeGazeVectorZ;

        public float RightEyeGazeVectorX;
        public float RightEyeGazeVectorY;
        public float RightEyeGazeVectorZ;

        public float CombinedEyeGazeVectorX;
        public float CombinedEyeGazeVectorY;
        public float CombinedEyeGazeVectorZ;

        public float LeftEyeOpenness;
        public float RightEyeOpenness;

        public float LeftEyePupilDilation;
        public float RightEyePupilDilation;

        public float LeftEyePositionGuideX;
        public float LeftEyePositionGuideY;
        public float LeftEyePositionGuideZ;

        public float RightEyePositionGuideX;
        public float RightEyePositionGuideY;
        public float RightEyePositionGuideZ;

        public float FoveatedGazeDirectionX;
        public float FoveatedGazeDirectionY;
        public float FoveatedGazeDirectionZ;

        public EyePoseStatus FoveatedGazeTrackingState;

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Timestamp: {Timestamp}");

            sb.AppendLine($"LeftEyePoseStatus: {(uint)LeftEyePoseStatus:B8}");
            sb.AppendLine($"RightEyePoseStatus: {(uint)RightEyePoseStatus:B8}");
            sb.AppendLine($"CombinedEyePoseStatus: {(uint)CombinedEyePoseStatus:B8}");

            sb.AppendLine($"LeftEyeGazePointX: {LeftEyeGazePointX}");
            sb.AppendLine($"LeftEyeGazePointY: {LeftEyeGazePointY}");
            sb.AppendLine($"LeftEyeGazePointZ: {LeftEyeGazePointZ}");

            sb.AppendLine($"RightEyeGazePointX: {RightEyeGazePointX}");
            sb.AppendLine($"RightEyeGazePointY: {RightEyeGazePointY}");
            sb.AppendLine($"RightEyeGazePointZ: {RightEyeGazePointZ}");

            sb.AppendLine($"CombinedEyeGazePointX: {CombinedEyeGazePointX}");
            sb.AppendLine($"CombinedEyeGazePointY: {CombinedEyeGazePointY}");
            sb.AppendLine($"CombinedEyeGazePointZ: {CombinedEyeGazePointZ}");

            sb.AppendLine($"LeftEyeGazeVectorX: {LeftEyeGazeVectorX}");
            sb.AppendLine($"LeftEyeGazeVectorY: {LeftEyeGazeVectorY}");
            sb.AppendLine($"LeftEyeGazeVectorZ: {LeftEyeGazeVectorZ}");

            sb.AppendLine($"RightEyeGazeVectorX: {RightEyeGazeVectorX}");
            sb.AppendLine($"RightEyeGazeVectorY: {RightEyeGazeVectorY}");
            sb.AppendLine($"RightEyeGazeVectorZ: {RightEyeGazeVectorZ}");

            sb.AppendLine($"CombinedEyeGazeVectorX: {CombinedEyeGazeVectorX}");
            sb.AppendLine($"CombinedEyeGazeVectorY: {CombinedEyeGazeVectorY}");
            sb.AppendLine($"CombinedEyeGazeVectorZ: {CombinedEyeGazeVectorZ}");

            sb.AppendLine($"LeftEyeOpenness: {LeftEyeOpenness}");
            sb.AppendLine($"RightEyeOpenness: {RightEyeOpenness}");

            sb.AppendLine($"LeftEyePupilDilation: {LeftEyePupilDilation}");
            sb.AppendLine($"RightEyePupilDilation: {RightEyePupilDilation}");

            sb.AppendLine($"LeftEyePositionGuideX: {LeftEyePositionGuideX}");
            sb.AppendLine($"LeftEyePositionGuideY: {LeftEyePositionGuideY}");
            sb.AppendLine($"LeftEyePositionGuideZ: {LeftEyePositionGuideZ}");

            sb.AppendLine($"FoveatedGazeDirectionX: {FoveatedGazeDirectionX}");
            sb.AppendLine($"FoveatedGazeDirectionY: {FoveatedGazeDirectionY}");
            sb.AppendLine($"FoveatedGazeDirectionZ: {FoveatedGazeDirectionZ}");

            sb.AppendLine($"FoveatedGazeTrackingState: {(uint)FoveatedGazeTrackingState:B8}");

            return sb.ToString();
        }
    }
}
