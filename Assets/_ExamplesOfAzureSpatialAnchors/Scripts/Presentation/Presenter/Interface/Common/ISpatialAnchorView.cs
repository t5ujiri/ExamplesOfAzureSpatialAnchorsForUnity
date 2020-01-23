using CAFU.Core;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEngine;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Interface.Common
{
    public interface ISpatialAnchorView : IView
    {
        GameObject GameObject { get; }
        CloudNativeAnchor CloudNativeAnchor { get; }
    }
}