using PicoFacialDataModule.Models;
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;

namespace PicoFacialDataModule
{
    using static PicoBlendshapes;
    using static UnifiedExpressions;
    using static EyePoseStatus;
    using static VideoInputType;

    public class EyeTrackingParser
    {
        private float eyePupilDilationEyeOpennessThreshold;

        public EyeTrackingParser(ModuleSettings settings)
        {
            this.eyePupilDilationEyeOpennessThreshold = settings.TrackingSettings.eyePupilDilationEyeOpennessThreshold;
        }

        public void Parse(PxrEyePoseDataV2 eyeData, PicoFTInfo picoFTInfo)
        {
            var eye = UnifiedTracking.Data.Eye;
            var blendshapes = picoFTInfo.BlendshapeWeight;
            var face = new UnifiedExpressionsSetter();

            if (picoFTInfo.VideoInputValid[(int)VIDEO_INPUT_EYE] != 1)
                return;

            #region LEFT EYE

            if (eyeData.LeftEyePoseStatus.HasFlag(EYE_GAZE_VECTOR_VALID))
            {
                eye.Left.Gaze.x = eyeData.LeftEyeGazeVectorX;
                eye.Left.Gaze.y = eyeData.LeftEyeGazeVectorY;

                eye.Left.Openness = eyeData.LeftEyeOpenness;
            } else // Fallback
            {
                eye.Left.Gaze.x = eyeData.CombinedEyeGazeVectorX;
                eye.Left.Gaze.y = eyeData.CombinedEyeGazeVectorY;

                eye.Left.Openness = 1f - blendshapes[EyeBlinkL];
            }

            if (eyeData.LeftEyePupilDilation == 0)
                eyeData.LeftEyePupilDilation = 35f;

            if (
                eyeData.LeftEyeOpenness > eyePupilDilationEyeOpennessThreshold
            )
                eye.Left.PupilDiameter_MM = eyeData.LeftEyePupilDilation / 10;

            #endregion

            #region RIGHT EYE

            if (eyeData.RightEyePoseStatus.HasFlag(EYE_GAZE_VECTOR_VALID))
            {
                eye.Right.Gaze.x = eyeData.RightEyeGazeVectorX;
                eye.Right.Gaze.y = eyeData.RightEyeGazeVectorY;

                eye.Right.Openness = eyeData.RightEyeOpenness;
            } else // Fallback
            {
                eye.Right.Gaze.x = eyeData.CombinedEyeGazeVectorX;
                eye.Right.Gaze.y = eyeData.CombinedEyeGazeVectorY;

                eye.Right.Openness = 1f - blendshapes[EyeBlinkR];
            }

            if (eyeData.RightEyePupilDilation == 0)
                eyeData.RightEyePupilDilation = 35f;

            if (
                eyeData.RightEyeOpenness > eyePupilDilationEyeOpennessThreshold
            )
                eye.Right.PupilDiameter_MM = eyeData.RightEyePupilDilation / 10;

            #endregion

            #region Eyebrow Expressions

            face[BrowPinchRight] = blendshapes[BrowDownR];
            face[BrowPinchLeft] = blendshapes[BrowDownL];
            face[BrowLowererRight] = blendshapes[BrowDownR];
            face[BrowLowererLeft] = blendshapes[BrowDownL];
            face[BrowInnerUpRight] = blendshapes[BrowInnerUp];
            face[BrowInnerUpLeft] = blendshapes[BrowInnerUp];
            face[BrowOuterUpRight] = blendshapes[BrowOuterUpR];
            face[BrowOuterUpLeft] = blendshapes[BrowOuterUpL];

            #endregion

#if EYEDEBUG
            Console.SetCursorPosition(0,0);
            Console.Write(eyeData);
#endif
        }
    }
}
