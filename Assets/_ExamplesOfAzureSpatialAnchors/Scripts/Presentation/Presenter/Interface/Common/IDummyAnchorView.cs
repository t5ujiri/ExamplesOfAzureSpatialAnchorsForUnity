using CAFU.Core;
using UnityEngine;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Interface.Common
{
    public interface IDummyAnchorView : IView
    {
        GameObject GameObject { get; }
        void SetPose(Pose pose);
    }
}